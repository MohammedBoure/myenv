# myenv - single source of truth for Windows PowerShell

# Ensure strict UTF-8 stream decoding
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding  = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding           = [System.Text.UTF8Encoding]::new($false)
chcp 65001 > $null

# Configure PSReadLine & Predictive IntelliSense if interactive
if (-not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected) {
    try {
        Import-Module PSReadLine -ErrorAction SilentlyContinue
        Set-PSReadLineOption -EditMode Windows
        Set-PSReadLineOption -PredictionSource HistoryAndPlugin
        Set-PSReadLineOption -PredictionViewStyle ListView
    } catch {}
    Import-Module CompletionPredictor -ErrorAction SilentlyContinue
}

$PSDefaultParameterValues['*:Encoding'] = 'utf8'
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'

$env:LANG = 'en_US.UTF-8'
$env:LC_ALL = 'en_US.UTF-8'
$env:PYTHONIOENCODING = 'utf-8'
$env:PYTHONUTF8 = '1'
$env:LESSCHARSET = 'utf-8'
$env:VISUAL = 'micro'
$env:EDITOR = 'micro'
$env:COLORTERM = 'truecolor'
$env:MICRO_TRUECOLOR = '1'

# Dynamic User Profile and Repository Path
$userProfile = $env:USERPROFILE
$myenvDir = Join-Path $userProfile "Documents\myenv"

