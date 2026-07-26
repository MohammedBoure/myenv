@echo off
:: MyEnv - CMD Sudo Helper (Runs command or opens CMD as Administrator in current working directory)
set "CURRENT_DIR=%CD%"
if "%~1"=="" (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -WorkingDirectory '%CURRENT_DIR%' -ArgumentList '/k cd /d \"%CURRENT_DIR%\"' -Verb RunAs"
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process powershell -WorkingDirectory '%CURRENT_DIR%' -Verb RunAs -ArgumentList '-NoProfile -Command %*'"
)
