<#
.SYNOPSIS
    Master Environment Setup Script for MyEnv (GlazeWM + YASB + PowerShell + CMD).
.DESCRIPTION
    Applies all system tweaks, junctions, profile bindings, taskbar auto-hide,
    CMD auto-completion & macros, and restarts GlazeWM and YASB.
#>

$ErrorActionPreference = "Stop"
$myenvPath = Split-Path -Parent $PSScriptRoot
$userProfile = $env:USERPROFILE

Write-Host "==========================================" -ForegroundColor Magenta
Write-Host "   MyEnv Master Environment Setup Script   " -ForegroundColor Magenta
Write-Host "==========================================" -ForegroundColor Magenta

# 1. Enable Taskbar Auto-Hide
Write-Host "`n[1/9] Applying Taskbar Auto-Hide..." -ForegroundColor Cyan
& "$myenvPath\scripts\set-taskbar-autohide.ps1"

# 2. Apply PowerShell Ctrl+Backspace
Write-Host "`n[2/9] Configuring Ctrl+Backspace word deletion..." -ForegroundColor Cyan
& "$myenvPath\scripts\set-ctrl-backspace.ps1"

# 3. Disable Alt+Shift Language Switching
Write-Host "`n[3/9] Disabling Alt+Shift language switching (Win+Space only)..." -ForegroundColor Cyan
& "$myenvPath\scripts\disable-alt-shift-lang.ps1"

# 4. Configure CMD Auto-Completion & Doskey Macros
Write-Host "`n[4/9] Configuring CMD Auto-Completion & Macros..." -ForegroundColor Cyan
& "$myenvPath\scripts\set-cmd-autocompletion.ps1"

# 5. Restore Winget Packages from Manifest
Write-Host "`n[5/9] Restoring Winget Packages..." -ForegroundColor Cyan
$installPackagesScript = "$myenvPath\scripts\install-packages.ps1"
if (Test-Path $installPackagesScript) {
    & $installPackagesScript
}

# 6. Create Junctions
Write-Host "`n[6/9] Verifying Directory Junctions..." -ForegroundColor Cyan
$junctions = @(
    @{ Source = "$userProfile\.config\yasb"; Target = "$myenvPath\yasb" },
    @{ Source = "$userProfile\.config\tacky-borders"; Target = "$myenvPath\tacky-borders" },
    @{ Source = "$userProfile\.glzr\glazewm"; Target = "$myenvPath\glazewm" },
    @{ Source = "$userProfile\.glzr\zebar"; Target = "$myenvPath\zebar" },
    @{ Source = "$userProfile\.gemini\config"; Target = "$myenvPath\gemini" }
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

# 7. Manage Services (Stop Zebar & Reload YASB)
Write-Host "`n[7/9] Managing Bar Services..." -ForegroundColor Cyan
Get-Process -Name zebar -ErrorAction SilentlyContinue | Stop-Process -Force
$zebarLink = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\Zebar.lnk"
if (Test-Path $zebarLink) { Remove-Item $zebarLink -Force }

if (Get-Command "yasbc.exe" -ErrorAction SilentlyContinue) {
    & "yasbc.exe" reload
    Write-Host "Reloaded YASB." -ForegroundColor Green
}

# 8. Configure Windows 10 Active Window Borders (FocusedBorder & DWM)
Write-Host "`n[8/10] Configuring Active Window Borders..." -ForegroundColor Cyan
& "$myenvPath\scripts\set-windows10-border.ps1"

# Terminate any legacy WPF PowerShell border scripts
Get-CimInstance Win32_Process -Filter "CommandLine LIKE '%focused-window-border.ps1%'" -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
}

# Build & Start FocusedBorder native service
$borderExe = "$myenvPath\scripts\focused-border\FocusedBorder.exe"
$borderProj = "$myenvPath\tools\focused-border\FocusedBorder.csproj"
if ((-not (Test-Path $borderExe)) -and (Test-Path $borderProj)) {
    if (Get-Command dotnet -ErrorAction SilentlyContinue) {
        Write-Host "Publishing FocusedBorder native service..." -ForegroundColor Yellow
        dotnet publish "$borderProj" -c Release -o "$myenvPath\scripts\focused-border" --nologo -v q
    }
}