# Preload consolidated MyEnv C# helper library (FolderSizeEngine, AgyCredVault)
$helpersDll = Join-Path $myenvDir "powershell\MyEnv.Helpers.dll"
if (Test-Path -LiteralPath $helpersDll) {
    [System.Reflection.Assembly]::LoadFrom($helpersDll) | Out-Null
}

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
    "$myenvDir\tools\rtlterminal",
    "$myenvDir\scripts\nightpad",
    "$myenvDir\scripts"
)
$existingPathSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($part in ($env:Path -split ';')) {
    if (-not [string]::IsNullOrWhiteSpace($part)) {
        [void]$existingPathSet.Add($part.TrimEnd('\'))
    }
}
$toPrependPaths = [System.Collections.Generic.List[string]]::new()
foreach ($p in $pathsToAdd) {
    $norm = $p.TrimEnd('\')
    if (-not $existingPathSet.Contains($norm) -and (Test-Path -LiteralPath $p)) {
        [void]$existingPathSet.Add($norm)
        $toPrependPaths.Add($p)
    }
}
if ($toPrependPaths.Count -gt 0) {
    $env:Path = ($toPrependPaths -join ';') + ';' + $env:Path
}

# Fast cached resolution of WinGet package executable directories (scrcpy, ffmpeg, fzf, fastfetch, micro)
function Update-WinGetPathCache {
    [CmdletBinding()]
    param()
    $wingetDir = Join-Path $env:USERPROFILE "AppData\Local\Microsoft\WinGet\Packages"
    $cachePath = Join-Path $myenvDir "powershell\winget-paths.cache"
    $bins = @()
    if (Test-Path -LiteralPath $wingetDir) {
        Get-ChildItem -LiteralPath $wingetDir -Directory -ErrorAction SilentlyContinue | ForEach-Object {
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
                    $bins += $td
                }
            }
        }
    }
    $bins = @($bins | Select-Object -Unique)
    $bins | Out-File -FilePath $cachePath -Encoding utf8 -Force
    return $bins
}

$wingetPackagesDir = Join-Path $userProfile "AppData\Local\Microsoft\WinGet\Packages"
$wingetCache = Join-Path $myenvDir "powershell\winget-paths.cache"
$wingetBinDirs = @()

if (Test-Path -LiteralPath $wingetPackagesDir) {
    if (Test-Path -LiteralPath $wingetCache) {
        $cacheItem = Get-Item -LiteralPath $wingetCache
        $pkgItem = Get-Item -LiteralPath $wingetPackagesDir
        if ($cacheItem.LastWriteTime -ge $pkgItem.LastWriteTime) {
            $wingetBinDirs = @(Get-Content -LiteralPath $wingetCache -Encoding utf8 | Where-Object { $_ -ne '' })
        } else {
            $wingetBinDirs = Update-WinGetPathCache
        }
    } else {
        $wingetBinDirs = Update-WinGetPathCache
    }

    $toPrependWinget = [System.Collections.Generic.List[string]]::new()
    foreach ($td in $wingetBinDirs) {
        $norm = $td.TrimEnd('\')
        if (-not $existingPathSet.Contains($norm) -and (Test-Path -LiteralPath $td)) {
            [void]$existingPathSet.Add($norm)
            $toPrependWinget.Add($td)
        }
    }
    if ($toPrependWinget.Count -gt 0) {
        $env:Path = ($toPrependWinget -join ';') + ';' + $env:Path
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

# Fastfetch - System & Hardware Summary (Deferred from synchronous startup for instant shell readiness)
# To display automatically on shell startup, set $env:MYENV_SHOW_FASTFETCH = '1'. Run 'sysinfo' or 'fetch' anytime.
function sysinfo {
    $fastfetch = Get-Command fastfetch.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -First 1
    $fastConfig = Join-Path $env:USERPROFILE ".config\fastfetch\config.jsonc"
    if ($fastfetch) {
        if (Test-Path $fastConfig) { & $fastfetch -c $fastConfig } else { & $fastfetch }
    } else {
        Write-Warning "fastfetch is not installed."
    }
}
Set-Alias -Name fetch -Value sysinfo -Option AllScope -ErrorAction SilentlyContinue

if ($env:MYENV_SHOW_FASTFETCH -eq '1' -and -not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected -and -not $global:__MyEnvFastfetchShown) {
    $global:__MyEnvFastfetchShown = $true
    sysinfo
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

# RtlTerminal - RTL & Arabic Terminal Emulator
function rtl {
    $rtlExe = Join-Path $myenvDir "tools\rtlterminal\RtlTerminal.exe"
    if (Test-Path -LiteralPath $rtlExe) {
        $workDir = (Get-Location).Path
        if ($args.Count -gt 0) {
            $target = $args -join ' '
            if (Test-Path -LiteralPath $target) {
                $workDir = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($target)
            }
        }
        Start-Process -FilePath $rtlExe -ArgumentList "`"$workDir`"" -WorkingDirectory $workDir
    } else {
        Write-Warning "RtlTerminal not found at $rtlExe. Run setup-arabic-terminal.ps1 to install it."
    }
}
Set-Alias -Name rtlterminal -Value rtl -Option AllScope -ErrorAction SilentlyContinue

# Micro Terminal Editor (Default VS Code-like Editor)
# Git core.editor is preconfigured to 'micro' globally in ~/.gitconfig to avoid spawning git.exe on shell startup.

function Invoke-MicroEditor {
    [Console]::Write("`e[6 q")
    try {
        $microApp = Get-Command micro.exe -CommandType Application -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -First 1
        if ($microApp) {
            & $microApp @args
        } else {
            micro @args
        }
    } finally {
        [Console]::Write("`e[6 q")
    }
}
function micro {
    Invoke-MicroEditor @args
}
Set-Alias -Name m -Value Invoke-MicroEditor -Option AllScope -ErrorAction SilentlyContinue
Set-Alias -Name mc -Value Invoke-MicroEditor -Option AllScope -ErrorAction SilentlyContinue
[Console]::Write("`e[6 q")

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

# Antigravity Multi-Account & Multi-Instance Switcher (agys, agy1..agyN)
$agySwitcherPath = Join-Path $myenvDir "powershell\antigravity-switcher.ps1"
if (Test-Path -LiteralPath $agySwitcherPath) {
    . $agySwitcherPath
}