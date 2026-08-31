using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyEnv.FocusedBorder
{
    public static class Program
    {
        private static Mutex? _singleInstanceMutex;

        [STAThread]
        public static void Main(string[] args)
        {
            const string mutexName = "MyEnv_FocusedWindowBorder_SingleInstance_V2";
            _singleInstanceMutex = new Mutex(true, mutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                // Already running, exit gracefully
                return;
            }

            var app = new Application();
            app.Startup += (s, e) =>
            {
                var borderService = new BorderService();
                borderService.Start();
            };

            app.Run();

            GC.KeepAlive(_singleInstanceMutex);
        }
    }

    public class BorderService
    {
        #region Win32 API Declarations

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;

            public bool Equals(RECT other)
            {
                return Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
            }
        }

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

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

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_NOACTIVATE = 0x08000000;

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

        private readonly Window _window;
        private readonly Border _border;
        private IntPtr _overlayHwnd = IntPtr.Zero;
        private readonly DispatcherTimer _timer;
        private IntPtr _foregroundHook = IntPtr.Zero;
        private IntPtr _locationHook = IntPtr.Zero;
        private IntPtr _destroyHook = IntPtr.Zero;
        private readonly WinEventDelegate _winEventProc;

        private IntPtr _lastHwnd = IntPtr.Zero;
        private RECT _lastRect;
        private readonly StringBuilder _classBuffer = new StringBuilder(256);

        public BorderService()
        {
            _window = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                Topmost = true,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                IsHitTestVisible = false,
                Focusable = false,
                ShowActivated = false,
                Left = -10000,
                Top = -10000,
                Width = 100,
                Height = 100,
                Visibility = Visibility.Visible
            };

            _border = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2.5),
                CornerRadius = new CornerRadius(0),
                IsHitTestVisible = false,
                Background = Brushes.Transparent
            };

            _window.Content = _border;

            _window.SourceInitialized += OnSourceInitialized;

            _winEventProc = new WinEventDelegate(OnWinEvent);

            _timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(40)
            };
            _timer.Tick += (s, e) => UpdateBorder();
        }

        public void Start()
        {
            _window.Show();

            // Set up WinEvent hooks
            _foregroundHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
            _locationHook = SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
            _destroyHook = SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_HIDE, IntPtr.Zero, _winEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);

            _timer.Start();
            UpdateBorder();
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(_window);
            _overlayHwnd = helper.Handle;

            // Register WndProc message filter to guarantee 100% click-through
            var source = HwndSource.FromHwnd(_overlayHwnd);
            source?.AddHook(HwndSourceHook);

            // Apply WS_EX_TRANSPARENT, WS_EX_TOOLWINDOW, WS_EX_NOACTIVATE, WS_EX_LAYERED
            int exStyle = GetWindowLong(_overlayHwnd, GWL_EXSTYLE);
            int newExStyle = exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_LAYERED;
            SetWindowLong(_overlayHwnd, GWL_EXSTYLE, newExStyle);
        }

        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                handled = true;
                return new IntPtr(HTTRANSPARENT);
            }
            return IntPtr.Zero;
        }

        private void OnWinEvent(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            _window.Dispatcher.BeginInvoke((Action)UpdateBorder, DispatcherPriority.Render);
        }

        public void UpdateBorder()
        {
            IntPtr fgHwnd = GetForegroundWindow();

            if (fgHwnd == IntPtr.Zero || fgHwnd == _overlayHwnd || !IsWindow(fgHwnd) || !IsWindowVisible(fgHwnd) || IsIconic(fgHwnd))
            {
                HideBorder();
                return;
            }

            // Check if window is cloaked (e.g. inactive GlazeWM workspace or virtual desktop)
            int cloaked = 0;
            if (DwmGetWindowAttribute(fgHwnd, DWMWA_CLOAKED, out cloaked, sizeof(int)) == 0 && cloaked != 0)
            {
                HideBorder();
                return;
            }

            // Filter out desktop, taskbars, and launchers
            _classBuffer.Length = 0;
            GetClassName(fgHwnd, _classBuffer, 256);
            string cls = _classBuffer.ToString();

            if (cls == "Progman" ||
                cls == "WorkerW" ||
                cls == "Shell_TrayWnd" ||
                cls == "Shell_SecondaryTrayWnd" ||
                cls == "Qt5152QWindowIcon" ||
                cls == "Windows.UI.Core.CoreWindow" ||
                cls.IndexOf("app-launcher", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                HideBorder();
                return;
            }

            // Obtain true visible bounds
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

            if (w <= 60 || h <= 60 || targetRect.Left < -10000 || targetRect.Top < -10000)
            {
                HideBorder();
                return;
            }

            if (_window.Visibility == Visibility.Visible && fgHwnd == _lastHwnd && targetRect.Equals(_lastRect))
            {
                return;
            }

            _lastHwnd = fgHwnd;
            _lastRect = targetRect;

            // Reposition WPF window and maintain HWND_TOPMOST without stealing focus
            _window.Left = targetRect.Left;
            _window.Top = targetRect.Top;
            _window.Width = w;
            _window.Height = h;

            if (_window.Visibility != Visibility.Visible)
            {
                _window.Visibility = Visibility.Visible;
            }

            if (_overlayHwnd != IntPtr.Zero)
            {
                SetWindowPos(_overlayHwnd, HWND_TOPMOST, targetRect.Left, targetRect.Top, w, h, SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
        }

        private void HideBorder()
        {
            if (_window.Visibility != Visibility.Collapsed)
            {
                _window.Visibility = Visibility.Collapsed;
                _lastHwnd = IntPtr.Zero;
            }
        }
    }
}
