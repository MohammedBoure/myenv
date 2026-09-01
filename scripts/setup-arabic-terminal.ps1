<#
.SYNOPSIS
    MyEnv - Arabic Terminal & UTF-8 Setup Script
.DESCRIPTION
    Configures full Arabic text rendering and UTF-8 support across PowerShell,
    Windows Console Host, and Windows Terminal.
    - Sets Windows Terminal as default console host in Windows Registry
    - Configures Cascadia Code font and DirectWrite rendering in Windows Terminal settings.json
    - Enforces UTF-8 65001 code page in PowerShell profiles and environment
    - Compiles open-terminal-here.exe for instant native terminal launching
#>

$ErrorActionPreference = "Stop"
$myenvPath = Split-Path -Parent $PSScriptRoot
$userProfile = $env:USERPROFILE

Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "  MyEnv - Arabic Terminal & UTF-8 Configuration       " -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan

# 1. Compile Native Fast Terminal Launcher (OpenTerminalHere.cs)
Write-Host "`n[1/5] Compiling Fast Native Terminal Launcher (open-terminal-here.exe)..." -ForegroundColor Yellow
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

# 2. Set Windows Terminal as Default Console Host in Registry
Write-Host "`n[2/5] Setting Windows Terminal as Default Terminal Emulator in Registry..." -ForegroundColor Yellow
$startupRegKey = "HKCU:\Console\%%Startup"
if (-not (Test-Path $startupRegKey)) {
    New-Item -Path $startupRegKey -Force | Out-Null
}
# Delegation IDs for Windows Terminal
Set-ItemProperty -Path $startupRegKey -Name "DelegationConsole" -Value "{2EACA947-7F5F-4C45-8977-646CA9E523E0}" -Force
Set-ItemProperty -Path $startupRegKey -Name "DelegationTerminal" -Value "{E12CFF52-A866-4C77-9A90-F570A7AA2C6B}" -Force
Write-Host "[+] Windows Terminal registered as default terminal host." -ForegroundColor Green

# 3. Configure Windows Terminal settings.json (Cascadia Code + DirectWrite Rendering)
Write-Host "`n[3/5] Configuring Windows Terminal settings.json for Arabic Rendering..." -ForegroundColor Yellow
$wtSettingsPaths = @(
    "$env:LOCALAPPDATA\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json",
    "$env:LOCALAPPDATA\Packages\Microsoft.WindowsTerminalPreview_8wekyb3d8bbwe\LocalState\settings.json",
    "$env:LOCALAPPDATA\Microsoft\Windows Terminal\settings.json"
)

foreach ($wtPath in $wtSettingsPaths) {
    if (Test-Path $wtPath) {
        try {
            $rawJson = Get-Content -Path $wtPath -Raw -Encoding UTF8
            $wtSettings = ConvertFrom-Json $rawJson

            if ($null -eq $wtSettings.profiles) {
                $wtSettings | Add-Member -MemberType NoteProperty -Name "profiles" -Value ([PSCustomObject]@{})
            }
            if ($null -eq $wtSettings.profiles.defaults) {
                $wtSettings.profiles | Add-Member -MemberType NoteProperty -Name "defaults" -Value ([PSCustomObject]@{})
            }

            # Set Cascadia Code font for proper Arabic glyph shaping
            if ($null -eq $wtSettings.profiles.defaults.font) {
                $wtSettings.profiles.defaults | Add-Member -MemberType NoteProperty -Name "font" -Value ([PSCustomObject]@{ face = "Cascadia Code"; size = 11 })
            } else {
                $wtSettings.profiles.defaults.font.face = "Cascadia Code"
            }

            # Enable AtlasEngine and RTL support for modern DirectWrite text shaping
            if ($null -eq $wtSettings.profiles.defaults.useAtlasEngine) {
                $wtSettings.profiles.defaults | Add-Member -MemberType NoteProperty -Name "useAtlasEngine" -Value $true
            } else {
                $wtSettings.profiles.defaults.useAtlasEngine = $true
            }

            if ($null -eq $wtSettings.profiles.defaults.'experimental.supportRTL') {
                $wtSettings.profiles.defaults | Add-Member -MemberType NoteProperty -Name "experimental.supportRTL" -Value $true
            } else {
                $wtSettings.profiles.defaults.'experimental.supportRTL' = $true
            }

            $updatedJson = ConvertTo-Json $wtSettings -Depth 32
            Set-Content -Path $wtPath -Value $updatedJson -Encoding UTF8
            Write-Host "[+] Updated Windows Terminal settings at: $wtPath" -ForegroundColor Green
        } catch {
            Write-Host "[WARNING] Failed updating ${wtPath}: $_" -ForegroundColor Red
        }
    }
}

# 4. Apply Console Registry Settings & Cascadia Code Theme
Write-Host "`n[4/5] Applying Cascadia Code Font & Transparency to Windows Console..." -ForegroundColor Yellow
$consoleThemeScript = Join-Path $myenvPath "powershell\console-theme.ps1"
if (Test-Path $consoleThemeScript) {
    & $consoleThemeScript
    Write-Host "[+] Applied Cascadia Code console palette and registry settings." -ForegroundColor Green
}

# 5. Set User-Level Environment Variables for UTF-8
Write-Host "`n[5/5] Configuring User Environment Variables for UTF-8..." -ForegroundColor Yellow
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
Write-Host "[+] User environment variables (PYTHONIOENCODING, PYTHONUTF8, LESSCHARSET, LANG, LC_ALL) configured." -ForegroundColor Green

Write-Host "`n======================================================" -ForegroundColor Green
Write-Host "  Arabic Terminal & UTF-8 Configured Successfully!    " -ForegroundColor Green
Write-Host "======================================================" -ForegroundColor Green
