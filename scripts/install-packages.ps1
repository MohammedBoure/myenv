# MyEnv - Winget Package Installer Script
# Installs / Restores all developer packages listed in winget-packages.json

$ErrorActionPreference = "Stop"
$myenvPath = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $myenvPath "winget-packages.json"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MyEnv Package Restorer (Winget)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if (-not (Test-Path $manifestPath)) {
    Write-Host "[ERROR] Manifest file not found at: $manifestPath" -ForegroundColor Red
    exit 1
}

Write-Host "[+] Found package manifest at: $manifestPath" -ForegroundColor Green
Write-Host "[+] Importing packages via Winget..." -ForegroundColor Yellow

try {
    winget import --import-file "$manifestPath" --accept-package-agreements --accept-source-agreements --ignore-unavailable
    Write-Host "[SUCCESS] Package restoration process completed!" -ForegroundColor Green
} catch {
    Write-Host "[WARNING] Package restoration encountered some errors: $_" -ForegroundColor Yellow
}
