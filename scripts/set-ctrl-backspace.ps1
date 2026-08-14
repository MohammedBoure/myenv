<#
.SYNOPSIS
    Configures Ctrl+Backspace word deletion in PowerShell & PowerShell 7 $PROFILE.
.DESCRIPTION
    Adds Set-PSReadLineKeyHandler for Ctrl+Backspace to delete the previous word (BackwardKillWord)
    in the active session and ensures it is present in both Windows PowerShell and PowerShell 7 profiles.
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

$profileSnippet = @"

# Redirect: all PowerShell settings live in myenv.
`$centralProfile = '$env:USERPROFILE\Documents\myenv\powershell\profile.ps1'
if (Test-Path `$centralProfile) { . `$centralProfile }

# Enable Ctrl+Backspace, Ctrl+C, and Ctrl+V key handlers
if (-not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected) {
    try {
        Set-PSReadLineKeyHandler -Key Ctrl+Backspace -Function BackwardKillWord
        Set-PSReadLineKeyHandler -Key Ctrl+v -Function Paste
        Set-PSReadLineKeyHandler -Key Ctrl+c -Function CopyOrCancelLine
    } catch {}
}
"@

$targetProfiles = @(
    "$env:USERPROFILE\Documents\WindowsPowerShell\Microsoft.PowerShell_profile.ps1",
    "$env:USERPROFILE\Documents\PowerShell\Microsoft.PowerShell_profile.ps1"
)

foreach ($prof in $targetProfiles) {
    $parent = Split-Path $prof
    if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
    if (-not (Test-Path $prof)) {
        Set-Content -Path $prof -Value $profileSnippet -Encoding UTF8
        Write-Host "Created $prof with redirect and Ctrl+Backspace binding." -ForegroundColor Green
    } else {
        $content = Get-Content -Path $prof -Raw
        if ($content -notmatch "BackwardKillWord") {
            Add-Content -Path $prof -Value $profileSnippet -Encoding UTF8
            Write-Host "Appended Ctrl+Backspace binding to $prof." -ForegroundColor Green
        } else {
            Write-Host "Binding already present in $prof." -ForegroundColor Yellow
        }
    }
}
