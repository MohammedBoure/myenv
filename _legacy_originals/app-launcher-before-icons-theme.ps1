param([switch]$ListOnly)

Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase

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
$window.Height = 500
$window.WindowStartupLocation = [System.Windows.WindowStartupLocation]::CenterScreen
$window.WindowStyle = [System.Windows.WindowStyle]::None
$window.ResizeMode = [System.Windows.ResizeMode]::NoResize
$window.Topmost = $true
$window.ShowInTaskbar = $false
$window.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(15,23,42))

$panel = New-Object System.Windows.Controls.StackPanel
$panel.Margin = New-Object System.Windows.Thickness(22)

$title = New-Object System.Windows.Controls.TextBlock
$title.Text = 'Open application'
$title.FontSize = 18
$title.FontWeight = [System.Windows.FontWeights]::SemiBold
$title.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
$title.Margin = New-Object System.Windows.Thickness(0,0,0,12)
[void]$panel.Children.Add($title)

$search = New-Object System.Windows.Controls.TextBox
$search.Height = 42
$search.FontSize = 18
$search.Padding = New-Object System.Windows.Thickness(12,7,12,7)
$search.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(30,41,59))
$search.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
$search.BorderBrush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(56,189,248))
$search.ToolTip = 'Type an application name'
[void]$panel.Children.Add($search)

$list = New-Object System.Windows.Controls.ListBox
$list.Margin = New-Object System.Windows.Thickness(0,14,0,0)
$list.FontSize = 16
$list.Background = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(15,23,42))
$list.Foreground = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
$list.BorderThickness = New-Object System.Windows.Thickness(0)
[void]$panel.Children.Add($list)

$window.Content = $panel

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
        $item.Content = $app.Name
        $item.Tag = $app
        $item.Padding = New-Object System.Windows.Thickness(10,8,10,8)
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

[void]$window.ShowDialog()