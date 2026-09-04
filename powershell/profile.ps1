# myenv - single source of truth for Windows PowerShell

# Ensure strict UTF-8 stream decoding
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding  = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding           = [System.Text.UTF8Encoding]::new($false)
chcp 65001 > $null

# Configure PSReadLine & Predictive IntelliSense if interactive
if (-not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected) {
    if (Get-Module -ListAvailable PSReadLine) {
        try {
            Import-Module PSReadLine -ErrorAction SilentlyContinue
            Set-PSReadLineOption -EditMode Windows
            Set-PSReadLineOption -PredictionSource HistoryAndPlugin
            Set-PSReadLineOption -PredictionViewStyle ListView
        } catch {}
    }
    if (Get-Module -ListAvailable CompletionPredictor) {
        Import-Module CompletionPredictor -ErrorAction SilentlyContinue
    }
}

$PSDefaultParameterValues['*:Encoding'] = 'utf8'
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'

$env:LANG = 'en_US.UTF-8'
$env:LC_ALL = 'en_US.UTF-8'
$env:PYTHONIOENCODING = 'utf-8'
$env:PYTHONUTF8 = '1'
$env:LESSCHARSET = 'utf-8'
$env:VISUAL = 'code --wait'
$env:EDITOR = 'code --wait'

# Dynamic User Profile and Repository Path
$userProfile = $env:USERPROFILE
$myenvDir = Join-Path $userProfile "Documents\myenv"

