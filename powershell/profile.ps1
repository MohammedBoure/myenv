# myenv - single source of truth for Windows PowerShell

$utf8 = New-Object System.Text.UTF8Encoding($false)
try {
    [Console]::OutputEncoding = $utf8
    [Console]::InputEncoding = $utf8
    $OutputEncoding = $utf8
    chcp 65001 > $null
} catch {}

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
    "$userProfile\AppData\Local\agy\bin",
    "$userProfile\.local\bin",
    "$userProfile\AppData\Local\Microsoft\WindowsApps",
    "$myenvDir\scripts",
    "$myenvDir\scripts\nightpad",
    'C:\Program Files\dotnet',
    "$userProfile\development\flutter\bin",
    "$userProfile\development\jdk-17.0.19+10\bin",
    "$userProfile\AppData\Local\Android\Sdk\cmdline-tools\latest\bin",
    "$userProfile\AppData\Local\Android\Sdk\platform-tools",
    "$userProfile\AppData\Local\Android\Sdk\emulator",
    'C:\Windows\System32\WindowsPowerShell\v1.0',
    "$userProfile\development\kotlin\bin",
    "$userProfile\development\msys64\ucrt64\bin",
    "$userProfile\development\php",
    "$userProfile\AppData\Roaming\Composer\vendor\bin",
    "$userProfile\development\nodejs",
    "$userProfile\AppData\Roaming\npm"
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