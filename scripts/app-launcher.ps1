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

    # Build WPF Application Window
    $window = New-Object System.Windows.Window
    $window.Title = 'myenv-app-launcher'
    $window.Width = 640
    $window.Height = 460
    $window.WindowStartupLocation = [System.Windows.WindowStartupLocation]::CenterScreen
    $window.WindowStyle = [System.Windows.WindowStyle]::None
    $window.ResizeMode = [System.Windows.ResizeMode]::NoResize
    $window.Topmost = $true
    $window.ShowInTaskbar = $false
    $window.AllowsTransparency = $true
    $window.Background = [System.Windows.Media.Brushes]::Transparent

    # Root Container - Sharp Dark Theme
    $rootBorder = New-Object System.Windows.Controls.Border
    $rootBorder.CornerRadius = New-Object System.Windows.CornerRadius(0)
    $rootBorder.BorderThickness = New-Object System.Windows.Thickness(1)
    $rootBorder.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(55,55,55))
    $rootBorder.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(248,14,14,14))
    $rootBorder.Padding = New-Object System.Windows.Thickness(16)

    $layout = New-Object System.Windows.Controls.Grid
    
    # Define Grid Rows: 0=Header (Auto), 1=Search (Auto), 2=List (*)
    $r0 = New-Object System.Windows.Controls.RowDefinition
    $r0.Height = [System.Windows.GridLength]::Auto
    [void]$layout.RowDefinitions.Add($r0)

    $r1 = New-Object System.Windows.Controls.RowDefinition
    $r1.Height = [System.Windows.GridLength]::Auto
    [void]$layout.RowDefinitions.Add($r1)

    $r2 = New-Object System.Windows.Controls.RowDefinition
    $r2.Height = New-Object System.Windows.GridLength(1, [System.Windows.GridUnitType]::Star)
    [void]$layout.RowDefinitions.Add($r2)

    # 1. Header Section
    $header = New-Object System.Windows.Controls.Grid
    $c0 = New-Object System.Windows.Controls.ColumnDefinition
    $c0.Width = New-Object System.Windows.GridLength(1, [System.Windows.GridUnitType]::Star)
    $c1 = New-Object System.Windows.Controls.ColumnDefinition
    $c1.Width = [System.Windows.GridLength]::Auto
    [void]$header.ColumnDefinitions.Add($c0)
    [void]$header.ColumnDefinitions.Add($c1)

    $title = New-Object System.Windows.Controls.TextBlock
    $title.Text = 'APPLICATION LAUNCHER'
    $title.FontSize = 12
    $title.FontWeight = [System.Windows.FontWeights]::Bold
    $title.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(170,170,170))
    $title.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
    [void]$header.Children.Add($title)
    [System.Windows.Controls.Grid]::SetColumn($title, 0)

    $closeButton = New-Object System.Windows.Controls.Button
    $closeButton.Content = 'X'
    $closeButton.FontSize = 12
    $closeButton.FontWeight = [System.Windows.FontWeights]::Bold
    $closeButton.Width = 26
    $closeButton.Height = 24
    $closeButton.HorizontalAlignment = [System.Windows.HorizontalAlignment]::Right
    $closeButton.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(40,255,255,255))
    $closeButton.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(200,200,200))
    $closeButton.BorderThickness = New-Object System.Windows.Thickness(0)
    $closeButton.ToolTip = 'Close (Esc)'
    $closeButton.Add_Click({ $window.Close() })
    [void]$header.Children.Add($closeButton)
    [System.Windows.Controls.Grid]::SetColumn($closeButton, 1)

    [void]$layout.Children.Add($header)
    [System.Windows.Controls.Grid]::SetRow($header, 0)

    # 2. Search Input Box
    $search = New-Object System.Windows.Controls.TextBox
    $search.Height = 36
    $search.FontSize = 14
    $search.Padding = New-Object System.Windows.Thickness(10,6,10,6)
    $search.Margin = New-Object System.Windows.Thickness(0,10,0,10)
    $search.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(24,24,24))
    $search.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
    $search.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(60,60,60))
    $search.BorderThickness = New-Object System.Windows.Thickness(1)
    $search.CaretBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
    [void]$layout.Children.Add($search)
    [System.Windows.Controls.Grid]::SetRow($search, 1)

    # 3. Application List Container
    $list = New-Object System.Windows.Controls.ListBox
    $list.FontSize = 13
    $list.Background = [System.Windows.Media.Brushes]::Transparent
    $list.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(240,240,240))
    $list.BorderThickness = New-Object System.Windows.Thickness(0)
    $list.ScrollViewer.HorizontalScrollBarVisibility = [System.Windows.Controls.ScrollBarVisibility]::Disabled

    [void]$layout.Children.Add($list)
    [System.Windows.Controls.Grid]::SetRow($list, 2)

    $rootBorder.Child = $layout
    $window.Content = $rootBorder

    # Correct Icon Extraction Helper (FromEmptyOptions)
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
                    [System.Windows.Media.Imaging.BitmapSizeOptions]::FromEmptyOptions()
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

    # Filter & Populate App Items
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
            $item.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(160,22,22,22))
            $item.BorderThickness = New-Object System.Windows.Thickness(1)
            $item.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(60,50,50,50))
            $item.Tag = $app

            $row = New-Object System.Windows.Controls.StackPanel
            $row.Orientation = [System.Windows.Controls.Orientation]::Horizontal

            # Icon Display
            $iconSource = Get-AppIcon $app.Path
            if ($iconSource) {
                $img = New-Object System.Windows.Controls.Image
                $img.Width = 22
                $img.Height = 22
                $img.Margin = New-Object System.Windows.Thickness(0,0,10,0)
                $img.Source = $iconSource
                $img.VerticalAlignment = [System.Windows.VerticalAlignment]::Center
                [void]$row.Children.Add($img)
            } else {
                # Fallback spacing icon block if icon fails
                $dummy = New-Object System.Windows.Controls.Border
                $dummy.Width = 22
                $dummy.Height = 22
                $dummy.Margin = New-Object System.Windows.Thickness(0,0,10,0)
                [void]$row.Children.Add($dummy)
            }

            # App Title Label
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
                $sender.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(90,90,90))
            })
            $item.Add_MouseLeave({
                param($sender,$event)
                $sender.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(160,22,22,22))
                $sender.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromArgb(60,50,50,50))
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