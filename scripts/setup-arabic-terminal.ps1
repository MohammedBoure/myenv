<#
.SYNOPSIS
    MyEnv - Native PowerShell & UTF-8 Setup Script
.DESCRIPTION
    Automates and configures UTF-8 65001 code page, Consolas console font,
    Midnight Aurora color palette, and compiles open-terminal-here.exe for native PowerShell/CMD launching.
#>

$ErrorActionPreference = "Stop"
$myenvPath = Split-Path -Parent $PSScriptRoot
$userProfile = $env:USERPROFILE

Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "  MyEnv - PowerShell & UTF-8 Terminal Configuration    " -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan

# 1. Compile Native Fast Terminal Launcher (OpenTerminalHere.cs)
Write-Host "`n[1/3] Compiling Fast Native Terminal Launcher (open-terminal-here.exe)..." -ForegroundColor Yellow
$cscPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $cscPath)) {
    $cscPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

$csSource = Join-Path $myenvPath "scripts\OpenTerminalHere.cs"
$exeTarget = Join-Path $myenvPath "scripts\open-terminal-here.exe"

if (Test-Path $cscPath) {
    & $cscPath /nologo /target:winexe /optimize+ /out:"$exeTarget" "$csSource"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[+] Compiled open-terminal-here.exe successfully." -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Compilation returned exit code $LASTEXITCODE." -ForegroundColor Red
    }
} else {
    Write-Host "[WARNING] csc.exe compiler not found at $cscPath" -ForegroundColor Red
}

# 2. Apply Console Registry Settings & Windows Terminal Theme
Write-Host "`n[2/3] Applying UTF-8, Consolas Font, and Transparency (Console & Windows Terminal)..." -ForegroundColor Yellow
$consoleThemeScript = Join-Path $myenvPath "powershell\console-theme.ps1"
if (Test-Path $consoleThemeScript) {
    & $consoleThemeScript
    Write-Host "[+] Applied Console Theme, Windows Terminal & UTF-8 registry settings." -ForegroundColor Green
}

# 3. Set User-Level Environment Variables for UTF-8
Write-Host "`n[3/3] Configuring User Environment Variables for UTF-8..." -ForegroundColor Yellow
$utf8EnvVars = @{
    "PYTHONIOENCODING" = "utf-8"
    "PYTHONUTF8"       = "1"
    "LESSCHARSET"      = "utf-8"
    "LANG"             = "en_US.UTF-8"
    "LC_ALL"           = "en_US.UTF-8"
}

foreach ($entry in $utf8EnvVars.GetEnumerator()) {
    [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, [EnvironmentVariableTarget]::User)
    [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, [EnvironmentVariableTarget]::Process)
}
Write-Host "[+] User environment variables (PYTHONIOENCODING, PYTHONUTF8, LESSCHARSET, LANG) configured." -ForegroundColor Green

Write-Host "`n======================================================" -ForegroundColor Green
Write-Host "  PowerShell & UTF-8 Environment Configured Successfully! " -ForegroundColor Green
Write-Host "======================================================" -ForegroundColor Green
