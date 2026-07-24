<#
.SYNOPSIS
    Master Environment Setup Script for MyEnv (GlazeWM + YASB + PowerShell + CMD).
.DESCRIPTION
    Applies all system tweaks, junctions, profile bindings, taskbar auto-hide,
    CMD auto-completion & macros, and restarts GlazeWM and YASB.
#>

$ErrorActionPreference = "Stop"
$myenvPath = "C:\Users\moham\Documents\myenv"

Write-Host "==========================================" -ForegroundColor Magenta
Write-Host "   MyEnv Master Environment Setup Script   " -ForegroundColor Magenta
Write-Host "==========================================" -ForegroundColor Magenta

# 1. Enable Taskbar Auto-Hide
Write-Host "`n[1/6] Applying Taskbar Auto-Hide..." -ForegroundColor Cyan
& "$myenvPath\scripts\set-taskbar-autohide.ps1"

# 2. Apply PowerShell Ctrl+Backspace
Write-Host "`n[2/6] Configuring Ctrl+Backspace word deletion..." -ForegroundColor Cyan
& "$myenvPath\scripts\set-ctrl-backspace.ps1"

# 3. Disable Alt+Shift Language Switching
Write-Host "`n[3/6] Disabling Alt+Shift language switching (Win+Space only)..." -ForegroundColor Cyan
& "$myenvPath\scripts\disable-alt-shift-lang.ps1"

# 4. Configure CMD Auto-Completion & Doskey Macros
Write-Host "`n[4/6] Configuring CMD Auto-Completion & Macros..." -ForegroundColor Cyan
& "$myenvPath\scripts\set-cmd-autocompletion.ps1"

# 5. Create Junctions
Write-Host "`n[5/6] Verifying Directory Junctions..." -ForegroundColor Cyan
$junctions = @(
    @{ Source = "C:\Users\moham\.config\yasb"; Target = "$myenvPath\yasb" },
    @{ Source = "C:\Users\moham\.glzr\glazewm"; Target = "$myenvPath\glazewm" },
    @{ Source = "C:\Users\moham\.glzr\zebar"; Target = "$myenvPath\zebar" }
)

foreach ($j in $junctions) {
    $src = $j.Source
    $tgt = $j.Target
    if (-not (Test-Path $src)) {
        $parent = Split-Path $src
        if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
        cmd /c "mklink /J `"$src`" `"$tgt`""
        Write-Host "Created junction: $src -> $tgt" -ForegroundColor Green
    } else {
        Write-Host "Junction exists: $src" -ForegroundColor Yellow
    }
}

# 6. Manage Services (Stop Zebar & Reload YASB)
Write-Host "`n[6/6] Managing Bar Services..." -ForegroundColor Cyan
Get-Process -Name zebar -ErrorAction SilentlyContinue | Stop-Process -Force
$zebarLink = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\Zebar.lnk"
if (Test-Path $zebarLink) { Remove-Item $zebarLink -Force }

if (Get-Command "yasbc.exe" -ErrorAction SilentlyContinue) {
    & "yasbc.exe" reload
    Write-Host "Reloaded YASB." -ForegroundColor Green
}

Write-Host "`nSetup completed successfully!" -ForegroundColor Green
