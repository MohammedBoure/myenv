<#
.SYNOPSIS
    Disables Alt+Shift language switching in Windows (forces Win+Space only).
.DESCRIPTION
    Updates registry keys under 'HKCU:\Keyboard Layout\Toggle' and
    'HKCU:\Control Panel\Input Method\Hot Keys' to disable Alt+Shift and Ctrl+Shift
    language/layout toggling.
#>

param(
    [switch]$Restore
)

$pathToggle = 'HKCU:\Keyboard Layout\Toggle'
if (-not (Test-Path $pathToggle)) {
    New-Item -Path $pathToggle -Force | Out-Null
}

if ($Restore) {
    Write-Host "Restoring default Alt+Shift language switching..." -ForegroundColor Cyan
    Set-ItemProperty -Path $pathToggle -Name 'Language Hotkey' -Value '1'
    Set-ItemProperty -Path $pathToggle -Name 'Hotkey' -Value '1'
    Set-ItemProperty -Path $pathToggle -Name 'Layout Hotkey' -Value '1'
    Write-Host "Alt+Shift language switching restored." -ForegroundColor Green
} else {
    Write-Host "Disabling Alt+Shift language switching (Win+Space only)..." -ForegroundColor Cyan
    Set-ItemProperty -Path $pathToggle -Name 'Language Hotkey' -Value '3'
    Set-ItemProperty -Path $pathToggle -Name 'Hotkey' -Value '3'
    Set-ItemProperty -Path $pathToggle -Name 'Layout Hotkey' -Value '3'

    # Clear Hot Keys under Input Method
    $hotkeysBase = 'HKCU:\Control Panel\Input Method\Hot Keys'
    $subkeys = @('00000100', '00000101', '00000102')
    foreach ($sub in $subkeys) {
        $p = "$hotkeysBase\$sub"
        if (-not (Test-Path $p)) { New-Item -Path $p -Force | Out-Null }
        Set-ItemProperty -Path $p -Name 'Key Modifiers' -Value ([byte[]](0x00,0x00,0x00,0x00))
        Set-ItemProperty -Path $p -Name 'Virtual Key' -Value ([byte[]](0x00,0x00,0x00,0x00))
    }

    Write-Host "Alt+Shift disabled successfully. Win+Space is now the primary language switcher." -ForegroundColor Green
}
