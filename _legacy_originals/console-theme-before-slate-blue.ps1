# myenv - Midnight Aurora theme for classic Windows PowerShell console
$ErrorActionPreference = 'Stop'

function Convert-HexToConsoleColor([string]$Hex) {
    $rgb = [Convert]::ToInt32($Hex.TrimStart('#'), 16)
    $r = $rgb -band 0xFF
    $g = ($rgb -shr 8) -band 0xFF
    $b = ($rgb -shr 16) -band 0xFF
    return [int]($r -bor ($g -shl 8) -bor ($b -shl 16))
}

$palette = @{
    ColorTable00 = '#000000'
    ColorTable01 = '#FF6B6B'
    ColorTable02 = '#50FA7B'
    ColorTable03 = '#F1FA8C'
    ColorTable04 = '#8BE9FD'
    ColorTable05 = '#BD93F9'
    ColorTable06 = '#8BE9FD'
    ColorTable07 = '#FFFFFF'
    ColorTable08 = '#666666'
    ColorTable09 = '#FF5555'
    ColorTable10 = '#69FF94'
    ColorTable11 = '#FFFFA5'
    ColorTable12 = '#A4FFFF'
    ColorTable13 = '#D6ACFF'
    ColorTable14 = '#A4FFFF'
    ColorTable15 = '#FFFFFF'
}

$keys = @(
    'HKCU:\Console',
    'HKCU:\Console\%SystemRoot%_System32_WindowsPowerShell_v1.0_powershell.exe',
    'HKCU:\Console\%SystemRoot%_SysWOW64_WindowsPowerShell_v1.0_powershell.exe'
)

foreach ($key in $keys) {
    if (-not (Test-Path $key)) { New-Item -Path $key -Force | Out-Null }
    foreach ($entry in $palette.GetEnumerator()) {
        Set-ItemProperty -Path $key -Name $entry.Key -Type DWord -Value (Convert-HexToConsoleColor $entry.Value)
    }
    Set-ItemProperty -Path $key -Name FaceName -Type String -Value 'Cascadia Mono'
    Set-ItemProperty -Path $key -Name FontFamily -Type DWord -Value 54
    Set-ItemProperty -Path $key -Name FontSize -Type DWord -Value 1048576
    Set-ItemProperty -Path $key -Name FontWeight -Type DWord -Value 400
    Set-ItemProperty -Path $key -Name WindowAlpha -Type DWord -Value 128
    Set-ItemProperty -Path $key -Name CursorColor -Type DWord -Value (Convert-HexToConsoleColor '#FFFFFF')
    Set-ItemProperty -Path $key -Name CursorType -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name CursorSize -Type DWord -Value 25
    Set-ItemProperty -Path $key -Name QuickEdit -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name InsertMode -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name HistoryNoDup -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name NumberOfHistoryBuffers -Type DWord -Value 4
    Set-ItemProperty -Path $key -Name HistoryBufferSize -Type DWord -Value 50
    Set-ItemProperty -Path $key -Name VirtualTerminalLevel -Type DWord -Value 1
    Set-ItemProperty -Path $key -Name WindowSize -Type DWord -Value 3932400
    Set-ItemProperty -Path $key -Name ScreenBufferSize -Type DWord -Value 6553840
}

Write-Output 'Midnight Aurora Console theme applied. Open a new PowerShell window.'