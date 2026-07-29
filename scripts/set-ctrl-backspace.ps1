<#
.SYNOPSIS
    Configures Ctrl+Backspace word deletion in PowerShell console & $PROFILE.
.DESCRIPTION
    Adds Set-PSReadLineKeyHandler for Ctrl+Backspace to delete the previous word (BackwardKillWord)
    in the active session and ensures it is present in the PowerShell $PROFILE script.
#>

Write-Host "Configuring Ctrl+Backspace word deletion in PowerShell..." -ForegroundColor Cyan

# Apply in current session
try {
    if (Get-Module -Name PSReadLine) {
        Set-PSReadLineKeyHandler -Key Ctrl+Backspace -Function BackwardKillWord
        Set-PSReadLineKeyHandler -Key Ctrl+v -Function Paste
        Set-PSReadLineKeyHandler -Key Ctrl+c -Function CopyOrCancelLine
        Write-Host "Applied Ctrl+Backspace, Ctrl+C, and Ctrl+V handlers in current session." -ForegroundColor Green
    }
} catch {
    Write-Warning "Could not bind in current session: $_"
}

# Ensure $PROFILE directory and file exist
$profileDir = Split-Path -Path $PROFILE
if (-not (Test-Path $profileDir)) {
    New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
}

$profileSnippet = @"

# Enable Ctrl+Backspace, Ctrl+C, and Ctrl+V key handlers
if (-not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected) {
    try {
        Set-PSReadLineKeyHandler -Key Ctrl+Backspace -Function BackwardKillWord
        Set-PSReadLineKeyHandler -Key Ctrl+v -Function Paste
        Set-PSReadLineKeyHandler -Key Ctrl+c -Function CopyOrCancelLine
    } catch {}
}
"@

if (-not (Test-Path $PROFILE)) {
    Set-Content -Path $PROFILE -Value $profileSnippet -Encoding UTF8
    Write-Host "Created `$PROFILE with Ctrl+Backspace binding." -ForegroundColor Green
} else {
    $content = Get-Content -Path $PROFILE -Raw
    if ($content -notmatch "BackwardKillWord") {
        Add-Content -Path $PROFILE -Value $profileSnippet -Encoding UTF8
        Write-Host "Appended Ctrl+Backspace binding to `$PROFILE." -ForegroundColor Green
    } else {
        Write-Host "Ctrl+Backspace binding is already present in `$PROFILE." -ForegroundColor Yellow
    }
}
