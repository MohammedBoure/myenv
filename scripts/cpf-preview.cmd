@echo off
setlocal enabledelayedexpansion

:: Normalize path parameter and convert forward slashes to backslashes
set "TARGET=%~1"
set "TARGET=%TARGET:/=\%"

if "%TARGET%"=="" (
    echo [No file selected]
    exit /b 0
)

if not exist "%TARGET%" (
    if not exist "%TARGET%\*" (
        echo [File or directory not found: %TARGET%]
        exit /b 0
    )
)

:: Directory Preview: list contents
if exist "%TARGET%\*" (
    echo ==========================================================
    echo  Folder: %TARGET%
    echo ==========================================================
    dir /b /o:n /a "%TARGET%" 2>nul
    exit /b 0
)

:: File Preview: print content
type "%TARGET%" 2>nul
