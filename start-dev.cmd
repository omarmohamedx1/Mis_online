@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0start-dev.ps1" %*
set "MIS_EXIT_CODE=%ERRORLEVEL%"
if not "%MIS_EXIT_CODE%"=="0" pause
exit /b %MIS_EXIT_CODE%

