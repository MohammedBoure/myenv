<#
.SYNOPSIS
    Automated Installer and Manager for LensBridge Server.
.DESCRIPTION
    Verifies prerequisites (Git and Python 3.10+), manages the LensBridge repository
    in D:\git\LensBridge (clones or updates via git pull) without storing source code
    inside myenv, provisions an isolated Python virtual environment (venv) in
    D:\git\LensBridge\server\venv, installs requirements.txt, and invokes service_manager.ps1
    to configure background service execution.
.PARAMETER InstallPath
    Target directory for the LensBridge repository. Defaults to D:\git\LensBridge.
.PARAMETER RepoUrl
    Remote Git repository URL (defaults to https://github.com/MohammedBoure/LensBridge.git).
.PARAMETER ConfigureService
    Action to pass to service_manager.ps1 after installation (install, start, status, or skip). Defaults to install.
#>
[CmdletBinding()]
param(
    [string]$InstallPath = "D:\git\LensBridge",
    [string]$RepoUrl = "https://github.com/MohammedBoure/LensBridge.git",
    [ValidateSet("install", "start", "status", "skip")]
    [string]$ConfigureService = "install"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "           LensBridge Server Automated Installer            " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# 1. Verify Prerequisites: Git
Write-Host "`n[1/5] Verifying Git prerequisite..." -ForegroundColor Cyan
$gitCmd = Get-Command git.exe -ErrorAction SilentlyContinue
if (-not $gitCmd) {
    $gitCandidate = "C:\Program Files\Git\cmd\git.exe"
    if (Test-Path -LiteralPath $gitCandidate) {
        $gitCmd = $gitCandidate
    } else {
        Write-Error "Git executable was not found. Please install Git and ensure it is available in PATH."
        exit 1
    }
}
$gitVersionStr = (& git --version 2>&1)
Write-Host "  Found Git: $gitVersionStr" -ForegroundColor Green

# 2. Verify Prerequisites: Python 3.10+
Write-Host "`n[2/5] Verifying Python 3.10+ prerequisite..." -ForegroundColor Cyan
$pythonExe = $null
$candidatePythons = @(
    "$env:LOCALAPPDATA\Programs\Python\Python314\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python313\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python311\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python310\python.exe",
    "$env:ProgramFiles\Python314\python.exe",
    "$env:ProgramFiles\Python313\python.exe",
    "$env:ProgramFiles\Python312\python.exe",
    "$env:ProgramFiles\Python311\python.exe",
    "$env:ProgramFiles\Python310\python.exe"
)

foreach ($cand in $candidatePythons) {
    if (Test-Path -LiteralPath $cand) {
        $pythonExe = $cand
        break
    }
}

if (-not $pythonExe) {
    $cmdPython = Get-Command python.exe -ErrorAction SilentlyContinue
    if ($cmdPython) {
        $pythonExe = $cmdPython.Source
    }
}

if (-not $pythonExe -or -not (Test-Path -LiteralPath $pythonExe)) {
    Write-Error "Python executable was not found. Please ensure Python 3.10+ is installed."
    exit 1
}

$pyCheckCode = "import sys; v = sys.version_info; print(f'{v.major}.{v.minor}.{v.micro}'); sys.exit(0 if (v.major == 3 and v.minor >= 10) or (v.major > 3) else 1)"
$pyVersion = & $pythonExe -c $pyCheckCode 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Found Python version ($pyVersion) which does not meet the minimum requirement of Python 3.10+."
    exit 1
}
Write-Host "  Found Python: $pythonExe ($pyVersion)" -ForegroundColor Green

# 3. Repository Provisioning: Clone or Git Pull
Write-Host "`n[3/5] Syncing repository at: $InstallPath..." -ForegroundColor Cyan
if (-not (Test-Path -LiteralPath $InstallPath)) {
    $parentDir = Split-Path -Parent $InstallPath
    if (-not (Test-Path -LiteralPath $parentDir)) {
        New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
    }
    Write-Host "  Cloning repository from $RepoUrl..." -ForegroundColor Yellow
    & git clone "$RepoUrl" "$InstallPath"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to clone repository from $RepoUrl."
        exit 1
    }
    Write-Host "  Successfully cloned repository." -ForegroundColor Green
} else {
    Write-Host "  Target directory exists. Pulling latest updates..." -ForegroundColor Yellow
    & git -C "$InstallPath" pull
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [!] git pull encountered warnings or conflicts. Proceeding with existing source." -ForegroundColor Yellow
    } else {
        Write-Host "  Repository is up to date." -ForegroundColor Green
    }
}

# 4. Initialize Isolated Virtual Environment and Dependencies
$serverDir = Join-Path $InstallPath "server"
if (-not (Test-Path -LiteralPath $serverDir)) {
    Write-Error "Server directory not found at $serverDir."
    exit 1
}

$venvDir = Join-Path $serverDir "venv"
$venvPython = Join-Path $venvDir "Scripts\python.exe"
$requirementsFile = Join-Path $serverDir "requirements.txt"

Write-Host "`n[4/5] Configuring isolated virtual environment in $venvDir..." -ForegroundColor Cyan
if (-not (Test-Path -LiteralPath $venvPython)) {
    Write-Host "  Creating venv with $pythonExe..." -ForegroundColor Yellow
    & $pythonExe -m venv "$venvDir"
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $venvPython)) {
        Write-Error "Failed to create virtual environment at $venvDir."
        exit 1
    }
    Write-Host "  Virtual environment created." -ForegroundColor Green
} else {
    Write-Host "  Existing virtual environment detected: $venvPython" -ForegroundColor Green
}

Write-Host "  Upgrading pip..." -ForegroundColor DarkGray
& $venvPython -m pip install --upgrade pip --quiet

if (Test-Path -LiteralPath $requirementsFile) {
    Write-Host "  Installing dependencies from requirements.txt..." -ForegroundColor Yellow
    & $venvPython -m pip install -r "$requirementsFile"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install dependencies from $requirementsFile."
        exit 1
    }
    Write-Host "  Dependencies successfully installed." -ForegroundColor Green
} else {
    Write-Host "  [!] requirements.txt not found at $requirementsFile. Skipping pip install." -ForegroundColor Yellow
}

# 5. Invoke service_manager.ps1 to Configure Background Execution
$serviceManager = Join-Path $serverDir "service_manager.ps1"
Write-Host "`n[5/5] Background service configuration ($ConfigureService)..." -ForegroundColor Cyan
if (Test-Path -LiteralPath $serviceManager) {
    if ($ConfigureService -ne "skip") {
        Write-Host "  Invoking service_manager.ps1 ($ConfigureService)..." -ForegroundColor Yellow
        powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$serviceManager" $ConfigureService
    } else {
        Write-Host "  Service configuration skipped by parameter." -ForegroundColor Gray
    }
} else {
    Write-Host "  [!] service_manager.ps1 not found at $serviceManager." -ForegroundColor Yellow
}

Write-Host "`n============================================================" -ForegroundColor Green
Write-Host "   LensBridge Server installation & setup complete!         " -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