# Central development environment paths.
$pathsToAdd = @(
    "$userProfile\development\msys64\ucrt64\bin",
    "$userProfile\development\php",
    "$userProfile\AppData\Roaming\Composer\vendor\bin",
    "$userProfile\development\nodejs",
    "$userProfile\AppData\Roaming\npm",
    "$userProfile\development\kotlin\bin",
    "$userProfile\AppData\Local\Android\Sdk\cmdline-tools\latest\bin",
    "$userProfile\AppData\Local\Android\Sdk\platform-tools",
    "$userProfile\AppData\Local\Android\Sdk\emulator",
    "$userProfile\development\flutter\bin",
    "$userProfile\development\jdk-17.0.19+10\bin",
    'C:\Program Files\dotnet',
    'C:\Windows\System32\WindowsPowerShell\v1.0',
    "$userProfile\AppData\Local\Microsoft\WindowsApps",
    "$userProfile\.local\bin",
    "$userProfile\AppData\Local\agy\bin",
    "$userProfile\AppData\Local\Programs\Python\Python314",
    "$userProfile\AppData\Local\Programs\Python\Python314\Scripts",
    "$myenvDir\scripts\nightpad",
    "$myenvDir\scripts"
)
foreach ($p in $pathsToAdd) {
    if (Test-Path -LiteralPath $p) {
        $existing = @($env:Path -split ';' | Where-Object { $_ -ne '' })
        if (-not ($existing | Where-Object { $_.TrimEnd('\') -ieq $p.TrimEnd('\') })) {
            $env:Path = "$p;$env:Path"
        }
    }
}

# Dynamically resolve WinGet package executable directories (scrcpy, ffmpeg, fzf, fastfetch, etc.)
$wingetPackagesDir = Join-Path $userProfile "AppData\Local\Microsoft\WinGet\Packages"
if (Test-Path -LiteralPath $wingetPackagesDir) {
    Get-ChildItem -LiteralPath $wingetPackagesDir -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        $pkgDir = $_.FullName
        $subDirs = Get-ChildItem -LiteralPath $pkgDir -Directory -ErrorAction SilentlyContinue
        $targetDirs = @($pkgDir)
        if ($subDirs) {
            foreach ($sub in $subDirs) {
                $targetDirs += $sub.FullName
                $nestedBin = Join-Path $sub.FullName "bin"
                if (Test-Path -LiteralPath $nestedBin) { $targetDirs += $nestedBin }
            }
        }
        foreach ($td in $targetDirs) {
            if (Get-ChildItem -LiteralPath $td -Filter "*.exe" -File -ErrorAction SilentlyContinue) {
                $existing = @($env:Path -split ';' | Where-Object { $_ -ne '' })
                if (-not ($existing | Where-Object { $_.TrimEnd('\') -ieq $td.TrimEnd('\') })) {
                    $env:Path = "$td;$env:Path"
                }
            }
        }
    }
}

$env:DOTNET_ROOT = 'C:\Program Files\dotnet'
$env:JAVA_HOME = "$userProfile\development\jdk-17.0.19+10"
$env:ANDROID_HOME = "$userProfile\AppData\Local\Android\Sdk"
$env:ANDROID_SDK_ROOT = "$userProfile\AppData\Local\Android\Sdk"
$env:KOTLIN_HOME = "$userProfile\development\kotlin"
$env:MSYS2_ROOT = "$userProfile\development\msys64"
$env:PHP_HOME = "$userProfile\development\php"
$env:PHPRC = "$userProfile\development\php"
$env:COMPOSER_HOME = "$userProfile\AppData\Roaming\Composer"
$env:NODE_HOME = "$userProfile\development\nodejs"
$env:NPM_CONFIG_PREFIX = "$userProfile\AppData\Roaming\npm"

# Keep PowerShell modules under myenv when present.
$myenvModules = Join-Path $myenvDir "powershell\Modules"
if (Test-Path $myenvModules) {
    $env:PSModulePath = $myenvModules + ';' + $env:PSModulePath
}

# Human-readable size format for ls, Get-ChildItem, and FileInfo
$customFormat = Join-Path $myenvDir "powershell\FileSystem.format.ps1xml"
if (Test-Path -LiteralPath $customFormat) {
    Update-FormatData -PrependPath $customFormat -ErrorAction SilentlyContinue
}
Update-TypeData -TypeName System.IO.FileInfo -MemberType ScriptProperty -MemberName size -Value {
    if ($null -ne $this.Length) {
        $len = $this.Length
        if ($len -ge 1TB) { [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0:0.#} TB", $len / 1TB) }
        elseif ($len -ge 1GB) { [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0:0.#} GB", $len / 1GB) }
        elseif ($len -ge 1MB) { [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0:0.#} MB", $len / 1MB) }
        elseif ($len -ge 1KB) { [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0:0.#} KB", $len / 1KB) }
        else { "$len B" }
    }
} -Force -ErrorAction SilentlyContinue
Update-TypeData -TypeName System.IO.DirectoryInfo -MemberType ScriptProperty -MemberName size -Value {
    if ($global:__MyEnvDirSizeCache -and $global:__MyEnvDirSizeCache.ContainsKey($this.FullName)) {
        return $global:__MyEnvDirSizeCache[$this.FullName].Formatted
    }
    return '-'
} -Force -ErrorAction SilentlyContinue

$themePath = Join-Path $myenvDir "powershell\midnight-aurora.ps1"
if (Test-Path $themePath) { . $themePath }

$consoleThemePath = Join-Path $myenvDir "powershell\console-theme.ps1"
if (Test-Path $consoleThemePath) { . $consoleThemePath }

# Fastfetch runs once per interactive console session.
if (-not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected -and -not $global:__MyEnvFastfetchShown) {
    $global:__MyEnvFastfetchShown = $true
    $fastfetch = Get-Command fastfetch -ErrorAction SilentlyContinue
    $fastConfig = Join-Path $userProfile ".config\fastfetch\config.jsonc"
    if ($fastfetch -and (Test-Path $fastConfig)) {
        fastfetch -c $fastConfig
    }
}

# Sudo Utility Function for PowerShell (Elevates command or opens elevated PowerShell in current working directory)
function sudo {
    $currentDir = (Get-Location).ProviderPath
    $shellExe = if (Get-Command pwsh.exe -ErrorAction SilentlyContinue) { "pwsh" } else { "powershell" }
    if ($args.Count -eq 0) {
        Start-Process $shellExe -WorkingDirectory $currentDir -ArgumentList "-NoExit -Command Set-Location -LiteralPath '$currentDir'" -Verb RunAs
    } else {
        $exe = $args[0]
        if ($args.Count -gt 1) {
            $cmdArgs = $args[1..($args.Count - 1)] -join ' '
            Start-Process -FilePath $exe -ArgumentList $cmdArgs -WorkingDirectory $currentDir -Verb RunAs
        } else {
            Start-Process -FilePath $exe -WorkingDirectory $currentDir -Verb RunAs
        }
    }
}

# Notepad - Lightweight Professional Text Editor
function np {
    $nightpadExe = Join-Path $myenvDir "scripts\nightpad\NightPad.exe"
    if (Test-Path -LiteralPath $nightpadExe) {
        if ($args.Count -eq 0) {
            Start-Process -FilePath $nightpadExe -WorkingDirectory (Get-Location).Path
        } else {
            $target = $args -join ' '
            $resolved = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($target)
            Start-Process -FilePath $nightpadExe -ArgumentList "`"$resolved`"" -WorkingDirectory (Get-Location).Path
        }
    } else {
        notepad.exe @args
    }
}
Set-Alias -Name nightpad -Value np -Option AllScope -ErrorAction SilentlyContinue
Set-Alias -Name notepad -Value np -Option AllScope -ErrorAction SilentlyContinue

# ==============================================================================
# Arabic Reshaper & Interactive CLI Helper Utilities
# ==============================================================================

# Function to reshape and format Arabic strings for terminal display
function Format-ArabicText {
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [string]$Text
    )
    process {
        $pyCode = @"
import sys, arabic_reshaper
from bidi.algorithm import get_display

input_text = sys.argv[1]
# Reshape Arabic characters and apply BiDi display ordering
reshaped = arabic_reshaper.reshape(input_text)
bidi_text = get_display(reshaped)
print(bidi_text)
"@
        $pythonExe = if (Test-Path "$env:LOCALAPPDATA\Programs\Python\Python314\python.exe") {
            "$env:LOCALAPPDATA\Programs\Python\Python314\python.exe"
        } else {
            "python"
        }
        & $pythonExe -c $pyCode $Text
    }
}

Set-Alias -Name ar -Value Format-ArabicText -Option AllScope -ErrorAction SilentlyContinue

# Helper function to prompt for Arabic input via a native Windows input box
function Get-ArabicInput {
    param([string]$Title = "Arabic Input Prompt", [string]$Prompt = "Enter text:")
    Add-Type -AssemblyName Microsoft.VisualBasic
    [Microsoft.VisualBasic.Interaction]::InputBox($Prompt, $Title)
}

# Wrapper for AI / CLI execution with Arabic support
function Invoke-ArabicCli {
    param(
        [Parameter(ValueFromRemainingArguments=$true)]
        [string[]]$ArgsList
    )
    $rawInput = Get-ArabicInput -Title "AI CLI Prompt" -Prompt "اكتب طلبك باللغة العربية:"
    if (![string]::IsNullOrWhiteSpace($rawInput)) {
        # Forward the properly shaped or direct UTF-8 string to the CLI tool
        $cli = if (Get-Command antigravity -ErrorAction SilentlyContinue) { "antigravity" } elseif (Get-Command agy -ErrorAction SilentlyContinue) { "agy" } else { "antigravity" }
        & $cli @ArgsList $rawInput
    }
}

Set-Alias -Name ask-ai -Value Invoke-ArabicCli -Option AllScope -ErrorAction SilentlyContinue
Set-Alias -Name antigravity -Value agy -Option AllScope -ErrorAction SilentlyContinue