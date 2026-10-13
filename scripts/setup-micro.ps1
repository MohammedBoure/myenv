<#
.SYNOPSIS
    Configures the Micro text editor for VS Code parity in MyEnv.
.DESCRIPTION
    1. Ensures Micro is installed (calls install-micro.ps1 if missing).
    2. Links or copies settings.json and bindings.json from myenv/micro to ~/.config/micro/.
    3. Deploys custom colorschemes (midnight-aurora).
    4. Automatically installs the filemanager plugin for sidebar tree navigation.
    5. Configures Git to use Micro as its global default editor.
#>

$ErrorActionPreference = 'Stop'
$myenvPath = Split-Path -Parent $PSScriptRoot
$microSrcDir = Join-Path $myenvPath "micro"
$microTargetDir = Join-Path $env:USERPROFILE ".config\micro"

Write-Host "==========================================" -ForegroundColor Magenta
Write-Host "     MyEnv - Micro Editor Configuration   " -ForegroundColor Magenta
Write-Host "==========================================" -ForegroundColor Magenta

# 1. Ensure Micro is installed
$microExe = Get-Command "micro" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
if (-not $microExe) {
    Write-Host "[*] Micro executable not detected. Running installer..." -ForegroundColor Yellow
    & "$PSScriptRoot\install-micro.ps1"
    $microExe = Get-Command "micro" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
}

if (-not $microExe) {
    # Check WinGet package directory fallback
    $found = Get-ChildItem -Path "$env:LOCALAPPDATA\Microsoft\WinGet\Packages" -Filter "micro.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $microExe = $found.FullName
        $env:Path = "$(Split-Path $microExe);$env:Path"
    }
}

if (-not $microExe) {
    Write-Host "[ERROR] Could not locate or install Micro executable. Please verify installation." -ForegroundColor Red
    exit 1
}

Write-Host "[+] Found Micro at: $microExe" -ForegroundColor Green

# 2. Ensure target config directory exists
if (-not (Test-Path -LiteralPath $microTargetDir)) {
    New-Item -ItemType Directory -Path $microTargetDir -Force | Out-Null
    Write-Host "[+] Created configuration directory: $microTargetDir" -ForegroundColor Green
}

# 3. Helper to robustly link or copy configuration files
function Sync-MicroConfigFile {
    param(
        [Parameter(Mandatory=$true)][string]$SourcePath,
        [Parameter(Mandatory=$true)][string]$TargetPath
    )

    if (-not (Test-Path -LiteralPath $SourcePath)) {
        Write-Host "[!] Source file not found: $SourcePath" -ForegroundColor Yellow
        return
    }

    $fileName = Split-Path -Leaf $SourcePath
    $parentDir = Split-Path -Parent $TargetPath
    if (-not (Test-Path -LiteralPath $parentDir)) {
        New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
    }

    if (Test-Path -LiteralPath $TargetPath) {
        Remove-Item -LiteralPath $TargetPath -Force -ErrorAction SilentlyContinue
    }

    $linked = $false
    # Attempt Symbolic Link
    try {
        New-Item -ItemType SymbolicLink -Path $TargetPath -Target $SourcePath -Force -ErrorAction Stop | Out-Null
        Write-Host "  -> Linked (Symlink): $fileName" -ForegroundColor Green
        $linked = $true
    } catch {
        # Fallback to Hard Link
        try {
            New-Item -ItemType HardLink -Path $TargetPath -Target $SourcePath -Force -ErrorAction Stop | Out-Null
            Write-Host "  -> Linked (HardLink): $fileName" -ForegroundColor Green
            $linked = $true
        } catch {
            # Fallback to Copy
            Copy-Item -LiteralPath $SourcePath -Destination $TargetPath -Force
            Write-Host "  -> Copied: $fileName" -ForegroundColor Yellow
        }
    }
}

Write-Host "`n[*] Deploying VS Code parity configurations..." -ForegroundColor Cyan
Sync-MicroConfigFile -SourcePath (Join-Path $microSrcDir "settings.json") -TargetPath (Join-Path $microTargetDir "settings.json")
Sync-MicroConfigFile -SourcePath (Join-Path $microSrcDir "bindings.json") -TargetPath (Join-Path $microTargetDir "bindings.json")

# 4. Deploy Custom Colorschemes
$srcColorsDir = Join-Path $microSrcDir "colorschemes"
$targetColorsDir = Join-Path $microTargetDir "colorschemes"
if (Test-Path -LiteralPath $srcColorsDir) {
    if (-not (Test-Path -LiteralPath $targetColorsDir)) {
        New-Item -ItemType Directory -Path $targetColorsDir -Force | Out-Null
    }
    Get-ChildItem -LiteralPath $srcColorsDir -Filter "*.micro" -File | ForEach-Object {
        Sync-MicroConfigFile -SourcePath $_.FullName -TargetPath (Join-Path $targetColorsDir $_.Name)
    }
}

# 5. Install / Verify 'filemanager' plugin
Write-Host "`n[*] Verifying Micro plugins (filemanager)..." -ForegroundColor Cyan
$filemanagerDir = Join-Path $microTargetDir "plug\filemanager"
if (-not (Test-Path -LiteralPath $filemanagerDir)) {
    Write-Host "[*] Installing 'filemanager' plugin for VS Code-like sidebar tree..." -ForegroundColor Yellow
    try {
        & $microExe -plugin install filemanager
        Write-Host "[+] 'filemanager' plugin installed successfully." -ForegroundColor Green
    } catch {
        Write-Host "[!] Failed to install filemanager plugin: $_" -ForegroundColor Red
    }
} else {
    Write-Host "[+] 'filemanager' plugin is already installed." -ForegroundColor Green
}

# 6. Configure Git core.editor
Write-Host "`n[*] Configuring Git default editor to Micro..." -ForegroundColor Cyan
if (Get-Command "git" -ErrorAction SilentlyContinue) {
    try {
        git config --global core.editor "micro"
        $currentEditor = git config --global --get core.editor
        Write-Host "[+] Git global core.editor set to: $currentEditor" -ForegroundColor Green
    } catch {
        Write-Host "[!] Could not configure git core.editor: $_" -ForegroundColor Yellow
    }
}

# 7. Set session environment variables
$env:EDITOR = "micro"
$env:VISUAL = "micro"

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host "   Micro setup completed successfully!    " -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Keybindings summary:" -ForegroundColor Cyan
Write-Host "  Ctrl+S       : Save file"
Write-Host "  Ctrl+C / V / X: Copy / Paste / Cut (Windows Clipboard)"
Write-Host "  Ctrl+Z / Y   : Undo / Redo"
Write-Host "  Ctrl+F       : Find in file"
Write-Host "  Ctrl+B       : Toggle File Manager Tree (VS Code Sidebar)"
Write-Host "  Ctrl+D       : Multi-cursor select next occurrence"
Write-Host "  Alt+Up/Down  : Move lines up / down"
Write-Host "  Shift+Alt+Up/Down : Duplicate line up / down"
Write-Host "  Ctrl+Q       : Quit editor"
