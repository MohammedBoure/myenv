# myenv - Black & Transparent theme for classic Windows PowerShell console & Windows Terminal
$ErrorActionPreference = 'Stop'

function Convert-HexToConsoleColor([string]$Hex) {
    $rgb = [Convert]::ToInt32($Hex.TrimStart('#'), 16)
    $r = $rgb -band 0xFF
    $g = ($rgb -shr 8) -band 0xFF
    $b = ($rgb -shr 16) -band 0xFF
    return [int]($r -bor ($g -shl 8) -bor ($b -shl 16))
}

$palette = @{
    ColorTable00 = '#000000'  # True Deep Black Background
    ColorTable01 = '#FF5555'  # Red
    ColorTable02 = '#50FA7B'  # Green
    ColorTable03 = '#F1FA8C'  # Yellow
    ColorTable04 = '#8BE9FD'  # Cyan
    ColorTable05 = '#BD93F9'  # Purple/Magenta
    ColorTable06 = '#8BE9FD'  # Light Cyan
    ColorTable07 = '#F8F8F2'  # Bright White/Silver Foreground
    ColorTable08 = '#6272A4'  # Comment Gray
    ColorTable09 = '#FF6E6E'  # Bright Red
    ColorTable10 = '#69FF94'  # Bright Green
    ColorTable11 = '#FFFFA5'  # Bright Yellow
    ColorTable12 = '#D6ACFF'  # Bright Purple
    ColorTable13 = '#FF79C6'  # Bright Pink
    ColorTable14 = '#A4FFFF'  # Bright Cyan
    ColorTable15 = '#FFFFFF'  # Pure White
}

# 1. Configure Classic Windows Console Host (conhost.exe) Registry Settings
$keys = @(
    'HKCU:\Console',
    'HKCU:\Console\%SystemRoot%_System32_cmd.exe',
    'HKCU:\Console\%SystemRoot%_SysWOW64_cmd.exe',
    'HKCU:\Console\C:_Windows_System32_cmd.exe',
    'HKCU:\Console\C:_Windows_SysWOW64_cmd.exe',
    'HKCU:\Console\%SystemRoot%_System32_WindowsPowerShell_v1.0_powershell.exe',
    'HKCU:\Console\%SystemRoot%_SysWOW64_WindowsPowerShell_v1.0_powershell.exe',
    'HKCU:\Console\Windows PowerShell',
    'HKCU:\Console\C:_Windows_System32_WindowsPowerShell_v1.0_powershell.exe',
    'HKCU:\Console\C:_Windows_SysWOW64_WindowsPowerShell_v1.0_powershell.exe'
)

foreach ($key in $keys) {
    if (-not (Test-Path $key)) { New-Item -Path $key -Force | Out-Null }
    foreach ($entry in $palette.GetEnumerator()) {
        Set-ItemProperty -Path $key -Name $entry.Key -Type DWord -Value (Convert-HexToConsoleColor $entry.Value)
    }
    Set-ItemProperty -Path $key -Name ScreenColors -Type DWord -Value 7
    Set-ItemProperty -Path $key -Name PopupColors -Type DWord -Value 245
    Set-ItemProperty -Path $key -Name FaceName -Type String -Value 'Consolas'
    Set-ItemProperty -Path $key -Name FontFamily -Type DWord -Value 54
    Set-ItemProperty -Path $key -Name FontSize -Type DWord -Value 1048576
    Set-ItemProperty -Path $key -Name FontWeight -Type DWord -Value 400
    # WindowAlpha: 173 = 68% opacity / 32% transparency
    Set-ItemProperty -Path $key -Name WindowAlpha -Type DWord -Value 173
    Set-ItemProperty -Path $key -Name CursorColor -Type DWord -Value (Convert-HexToConsoleColor '#FFFFFF')
    Set-ItemProperty -Path $key -Name CursorType -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name CursorSize -Type DWord -Value 25
    Set-ItemProperty -Path $key -Name QuickEdit -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name InsertMode -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name InterceptCopyPaste -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name FilterOnPaste -Type DWord -Value 0
    Set-ItemProperty -Path $key -Name HistoryNoDup -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name NumberOfHistoryBuffers -Type DWord -Value 4
    Set-ItemProperty -Path $key -Name HistoryBufferSize -Type DWord -Value 50
    Set-ItemProperty -Path $key -Name VirtualTerminalLevel -Type DWord -Value 1
    Remove-ItemProperty -Path $key -Name CodePage -ErrorAction SilentlyContinue
}

