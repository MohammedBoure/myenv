# Download and install Clink portable zip
$ErrorActionPreference = 'Stop'
$zipUrl = 'https://github.com/chrisant996/clink/releases/download/v1.9.30/clink.1.9.30.85f10f.zip'
$clinkBaseDir = "$env:LOCALAPPDATA\clink"
$clinkBinDir = "$clinkBaseDir\bin"
$zipFile = "$clinkBaseDir\clink.zip"

if (-not (Test-Path $clinkBinDir)) {
    New-Item -ItemType Directory -Path $clinkBinDir -Force | Out-Null
}

Write-Host "Downloading Clink portable zip from GitHub..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $zipUrl -OutFile $zipFile

Write-Host "Extracting Clink binaries..." -ForegroundColor Cyan
Expand-Archive -Path $zipFile -DestinationPath $clinkBinDir -Force
Remove-Item -Path $zipFile -Force

$clinkExe = Get-ChildItem -Path $clinkBinDir -Filter "clink_x64.exe" -Recurse | Select-Object -First 1
if ($clinkExe) {
    $binDir = $clinkExe.DirectoryName
    Write-Host "Clink binary located at: $binDir" -ForegroundColor Green

    # Create clink.exe copy if missing
    $clinkAlias = Join-Path $binDir "clink.exe"
    if (-not (Test-Path $clinkAlias)) {
        Copy-Item -Path $clinkExe.FullName -Destination $clinkAlias -Force
    }

    # Add to User PATH
    $userPath = [System.Environment]::GetEnvironmentVariable("Path", "User")
    if ($userPath -notlike "*$binDir*") {
        [System.Environment]::SetEnvironmentVariable("Path", "$userPath;$binDir", "User")
        $env:Path = "$env:Path;$binDir"
        Write-Host "Added $binDir to User PATH." -ForegroundColor Green
    }

    # Register Clink AutoRun
    Write-Host "Registering Clink AutoRun..." -ForegroundColor Cyan
    & "$clinkAlias" autorun install
    Write-Host "Clink AutoRun registered successfully!" -ForegroundColor Green
} else {
    Write-Host "Error: Could not locate clink_x64.exe after extraction." -ForegroundColor Red
}
