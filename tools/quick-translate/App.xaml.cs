using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Clipboard = System.Windows.Clipboard;

namespace QuickTranslate {
    public partial class App : Application {
        protected override async void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            try {
                bool isClipboardMode = e.Args.Any(arg => 
                    arg.Equals("--clipboard", StringComparison.OrdinalIgnoreCase) || 
                    arg.Equals("-c", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("clipboard", StringComparison.OrdinalIgnoreCase)
                );

                if (isClipboardMode) {
                    // Fast Clipboard Translation Mode
                    string clipboardText = string.Empty;
                    try {
                        if (Clipboard.ContainsText()) {
                            clipboardText = Clipboard.GetText();
                        }
                    } catch {}

                    if (string.IsNullOrWhiteSpace(clipboardText)) {
                        MessageBox.Show("محفظة النسخ (Clipboard) فارغة، يرجى نسخ نص أولاً.", 
                                        "ترجمة الحافظة السريعة", 
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
    }
}
