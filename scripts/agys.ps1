<#
.SYNOPSIS
    Antigravity Multi-Instance & Account Manager CLI (agys)
.DESCRIPTION
    Manages isolated Antigravity CLI instances (agy1, agy2, ..., agyN) with independent
    sessions, settings, and Google account tokens.
#>
param(
    [Parameter(Position = 0)]
    [string]$Action = "list",
    [Parameter(Position = 1)]
    [string]$Target,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$RemainingArgs
)

$switcherScript = "$PSScriptRoot\..\powershell\antigravity-switcher.ps1"
if (-not (Test-Path -LiteralPath $switcherScript)) {
    $switcherScript = "$env:USERPROFILE\Documents\myenv\powershell\antigravity-switcher.ps1"
}

. $switcherScript

$allArgs = @()
if ($Action) { $allArgs += $Action }
if ($Target) { $allArgs += $Target }
if ($RemainingArgs) { $allArgs += $RemainingArgs }

& agys @allArgs
