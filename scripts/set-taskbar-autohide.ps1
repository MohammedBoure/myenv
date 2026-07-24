<#
.SYNOPSIS
    Configures Windows Taskbar Auto-Hide (Primary and Multi-Monitor).
.DESCRIPTION
    Updates registry keys StuckRects3 and MMStuckRects3 to enable Taskbar Auto-Hide,
    then restarts Explorer to apply the changes immediately.
#>

param(
    [switch]$Disable
)

$autohideValue = if ($Disable) { 2 } else { 3 }
$statusText = if ($Disable) { "Disabled" } else { "Enabled" }

Write-Host "Setting Windows Taskbar Auto-Hide to: $statusText..." -ForegroundColor Cyan

# Primary Monitor
$path = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3'
if (Test-Path $path) {
    $val = (Get-ItemProperty -Path $path).Settings
    $val[8] = $autohideValue
    Set-ItemProperty -Path $path -Name 'Settings' -Value $val
    Write-Host "Updated primary StuckRects3 settings." -ForegroundColor Green
}

# Multi-Monitors
$mmPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\MMStuckRects3'
if (Test-Path $mmPath) {
    $item = Get-ItemProperty -Path $mmPath
    foreach ($p in $item.psobject.properties) {
        if ($p.Value -is [byte[]] -and $p.Value.Length -ge 9) {
            $b = $p.Value
            $b[8] = $autohideValue
            Set-ItemProperty -Path $mmPath -Name $p.Name -Value $b
            Write-Host "Updated MMStuckRects3 property '$($p.Name)'." -ForegroundColor Green
        }
    }
}

# Restart Explorer to apply changes
Write-Host "Restarting Windows Explorer..." -ForegroundColor Yellow
Stop-Process -Name explorer -Force
Write-Host "Windows Taskbar Auto-Hide has been $statusText." -ForegroundColor Green
