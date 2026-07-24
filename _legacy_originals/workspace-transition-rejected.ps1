param(
    [ValidateSet('next','prev')]
    [string]$Direction = 'next'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase, System.Windows.Forms

$cli = 'C:\Program Files\glzr.io\GlazeWM\cli\glazewm.exe'
$accent = [System.Windows.Media.Color]::FromRgb(56, 189, 248)
$surface = [System.Windows.Media.Color]::FromRgb(7, 15, 30)
$labelText = if ($Direction -eq 'next') { 'WORKSPACE  >' } else { '<  WORKSPACE' }

$overlayWindows = @()
foreach ($screen in [System.Windows.Forms.Screen]::AllScreens) {
    $window = New-Object System.Windows.Window
    $window.Title = 'myenv-workspace-transition'
    $window.WindowStyle = [System.Windows.WindowStyle]::None
    $window.ResizeMode = [System.Windows.ResizeMode]::NoResize
    $window.ShowInTaskbar = $false
    $window.Topmost = $true
    $window.AllowsTransparency = $false
    $window.Background = New-Object System.Windows.Media.SolidColorBrush($surface)
    $window.Left = $screen.Bounds.Left
    $window.Top = $screen.Bounds.Top
    $window.Width = $screen.Bounds.Width
    $window.Height = $screen.Bounds.Height

    $grid = New-Object System.Windows.Controls.Grid
    $grid.Background = New-Object System.Windows.Media.SolidColorBrush($surface)

    $stripe = New-Object System.Windows.Shapes.Rectangle
    $stripe.Width = 220
    $stripe.HorizontalAlignment = [System.Windows.HorizontalAlignment]::Left
    $stripe.VerticalAlignment = [System.Windows.VerticalAlignment]::Stretch
    $stripe.Fill = New-Object System.Windows.Media.SolidColorBrush($accent)
    $stripe.Opacity = 0.78
    $stripe.RenderTransform = New-Object System.Windows.Media.TranslateTransform
    [void]$grid.Children.Add($stripe)

    $text = New-Object System.Windows.Controls.TextBlock
    $text.Text = $labelText
    $text.Foreground = New-Object System.Windows.Media.SolidColorBrush($accent)
    $text.FontFamily = New-Object System.Windows.Media.FontFamily('Segoe UI Semibold')
    $text.FontSize = 24
    $text.Opacity = 0.95
    $text.HorizontalAlignment = [System.Windows.HorizontalAlignment]::Center
    $text.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
    [void]$grid.Children.Add($text)

    $window.Content = $grid
    $window.Show()
    $window.Activate()
    $overlayWindows += [pscustomobject]@{
        Window = $window
        Stripe = $stripe
        Width = $screen.Bounds.Width
    }
}

# Let the opaque cover paint before the workspace changes.
foreach ($item in $overlayWindows) {
    $item.Window.Dispatcher.Invoke([action]{})
}
Start-Sleep -Milliseconds 35

if ($Direction -eq 'next') {
    & $cli command focus --next-workspace | Out-Null
} else {
    & $cli command focus --prev-workspace | Out-Null
}

# Directional wipe: update the stripe in small frames while the cover hides the desktop.
$frames = 14
for ($frame = 0; $frame -le $frames; $frame++) {
    $progress = $frame / $frames
    foreach ($item in $overlayWindows) {
        $x = if ($Direction -eq 'next') {
            -$item.Width + (($item.Width + 220) * $progress)
        } else {
            $item.Width - (($item.Width + 220) * $progress)
        }
        $item.Stripe.RenderTransform.X = $x
        $item.Window.Dispatcher.Invoke([action]{})
    }
    Start-Sleep -Milliseconds 14
}

foreach ($item in $overlayWindows) {
    $item.Window.Close()
}