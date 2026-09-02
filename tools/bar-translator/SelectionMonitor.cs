using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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

        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_C = 0x43;
        private const ushort VK_MENU = 0x12; // Alt
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint CF_UNICODETEXT = 13;

        private static LowLevelMouseProc? mouseProc;
        private static IntPtr hookId = IntPtr.Zero;

        private static bool isMouseDown = false;
        private static POINT downPoint;
        private static long downTime = 0;
        private static bool hasMoved = false;

        private static long lastClickUpTime = 0;
        private static POINT lastClickUpPoint;

        private static volatile bool isSimulatingCopy = false;
        private static string lastProcessedText = string.Empty;
        private static long lastProcessedTimestamp = 0;
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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

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

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        public static void Start() {
            mouseProc = HookCallback;
            IntPtr hMod = GetModuleHandle(null);
            hookId = SetWindowsHookEx(WH_MOUSE_LL, mouseProc, hMod, 0);

            // Message-only window for manual clipboard listener
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
                    hasMoved = false;
                } else if (msg == WM_MOUSEMOVE) {
                    if (isMouseDown) {
                        int dist = Math.Abs(hookStruct.pt.x - downPoint.x) + Math.Abs(hookStruct.pt.y - downPoint.y);
                        if (dist >= 5) {
                            hasMoved = true;
                        }
                    }
                } else if (msg == WM_LBUTTONUP) {
                    if (isMouseDown) {
                        isMouseDown = false;
                        long now = Environment.TickCount64;
                        long holdDuration = now - downTime;
                        int totalDist = Math.Abs(hookStruct.pt.x - downPoint.x) + Math.Abs(hookStruct.pt.y - downPoint.y);

                        // 1. Drag selection: mouse was moved while holding left button
                        bool isDragSelection = (totalDist >= 5 || hasMoved) && holdDuration >= 25 && holdDuration < 15000;

                        // 2. Double-click or Triple-click word/paragraph selection
                        int distFromLastClick = Math.Abs(hookStruct.pt.x - lastClickUpPoint.x) + Math.Abs(hookStruct.pt.y - lastClickUpPoint.y);
                        long timeSinceLastClick = now - lastClickUpTime;
                        bool isMultiClick = (timeSinceLastClick < 550 && distFromLastClick < 10);

                        lastClickUpTime = now;
                        lastClickUpPoint = hookStruct.pt;

                        if (isDragSelection || isMultiClick) {
                            if (StateManager.AutoCaptureEnabled && !IsIgnoredWindow(hookStruct.pt)) {
                                TriggerSelectionCapture();
                            }
                        }
                    }
                } else if (msg == WM_RBUTTONDOWN) {
                    isMouseDown = false;
                    hasMoved = false;
                }
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private static bool IsIgnoredWindow(POINT pt) {
            try {
                IntPtr hWnd = WindowFromPoint(pt);
                if (hWnd == IntPtr.Zero) return false;

                var sbClass = new StringBuilder(256);
                GetClassName(hWnd, sbClass, 256);
                string className = sbClass.ToString();

                // Ignore taskbar, desktop background
                if (className.Equals("Shell_TrayWnd", StringComparison.OrdinalIgnoreCase) ||
                    className.Equals("Shell_SecondaryTrayWnd", StringComparison.OrdinalIgnoreCase) ||
                    className.Equals("Progman", StringComparison.OrdinalIgnoreCase) ||
                    className.Equals("WorkerW", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }

                // Ignore YASB, Zebar, GlazeWM, and BarTranslator
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
                    // Small delay for target app highlight to settle
                    await Task.Delay(40, token);
                    if (token.IsCancellationRequested) return;

                    // Skip if modifier keys are being held
                    if ((GetAsyncKeyState((int)VK_CONTROL) & 0x8000) != 0 || (GetAsyncKeyState((int)VK_MENU) & 0x8000) != 0) {
                        return;
                    }

                    // Record sequence number before copy
                    uint seqBefore = GetClipboardSequenceNumber();

                    // Send Ctrl+C
                    isSimulatingCopy = true;
                    SendCopyKeystrokes();

                    // Poll for clipboard sequence change (up to 300ms)
                    bool sequenceChanged = false;
                    for (int i = 0; i < 30; i++) {
                        await Task.Delay(10, token);
                        if (token.IsCancellationRequested) return;

                        if (GetClipboardSequenceNumber() != seqBefore) {
                            sequenceChanged = true;
                            break;
                        }
                    }

                    isSimulatingCopy = false;

                    if (!sequenceChanged) {
                        await Task.Delay(20, token);
                    }

                    if (token.IsCancellationRequested) return;

                    string text = ReadClipboardTextWithRetry();
                    if (!string.IsNullOrWhiteSpace(text)) {
                        await ProcessSelectedTextAsync(text);
                    }
                } catch {
                    isSimulatingCopy = false;
                }
            }, token);
        }

        private static void SendCopyKeystrokes() {
            try {
                INPUT[] inputs = new INPUT[4];

                // 1. Ctrl Down
                inputs[0].type = INPUT_KEYBOARD;
                inputs[0].u.ki.wVk = VK_CONTROL;
                inputs[0].u.ki.wScan = 0;
                inputs[0].u.ki.dwFlags = 0;

                // 2. C Down
                inputs[1].type = INPUT_KEYBOARD;
                inputs[1].u.ki.wVk = VK_C;
                inputs[1].u.ki.wScan = 0;
                inputs[1].u.ki.dwFlags = 0;

                // 3. C Up
                inputs[2].type = INPUT_KEYBOARD;
                inputs[2].u.ki.wVk = VK_C;
                inputs[2].u.ki.wScan = 0;
                inputs[2].u.ki.dwFlags = KEYEVENTF_KEYUP;

                // 4. Ctrl Up
                inputs[3].type = INPUT_KEYBOARD;
                inputs[3].u.ki.wVk = VK_CONTROL;
                inputs[3].u.ki.wScan = 0;
                inputs[3].u.ki.dwFlags = KEYEVENTF_KEYUP;

                uint sent = SendInput(4, inputs, Marshal.SizeOf<INPUT>());
                if (sent == 0) {
                    keybd_event((byte)VK_CONTROL, 0x1D, 0, UIntPtr.Zero);
                    keybd_event((byte)VK_C, 0x2E, 0, UIntPtr.Zero);
                    keybd_event((byte)VK_C, 0x2E, KEYEVENTF_KEYUP, UIntPtr.Zero);
                    keybd_event((byte)VK_CONTROL, 0x1D, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
            } catch {
                keybd_event((byte)VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event((byte)VK_C, 0, 0, UIntPtr.Zero);
                keybd_event((byte)VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event((byte)VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }

        private static void OnManualClipboardChanged() {
            if (isSimulatingCopy) return;
            if (!StateManager.ClipboardTranslateEnabled) return;

            Task.Run(async () => {
                try {
                    await Task.Delay(10);
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

            // Word count limit: Up to 60 words for quick sentence translation
            string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 60) {
                return;
            }

            long now = Environment.TickCount64;
            // Debounce if the exact same text was processed within the last 400ms
            if (text.Equals(lastProcessedText, StringComparison.OrdinalIgnoreCase) && (now - lastProcessedTimestamp < 400)) {
                return;
            }

            lastProcessedText = text;
            lastProcessedTimestamp = now;

            var result = await TranslationEngine.TranslateToEnglishArabicAsync(text);
            if (result != null) {
                StateManager.UpdateState(result);
            }
        }

        private static string ReadClipboardTextWithRetry() {
            for (int i = 0; i < 6; i++) {
                try {
                    if (OpenClipboard(IntPtr.Zero)) {
                        try {
                            IntPtr hData = GetClipboardData(CF_UNICODETEXT);
                            if (hData != IntPtr.Zero) {
                                IntPtr pText = GlobalLock(hData);
                                if (pText != IntPtr.Zero) {
                                    try {
                                        string? text = Marshal.PtrToStringUni(pText);
                                        if (!string.IsNullOrWhiteSpace(text)) {
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
                } catch {}
                Thread.Sleep(15);
            }

            // Fallback: STA thread WinForms Clipboard
            string fallback = string.Empty;
            Thread t = new Thread(() => {
                try {
                    if (Clipboard.ContainsText()) {
                        fallback = Clipboard.GetText();
                    }
                } catch {}
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join(120);
            return fallback;
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
