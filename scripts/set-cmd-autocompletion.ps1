<#
.SYNOPSIS
    Configures CMD (Command Prompt) Auto-Completion and Macro AutoRun.
.DESCRIPTION
    Enables Tab auto-completion for files and paths in cmd.exe and links
    myenv\scripts\cmd-init.cmd as the CMD AutoRun script.
#>

$ErrorActionPreference = 'Stop'
$cmdKey = 'HKCU:\Software\Microsoft\Command Processor'

if (-not (Test-Path $cmdKey)) {
    New-Item -Path $cmdKey -Force | Out-Null
}

# 1. Enable Tab Auto-Completion for Files and Paths (CompletionChar = 9)
Set-ItemProperty -Path $cmdKey -Name 'CompletionChar' -Type DWord -Value 9
Set-ItemProperty -Path $cmdKey -Name 'PathCompletionChar' -Type DWord -Value 9
Set-ItemProperty -Path $cmdKey -Name 'EnableExtensions' -Type DWord -Value 1

# 2. Register AutoRun Script for CMD
$initScript = 'C:\Users\moham\Documents\myenv\scripts\cmd-init.cmd'
if (Test-Path $initScript) {
    Set-ItemProperty -Path $cmdKey -Name 'AutoRun' -Type String -Value "`"$initScript`""
    Write-Host "CMD AutoRun script registered: $initScript" -ForegroundColor Green
}

Write-Host "CMD Auto-Completion and Doskey Macros configured successfully." -ForegroundColor Green
