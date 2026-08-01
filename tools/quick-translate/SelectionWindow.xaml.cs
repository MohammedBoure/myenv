using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Forms = System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace QuickTranslate {
    public partial class SelectionWindow : Window {
        private bool isSelecting = false;
        private System.Windows.Point startPoint;
        public Bitmap? CapturedBitmap { get; private set; }

        public SelectionWindow() {
            InitializeComponent();
            SetupVirtualScreenBounds();
        }

        private void SetupVirtualScreenBounds() {
            var screenLeft = Forms.SystemInformation.VirtualScreen.Left;
            var screenTop = Forms.SystemInformation.VirtualScreen.Top;
            var screenWidth = Forms.SystemInformation.VirtualScreen.Width;
            var screenHeight = Forms.SystemInformation.VirtualScreen.Height;

            this.Left = screenLeft;
            this.Top = screenTop;
            this.Width = screenWidth;
            this.Height = screenHeight;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                isSelecting = true;
                startPoint = e.GetPosition(CanvasOverlay);
                
                Canvas.SetLeft(SelectionBox, startPoint.X);
                Canvas.SetTop(SelectionBox, startPoint.Y);
                SelectionBox.Width = 0;
                SelectionBox.Height = 0;
                SelectionBox.Visibility = Visibility.Visible;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e) {
            System.Windows.Point currentPoint = e.GetPosition(CanvasOverlay);

            // Move instruction tip near cursor
            Canvas.SetLeft(InstructionTip, Math.Min(currentPoint.X + 15, this.Width - 250));
            Canvas.SetTop(InstructionTip, Math.Min(currentPoint.Y + 15, this.Height - 50));

            if (isSelecting) {
                double left = Math.Min(startPoint.X, currentPoint.X);
                double top = Math.Min(startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - startPoint.X);
                double height = Math.Abs(currentPoint.Y - startPoint.Y);

                Canvas.SetLeft(SelectionBox, left);
                Canvas.SetTop(SelectionBox, top);
                SelectionBox.Width = width;
                SelectionBox.Height = height;
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e) {
            if (isSelecting) {
                isSelecting = false;
                System.Windows.Point endPoint = e.GetPosition(CanvasOverlay);

                double x = Math.Min(startPoint.X, endPoint.X) + this.Left;
                double y = Math.Min(startPoint.Y, endPoint.Y) + this.Top;
                double width = Math.Abs(endPoint.X - startPoint.X);
                double height = Math.Abs(endPoint.Y - startPoint.Y);

                // Use DPI scaling awareness or SystemForms Screen Capture
                if (width > 8 && height > 8) {
                    CaptureScreenRegion((int)x, (int)y, (int)width, (int)height);
                    this.DialogResult = true;
                } else {
                    this.DialogResult = false;
                }

                this.Close();
            }
        }

        private void CaptureScreenRegion(int x, int y, int width, int height) {
            try {
                Bitmap bmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bmp)) {
                    g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
                }
                CapturedBitmap = bmp;
            } catch {
                CapturedBitmap = null;
            }
        }
    }
}
