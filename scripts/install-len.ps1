<#
.SYNOPSIS
    Automated Installer and Version Checker for len (ProjectLens) CLI Tool.
.DESCRIPTION
    Installs and maintains the latest version of len (ProjectLens) from the remote GitHub repository
    (https://github.com/MohammedBoure/len.git).
    
    Behavior:
    - Queries the remote repository for the latest HEAD commit hash.
    - Inspects the local Python environment for the installed 'projectlens' distribution.
    - If already installed and matching the latest remote commit (and not in local editable mode),
      it skips reinstallation and reports that the tool is up to date (no-op).
    - If not installed, outdated, or installed from a local/editable path, it automatically
      installs/upgrades to the latest remote version via pip.
    - Verifies executable functionality and ensures PATH readiness.
.PARAMETER RepoUrl
    Remote Git repository URL (defaults to https://github.com/MohammedBoure/len.git).
.PARAMETER Force
    Force reinstall even if the local version matches the remote commit.
.PARAMETER CheckOnly
    Only check if an update is available without performing installation.
#>
[CmdletBinding()]
param(
    [string]$RepoUrl = "https://github.com/MohammedBoure/len.git",
    [switch]$Force,
    [switch]$CheckOnly
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   len (ProjectLens) Remote Auto-Installer" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Locate Python 3 Executable
$pythonExe = $null
$candidatePythons = @(
    "$env:LOCALAPPDATA\Programs\Python\Python314\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python313\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python311\python.exe",
    "$env:ProgramFiles\Python314\python.exe",
    "$env:ProgramFiles\Python313\python.exe",
    "$env:ProgramFiles\Python312\python.exe"
)

foreach ($c in $candidatePythons) {
    if (Test-Path -LiteralPath $c) {
        $pythonExe = $c
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
    Write-Error "Python executable could not be found. Please ensure Python is installed and accessible."
    exit 1
}

$pythonDir = Split-Path -Parent $pythonExe
$pythonScripts = Join-Path $pythonDir "Scripts"
$lenExe = Join-Path $pythonScripts "len.exe"

Write-Host "[Python] Using: $pythonExe" -ForegroundColor DarkGray
Write-Host "[Scripts] Target: $pythonScripts" -ForegroundColor DarkGray

# 2. Locate Git Executable
$gitCmd = Get-Command git.exe -ErrorAction SilentlyContinue
if (-not $gitCmd) {
    $gitCandidate = "C:\Program Files\Git\cmd\git.exe"
    if (Test-Path -LiteralPath $gitCandidate) {
        $gitCmd = $gitCandidate
    } else {
        Write-Error "Git executable could not be found. Please ensure Git is installed to query remote repository."
        exit 1
    }
}

# 3. Query Remote Git Repository for Latest HEAD Commit
Write-Host "`n[1/3] Checking remote repository: $RepoUrl..." -ForegroundColor Cyan
$remoteCommit = $null
try {
    $lsRemoteOutput = & git ls-remote "$RepoUrl" HEAD 2>$null
    if ($LASTEXITCODE -eq 0 -and $lsRemoteOutput) {
        $remoteCommit = ($lsRemoteOutput -split "`t")[0].Trim()
    }
} catch {
    $remoteCommit = $null
}

if (-not $remoteCommit) {
    Write-Host "[WARNING] Could not retrieve latest remote commit from $RepoUrl (network issue or repo inaccessible)." -ForegroundColor Yellow
} else {
    Write-Host "Remote HEAD Commit: $remoteCommit" -ForegroundColor DarkGray
}

# 4. Query Currently Installed 'projectlens' Package in Python
Write-Host "`n[2/3] Inspecting local installation..." -ForegroundColor Cyan
$pyCheckScript = @"
import importlib.metadata, json, sys
try:
    dist = importlib.metadata.distribution('projectlens')
    version = dist.version
    direct_url_raw = dist.read_text('direct_url.json')
    direct_url = json.loads(direct_url_raw) if direct_url_raw else {}
    vcs_info = direct_url.get('vcs_info', {})
    commit_id = vcs_info.get('commit_id', '') if isinstance(vcs_info, dict) else ''
    url = direct_url.get('url', '')
    is_editable = bool(direct_url.get('dir_info', {}).get('editable', False))
    print(json.dumps({
        'installed': True,
        'version': version,
        'commit_id': commit_id,
        'url': url,
        'editable': is_editable
    }))
except Exception as e:
    print(json.dumps({'installed': False, 'error': str(e)}))
"@

$localInfoRaw = & $pythonExe -c $pyCheckScript
$localInfo = $null
try {
    $localInfo = $localInfoRaw | ConvertFrom-Json
} catch {
    $localInfo = [PSCustomObject]@{ installed = $false }
}

$isInstalled = [bool]$localInfo.installed
$installedVersion = if ($isInstalled) { $localInfo.version } else { "" }
$installedCommit = if ($isInstalled) { $localInfo.commit_id } else { "" }
$isEditable = if ($isInstalled) { [bool]$localInfo.editable } else { $false }
$installedUrl = if ($isInstalled) { $localInfo.url } else { "" }

if ($isInstalled) {
    Write-Host "Local Version   : v$installedVersion" -ForegroundColor DarkGray
    if ($installedCommit) {
        Write-Host "Local Commit    : $installedCommit" -ForegroundColor DarkGray
    }
    if ($isEditable) {
        Write-Host "Install Type    : Local Editable ($installedUrl)" -ForegroundColor Yellow
    } else {
        Write-Host "Install URL     : $installedUrl" -ForegroundColor DarkGray
    }
} else {
    Write-Host "Local Status    : Not installed" -ForegroundColor DarkGray
}

# 5. Determine if Action is Needed
$isUpToDate = $false
if ($isInstalled -and (-not $isEditable) -and (Test-Path -LiteralPath $lenExe)) {
    if ($remoteCommit -and $installedCommit -and ($installedCommit -eq $remoteCommit)) {
        $isUpToDate = $true
    } elseif (-not $remoteCommit -and $installedVersion) {
        # Remote was unreachable, but local installation exists and is functional
        $isUpToDate = $true
    }
}

if ($CheckOnly) {
    if ($isUpToDate) {
        Write-Host "`n[STATUS] len (projectlens v$installedVersion) is UP-TO-DATE." -ForegroundColor Green
        exit 0
    } else {
        Write-Host "`n[STATUS] len (projectlens) has updates available or requires installation." -ForegroundColor Yellow
        exit 1
    }
}

if ($isUpToDate -and (-not $Force)) {
    Write-Host "`n[OK] len (projectlens v$installedVersion) is already installed and up-to-date with remote ($($remoteCommit.Substring(0, [Math]::Min(7, $remoteCommit.Length))))." -ForegroundColor Green
    Write-Host "No action required." -ForegroundColor Green
    
    # Ensure current session PATH has Python Scripts
    if ($env:Path -notlike "*$pythonScripts*") {
        $env:Path = "$pythonScripts;$env:Path"
    }
    exit 0
}

# 6. Perform Installation / Upgrade from Remote Git Repository
Write-Host "`n[3/3] Installing / Upgrading len from remote repository..." -ForegroundColor Cyan
if ($isEditable) {
    Write-Host "Replacing local editable version with official remote package..." -ForegroundColor Yellow
} elseif ($isInstalled -and $remoteCommit) {
    Write-Host "Updating local commit ($($installedCommit.Substring(0, [Math]::Min(7, $installedCommit.Length)))) to latest remote ($($remoteCommit.Substring(0, [Math]::Min(7, $remoteCommit.Length))))..." -ForegroundColor Yellow
} else {
    Write-Host "Performing clean remote installation from $RepoUrl..." -ForegroundColor Cyan
}

$pipTarget = "git+$RepoUrl"
& $pythonExe -m pip install --upgrade --force-reinstall --no-cache-dir "$pipTarget"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to install len from $pipTarget. Please check your internet connection and git access."
    exit $LASTEXITCODE
}

# 7. Verification & Environment Readiness
if (Test-Path -LiteralPath $lenExe) {
    $verifyVersion = & $lenExe -h 2>$null
    if ($LASTEXITCODE -eq 0) {
        # Fetch updated metadata
        $postInstallRaw = & $pythonExe -c $pyCheckScript
        $postInstall = $postInstallRaw | ConvertFrom-Json
        $shortHash = if ($postInstall.commit_id) { $postInstall.commit_id.Substring(0, [Math]::Min(7, $postInstall.commit_id.Length)) } else { "latest" }
        
        Write-Host "`n==================================================" -ForegroundColor Green
        Write-Host " [SUCCESS] len (ProjectLens v$($postInstall.version)) is ready!" -ForegroundColor Green
        Write-Host " Remote Commit : $shortHash" -ForegroundColor Green
        Write-Host " Binary Path   : $lenExe" -ForegroundColor Green
        Write-Host "==================================================" -ForegroundColor Green
        
        # Ensure current session PATH includes Python Scripts
        if ($env:Path -notlike "*$pythonScripts*") {
            $env:Path = "$pythonScripts;$env:Path"
        }
    } else {
        Write-Warning "len.exe was found at $lenExe, but exited with code $LASTEXITCODE during verification."
    }
} else {
    Write-Warning "Installation completed, but len.exe was not detected at $lenExe. Please verify your Python Scripts PATH."
}
