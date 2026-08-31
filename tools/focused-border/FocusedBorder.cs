using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MyEnv.FocusedBorder
{
    internal class Program
    {
        private static Mutex _singleInstanceMutex;

        [STAThread]
        private static void Main(string[] args)
        {
            const string mutexName = "MyEnv_FocusedWindowBorder_SingleInstance";
            bool isNewInstance;
            _singleInstanceMutex = new Mutex(true, mutexName, out isNewInstance);

            if (!isNewInstance)
            {
                // Already running, exit gracefully
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BorderOverlayForm());

            GC.KeepAlive(_singleInstanceMutex);
        }
    }

    internal class BorderOverlayForm : Form
    {
        #region Win32 API Declarations

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width
            {
                get { return Right - Left; }
            }

            public int Height
            {
                get { return Bottom - Top; }
            }

            public override bool Equals(object obj)
            {
                if (!(obj is RECT)) return false;
                RECT r = (RECT)obj;
                return Left == r.Left && Top == r.Top && Right == r.Right && Bottom == r.Bottom;
            }

            public override int GetHashCode()
            {
                return Left ^ Top ^ Right ^ Bottom;
            }
        }

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const int SW_HIDE = 0;
        private const int SW_SHOWNOACTIVATE = 4;

        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        private const int DWMWA_CLOAKED = 14;

        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        private const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;
        private const uint EVENT_OBJECT_DESTROY = 0x8001;
        private const uint EVENT_OBJECT_HIDE = 0x8003;
        private const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
        private const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;
        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        private const int WM_NCHITTEST = 0x0084;
        private const int HTTRANSPARENT = -1;

        #endregion

        private readonly Color _borderColor = Color.White;
        private readonly float _borderThickness = 2.5f;
        private readonly System.Windows.Forms.Timer _pollTimer;
        private IntPtr _foregroundHook = IntPtr.Zero;
        private IntPtr _locationHook = IntPtr.Zero;
        private IntPtr _destroyHook = IntPtr.Zero;
        private WinEventDelegate _winEventProc;

        private IntPtr _lastHwnd = IntPtr.Zero;
        private RECT _lastRect;
        private bool _isCurrentlyVisible = false;
        private readonly StringBuilder _classBuffer = new StringBuilder(256);

        public BorderOverlayForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(10, 10);
            this.Location = new Point(-10000, -10000);

            // Transparent color-key background
            Color transparentKey = Color.FromArgb(255, 1, 1, 1);
            this.BackColor = transparentKey;
            this.TransparencyKey = transparentKey;
            this.DoubleBuffered = true;

            // Initialize Event-driven win hooks
            _winEventProc = new WinEventDelegate(OnWinEvent);
            _foregroundHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
            _locationHook = SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
            _destroyHook = SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_HIDE, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);

            // Lightweight fallback timer for continuous window drags/resizes (60ms)
            _pollTimer = new System.Windows.Forms.Timer();
            _pollTimer.Interval = 60;
            _pollTimer.Tick += (s, e) => UpdateBorder();
            _pollTimer.Start();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW (does not show in Alt+Tab)
                cp.ExStyle |= 0x00080000; // WS_EX_LAYERED
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT (kernel-level click-through)
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE (never steals keyboard/mouse focus)
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Guarantee 100% click-through for all mouse events
            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
                return;
            }
            base.WndProc(ref m);
        }

        private void OnWinEvent(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (this.IsDisposed) return;
            try
            {
                this.BeginInvoke((Action)UpdateBorder);
            }
            catch { }
        }

        private void UpdateBorder()
        {
            if (this.IsDisposed) return;

            IntPtr fgHwnd = GetForegroundWindow();
            if (fgHwnd == IntPtr.Zero || fgHwnd == this.Handle || !IsWindow(fgHwnd) || !IsWindowVisible(fgHwnd) || IsIconic(fgHwnd))
            {
                HideBorder();
                return;
            }

            // Check if window is cloaked (e.g. GlazeWM inactive workspace)
            int cloaked = 0;
            if (DwmGetWindowAttribute(fgHwnd, DWMWA_CLOAKED, out cloaked, sizeof(int)) == 0 && cloaked != 0)
            {
                HideBorder();
                return;
            }

            // Filter out desktop, taskbar, YASB, and overlay classes
            _classBuffer.Length = 0;
            GetClassName(fgHwnd, _classBuffer, 256);
            string cls = _classBuffer.ToString();

            if (cls == "Progman" ||
                cls == "WorkerW" ||
                cls == "Shell_TrayWnd" ||
                cls == "Shell_SecondaryTrayWnd" ||
                cls == "Qt5152QWindowIcon" ||
                cls == "Windows.UI.Core.CoreWindow" ||
                cls.IndexOf("tacky-borders", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cls.IndexOf("app-launcher", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                HideBorder();
                return;
            }

            // Get exact visible frame bounds (DWM extended bounds exclude invisible drop shadow margins on Win10)
            RECT targetRect = new RECT();
            int hr = DwmGetWindowAttribute(fgHwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out targetRect, Marshal.SizeOf(typeof(RECT)));
            if (hr != 0 || targetRect.Width <= 0 || targetRect.Height <= 0)
            {
                if (!GetWindowRect(fgHwnd, out targetRect))
                {
                    HideBorder();
                    return;
                }
            }

            int w = targetRect.Width;
            int h = targetRect.Height;

            // Ignore tiny tooltips or hidden offscreen containers
            if (w <= 60 || h <= 60 || targetRect.Left < -10000 || targetRect.Top < -10000)
            {
                HideBorder();
                return;
            }

            // Avoid redundant repositioning/repaints if window hasn't moved
            if (_isCurrentlyVisible && fgHwnd == _lastHwnd && targetRect.Equals(_lastRect))
            {
                return;
            }

            _lastHwnd = fgHwnd;
            _lastRect = targetRect;

            // Position and resize the overlay precisely over the active window
            this.Location = new Point(targetRect.Left, targetRect.Top);
            this.Size = new Size(w, h);

            SetWindowPos(this.Handle, HWND_TOPMOST, targetRect.Left, targetRect.Top, w, h, SWP_NOACTIVATE | SWP_SHOWWINDOW);

            if (!_isCurrentlyVisible)
            {
                ShowWindow(this.Handle, SW_SHOWNOACTIVATE);
                _isCurrentlyVisible = true;
            }

            this.Invalidate();
        }

        private void HideBorder()
        {
            if (_isCurrentlyVisible)
            {
                _isCurrentlyVisible = false;
                _lastHwnd = IntPtr.Zero;
                ShowWindow(this.Handle, SW_HIDE);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_isCurrentlyVisible || this.Width <= 0 || this.Height <= 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.None;

            using (Pen pen = new Pen(_borderColor, _borderThickness))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_pollTimer != null)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
            }

            if (_foregroundHook != IntPtr.Zero) UnhookWinEvent(_foregroundHook);
            if (_locationHook != IntPtr.Zero) UnhookWinEvent(_locationHook);
            if (_destroyHook != IntPtr.Zero) UnhookWinEvent(_destroyHook);

            base.OnFormClosing(e);
        }
    }
}
