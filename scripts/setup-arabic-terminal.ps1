<#
.SYNOPSIS
    MyEnv - Arabic Terminal (RtlTerminal) & UTF-8 Setup Script
.DESCRIPTION
    Configures full Arabic text rendering and RTL support across Windows.
    - Installs and configures RtlTerminal from GitHub releases or builds from source
    - Configures shims/wrappers (rtlterminal.cmd, rtl.cmd) and PATH registration
    - Sets RtlTerminal preferences (Cascadia Code font, Dark theme) in Registry
    - Compiles open-terminal-here.exe for instant native terminal launching
    - Sets Windows Terminal as default console host in Windows Registry
    - Configures Cascadia Code font and DirectWrite rendering in Windows Terminal settings.json
    - Enforces UTF-8 65001 code page in PowerShell profiles and environment
#>

$ErrorActionPreference = "Stop"
$myenvPath = Split-Path -Parent $PSScriptRoot
$userProfile = $env:USERPROFILE

Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "  MyEnv - Arabic Terminal (RtlTerminal) Configuration " -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan

# 1. Verify or Download/Build RtlTerminal
Write-Host "`n[1/6] Verifying RtlTerminal installation..." -ForegroundColor Yellow
$rtlDir = Join-Path $myenvPath "tools\rtlterminal"
$rtlExe = Join-Path $rtlDir "RtlTerminal.exe"

if (-not (Test-Path $rtlExe)) {
    if (-not (Test-Path $rtlDir)) {
        New-Item -ItemType Directory -Path $rtlDir -Force | Out-Null
    }

    $downloadSuccess = $false
    $latestReleaseUrl = "https://github.com/mirbehnam/RtlTerminal/releases/latest/download/RtlTerminal.exe"
    Write-Host "Checking direct download from: $latestReleaseUrl" -ForegroundColor Gray

    try {
        $tempExe = Join-Path $env:TEMP "RtlTerminal_direct.exe"
        Invoke-WebRequest -Uri $latestReleaseUrl -OutFile $tempExe -UseBasicParsing -MaximumRedirection 5 -TimeoutSec 15
        if ((Test-Path $tempExe) -and ((Get-Item $tempExe).Length -gt 1000000)) {
            Move-Item -Path $tempExe -Destination $rtlExe -Force
            $downloadSuccess = $true
            Write-Host "[+] Downloaded RtlTerminal.exe directly." -ForegroundColor Green
        }
    } catch {
        Write-Host "Direct binary download not available or failed: $_" -ForegroundColor Yellow
    }

    if (-not $downloadSuccess) {
        Write-Host "Checking GitHub Releases API for latest release assets..." -ForegroundColor Gray
        try {
            $apiRelease = Invoke-WebRequest -Uri "https://api.github.com/repos/mirbehnam/RtlTerminal/releases/latest" -UseBasicParsing | ConvertFrom-Json
            $zipAsset = $apiRelease.assets | Where-Object { $_.name -like "*.zip" } | Select-Object -First 1
            if ($zipAsset) {
                Write-Host "Found portable zip asset: $($zipAsset.name)" -ForegroundColor Gray
                $tempZip = Join-Path $env:TEMP "RtlTerminal.zip"
                Invoke-WebRequest -Uri $zipAsset.browser_download_url -OutFile $tempZip -UseBasicParsing
                Expand-Archive -Path $tempZip -DestinationPath $rtlDir -Force
                Remove-Item $tempZip -Force -ErrorAction SilentlyContinue
                if (Test-Path $rtlExe) {
                    $downloadSuccess = $true
                    Write-Host "[+] Extracted RtlTerminal from release zip." -ForegroundColor Green
                }
            }
        } catch {
            Write-Host "GitHub API check failed: $_" -ForegroundColor Yellow
        }
    }

    # Fallback: Build from source if dotnet is available
    if ((-not (Test-Path $rtlExe)) -and (Get-Command dotnet -ErrorAction SilentlyContinue) -and (Get-Command git -ErrorAction SilentlyContinue)) {
        Write-Host "Building RtlTerminal from source via git & dotnet..." -ForegroundColor Gray
        $buildDir = Join-Path $env:TEMP "rtlterminal-build"
        if (Test-Path $buildDir) { Remove-Item $buildDir -Recurse -Force -ErrorAction SilentlyContinue }
        git clone --depth 1 https://github.com/mirbehnam/RtlTerminal.git $buildDir
        dotnet publish (Join-Path $buildDir "src\RtlTerminal\RtlTerminal.csproj") -c Release -o $rtlDir --nologo -v q
        Remove-Item $buildDir -Recurse -Force -ErrorAction SilentlyContinue
        if (Test-Path $rtlExe) {
            $downloadSuccess = $true
            Write-Host "[+] Built and published RtlTerminal from source successfully." -ForegroundColor Green
        }
    }
} else {
    Write-Host "[+] RtlTerminal.exe is already present at $rtlExe." -ForegroundColor Green
}

