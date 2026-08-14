@echo off
:: Enable UTF-8 Code Page for Full Arabic & Unicode Support
chcp 65001 >nul
set "PYTHONIOENCODING=utf-8"
set "PYTHONUTF8=1"
set "LESSCHARSET=utf-8"
set "LANG=en_US.UTF-8"
set "LC_ALL=en_US.UTF-8"

:: MyEnv - CMD Environment Initialization Script
:: DOSKEY Aliases & Macros for CMD

doskey cd=cd /d $* $T ls
doskey chdir=cd /d $* $T ls
doskey ls=dir /b $1
doskey ll=dir $1
doskey la=dir /a $1
doskey clear=cls
doskey croot=cd /d "%USERPROFILE%" $T ls
doskey gs=git status $1
doskey ga=git add $1
doskey gc=git commit -m $1
doskey gp=git push $1
doskey gl=git log -n 10 $1
doskey sudo="%USERPROFILE%\Documents\myenv\scripts\sudo.cmd" $1 $2 $3 $4 $5 $6 $7 $8 $9
doskey cb="%USERPROFILE%\Documents\myenv\scripts\cb.cmd" $*
doskey c="%USERPROFILE%\Documents\myenv\scripts\cb.cmd" $*
doskey docs=powershell -NoProfile -ExecutionPolicy Bypass -File "%USERPROFILE%\Documents\myenv\scripts\docs.ps1" $*

:: Prompt Styling
prompt $E[36m$T[0..5]$E[0m $E[32m%USERNAME%@%COMPUTERNAME%$E[0m $E[34m$P$E[0m $E[35m$+$E[0m$G 

:: Add Clink and MyEnv Tools (fzf) to PATH & Inject Clink for History Auto-Suggestions
set "PATH=%USERPROFILE%\Documents\myenv\tools\fzf;%LOCALAPPDATA%\clink\bin;%PATH%"
if exist "%LOCALAPPDATA%\clink\bin\clink.exe" (
    "%LOCALAPPDATA%\clink\bin\clink.exe" inject --quiet
) else (
    where clink >nul 2>&1 && clink inject --quiet
)

