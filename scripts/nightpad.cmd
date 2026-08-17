@echo off
if "%~1"=="" (
    start "" "%~dp0nightpad\NightPad.exe" "%CD%"
) else (
    start "" "%~dp0nightpad\NightPad.exe" %*
)