# 2. Configure Windows 11 Windows Terminal (wt.exe) Settings & Transparency
$wtSettingsPaths = @(
    "$env:LOCALAPPDATA\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json",
    "$env:LOCALAPPDATA\Packages\Microsoft.WindowsTerminalPreview_8wekyb3d8bbwe\LocalState\settings.json",
    "$env:LOCALAPPDATA\Microsoft\Windows Terminal\settings.json"
)

foreach ($wtPath in $wtSettingsPaths) {
    if (Test-Path -LiteralPath $wtPath) {
        try {
            $rawJson = Get-Content -LiteralPath $wtPath -Raw -Encoding utf8
            $wtConfig = $rawJson | ConvertFrom-Json

            $midnightAuroraScheme = [PSCustomObject]@{
                name                = "Midnight Aurora"
                background          = "#000000"
                foreground          = "#F8F8F2"
                cursorColor         = "#FFFFFF"
                selectionBackground = "#44475A"
                black               = "#000000"
                red                 = "#FF5555"
                green               = "#50FA7B"
                yellow              = "#F1FA8C"
                blue                = "#8BE9FD"
                purple              = "#BD93F9"
                cyan                = "#8BE9FD"
                white               = "#F8F8F2"
                brightBlack         = "#6272A4"
                brightRed           = "#FF6E6E"
                brightGreen         = "#69FF94"
                brightYellow        = "#FFFFA5"
                brightBlue          = "#D6ACFF"
                brightPurple        = "#FF79C6"
                brightCyan          = "#A4FFFF"
                brightWhite         = "#FFFFFF"
            }

            # Schemes array
            $existingSchemes = if ($wtConfig.schemes) { @($wtConfig.schemes | Where-Object { $_.name -ne "Midnight Aurora" }) } else { @() }
            $existingSchemes += $midnightAuroraScheme
            $wtConfig.schemes = $existingSchemes

            # Defaults configuration
            if (-not $wtConfig.profiles) {
                $wtConfig | Add-Member -NotePropertyName profiles -NotePropertyValue ([PSCustomObject]@{}) -Force
            }
            if (-not $wtConfig.profiles.defaults) {
                $wtConfig.profiles | Add-Member -NotePropertyName defaults -NotePropertyValue ([PSCustomObject]@{}) -Force
            }

            $defaults = $wtConfig.profiles.defaults
            $defaults | Add-Member -NotePropertyName colorScheme -NotePropertyValue "Midnight Aurora" -Force
            $defaults | Add-Member -NotePropertyName opacity -NotePropertyValue 68 -Force
            $defaults | Add-Member -NotePropertyName useAcrylic -NotePropertyValue $false -Force
            $defaults | Add-Member -NotePropertyName padding -NotePropertyValue "8, 8, 8, 8" -Force

            if (-not $defaults.font) {
                $defaults | Add-Member -NotePropertyName font -NotePropertyValue ([PSCustomObject]@{}) -Force
            }
            $defaults.font | Add-Member -NotePropertyName face -NotePropertyValue "Consolas" -Force
            $defaults.font | Add-Member -NotePropertyName size -NotePropertyValue 11 -Force

            $updatedJson = $wtConfig | ConvertTo-Json -Depth 15
            if ($updatedJson -ne $rawJson) {
                Set-Content -LiteralPath $wtPath -Value $updatedJson -Encoding utf8
            }
        } catch {
            # Silently skip on parsing error during shell load
        }
    }
}
# Theme applied silently on profile load.