using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace QuickTranslate {
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            try {
                bool isClipboardMode = e.Args.Any(arg => 
                    arg.Equals("--clipboard", StringComparison.OrdinalIgnoreCase) || 
                    arg.Equals("-c", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--selected", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("clipboard", StringComparison.OrdinalIgnoreCase)
                );

                if (isClipboardMode) {
                    // Show ResultWindow INSTANTLY (< 15ms)
                    var resultWindow = new ResultWindow();
                    resultWindow.Show();
                    
                    // Run copy & translation asynchronously in background while window is visible
                    _ = resultWindow.StartClipboardAutoTranslateAsync();

                    resultWindow.Closed += (s, args) => Shutdown();
                } else {
                    // Screen Region Snipping Mode
                    var selectionWindow = new SelectionWindow();
                    bool? result = selectionWindow.ShowDialog();

                    if (result == true && selectionWindow.CapturedBitmap != null) {
                        Bitmap bmp = selectionWindow.CapturedBitmap;
                        var resultWindow = new ResultWindow();
                        resultWindow.Show();
                        
                        _ = resultWindow.StartOcrAndTranslateAsync(bmp);
                        resultWindow.Closed += (s, args) => {
                            bmp.Dispose();
                            Shutdown();
                        };
                    } else {
                        Shutdown();
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show($"حدث خطأ أثناء التشغيل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
