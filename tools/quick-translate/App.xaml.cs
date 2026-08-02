using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Clipboard = System.Windows.Clipboard;

namespace QuickTranslate {
    public partial class App : Application {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_CONTROL = 0x11;
        private const byte VK_C = 0x43;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        protected override async void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            try {
                bool isClipboardMode = e.Args.Any(arg => 
                    arg.Equals("--clipboard", StringComparison.OrdinalIgnoreCase) || 
                    arg.Equals("-c", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--selected", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("clipboard", StringComparison.OrdinalIgnoreCase)
                );

                if (isClipboardMode) {
                    // Fast Selected Text / Clipboard Translation Mode
                    // 1. Simulate Ctrl+C to copy whatever text is currently highlighted in active window
                    await TriggerCopyAsync();

                    // 2. Read clipboard with retries
                    string clipboardText = GetClipboardTextWithRetry();

                    if (string.IsNullOrWhiteSpace(clipboardText)) {
                        MessageBox.Show("لم يتم العثور على أي نص محدد أو منسوخ في الحافظة.\nيرجى تحديد النص المطلوب ترجمته ثم تجربة الاختصار مجدداً.", 
                                        "ترجمة النص المحدد", 
                                        MessageBoxButton.OK, 
                                        MessageBoxImage.Information);
                        Shutdown();
                        return;
                    }

                    TranslationResult translationResult = await TranslationService.TranslateToArabicAsync(clipboardText.Trim());
                    var resultWindow = new ResultWindow(translationResult);
                    resultWindow.ShowDialog();
                } else {
                    // Screen Region Snipping Mode
                    var selectionWindow = new SelectionWindow();
                    bool? result = selectionWindow.ShowDialog();

                    if (result == true && selectionWindow.CapturedBitmap != null) {
                        using (Bitmap bmp = selectionWindow.CapturedBitmap) {
                            string recognizedText = await OcrService.RecognizeTextAsync(bmp);

                            if (string.IsNullOrWhiteSpace(recognizedText)) {
                                MessageBox.Show("لم يتم التعرف على أي نص في المنطقة المحددة.", 
                                                "ترجمة سريعة", 
                                                MessageBoxButton.OK, 
                                                MessageBoxImage.Information);
                                Shutdown();
                                return;
                            }

                            TranslationResult translationResult = await TranslationService.TranslateToArabicAsync(recognizedText);
                            var resultWindow = new ResultWindow(translationResult);
                            resultWindow.ShowDialog();
                        }
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show($"حدث خطأ أثناء التشغيل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            } finally {
                Shutdown();
            }
        }

        private static async Task TriggerCopyAsync() {
            // Give time for hotkey keys (Win/Shift/Alt/C) to be released by user
            await Task.Delay(120);

            // Simulate Ctrl+C
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            keybd_event(VK_C, 0, 0, UIntPtr.Zero);
            await Task.Delay(25);
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            // Wait for foreground application to write selected text into Clipboard
            await Task.Delay(150);
        }

        private static string GetClipboardTextWithRetry(int maxRetries = 6, int delayMs = 40) {
            for (int i = 0; i < maxRetries; i++) {
                try {
                    if (Clipboard.ContainsText()) {
                        string text = Clipboard.GetText();
                        if (!string.IsNullOrWhiteSpace(text)) {
                            return text;
                        }
                    }
                } catch {
                    // Transient lock on clipboard
                }
                Thread.Sleep(delayMs);
            }
            return string.Empty;
        }
    }
}
