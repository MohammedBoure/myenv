

# >>> dev-env >>>
$pathsToAdd = @(
    'C:\Users\moham\AppData\Local\Microsoft\WindowsApps',
    'C:\Program Files\dotnet',
    'C:\Users\moham\development\flutter\bin',
    'C:\Users\moham\development\jdk-17.0.19+10\bin',
    'C:\Users\moham\AppData\Local\Android\Sdk\cmdline-tools\latest\bin',
    'C:\Users\moham\AppData\Local\Android\Sdk\platform-tools',
    'C:\Users\moham\AppData\Local\Android\Sdk\emulator',
    'C:\Windows\System32\WindowsPowerShell\v1.0'
)
for ($i = $pathsToAdd.Count - 1; $i -ge 0; $i--) {
    $p = $pathsToAdd[$i]
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
# <<< dev-env <<<
# kotlin-env
$env:KOTLIN_HOME = 'C:\Users\moham\development\kotlin'
$kotlinBin = Join-Path $env:KOTLIN_HOME 'bin'
foreach ($p in @($kotlinBin, 'C:\Users\moham\AppData\Local\Microsoft\WindowsApps')) {
    if ($env:Path -notlike "*$p*") { $env:Path = "$p;$env:Path" }
}
# /kotlin-env
# mingw-env
$env:MSYS2_ROOT = 'C:\Users\moham\development\msys64'
$mingwBin = 'C:\Users\moham\development\msys64\ucrt64\bin'
foreach ($p in @($mingwBin, 'C:\Users\moham\AppData\Local\Microsoft\WindowsApps')) {
    if ($env:Path -notlike "*$p*") { $env:Path = "$p;$env:Path" }
}
# /mingw-env
# php-env
$env:PHP_HOME = 'C:\Users\moham\development\php'
$env:PHPRC = 'C:\Users\moham\development\php'
$env:COMPOSER_HOME = 'C:\Users\moham\AppData\Roaming\Composer'
foreach ($p in @('C:\Users\moham\development\php', 'C:\Users\moham\AppData\Roaming\Composer\vendor\bin', 'C:\Users\moham\AppData\Local\Microsoft\WindowsApps')) {
    if ($env:Path -notlike "*$p*") { $env:Path = "$p;$env:Path" }
}
# /php-env
# node-env
$env:NODE_HOME = 'C:\Users\moham\development\nodejs'
$env:NPM_CONFIG_PREFIX = 'C:\Users\moham\AppData\Roaming\npm'
foreach ($p in @('C:\Users\moham\development\nodejs', 'C:\Users\moham\AppData\Roaming\npm', 'C:\Users\moham\AppData\Local\Microsoft\WindowsApps')) {
    if ($env:Path -notlike "*$p*") { $env:Path = "$p;$env:Path" }
}
# /node-env

# Minimal profile: UTF‑8 + Oh My Posh (if installed) + Fastfetch with explicit config path
try {
    [Console]::InputEncoding  = [System.Text.Encoding]::UTF8
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $OutputEncoding = [System.Text.UTF8Encoding]::new($false)
    chcp 65001 > $null
} catch {}

Clear-Host

# Force Fastfetch to use YOUR config every time (bypass path confusion)
if (Get-Command fastfetch -ErrorAction SilentlyContinue) {
    fastfetch -c "C:/Users/moham/.config/fastfetch/config.jsonc"
}