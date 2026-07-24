param([switch]$ListOnly)

Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase, System.Drawing, System.Windows.Forms

$mutexName = 'myenv-app-launcher-mutex'
$eventName = 'myenv-app-launcher-close'
$created = $false
$closeEvent = New-Object System.Threading.EventWaitHandle($false, [System.Threading.EventResetMode]::ManualReset, $eventName, [ref]$created)
$mutex = New-Object System.Threading.Mutex($false, $mutexName)
$closeFile = Join-Path $env:TEMP 'myenv-app-launcher.close'

if (-not $mutex.WaitOne(0)) {
    try { New-Item -ItemType File -Force -Path $closeFile | Out-Null } catch {}
    exit 0
}

try {
    $roots = @(
        (Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'),
        (Join-Path $env:ProgramData 'Microsoft\Windows\Start Menu\Programs')
    ) | Where-Object { Test-Path $_ }

    $apps = @(
        foreach ($root in $roots) {
            Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue |
                Where-Object { $_.Extension -in '.lnk','.url' } |
                ForEach-Object {
                    [pscustomobject]@{
                        Name = $_.BaseName
                        Path = $_.FullName
                    }
                }
        }
    ) | Sort-Object Name,Path -Unique

    if ($ListOnly) {
        $apps | Select-Object -First 20 Name,Path
        exit 0
    }

    $window = New-Object System.Windows.Window
    $window.Title = 'myenv-app-launcher'
    $window.Width = 720
    $window.Height = 520
    $window.WindowStartupLocation = [System.Windows.WindowStartupLocation]::CenterScreen
    $window.WindowStyle = [System.Windows.WindowStyle]::None
    $window.ResizeMode = [System.Windows.ResizeMode]::NoResize
    $window.Topmost = $true
    $window.ShowInTaskbar = $false
    $window.AllowsTransparency = $true
    $window.Background = [System.Windows.Media.Brushes]::Transparent

    $rootBorder = New-Object System.Windows.Controls.Border
    $rootBorder.CornerRadius = New-Object System.Windows.CornerRadius(14)
    $rootBorder.BorderThickness = New-Object System.Windows.Thickness(1)
    $rootBorder.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(210,56,189,248))
    $rootBorder.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(238,15,23,42))
    $rootBorder.Padding = New-Object System.Windows.Thickness(18)

    $layout = New-Object System.Windows.Controls.Grid
    [void]$layout.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition))
    [void]$layout.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition))
    [void]$layout.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition))

    $header = New-Object System.Windows.Controls.Grid
    [void]$header.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition))
    $closeButton = New-Object System.Windows.Controls.Button
    $closeButton.Content = '×'
    $closeButton.FontSize = 22
    $closeButton.Width = 34
    $closeButton.Height = 30
    $closeButton.HorizontalAlignment = [System.Windows.HorizontalAlignment]::Right
    $closeButton.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(35,125,211,252))
    $closeButton.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
    $closeButton.BorderThickness = New-Object System.Windows.Thickness(0)
    $closeButton.ToolTip = 'Close (Esc)'
    $closeButton.Add_Click({ $window.Close() })
    [void]$header.Children.Add($closeButton)
    [System.Windows.Controls.Grid]::SetColumn($closeButton,0)

    $title = New-Object System.Windows.Controls.TextBlock
    $title.Text = 'Open application'
    $title.FontSize = 18
    $title.FontWeight = [System.Windows.FontWeights]::SemiBold
    $title.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(226,232,240))
    $title.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
    [void]$header.Children.Add($title)
    [System.Windows.Controls.Grid]::SetColumn($title,0)
    $title.Margin = New-Object System.Windows.Thickness(4,0,48,0)
    [void]$layout.Children.Add($header)
    [System.Windows.Controls.Grid]::SetRow($header,0)

    $search = New-Object System.Windows.Controls.TextBox
    $search.Height = 42
    $search.FontSize = 18
    $search.Padding = New-Object System.Windows.Thickness(12,7,12,7)
    $search.Margin = New-Object System.Windows.Thickness(0,14,0,0)
    $search.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(170,30,41,59))
    $search.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
    $search.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(56,189,248))
    $search.BorderThickness = New-Object System.Windows.Thickness(1)
    $search.ToolTip = 'Type an application name'
    [void]$layout.Children.Add($search)
    [System.Windows.Controls.Grid]::SetRow($search,1)

    $list = New-Object System.Windows.Controls.ListBox
    $list.Margin = New-Object System.Windows.Thickness(0,14,0,0)
    $list.FontSize = 16
    $list.Background = [System.Windows.Media.Brushes]::Transparent
    $list.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(226,232,240))
    $list.BorderThickness = New-Object System.Windows.Thickness(0)
    [void]$layout.Children.Add($list)
    [System.Windows.Controls.Grid]::SetRow($list,2)

    $rootBorder.Child = $layout
    $window.Content = $rootBorder

    $iconCache = @{}
    function Get-AppIcon($path) {
        if ($iconCache.ContainsKey($path)) { return $iconCache[$path] }
        try {
            $shell = New-Object -ComObject WScript.Shell
            $shortcut = $shell.CreateShortcut($path)
            $target = $shortcut.TargetPath
            if ($target -and (Test-Path $target) -and [System.IO.Path]::GetExtension($target) -ieq '.exe') {
                $icon = [System.Drawing.Icon]::ExtractAssociatedIcon($target)
                if ($icon) {
                    $source = [System.Windows.Interop.Imaging]::CreateBitmapSourceFromHIcon(
                        $icon.Handle,
                        (New-Object System.Windows.Int32Rect(0,0,$icon.Width,$icon.Height)),
                        [System.Windows.Media.Imaging.BitmapSizeOptions]::FromEmpty()
                    )
                    $source.Freeze()
                    $icon.Dispose()
                    $iconCache[$path] = $source
                    return $source
                }
            }
        } catch {}
        $iconCache[$path] = $null
        return $null
    }

    $refresh = {
        $query = $search.Text.Trim().ToLowerInvariant()
        $list.Items.Clear()
        $matches = if ([string]::IsNullOrWhiteSpace($query)) {
            $apps | Select-Object -First 40
        } else {
            $apps | Where-Object { $_.Name.ToLowerInvariant().Contains($query) } | Select-Object -First 40
        }
        foreach ($app in $matches) {
            $item = New-Object System.Windows.Controls.ListBoxItem
            $item.Padding = New-Object System.Windows.Thickness(10,7,10,7)
            $item.Background = [System.Windows.Media.Brushes]::Transparent
            $item.Tag = $app

            $row = New-Object System.Windows.Controls.StackPanel
            $row.Orientation = [System.Windows.Controls.Orientation]::Horizontal
            $icon = New-Object System.Windows.Controls.Image
            $icon.Width = 26
            $icon.Height = 26
            $icon.Margin = New-Object System.Windows.Thickness(0,0,12,0)
            $icon.Source = Get-AppIcon $app.Path
            [void]$row.Children.Add($icon)

            $label = New-Object System.Windows.Controls.TextBlock
            $label.Text = $app.Name
            $label.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
            $label.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(226,232,240))
            [void]$row.Children.Add($label)

            $item.Content = $row
            $item.Add_MouseEnter({
                param($sender,$event)
                $sender.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(45,125,211,252))
            })
            $item.Add_MouseLeave({
                param($sender,$event)
                $sender.Background = [System.Windows.Media.Brushes]::Transparent
            })
            [void]$list.Items.Add($item)
        }
        if ($list.Items.Count -gt 0) { $list.SelectedIndex = 0 }
    }

    $launch = {
        if ($list.SelectedItem) {
            $app = $list.SelectedItem.Tag
            $window.Close()
            Start-Process -FilePath $app.Path
        }
    }

    $search.Add_TextChanged($refresh)
    $search.Add_KeyDown({
        param($sender,$event)
        if ($event.Key -eq [System.Windows.Input.Key]::Enter) {
            & $launch
            $event.Handled = $true
        } elseif ($event.Key -eq [System.Windows.Input.Key]::Escape) {
            $window.Close()
            $event.Handled = $true
        } elseif ($event.Key -eq [System.Windows.Input.Key]::Down) {
            $list.Focus()
            $event.Handled = $true
        }
    })
    $list.Add_MouseDoubleClick({ & $launch })
    $list.Add_KeyDown({
        param($sender,$event)
        if ($event.Key -eq [System.Windows.Input.Key]::Enter) {
            & $launch
            $event.Handled = $true
        } elseif ($event.Key -eq [System.Windows.Input.Key]::Escape) {
            $window.Close()
            $event.Handled = $true
        }
    })
    $window.Add_ContentRendered({
        $search.Focus()
        & $refresh
    })

    $timer = New-Object System.Windows.Threading.DispatcherTimer
    $timer.Interval = New-Object System.TimeSpan(0,0,0,0,120)
    $timer.Add_Tick({
        if ($closeEvent.WaitOne(0) -or (Test-Path $closeFile)) {
            $window.Close()
            $timer.Stop()
        }
    })
    $timer.Start()

    [void]$window.ShowDialog()
    $timer.Stop()
}
finally {
    try { Remove-Item -LiteralPath $closeFile -Force -ErrorAction SilentlyContinue } catch {}
    try { $closeEvent.Dispose() } catch {}
    try { $mutex.ReleaseMutex() | Out-Null } catch {}
    try { $mutex.Dispose() } catch {}
}