@echo off
:: MyEnv - Run command, display output in Terminal, and copy output to Clipboard
if "%~1"=="" (
    echo Usage: cb ^<command^>
    echo Example: cb ipconfig
    echo Example: cb git status
    goto :eof
)

powershell -NoProfile -ExecutionPolicy Bypass -Command "$cmd = '%*'; $out = Invoke-Expression $cmd; if ($null -ne $out) { Write-Host ($out | Out-String) -NoNewline; ($out | Out-String).TrimEnd() | Set-Clipboard }"
