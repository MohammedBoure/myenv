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
    ColorTable00 = '#0F172A'
    ColorTable01 = '#F87171'
    ColorTable02 = '#34D399'
    ColorTable03 = '#FBBF24'
    ColorTable04 = '#60A5FA'
    ColorTable05 = '#C084FC'
    ColorTable06 = '#22D3EE'
    ColorTable07 = '#E2E8F0'
    ColorTable08 = '#475569'
    ColorTable09 = '#FB7185'
    ColorTable10 = '#6EE7B7'
    ColorTable11 = '#FCD34D'
    ColorTable12 = '#7DD3FC'
    ColorTable13 = '#D8B4FE'
    ColorTable14 = '#67E8F9'
    ColorTable15 = '#F8FAFC'
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
    Set-ItemProperty -Path $key -Name WindowAlpha -Type DWord -Value 242
    Set-ItemProperty -Path $key -Name CursorColor -Type DWord -Value (Convert-HexToConsoleColor '#38BDF8')
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