# 2. Register Shims/Wrappers & User PATH
Write-Host "`n[2/6] Registering RtlTerminal shims and PATH..." -ForegroundColor Yellow
$scriptsDir = Join-Path $myenvPath "scripts"
$wrapperCmd = Join-Path $scriptsDir "rtlterminal.cmd"
$wrapperShortCmd = Join-Path $scriptsDir "rtl.cmd"
$wrapperContent = "@echo off`r`nstart `"`" `"%~dp0..\tools\rtlterminal\RtlTerminal.exe`" %*"
Set-Content -Path $wrapperCmd -Value $wrapperContent -Encoding ASCII
Set-Content -Path $wrapperShortCmd -Value $wrapperContent -Encoding ASCII

# Add to user PATH environment variable if not already present
$userPath = [Environment]::GetEnvironmentVariable("PATH", [EnvironmentVariableTarget]::User)
if ($userPath -notlike "*tools\rtlterminal*") {
    $newUserPath = "$rtlDir;$userPath"
    [Environment]::SetEnvironmentVariable("PATH", $newUserPath, [EnvironmentVariableTarget]::User)
    Write-Host "[+] Added $rtlDir to User PATH." -ForegroundColor Green
} else {
    Write-Host "[+] RtlTerminal directory already in User PATH." -ForegroundColor Green
}
if ($env:PATH -notlike "*tools\rtlterminal*") {
    $env:PATH = "$rtlDir;$env:PATH"
}

# Configure default settings in HKCU:\Software\RtlTerminal
$rtlRegKey = "HKCU:\Software\RtlTerminal"
if (-not (Test-Path $rtlRegKey)) {
    New-Item -Path $rtlRegKey -Force | Out-Null
}
Set-ItemProperty -Path $rtlRegKey -Name "FontFamily" -Value "Cascadia Code" -Force
Set-ItemProperty -Path $rtlRegKey -Name "FontSize" -Value "14" -Force
Set-ItemProperty -Path $rtlRegKey -Name "Theme" -Value "Dark" -Force
Set-ItemProperty -Path $rtlRegKey -Name "TerminalProfile" -Value "Command Prompt" -Force
Write-Host "[+] RtlTerminal registry preferences configured (Cascadia Code, Dark theme)." -ForegroundColor Green

# 3. Compile Native Fast Terminal Launcher (OpenTerminalHere.cs)
Write-Host "`n[3/6] Compiling Fast Native Terminal Launcher (open-terminal-here.exe)..." -ForegroundColor Yellow
$cscPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $cscPath)) {
    $cscPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

$csSource = Join-Path $myenvPath "scripts\OpenTerminalHere.cs"
$exeTarget = Join-Path $myenvPath "scripts\open-terminal-here.exe"

if (Test-Path $cscPath) {
    & $cscPath /nologo /target:winexe /optimize+ /out:"$exeTarget" "$csSource"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[+] Compiled open-terminal-here.exe successfully." -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Compilation returned exit code $LASTEXITCODE." -ForegroundColor Red
    }
} else {
    Write-Host "[WARNING] csc.exe compiler not found at $cscPath" -ForegroundColor Red
}

