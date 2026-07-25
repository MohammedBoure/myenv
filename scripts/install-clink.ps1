<#
.SYNOPSIS
    Installs and Configures Clink for CMD (Command Prompt).
.DESCRIPTION
    Checks if Clink is installed. If missing, installs Clink via winget.
    Ensures clink_x64.exe is added to User PATH and registered in CMD AutoRun.
#>

$ErrorActionPreference = 'Stop'
$myenvPath = "C:\Users\moham\Documents\myenv"
$clinkSrcSettings = "$myenvPath\clink\clink_settings"
$clinkTargetDir = "$env:LOCALAPPDATA\clink"
$clinkTargetSettings = "$clinkTargetDir\clink_settings"

Write-Host "Checking Clink installation..." -ForegroundColor Cyan

# 1. Locate clink executable
$clinkExe = Get-Command "clink" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
if (-not $clinkExe) {
    # Search WinGet packages or system folders for clink_x64.exe or clink.exe
    $found = Get-ChildItem -Path "$env:LOCALAPPDATA\Microsoft\WinGet\Packages", "$env:ProgramFiles", "${env:ProgramFiles(x86)}" -Filter "clink_x64.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $clinkExe = $found.FullName
    }
}

if (-not $clinkExe) {
    Write-Host "Clink is not installed. Installing via winget..." -ForegroundColor Yellow
    try {
        winget install --id chrisant996.Clink --exact --accept-source-agreements --accept-package-agreements --silent
        Write-Host "Clink package installed." -ForegroundColor Green
    } catch {
        Write-Host "Failed to install Clink via winget: $_" -ForegroundColor Red
    }

    # Re-search after winget install
    $found = Get-ChildItem -Path "$env:LOCALAPPDATA\Microsoft\WinGet\Packages", "$env:ProgramFiles", "${env:ProgramFiles(x86)}" -Filter "clink_x64.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $clinkExe = $found.FullName
    }
}

if ($clinkExe) {
    Write-Host "Found Clink executable at: $clinkExe" -ForegroundColor Green
    $clinkBinDir = Split-Path $clinkExe

    # Ensure Clink directory is in User PATH
    $userPath = [System.Environment]::GetEnvironmentVariable("Path", "User")
    if ($userPath -notlike "*$clinkBinDir*") {
        [System.Environment]::SetEnvironmentVariable("Path", "$userPath;$clinkBinDir", "User")
        Write-Host "Added $clinkBinDir to User PATH." -ForegroundColor Green
    }

    # Create clink.exe alias if only clink_x64.exe exists
    $clinkAlias = Join-Path $clinkBinDir "clink.exe"
    if (-not (Test-Path $clinkAlias) -and (Test-Path (Join-Path $clinkBinDir "clink_x64.exe"))) {
        Copy-Item -Path (Join-Path $clinkBinDir "clink_x64.exe") -Destination $clinkAlias -Force
    }

    # 2. Register Clink AutoRun
    Write-Host "Registering Clink AutoRun..." -ForegroundColor Cyan
    try {
        Start-Process -FilePath $clinkExe -ArgumentList "autorun install" -Wait -WindowStyle Hidden
        Write-Host "Clink AutoRun registered successfully." -ForegroundColor Green
    } catch {
        Write-Host "Clink autorun registration warning: $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "Warning: Could not locate Clink executable." -ForegroundColor Yellow
}

# 3. Apply Clink Settings from myenv\clink\clink_settings
if (Test-Path $clinkSrcSettings) {
    if (-not (Test-Path $clinkTargetDir)) {
        New-Item -ItemType Directory -Path $clinkTargetDir -Force | Out-Null
    }
    Copy-Item -Path $clinkSrcSettings -Destination $clinkTargetSettings -Force
    Write-Host "Applied Clink history auto-suggestions settings to $clinkTargetSettings." -ForegroundColor Green
}
