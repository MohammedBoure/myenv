<#
.SYNOPSIS
    Sets up and synchronizes the central Gemini Antigravity configuration.
.DESCRIPTION
    Links %USERPROFILE%\.gemini\config to %USERPROFILE%\Documents\myenv\gemini
    allowing global Antigravity rules, skills, MCP servers, and configurations
    to be version-controlled and managed centrally within MyEnv.
#>

$ErrorActionPreference = "Continue"
$myenvPath = Split-Path -Parent $PSScriptRoot
$userProfile = $env:USERPROFILE

$source = "$userProfile\.gemini\config"
$target = "$myenvPath\gemini"

Write-Host "==========================================" -ForegroundColor Magenta
Write-Host "   Gemini Antigravity Environment Setup   " -ForegroundColor Magenta
Write-Host "==========================================" -ForegroundColor Magenta

# Ensure target folder exists in myenv
if (-not (Test-Path $target)) {
    New-Item -ItemType Directory -Path $target -Force | Out-Null
}

$geminiParent = "$userProfile\.gemini"
if (-not (Test-Path $geminiParent)) {
    New-Item -ItemType Directory -Path $geminiParent -Force | Out-Null
}

if (-not (Test-Path $source)) {
    cmd /c "mklink /J `"$source`" `"$target`""
    Write-Host "Created directory junction: $source -> $target" -ForegroundColor Green
} else {
    $item = Get-Item $source -Force
    if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        Write-Host "Junction already configured and active: $source -> $target" -ForegroundColor Green
    } else {
        # Sync files from myenv/gemini into ~/.gemini/config and vice-versa
        Write-Host "Synchronizing Gemini config between MyEnv and ~/.gemini/config..." -ForegroundColor Cyan
        
        # Copy from target to source
        Get-ChildItem -Path $target -Recurse | ForEach-Object {
            $rel = $_.FullName.Substring($target.Length).TrimStart('\', '/')
            $dest = Join-Path $source $rel
            if ($_.PSIsContainer) {
                if (-not (Test-Path $dest)) { New-Item -ItemType Directory -Path $dest -Force | Out-Null }
            } else {
                Copy-Item -Path $_.FullName -Destination $dest -Force -ErrorAction SilentlyContinue
            }
        }
        
        Write-Host "Configuration synchronized successfully." -ForegroundColor Green
        Write-Host "Note: On fresh machine setups, setup-all.ps1 will automatically mount it as a junction directly." -ForegroundColor Gray
    }
}
