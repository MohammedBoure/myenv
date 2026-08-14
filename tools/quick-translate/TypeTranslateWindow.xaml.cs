using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Clipboard = System.Windows.Clipboard;
using WpfFlowDirection = System.Windows.FlowDirection;

namespace QuickTranslate {
    public partial class TypeTranslateWindow : Window {
        private readonly IntPtr targetHwnd;
        private readonly DispatcherTimer debounceTimer;
        private bool isPasting = false;
        private bool isUserEditingTranslation = false;

        private struct LangPair {
            public string Label;
            public string SourceLang;
            public string TargetLang;
            public bool IsAuto;

            public LangPair(string label, string sourceLang, string targetLang, bool isAuto = false) {
                Label = label;
                SourceLang = sourceLang;
                TargetLang = targetLang;
                IsAuto = isAuto;
            }
        }

        private readonly LangPair[] languagePairs = new[] {
            new LangPair("تلقائي (AR ⇄ EN)", "auto", "en", true),
            new LangPair("AR ➔ EN", "ar", "en", false),
            new LangPair("EN ➔ AR", "en", "ar", false),
            new LangPair("AR ➔ FR", "ar", "fr", false),
            new LangPair("AR ➔ DE", "ar", "de", false),
            new LangPair("AR ➔ ES", "ar", "es", false),
            new LangPair("AR ➔ TR", "ar", "tr", false),
        };
        private int currentPairIndex = 0;

        #region Win32 API Imports & Structs
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;

        private const byte VK_SHIFT = 0x10;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_MENU = 0x12; // Alt
        private const byte VK_RETURN = 0x0D;
        private const byte VK_INSERT = 0x2D;
        private const byte VK_V = 0x56;
        private const byte VK_LWIN = 0x5B;
        private const byte VK_RWIN = 0x5C;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        #endregion

        public TypeTranslateWindow(IntPtr previousHwnd) {
            InitializeComponent();
            targetHwnd = previousHwnd;

            debounceTimer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(160)
            };
            debounceTimer.Tick += async (s, e) => {
                debounceTimer.Stop();
                await PerformLiveTranslationAsync();
            };

            UpdateLanguageBadge();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            TxtInput.Focus();
            Keyboard.Focus(TxtInput);
        }

        private void UpdateLanguageBadge() {
            var pair = languagePairs[currentPairIndex];
            if (TxtLangDirection != null) {
                TxtLangDirection.Text = pair.Label;
            }
        }

        private void CycleLanguagePair() {
            currentPairIndex = (currentPairIndex + 1) % languagePairs.Length;
            UpdateLanguageBadge();
            
            // Re-trigger translation for current input
            if (!string.IsNullOrWhiteSpace(TxtInput.Text)) {
                debounceTimer.Stop();
                debounceTimer.Start();
            }
        }

