using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Clipboard = System.Windows.Clipboard;

namespace QuickTranslate {
    public partial class ResultWindow : Window {
        private bool isInitializing = true;
        private DispatcherTimer? retranslateTimer;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_CONTROL = 0x11;
        private const byte VK_C = 0x43;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public ResultWindow() {
            InitializeComponent();
            isInitializing = false;
        }

        public ResultWindow(TranslationResult preloadedResult) : this() {
            PopulateData(preloadedResult);
        }

        public async Task StartClipboardAutoTranslateAsync() {
            isInitializing = true;
            TxtOriginal.Text = "جاري جلب النص المظلل...";
            TxtTranslation.Text = "جاري الترجمة...";
            TxtLangBadge.Text = "جاري الجلب...";

            // 1. Send Ctrl+C to copy active selection with minimal delay
            await TriggerCopyAsync();

            // 2. Fetch clipboard text
            string text = GetClipboardTextWithRetry();

            if (string.IsNullOrWhiteSpace(text)) {
                TxtOriginal.Text = "(لا يوجد نص محدد)";
                TxtTranslation.Text = "لم يتم العثور على أي نص محدد في الحافظة.\nيرجى تحديد نص ثم ضغط الاختصار مجدداً.";
                TxtLangBadge.Text = "تنبيه";
                isInitializing = false;
                return;
            }

            TxtOriginal.Text = text;
            await PerformTranslationForTextAsync(text);
        }

        public async Task StartOcrAndTranslateAsync(Bitmap bitmap) {
            isInitializing = true;
            TxtOriginal.Text = "جاري تحليل النص من المنطقة المحددة...";
            TxtTranslation.Text = "جاري الترجمة...";
            TxtLangBadge.Text = "OCR";

            string recognizedText = await OcrService.RecognizeTextAsync(bitmap);
            if (string.IsNullOrWhiteSpace(recognizedText)) {
                TxtOriginal.Text = "(لم يتم العثور على نص)";
                TxtTranslation.Text = "لم يتم التعرف على أي نصوص في المنطقة المحددة.";
                TxtLangBadge.Text = "تنبيه";
                isInitializing = false;
                return;
            }

            TxtOriginal.Text = recognizedText;
            await PerformTranslationForTextAsync(recognizedText);
        }

        private async Task PerformTranslationForTextAsync(string text) {
            isInitializing = true;
            TranslationResult result = await TranslationService.TranslateToArabicAsync(text);
            PopulateData(result);
            isInitializing = false;
        }

        private void PopulateData(TranslationResult result) {
            TxtOriginal.Text = result.OriginalText;
            string translation = result.IsSuccess ? result.TranslatedText : result.ErrorMessage;
            TxtTranslation.Text = translation;
            
            string sourceLang = string.IsNullOrEmpty(result.SourceLanguage) ? "AUTO" : result.SourceLanguage;
            TxtLangBadge.Text = $"{sourceLang} ➔ AR";

            if (result.IsSuccess && !string.IsNullOrEmpty(translation)) {
                CopyTranslationToClipboard(translation);
            }
        }

        private static async Task TriggerCopyAsync() {
            // Minimal release delay (20ms)
            await Task.Delay(20);

            // Simulate Ctrl+C
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            keybd_event(VK_C, 0, 0, UIntPtr.Zero);
            await Task.Delay(15);
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            // Wait 35ms for foreground application to process Ctrl+C
            await Task.Delay(35);
        }

        private static string GetClipboardTextWithRetry(int maxRetries = 6, int delayMs = 30) {
            for (int i = 0; i < maxRetries; i++) {
                try {
                    if (Clipboard.ContainsText()) {
                        string text = Clipboard.GetText();
                        if (!string.IsNullOrWhiteSpace(text)) {
                            return text;
                        }
                    }
                } catch {
                    // Clipboard temporarily locked
                }
                Thread.Sleep(delayMs);
            }
            return string.Empty;
        }

        private void TxtTranslation_TextChanged(object sender, TextChangedEventArgs e) {
            if (!isInitializing && TxtTranslation != null) {
                CopyTranslationToClipboard(TxtTranslation.Text);
            }
        }

        private void TxtOriginal_TextChanged(object sender, TextChangedEventArgs e) {
            if (isInitializing || TxtOriginal == null) return;

            if (retranslateTimer == null) {
                retranslateTimer = new DispatcherTimer {
                    Interval = TimeSpan.FromMilliseconds(450)
                };
                retranslateTimer.Tick += async (s, args) => {
                    retranslateTimer.Stop();
                    await PerformLiveRetranslationAsync();
                };
            }

            retranslateTimer.Stop();
            retranslateTimer.Start();
        }

        private async Task PerformLiveRetranslationAsync() {
            string text = TxtOriginal.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            TranslationResult result = await TranslationService.TranslateToArabicAsync(text);
            if (result.IsSuccess) {
                isInitializing = true;
                TxtTranslation.Text = result.TranslatedText;
                string sourceLang = string.IsNullOrEmpty(result.SourceLanguage) ? "AUTO" : result.SourceLanguage;
                TxtLangBadge.Text = $"{sourceLang} ➔ AR";
                isInitializing = false;

                CopyTranslationToClipboard(result.TranslatedText);
            }
        }

        private void CopyTranslationToClipboard(string text) {
            if (string.IsNullOrEmpty(text)) return;
            try {
                Clipboard.SetText(text);
            } catch {}
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

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
