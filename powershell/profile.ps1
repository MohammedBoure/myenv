# myenv - single source of truth for Windows PowerShell
if ($global:MyEnvPowerShellProfileLoaded) { return }
$global:MyEnvPowerShellProfileLoaded = $true

$utf8 = New-Object System.Text.UTF8Encoding($false)
try {
    chcp.com 65001 > $null
    [Console]::InputEncoding = $utf8
    [Console]::OutputEncoding = $utf8
    $OutputEncoding = $utf8
} catch {}

$env:LANG = 'en_US.UTF-8'
$env:LC_ALL = 'en_US.UTF-8'
$env:VISUAL = 'code --wait'
$env:EDITOR = 'code --wait'

# Central development environment paths.
$pathsToAdd = @(
    'C:\Users\moham\AppData\Local\Microsoft\WindowsApps',
    'C:\Program Files\dotnet',
    'C:\Users\moham\development\flutter\bin',
    'C:\Users\moham\development\jdk-17.0.19+10\bin',
    'C:\Users\moham\AppData\Local\Android\Sdk\cmdline-tools\latest\bin',
    'C:\Users\moham\AppData\Local\Android\Sdk\platform-tools',
    'C:\Users\moham\AppData\Local\Android\Sdk\emulator',
    'C:\Windows\System32\WindowsPowerShell\v1.0',
    'C:\Users\moham\development\kotlin\bin',
    'C:\Users\moham\development\msys64\ucrt64\bin',
    'C:\Users\moham\development\php',
    'C:\Users\moham\AppData\Roaming\Composer\vendor\bin',
    'C:\Users\moham\development\nodejs',
    'C:\Users\moham\AppData\Roaming\npm'
)
foreach ($p in $pathsToAdd) {
    if (Test-Path -LiteralPath $p) {
        $existing = @($env:Path -split ';' | Where-Object { $_ -ne '' })
        if (-not ($existing | Where-Object { $_.TrimEnd('\') -ieq $p.TrimEnd('\') })) {
            $env:Path = "$p;$env:Path"
        }
    }
}

$env:DOTNET_ROOT = 'C:\Program Files\dotnet'
$env:JAVA_HOME = 'C:\Users\moham\development\jdk-17.0.19+10'
$env:ANDROID_HOME = 'C:\Users\moham\AppData\Local\Android\Sdk'
$env:ANDROID_SDK_ROOT = 'C:\Users\moham\AppData\Local\Android\Sdk'
$env:KOTLIN_HOME = 'C:\Users\moham\development\kotlin'
$env:MSYS2_ROOT = 'C:\Users\moham\development\msys64'
$env:PHP_HOME = 'C:\Users\moham\development\php'
$env:PHPRC = 'C:\Users\moham\development\php'
$env:COMPOSER_HOME = 'C:\Users\moham\AppData\Roaming\Composer'
$env:NODE_HOME = 'C:\Users\moham\development\nodejs'
$env:NPM_CONFIG_PREFIX = 'C:\Users\moham\AppData\Roaming\npm'

# Keep PowerShell modules under myenv when present.
$myenvModules = 'C:\Users\moham\Documents\myenv\powershell\Modules'
if (Test-Path $myenvModules) {
    $env:PSModulePath = $myenvModules + ';' + $env:PSModulePath
}

$themePath = 'C:\Users\moham\Documents\myenv\powershell\midnight-aurora.ps1'
if (Test-Path $themePath) { . $themePath }

$consoleThemePath = 'C:\Users\moham\Documents\myenv\powershell\console-theme.ps1'
if (Test-Path $consoleThemePath) { . $consoleThemePath }

# Fastfetch is optional and uses the centralized config.
if (-not [Console]::IsInputRedirected -and -not [Console]::IsOutputRedirected) {
    $fastfetch = Get-Command fastfetch -ErrorAction SilentlyContinue
    $fastConfig = 'C:\Users\moham\.config\fastfetch\config.jsonc'
    if ($fastfetch -and (Test-Path $fastConfig)) {
        fastfetch -c $fastConfig
    }
}