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

# Ensure tokens directory exists
$tokensDir = "$userProfile\.gemini\tokens"
if (-not (Test-Path $tokensDir)) {
    New-Item -ItemType Directory -Path $tokensDir -Force | Out-Null
    Write-Host "Created tokens directory: $tokensDir" -ForegroundColor Green
} else {
    Write-Host "Tokens directory active: $tokensDir" -ForegroundColor Green
}

# Ensure both PowerShell profiles redirect to MyEnv profile
$centralProfile = "$myenvPath\powershell\profile.ps1"
$profileLocations = @(
    "$userProfile\Documents\WindowsPowerShell\Microsoft.PowerShell_profile.ps1",
    "$userProfile\Documents\PowerShell\Microsoft.PowerShell_profile.ps1"
)

$profileSnippet = @"
# Redirect: all PowerShell settings live in myenv.
`$centralProfile = '$centralProfile'
if (Test-Path `$centralProfile) { . `$centralProfile }
# Enable Ctrl+Backspace word deletion
if (-not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected) {
    try {
        Set-PSReadLineKeyHandler -Key Ctrl+Backspace -Function BackwardKillWord
    } catch {}
}
"@

foreach ($pPath in $profileLocations) {
    $parentDir = Split-Path -Parent $pPath
    if (-not (Test-Path $parentDir)) {
        New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
    }
    if (-not (Test-Path $pPath)) {
        Set-Content -LiteralPath $pPath -Value $profileSnippet -Encoding UTF8
        Write-Host "Configured profile redirection in: $pPath" -ForegroundColor Green
    } else {
        $content = Get-Content -LiteralPath $pPath -Raw -ErrorAction SilentlyContinue
        if ($content -notmatch [regex]::Escape($centralProfile)) {
            $updated = $profileSnippet + "`r`n`r`n" + $content
            Set-Content -LiteralPath $pPath -Value $updated -Encoding UTF8
            Write-Host "Injected central profile redirection into: $pPath" -ForegroundColor Green
        } else {
            Write-Host "Profile redirection already active in: $pPath" -ForegroundColor Green
        }
    }
}

