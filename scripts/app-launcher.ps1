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

    # Create WPF Window
    $window = New-Object System.Windows.Window
    $window.Title = 'myenv-app-launcher'
    $window.Width = 680
    $window.Height = 480
    $window.WindowStartupLocation = [System.Windows.WindowStartupLocation]::CenterScreen
    $window.WindowStyle = [System.Windows.WindowStyle]::None
    $window.ResizeMode = [System.Windows.ResizeMode]::NoResize
    $window.Topmost = $true
    $window.ShowInTaskbar = $false
    $window.AllowsTransparency = $true
    $window.Background = [System.Windows.Media.Brushes]::Transparent

    # Root Border - Sharp Dark Classic Theme (0px CornerRadius)
    $rootBorder = New-Object System.Windows.Controls.Border
    $rootBorder.CornerRadius = New-Object System.Windows.CornerRadius(0)
    $rootBorder.BorderThickness = New-Object System.Windows.Thickness(1)
    $rootBorder.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(55,55,55))
    $rootBorder.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(246,14,14,14))
    $rootBorder.Padding = New-Object System.Windows.Thickness(16)

    $layout = New-Object System.Windows.Controls.Grid
    [void]$layout.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition))
    [void]$layout.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition))
    [void]$layout.RowDefinitions.Add((New-Object System.Windows.Controls.RowDefinition))

    # Header Row
    $header = New-Object System.Windows.Controls.Grid
    [void]$header.ColumnDefinitions.Add((New-Object System.Windows.Controls.ColumnDefinition))
    
    $closeButton = New-Object System.Windows.Controls.Button
    $closeButton.Content = '✕'
    $closeButton.FontSize = 14
    $closeButton.Width = 28
    $closeButton.Height = 28
    $closeButton.HorizontalAlignment = [System.Windows.HorizontalAlignment]::Right
    $closeButton.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(40,255,255,255))
    $closeButton.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
    $closeButton.BorderThickness = New-Object System.Windows.Thickness(0)
    $closeButton.ToolTip = 'Close (Esc)'
    $closeButton.Add_Click({ $window.Close() })
    [void]$header.Children.Add($closeButton)
    [System.Windows.Controls.Grid]::SetColumn($closeButton,0)

    $title = New-Object System.Windows.Controls.TextBlock
    $title.Text = 'APPLICATION LAUNCHER'
    $title.FontSize = 13
    $title.FontWeight = [System.Windows.FontWeights]::Bold
    $title.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(200,200,200))
    $title.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
    [void]$header.Children.Add($title)
    [System.Windows.Controls.Grid]::SetColumn($title,0)
    $title.Margin = New-Object System.Windows.Thickness(2,0,36,0)
    [void]$layout.Children.Add($header)
    [System.Windows.Controls.Grid]::SetRow($header,0)

    # Search Box - Sharp Rectangular Input
    $search = New-Object System.Windows.Controls.TextBox
    $search.Height = 38
    $search.FontSize = 15
    $search.Padding = New-Object System.Windows.Thickness(10,6,10,6)
    $search.Margin = New-Object System.Windows.Thickness(0,12,0,0)
    $search.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(26,26,26))
    $search.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
    $search.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(70,70,70))
    $search.BorderThickness = New-Object System.Windows.Thickness(1)
    [void]$layout.Children.Add($search)
    [System.Windows.Controls.Grid]::SetRow($search,1)

    # App List Box
    $list = New-Object System.Windows.Controls.ListBox
    $list.Margin = New-Object System.Windows.Thickness(0,12,0,0)
    $list.FontSize = 14
    $list.Background = [System.Windows.Media.Brushes]::Transparent
    $list.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(240,240,240))
    $list.BorderThickness = New-Object System.Windows.Thickness(0)
    [void]$layout.Children.Add($list)
    [System.Windows.Controls.Grid]::SetRow($list,2)

    $rootBorder.Child = $layout
    $window.Content = $rootBorder

    # Robust Icon Extraction Helper
    $iconCache = @{}
    function Get-AppIcon($path) {
        if ($iconCache.ContainsKey($path)) { return $iconCache[$path] }
        try {
            $icon = [System.Drawing.Icon]::ExtractAssociatedIcon($path)
            if (-not $icon) {
                $shell = New-Object -ComObject WScript.Shell
                $shortcut = $shell.CreateShortcut($path)
                if ($shortcut.TargetPath -and (Test-Path $shortcut.TargetPath)) {
                    $icon = [System.Drawing.Icon]::ExtractAssociatedIcon($shortcut.TargetPath)
                }
            }
            if ($icon) {
                $source = [System.Windows.Interop.Imaging]::CreateBitmapSourceFromHIcon(
                    $icon.Handle,
                    [System.Windows.Int32Rect]::Empty,
                    [System.Windows.Media.Imaging.BitmapSizeOptions]::FromEmpty()
                )
                $source.Freeze()
                $icon.Dispose()
                $iconCache[$path] = $source
                return $source
            }
        } catch {}
        $iconCache[$path] = $null
        return $null
    }

    # Filter & Render Apps List
    $refresh = {
        $query = $search.Text.Trim().ToLowerInvariant()
        $list.Items.Clear()
        $matches = if ([string]::IsNullOrWhiteSpace($query)) {
            $apps | Select-Object -First 35
        } else {
            $apps | Where-Object { $_.Name.ToLowerInvariant().Contains($query) } | Select-Object -First 35
        }

        foreach ($app in $matches) {
            $item = New-Object System.Windows.Controls.ListBoxItem
            $item.Padding = New-Object System.Windows.Thickness(8,6,8,6)
            $item.Margin = New-Object System.Windows.Thickness(0,1,0,1)
            $item.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(180,22,22,22))
            $item.BorderThickness = New-Object System.Windows.Thickness(1)
            $item.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(80,60,60,60))
            $item.Tag = $app

            $row = New-Object System.Windows.Controls.StackPanel
            $row.Orientation = [System.Windows.Controls.Orientation]::Horizontal

            # Icon Element
            $iconSource = Get-AppIcon $app.Path
            if ($iconSource) {
                $img = New-Object System.Windows.Controls.Image
                $img.Width = 22
                $img.Height = 22
                $img.Margin = New-Object System.Windows.Thickness(0,0,10,0)
                $img.Source = $iconSource
                [void]$row.Children.Add($img)
            }

            # Label Element
            $label = New-Object System.Windows.Controls.TextBlock
            $label.Text = $app.Name
            $label.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
            $label.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(240,240,240))
            $label.FontWeight = [System.Windows.FontWeights]::Medium
            [void]$row.Children.Add($label)

            $item.Content = $row
            $item.Add_MouseEnter({
                param($sender,$event)
                $sender.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(45,45,45))
                $sender.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(100,100,100))
            })
            $item.Add_MouseLeave({
                param($sender,$event)
                $sender.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(180,22,22,22))
                $sender.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(80,60,60,60))
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
            if ($list.Items.Count -gt 0) {
                $list.Focus()
                $list.SelectedIndex = 0
            }
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
        } elseif ($event.Key -eq [System.Windows.Input.Key]::Up -and $list.SelectedIndex -eq 0) {
            $search.Focus()
            $event.Handled = $true
        }
    })

    $window.Add_ContentRendered({
        $search.Focus()
        & $refresh
    })

    $timer = New-Object System.Windows.Threading.DispatcherTimer
    $timer.Interval = [System.TimeSpan]::FromMilliseconds(100)
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