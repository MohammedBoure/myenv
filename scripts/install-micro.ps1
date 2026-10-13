<#
.SYNOPSIS
    Installs the Micro text editor for Windows.
.DESCRIPTION
    Checks if 'micro' is already present in PATH.
    If not installed, attempts installation via:
    1. WinGet (zyedidia.micro)
    2. Scoop (micro)
    3. Direct GitHub release download fallback
    Ensures the executable is accessible in PATH.
#>

$ErrorActionPreference = 'Stop'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "       Micro Editor Installer           " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. Check if micro already exists in PATH
$microCmd = Get-Command "micro.exe" -ErrorAction SilentlyContinue
if (-not $microCmd) {
    $microCmd = Get-Command "micro" -ErrorAction SilentlyContinue
}

if ($microCmd) {
    Write-Host "[+] Micro is already installed at: $($microCmd.Source)" -ForegroundColor Green
    & $microCmd.Source -version
    exit 0
}

Write-Host "[*] Micro not found in PATH. Initiating installation..." -ForegroundColor Yellow

$installed = $false

# 2. Try WinGet
if (Get-Command "winget" -ErrorAction SilentlyContinue) {
    Write-Host "[*] Attempting installation via WinGet..." -ForegroundColor Cyan
    try {
        winget install --id zyedidia.micro --exact --accept-source-agreements --accept-package-agreements --silent
        if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq -1978335189) { # 0 = success, -1978335189 = already installed
            $installed = $true
            Write-Host "[+] Micro successfully installed via WinGet." -ForegroundColor Green
        }
    } catch {
        Write-Host "[!] WinGet installation failed: $_" -ForegroundColor Yellow
    }
}

# 3. Try Scoop if WinGet was unsuccessful
if (-not $installed -and (Get-Command "scoop" -ErrorAction SilentlyContinue)) {
    Write-Host "[*] Attempting installation via Scoop..." -ForegroundColor Cyan
    try {
        scoop install micro
        if ($LASTEXITCODE -eq 0) {
            $installed = $true
            Write-Host "[+] Micro successfully installed via Scoop." -ForegroundColor Green
        }
    } catch {
        Write-Host "[!] Scoop installation failed: $_" -ForegroundColor Yellow
    }
}

# 4. Fallback: Direct GitHub Release Download
if (-not $installed) {
    Write-Host "[*] Downloading official Micro release from GitHub..." -ForegroundColor Cyan
    $version = "2.0.15"
    $downloadUrl = "https://github.com/zyedidia/micro/releases/download/v$version/micro-$version-win64.zip"
    $installDir = Join-Path $env:USERPROFILE ".local\bin"
    if (-not (Test-Path $installDir)) {
        New-Item -ItemType Directory -Path $installDir -Force | Out-Null
    }

    $tempZip = Join-Path $env:TEMP "micro-$version-win64.zip"
    $tempExtract = Join-Path $env:TEMP "micro_extracted"

    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $tempZip -UseBasicParsing
        Expand-Archive -Path $tempZip -DestinationPath $tempExtract -Force
        $microBin = Get-ChildItem -Path $tempExtract -Filter "micro.exe" -Recurse | Select-Object -First 1
        if ($microBin) {
            Copy-Item -Path $microBin.FullName -Destination "$installDir\micro.exe" -Force
            Write-Host "[+] Downloaded and placed micro.exe into $installDir" -ForegroundColor Green
            $installed = $true
        }
    } catch {
        Write-Host "[ERROR] Failed to download Micro directly: $_" -ForegroundColor Red
        exit 1
    } finally {
        Remove-Item -Path $tempZip -Force -ErrorAction SilentlyContinue
        Remove-Item -Path $tempExtract -Recurse -Force -ErrorAction SilentlyContinue
    }

    # Ensure $installDir is in User PATH
    $userPath = [System.Environment]::GetEnvironmentVariable("Path", "User")
    if ($userPath -notlike "*$installDir*") {
        [System.Environment]::SetEnvironmentVariable("Path", "$userPath;$installDir", "User")
        $env:Path = "$installDir;$env:Path"
        Write-Host "[+] Added $installDir to User PATH." -ForegroundColor Green
    }
}

# 5. Verification
$verifyCmd = Get-Command "micro" -ErrorAction SilentlyContinue
if ($verifyCmd) {
    Write-Host "[SUCCESS] Micro text editor ready to use!" -ForegroundColor Green
    & $verifyCmd.Source -version
} else {
    Write-Host "[WARNING] Micro installed, but new terminal session may be required to refresh PATH." -ForegroundColor Yellow
}
