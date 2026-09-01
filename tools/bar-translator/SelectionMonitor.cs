using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarTranslator {
    public static class SelectionMonitor {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private const byte VK_CONTROL = 0x11;
        private const byte VK_C = 0x43;
        private const byte VK_MENU = 0x12; // Alt
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint CF_UNICODETEXT = 13;

        private static LowLevelMouseProc? mouseProc;
        private static IntPtr hookId = IntPtr.Zero;

        private static bool isMouseDown = false;
        private static POINT downPoint;
        private static long downTime = 0;
        private static bool hasDragged = false;
        private static long lastUpTime = 0;
        private static POINT lastUpPoint;

        private static volatile bool isSimulatingCopy = false;
        private static string lastProcessedText = string.Empty;
        private static CancellationTokenSource? pendingCts;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static void Start() {
            mouseProc = HookCallback;
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            hookId = SetWindowsHookEx(WH_MOUSE_LL, mouseProc, GetModuleHandle(curModule?.ModuleName), 0);

            // Message-only window for clipboard listener
            var msgWindow = new ClipboardMessageWindow();
            msgWindow.ClipboardChanged += OnManualClipboardChanged;
            AddClipboardFormatListener(msgWindow.Handle);
        }

        public static void Stop() {
            if (hookId != IntPtr.Zero) {
                UnhookWindowsHookEx(hookId);
                hookId = IntPtr.Zero;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                int msg = wParam.ToInt32();
                var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                if (msg == WM_LBUTTONDOWN) {
                    isMouseDown = true;
                    downPoint = hookStruct.pt;
                    downTime = Environment.TickCount64;
                    hasDragged = false;
                } else if (msg == WM_MOUSEMOVE) {
                    if (isMouseDown) {
                        int dx = Math.Abs(hookStruct.pt.x - downPoint.x);
                        int dy = Math.Abs(hookStruct.pt.y - downPoint.y);
                        if (dx > 7 || dy > 7) {
                            hasDragged = true;
                        }
                    }
                } else if (msg == WM_LBUTTONUP) {
                    if (isMouseDown) {
                        isMouseDown = false;
                        long now = Environment.TickCount64;
                        long holdTime = now - downTime;

                        bool isDrag = hasDragged && holdTime >= 70 && holdTime < 8000;
                        bool isDouble = !hasDragged && (now - lastUpTime < 450) &&
                                        (Math.Abs(hookStruct.pt.x - lastUpPoint.x) + Math.Abs(hookStruct.pt.y - lastUpPoint.y) < 6);

                        lastUpTime = now;
                        lastUpPoint = hookStruct.pt;

                        if (isDrag || isDouble) {
                            if (!IsIgnoredWindow(hookStruct.pt)) {
                                TriggerSelectionCapture();
                            }
                        }
                    }
                } else if (msg == WM_RBUTTONDOWN) {
                    isMouseDown = false;
                    hasDragged = false;
                }
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private static bool IsIgnoredWindow(POINT pt) {
            try {
                IntPtr hWnd = WindowFromPoint(pt);
                if (hWnd == IntPtr.Zero) return false;

                // Check class name
                var sbClass = new StringBuilder(256);
                GetClassName(hWnd, sbClass, 256);
                string className = sbClass.ToString();

                if (className.Equals("Shell_TrayWnd", StringComparison.OrdinalIgnoreCase) ||
                    className.Equals("Shell_SecondaryTrayWnd", StringComparison.OrdinalIgnoreCase) ||
                    className.Equals("Progman", StringComparison.OrdinalIgnoreCase) ||
                    className.Equals("WorkerW", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }

                // Check process name
                GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid > 0) {
                    using var proc = Process.GetProcessById((int)pid);
                    string procName = proc.ProcessName.ToLowerInvariant();
                    if (procName == "yasb" || procName == "zebar" || procName == "glazewm" || procName == "bartranslator") {
                        return true;
                    }
                }
            } catch {}

            return false;
        }

        private static void TriggerSelectionCapture() {
            pendingCts?.Cancel();
            pendingCts = new CancellationTokenSource();
            var token = pendingCts.Token;

            Task.Run(async () => {
                try {
                    // Wait for target application to finalize selection
                    await Task.Delay(75, token);
                    if (token.IsCancellationRequested) return;

                    // Skip if modifier keys are being held
                    if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0 || (GetAsyncKeyState(VK_MENU) & 0x8000) != 0) {
                        return;
                    }

                    // Simulate Ctrl+C
                    isSimulatingCopy = true;
                    try {
                        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                        keybd_event(VK_C, 0, 0, UIntPtr.Zero);
                        keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                    } finally {
                        // Keep flag on briefly to filter WM_CLIPBOARDUPDATE
                        await Task.Delay(40, token);
                        isSimulatingCopy = false;
                    }

                    if (token.IsCancellationRequested) return;

                    string text = ReadClipboardTextWithRetry();
                    await ProcessSelectedTextAsync(text);
                } catch {}
            }, token);
        }

        private static void OnManualClipboardChanged() {
            // If the clipboard update was triggered by our simulated Ctrl+C, ignore
            if (isSimulatingCopy) return;

            Task.Run(async () => {
                try {
                    await Task.Delay(30);
                    string text = ReadClipboardTextWithRetry();
                    await ProcessSelectedTextAsync(text);
                } catch {}
            });
        }

        private static async Task ProcessSelectedTextAsync(string text) {
            if (string.IsNullOrWhiteSpace(text)) return;

            text = TranslationEngine.SanitizeText(text);
            if (string.IsNullOrWhiteSpace(text) || !TranslationEngine.ContainsEnglish(text)) {
                return;
            }

            if (text.Equals(lastProcessedText, StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            lastProcessedText = text;

            var result = await TranslationEngine.TranslateToEnglishArabicAsync(text);
            if (result != null) {
                StateManager.UpdateState(result);
            }
        }

        private static string ReadClipboardTextWithRetry() {
            for (int i = 0; i < 4; i++) {
                if (OpenClipboard(IntPtr.Zero)) {
                    try {
                        IntPtr hData = GetClipboardData(CF_UNICODETEXT);
                        if (hData != IntPtr.Zero) {
                            IntPtr pText = GlobalLock(hData);
                            if (pText != IntPtr.Zero) {
                                try {
                                    string? text = Marshal.PtrToStringUni(pText);
                                    if (!string.IsNullOrEmpty(text)) {
                                        return text;
                                    }
                                } finally {
                                    GlobalUnlock(hData);
                                }
                            }
                        }
                    } finally {
                        CloseClipboard();
                    }
                }
                Thread.Sleep(15);
            }
            return string.Empty;
        }

        private class ClipboardMessageWindow : NativeWindow {
            public event Action? ClipboardChanged;

            public ClipboardMessageWindow() {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m) {
                if (m.Msg == WM_CLIPBOARDUPDATE) {
                    ClipboardChanged?.Invoke();
                }
                base.WndProc(ref m);
            }
        }
    }
}
