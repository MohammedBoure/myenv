# Focused Active Window White Border for Windows 10 & GlazeWM
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase, System.Windows.Forms

$code = @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public class WinAPI {
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_SHOWWINDOW = 0x0040;
}
"@

Add-Type -TypeDefinition $code

# Create WPF Border Overlay Window
$window = New-Object System.Windows.Window
$window.WindowStyle = [System.Windows.WindowStyle]::None
$window.AllowsTransparency = $true
$window.Background = [System.Windows.Media.Brushes]::Transparent
$window.Topmost = $true
$window.ShowInTaskbar = $false
$window.ResizeMode = [System.Windows.ResizeMode]::NoResize

$border = New-Object System.Windows.Controls.Border
$border.BorderBrush = [System.Windows.Media.Brushes]::White
$border.BorderThickness = New-Object System.Windows.Thickness(2.5)
$border.CornerRadius = New-Object System.Windows.CornerRadius(0)
$window.Content = $border

# Set Window ExStyle to WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE (Click-through)
$window.add_SourceInitialized({
    $helper = New-Object System.Windows.Interop.WindowInteropHelper($window)
    $hwnd = $helper.Handle
    $exStyle = [WinAPI]::GetWindowLong($hwnd, -20)
    # WS_EX_TRANSPARENT (0x20) + WS_EX_TOOLWINDOW (0x80) + WS_EX_NOACTIVATE (0x08000000) + WS_EX_LAYERED (0x80000)
    $newExStyle = $exStyle -bor 0x20 -bor 0x80 -bor 0x08000000 -bor 0x80000
    [WinAPI]::SetWindowLong($hwnd, -20, $newExStyle) | Out-Null
})

$window.Show()

$timer = New-Object System.Windows.Threading.DispatcherTimer
$timer.Interval = [TimeSpan]::FromMilliseconds(30)
$script:lastHwnd = [IntPtr]::Zero
$script:lastRect = ""

$timer.add_Tick({
    $hwnd = [WinAPI]::GetForegroundWindow()
    $ourHwnd = (New-Object System.Windows.Interop.WindowInteropHelper($window)).Handle

    if ($hwnd -eq [IntPtr]::Zero -or $hwnd -eq $ourHwnd) {
        return
    }

    # Ignore desktop and taskbars
    $className = New-Object System.Text.StringBuilder 256
    [WinAPI]::GetClassName($hwnd, $className, 256) | Out-Null
    $cls = $className.ToString()
    if ($cls -in @("Progman", "WorkerW", "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "Qt5152QWindowIcon")) {
        if ($window.Visibility -ne [System.Windows.Visibility]::Collapsed) {
            $window.Visibility = [System.Windows.Visibility]::Collapsed
        }
        return
    }

    if ([WinAPI]::IsIconic($hwnd)) {
        if ($window.Visibility -ne [System.Windows.Visibility]::Collapsed) {
            $window.Visibility = [System.Windows.Visibility]::Collapsed
        }
        return
    }

    $rect = New-Object WinAPI+RECT
    if ([WinAPI]::GetWindowRect($hwnd, [ref]$rect)) {
        $w = $rect.Right - $rect.Left
        $h = $rect.Bottom - $rect.Top

        if ($w -le 50 -or $h -le 50) {
            if ($window.Visibility -ne [System.Windows.Visibility]::Collapsed) {
                $window.Visibility = [System.Windows.Visibility]::Collapsed
            }
            return
        }

        $rectStr = "$($rect.Left),$($rect.Top),$w,$h"
        if ($hwnd -ne $script:lastHwnd -or $rectStr -ne $script:lastRect) {
            $script:lastHwnd = $hwnd
            $script:lastRect = $rectStr

            $window.Left = $rect.Left
            $window.Top = $rect.Top
            $window.Width = $w
            $window.Height = $h
            if ($window.Visibility -ne [System.Windows.Visibility]::Visible) {
                $window.Visibility = [System.Windows.Visibility]::Visible
            }
            [WinAPI]::SetWindowPos($ourHwnd, [WinAPI]::HWND_TOPMOST, 0, 0, 0, 0, 0x0001 -bor 0x0002 -bor 0x0010 -bor 0x0040) | Out-Null
        }
    }
})

$timer.Start()

$app = [System.Windows.Application]::Current
if ($null -eq $app) {
    $app = New-Object System.Windows.Application
}
$app.Run($window) | Out-Null
