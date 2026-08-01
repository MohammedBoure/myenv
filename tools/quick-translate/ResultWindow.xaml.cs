using System;
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

        public ResultWindow(TranslationResult result) {
            InitializeComponent();
            PopulateData(result);
            isInitializing = false;
        }

        private void PopulateData(TranslationResult result) {
            TxtOriginal.Text = result.OriginalText;
            string translation = result.IsSuccess ? result.TranslatedText : result.ErrorMessage;
            TxtTranslation.Text = translation;
            
            string sourceLang = string.IsNullOrEmpty(result.SourceLanguage) ? "AUTO" : result.SourceLanguage;
            TxtLangBadge.Text = $"{sourceLang} ➔ AR";

            // Automatically copy translation to clipboard
            if (result.IsSuccess && !string.IsNullOrEmpty(translation)) {
                CopyTranslationToClipboard(translation);
            }
        }

        private void TxtTranslation_TextChanged(object sender, TextChangedEventArgs e) {
            if (!isInitializing && TxtTranslation != null) {
                // Keep clipboard updated in real-time as user edits the translated text
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
                isInitializing = true; // prevent re-trigger loop
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