if (Test-Path $borderExe) {
    if (-not (Get-Process -Name "FocusedBorder" -ErrorAction SilentlyContinue)) {
        Start-Process -FilePath $borderExe -WindowStyle Hidden
        Write-Host "Started FocusedBorder service." -ForegroundColor Green
    } else {
        Write-Host "FocusedBorder is already running." -ForegroundColor Green
    }
}

# 9. Configure Arabic Terminal & Windows Terminal Support
Write-Host "`n[9/10] Configuring Arabic & Windows Terminal Support..." -ForegroundColor Cyan
& "$myenvPath\scripts\setup-arabic-terminal.ps1"

# 10. Build & Register NightPad (Professional Night Mode Text Editor)
Write-Host "`n[10/10] Building & Registering NightPad Text Editor..." -ForegroundColor Cyan
$nightpadExe = "$myenvPath\scripts\nightpad\NightPad.exe"
if (-not (Test-Path $nightpadExe)) {
    if (Get-Command dotnet -ErrorAction SilentlyContinue) {
        Write-Host "Publishing NightPad..." -ForegroundColor Yellow
        dotnet publish "$myenvPath\tools\nightpad\NightPad.csproj" -c Release -o "$myenvPath\scripts\nightpad" --nologo -v q
    }
}
$wsh = New-Object -ComObject WScript.Shell
$scPath = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\NightPad.lnk"
if (-not (Test-Path $scPath) -and (Test-Path $nightpadExe)) {
    $sc = $wsh.CreateShortcut($scPath)
    $sc.TargetPath = $nightpadExe
    $sc.Description = "NightPad - Professional Night Mode Text Editor"
    $sc.WorkingDirectory = $myenvPath
    $sc.Save()
    Write-Host "Created NightPad shortcut." -ForegroundColor Green
}

# 11. Build & Start BarTranslator (Top Bar Real-Time Selection Translator)
Write-Host "`n[11/11] Building & Starting BarTranslator Service..." -ForegroundColor Cyan
$translatorExe = "$myenvPath\scripts\bar-translator\BarTranslator.exe"
$translatorProj = "$myenvPath\tools\bar-translator\BarTranslator.csproj"
if ((-not (Test-Path $translatorExe)) -and (Test-Path $translatorProj)) {
    if (Get-Command dotnet -ErrorAction SilentlyContinue) {
        Write-Host "Publishing BarTranslator..." -ForegroundColor Yellow
        dotnet publish "$translatorProj" -c Release -o "$myenvPath\scripts\bar-translator" --nologo -v q
    }
}
$getStateExe = "$myenvPath\scripts\bar-translator\get-state-reader.exe"
$getStateCs = "$myenvPath\scripts\bar-translator\GetState.cs"
if ((-not (Test-Path $getStateExe)) -and (Test-Path $getStateCs)) {
    & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /optimize /target:exe /out:"$getStateExe" "$getStateCs"
}
$actionExe = "$myenvPath\scripts\bar-translator\translator-action.exe"
$actionCs = "$myenvPath\scripts\bar-translator\Actions.cs"
if ((-not (Test-Path $actionExe)) -and (Test-Path $actionCs)) {
    & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /optimize /target:winexe /out:"$actionExe" "$actionCs"
}
if (Test-Path $translatorExe) {
    if (-not (Get-Process -Name "BarTranslator" -ErrorAction SilentlyContinue)) {
        Start-Process -FilePath $translatorExe -WindowStyle Hidden
        Write-Host "Started BarTranslator daemon." -ForegroundColor Green
    } else {
        Write-Host "BarTranslator is already running." -ForegroundColor Green
    }
}

Write-Host "`nSetup completed successfully!" -ForegroundColor Green