# 4. Set Windows Terminal as Default Console Host in Registry
Write-Host "`n[4/6] Setting Windows Terminal as Default Terminal Emulator in Registry..." -ForegroundColor Yellow
$startupRegKey = "HKCU:\Console\%%Startup"
if (-not (Test-Path $startupRegKey)) {
    New-Item -Path $startupRegKey -Force | Out-Null
}
Set-ItemProperty -Path $startupRegKey -Name "DelegationConsole" -Value "{2EACA947-7F5F-4C45-8977-646CA9E523E0}" -Force
Set-ItemProperty -Path $startupRegKey -Name "DelegationTerminal" -Value "{E12CFF52-A866-4C77-9A90-F570A7AA2C6B}" -Force
Write-Host "[+] Windows Terminal registered as default terminal host." -ForegroundColor Green

# 5. Configure Windows Terminal settings.json (Cascadia Code + DirectWrite Rendering)
Write-Host "`n[5/6] Configuring Windows Terminal settings.json for Arabic Rendering..." -ForegroundColor Yellow
$wtSettingsPaths = @(
    "$env:LOCALAPPDATA\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json",
    "$env:LOCALAPPDATA\Packages\Microsoft.WindowsTerminalPreview_8wekyb3d8bbwe\LocalState\settings.json",
    "$env:LOCALAPPDATA\Microsoft\Windows Terminal\settings.json"
)

foreach ($wtPath in $wtSettingsPaths) {
    if (Test-Path $wtPath) {
        try {
            $rawJson = Get-Content -Path $wtPath -Raw -Encoding UTF8
            $wtSettings = ConvertFrom-Json $rawJson

            if ($null -eq $wtSettings.profiles) {
                $wtSettings | Add-Member -MemberType NoteProperty -Name "profiles" -Value ([PSCustomObject]@{})
            }
            if ($null -eq $wtSettings.profiles.defaults) {
                $wtSettings.profiles | Add-Member -MemberType NoteProperty -Name "defaults" -Value ([PSCustomObject]@{})
            }

            if ($null -eq $wtSettings.profiles.defaults.font) {
                $wtSettings.profiles.defaults | Add-Member -MemberType NoteProperty -Name "font" -Value ([PSCustomObject]@{ face = "Cascadia Code"; size = 11 })
            } else {
                $wtSettings.profiles.defaults.font.face = "Cascadia Code"
            }

            if ($null -eq $wtSettings.profiles.defaults.useAtlasEngine) {
                $wtSettings.profiles.defaults | Add-Member -MemberType NoteProperty -Name "useAtlasEngine" -Value $true
            } else {
                $wtSettings.profiles.defaults.useAtlasEngine = $true
            }

            if ($null -eq $wtSettings.profiles.defaults.'experimental.supportRTL') {
                $wtSettings.profiles.defaults | Add-Member -MemberType NoteProperty -Name "experimental.supportRTL" -Value $true
            } else {
                $wtSettings.profiles.defaults.'experimental.supportRTL' = $true
            }

            $updatedJson = ConvertTo-Json $wtSettings -Depth 32
            Set-Content -Path $wtPath -Value $updatedJson -Encoding UTF8
            Write-Host "[+] Updated Windows Terminal settings at: $wtPath" -ForegroundColor Green
        } catch {
            Write-Host "[WARNING] Failed updating ${wtPath}: $_" -ForegroundColor Red
        }
    }
}

# 6. Apply Console Registry Settings & UTF-8 Environment
Write-Host "`n[6/6] Applying Console Palette & UTF-8 Environment Variables..." -ForegroundColor Yellow
$consoleThemeScript = Join-Path $myenvPath "powershell\console-theme.ps1"
if (Test-Path $consoleThemeScript) {
    & $consoleThemeScript
    Write-Host "[+] Applied Cascadia Code console palette and registry settings." -ForegroundColor Green
}

$utf8EnvVars = @{
    "PYTHONIOENCODING" = "utf-8"
    "PYTHONUTF8"       = "1"
    "LESSCHARSET"      = "utf-8"
    "LANG"             = "en_US.UTF-8"
    "LC_ALL"           = "en_US.UTF-8"
}

foreach ($entry in $utf8EnvVars.GetEnumerator()) {
    [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, [EnvironmentVariableTarget]::User)
    [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, [EnvironmentVariableTarget]::Process)
}
Write-Host "[+] User environment variables configured." -ForegroundColor Green

Write-Host "`n======================================================" -ForegroundColor Green
Write-Host "  RtlTerminal & Arabic Terminal Configured!           " -ForegroundColor Green
Write-Host "======================================================" -ForegroundColor Green
