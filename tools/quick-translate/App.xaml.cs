using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace QuickTranslate {
    public partial class App : Application {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        protected override void OnStartup(StartupEventArgs e) {
            // 1. Capture active foreground window immediately before creating any WPF window
            IntPtr previousForegroundHwnd = GetForegroundWindow();

            base.OnStartup(e);

            try {
                bool isTypeMode = e.Args.Any(arg => 
                    arg.Equals("--type", StringComparison.OrdinalIgnoreCase) || 
                    arg.Equals("-t", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--prompt", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-p", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--input", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("type", StringComparison.OrdinalIgnoreCase)
                );

                bool isClipboardMode = e.Args.Any(arg => 
                    arg.Equals("--clipboard", StringComparison.OrdinalIgnoreCase) || 
                    arg.Equals("-c", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--selected", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("clipboard", StringComparison.OrdinalIgnoreCase)
                );

                if (isTypeMode) {
                    // Type & Paste Mode (Win+Shift+X)
                    var typeWindow = new TypeTranslateWindow(previousForegroundHwnd);
                    typeWindow.Show();
                    typeWindow.Closed += (s, args) => Shutdown();
                } else if (isClipboardMode) {
                    // Quick Clipboard Text Translation (Win+Shift+C)
                    ResultWindow.SendCopyKeys();

                    var resultWindow = new ResultWindow();
                    resultWindow.Show();
                    
                    _ = resultWindow.FetchClipboardAndTranslateAsync();
                    resultWindow.Closed += (s, args) => Shutdown();
                } else {
                    // Screen Region OCR & Snipping Mode (Win+Shift+Q)
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