        private void BtnLangToggle_Click(object sender, RoutedEventArgs e) {
            CycleLanguagePair();
            TxtInput.Focus();
        }

        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e) {
            if (TxtPlaceholder == null) return;

            string text = TxtInput.Text;
            TxtPlaceholder.Visibility = string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;

            // Auto switch input direction based on entered text
            if (TranslationService.ContainsArabic(text)) {
                TxtInput.FlowDirection = WpfFlowDirection.RightToLeft;
            } else if (!string.IsNullOrWhiteSpace(text)) {
                TxtInput.FlowDirection = WpfFlowDirection.LeftToRight;
            }

            isUserEditingTranslation = false;
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        private async Task PerformLiveTranslationAsync() {
            string text = TxtInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) {
                TxtTranslation.Text = string.Empty;
                TxtStatus.Text = "جاهز";
                TxtLiveIndicator.Text = "⚡ مباشر";
                return;
            }

            var pair = languagePairs[currentPairIndex];
            string sourceLang = pair.SourceLang;
            string targetLang = pair.TargetLang;

            if (pair.IsAuto) {
                bool hasArabic = TranslationService.ContainsArabic(text);
                targetLang = hasArabic ? "en" : "ar";
                sourceLang = "auto";
            }

            TxtStatus.Text = "جاري الترجمة...";
            TxtLiveIndicator.Text = "⏳ جاري الترجمة...";

            TranslationResult result = await TranslationService.TranslateAsync(text, targetLang, sourceLang);

            if (!isUserEditingTranslation) {
                if (result.IsSuccess) {
                    TxtTranslation.Text = result.TranslatedText;
                    TxtStatus.Text = "جاهز للتطبيق";
                    TxtLiveIndicator.Text = "⚡ مباشر";
                    
                    // Adjust translation box text direction
                    TxtTranslation.FlowDirection = (targetLang.Equals("ar", StringComparison.OrdinalIgnoreCase)) 
                        ? WpfFlowDirection.RightToLeft 
                        : WpfFlowDirection.LeftToRight;
                } else {
                    TxtStatus.Text = "خطأ بالاتصال";
                    TxtLiveIndicator.Text = "⚠ تنبيه";
                }
            }
        }

        private void TxtTranslation_TextChanged(object sender, TextChangedEventArgs e) {
            if (TxtTranslation.IsFocused) {
                isUserEditingTranslation = true;
            }
        }

        private void TxtInput_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                    // Allow Shift+Enter for newline
                    int caretIndex = TxtInput.CaretIndex;
                    TxtInput.Text = TxtInput.Text.Insert(caretIndex, Environment.NewLine);
                    TxtInput.CaretIndex = caretIndex + Environment.NewLine.Length;
                    e.Handled = true;
                    return;
                }

                // Normal Enter -> Apply & Paste directly
                e.Handled = true;
                _ = ApplyAndPasteAsync();
            } else if (e.Key == Key.Tab) {
                e.Handled = true;
                CycleLanguagePair();
            } else if (e.Key == Key.Escape) {
                e.Handled = true;
                this.Close();
            }
        }

        public async Task ApplyAndPasteAsync() {
            if (isPasting) return;
            isPasting = true;

            string textToPaste = TxtTranslation.Text.Trim();

            // If user pressed Enter before live translation debounce completed
            if (string.IsNullOrWhiteSpace(textToPaste) && !string.IsNullOrWhiteSpace(TxtInput.Text)) {
                var pair = languagePairs[currentPairIndex];
                string sourceLang = pair.SourceLang;
                string targetLang = pair.TargetLang;
                if (pair.IsAuto) {
                    targetLang = TranslationService.ContainsArabic(TxtInput.Text) ? "en" : "ar";
                }

                var result = await TranslationService.TranslateAsync(TxtInput.Text, targetLang, sourceLang);
                textToPaste = result.IsSuccess ? result.TranslatedText : TxtInput.Text.Trim();
            }

            if (string.IsNullOrWhiteSpace(textToPaste)) {
                this.Close();
                return;
            }

            // 1. Hide UI immediately for instant perception
            this.Hide();

            // 2. Put translated text into Windows Clipboard (as reliable backup)
            try {
                Clipboard.SetText(textToPaste);
            } catch {}

            // 3. Restore target window focus cleanly (without sending any Alt or menu keys)
            RestoreTargetFocus(targetHwnd);

            // 4. Short delay for active window focus switch
            await Task.Delay(50);

            // 5. Release modifier keys only if physically pressed
            ReleasePhysicallyPressedModifiers();

            // 6. Direct Unicode Text Injection (100% universal across Terminals, CLI, GUI apps)
            bool typedViaUnicode = TypeTextUnicode(textToPaste);

            // 7. Fallback to smart keystrokes if Unicode injection was not sent
            if (!typedViaUnicode) {
                try {
                    if (IsWindowsTerminalOrModernConsole(targetHwnd)) {
                        SendCtrlShiftV();
                    } else if (IsLegacyConsole(targetHwnd)) {
                        SendShiftInsert();
                    } else {
                        SendCtrlV();
                    }
                } catch {}
            }

            // 8. Finish & Close
            await Task.Delay(30);
            this.Close();
        }

        #region Non-Invasive Focus & Direct Typing Helpers
        private static void RestoreTargetFocus(IntPtr hWnd) {
            if (hWnd == IntPtr.Zero) return;

            try {
                IntPtr foregroundWnd = GetForegroundWindow();
                if (foregroundWnd == hWnd) return;

                uint targetThread = GetWindowThreadProcessId(hWnd, out _);
                uint currentThread = GetCurrentThreadId();

                if (targetThread != currentThread && targetThread != 0) {
                    AttachThreadInput(currentThread, targetThread, true);
                    BringWindowToTop(hWnd);
                    SetForegroundWindow(hWnd);
                    SetFocus(hWnd);
                    AttachThreadInput(currentThread, targetThread, false);
                } else {
                    BringWindowToTop(hWnd);
                    SetForegroundWindow(hWnd);
                    SetFocus(hWnd);
                }
            } catch {}
        }

        private static bool TypeTextUnicode(string text) {
            if (string.IsNullOrEmpty(text)) return false;

            try {
                var inputs = new List<INPUT>(text.Length * 2);
                int structSize = Marshal.SizeOf<INPUT>();

                foreach (char c in text) {
                    if (c == '\r') continue;

                    // Key Down
                    inputs.Add(new INPUT {
                        type = INPUT_KEYBOARD,
                        U = new InputUnion {
                            ki = new KEYBDINPUT {
                                wVk = 0,
                                wScan = c,
                                dwFlags = KEYEVENTF_UNICODE,
                                time = 0,
                                dwExtraInfo = UIntPtr.Zero
                            }
                        }
                    });

                    // Key Up
                    inputs.Add(new INPUT {
                        type = INPUT_KEYBOARD,
                        U = new InputUnion {
                            ki = new KEYBDINPUT {
                                wVk = 0,
                                wScan = c,
                                dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                                time = 0,
                                dwExtraInfo = UIntPtr.Zero
                            }
                        }
                    });
                }

                uint result = SendInput((uint)inputs.Count, inputs.ToArray(), structSize);
                return result > 0;
            } catch {
                return false;
            }
        }

        private static void ReleasePhysicallyPressedModifiers() {
            try {
                if ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
                    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
                    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
                    keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if ((GetAsyncKeyState(VK_LWIN) & 0x8000) != 0)
                    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if ((GetAsyncKeyState(VK_RWIN) & 0x8000) != 0)
                    keybd_event(VK_RWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if ((GetAsyncKeyState(VK_RETURN) & 0x8000) != 0)
                    keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            } catch {}
        }

        private static void SendCtrlV() {
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void SendCtrlShiftV() {
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            keybd_event(VK_SHIFT, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void SendShiftInsert() {
            keybd_event(VK_SHIFT, 0, 0, UIntPtr.Zero);
            keybd_event(VK_INSERT, 0, 0, UIntPtr.Zero);
            keybd_event(VK_INSERT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static bool IsWindowsTerminalOrModernConsole(IntPtr hWnd) {
            if (hWnd == IntPtr.Zero) return false;
            try {
                var sb = new StringBuilder(256);
                GetClassName(hWnd, sb, sb.Capacity);
                string className = sb.ToString();

                if (className.Contains("CASCADIA_HOSTING_WINDOW_CLASS", StringComparison.OrdinalIgnoreCase) ||
                    className.Contains("mintty", StringComparison.OrdinalIgnoreCase) ||
                    className.Contains("Alacritty", StringComparison.OrdinalIgnoreCase) ||
                    className.Contains("WezTerm", StringComparison.OrdinalIgnoreCase) ||
                    className.Contains("VirtualConsoleClass", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }

                GetWindowThreadProcessId(hWnd, out uint processId);
                if (processId > 0) {
                    using var proc = Process.GetProcessById((int)processId);
                    string procName = proc.ProcessName.ToLowerInvariant();
                    if (procName.Contains("windowsterminal") || procName.Contains("wt") ||
                        procName.Contains("alacritty") || procName.Contains("wezterm") || procName.Contains("mintty")) {
                        return true;
                    }
                }
            } catch {}
            return false;
        }

        private static bool IsLegacyConsole(IntPtr hWnd) {
            if (hWnd == IntPtr.Zero) return false;
            try {
                var sb = new StringBuilder(256);
                GetClassName(hWnd, sb, sb.Capacity);
                string className = sb.ToString();

                if (className.Contains("ConsoleWindowClass", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }

                GetWindowThreadProcessId(hWnd, out uint processId);
                if (processId > 0) {
                    using var proc = Process.GetProcessById((int)processId);
                    string procName = proc.ProcessName.ToLowerInvariant();
                    if (procName.Contains("cmd") || procName.Contains("powershell") || procName.Contains("pwsh") || procName.Contains("conhost")) {
                        return true;
                    }
                }
            } catch {}
            return false;
        }
        #endregion

        private void BtnApply_Click(object sender, RoutedEventArgs e) {
            _ = ApplyAndPasteAsync();
        }

        private void BtnCopyOnly_Click(object sender, RoutedEventArgs e) {
            string text = !string.IsNullOrWhiteSpace(TxtTranslation.Text) ? TxtTranslation.Text : TxtInput.Text;
            if (!string.IsNullOrWhiteSpace(text)) {
                try {
                    Clipboard.SetText(text);
                    TxtStatus.Text = "تم النسخ ✓";
                } catch {}
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                this.Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.DragMove();
            }
        }
    }
}
