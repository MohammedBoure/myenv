@echo off
:: MyEnv - CMD Sudo Helper (Runs command or opens CMD/RtlTerminal as Administrator in current working directory)
set "CURRENT_DIR=%CD%"
set "RTL_EXE=%USERPROFILE%\Documents\myenv\tools\rtlterminal\RtlTerminal.exe"

if "%~1"=="" (
    if /i "%TERM_PROGRAM%"=="RtlTerminal" if exist "%RTL_EXE%" (
        powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process '%RTL_EXE%' -WorkingDirectory '%CURRENT_DIR%' -ArgumentList '"%CURRENT_DIR%"' -Verb RunAs"
        goto :eof
    )
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -WorkingDirectory '%CURRENT_DIR%' -ArgumentList '/k cd /d "%CURRENT_DIR%"' -Verb RunAs"
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process powershell -WorkingDirectory '%CURRENT_DIR%' -Verb RunAs -ArgumentList '-NoProfile -Command %*'"
)
