@echo off
:: MyEnv - CMD Environment Initialization Script
:: DOSKEY Aliases & Macros for CMD

doskey ls=dir /b $1
doskey ll=dir $1
doskey la=dir /a $1
doskey clear=cls
doskey croot=cd /d "%USERPROFILE%"
doskey gs=git status $1
doskey ga=git add $1
doskey gc=git commit -m $1
doskey gp=git push $1
doskey gl=git log -n 10 $1
doskey sudo=C:\Users\moham\Documents\myenv\scripts\sudo.cmd $1 $2 $3 $4 $5 $6 $7 $8 $9

:: Prompt Styling
prompt $E[36m$T[0..5]$E[0m $E[32m%USERNAME%@%COMPUTERNAME%$E[0m $E[34m$P$E[0m $E[35m$+$E[0m$G 

:: Add Clink to PATH & Inject Clink for History Auto-Suggestions & Popup Menus
set "PATH=%LOCALAPPDATA%\clink\bin;%PATH%"
if exist "%LOCALAPPDATA%\clink\bin\clink.exe" (
    "%LOCALAPPDATA%\clink\bin\clink.exe" inject --quiet
) else (
    where clink >nul 2>&1 && clink inject --quiet
)

