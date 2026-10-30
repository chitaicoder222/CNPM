@echo off
REM Purpose: Stops only the VIVOICE web host and Cloudflare tunnel previously recorded as launcher-owned.
REM Responsibility: Delegates ownership checks to the shared launcher and never stops arbitrary Docker, database, or cloudflared processes.
setlocal
set "LAUNCHER=%~dp0..\tools\Start-VIVOICE-Demo.ps1"

if not exist "%LAUNCHER%" (
  echo [VIVOICE] Shared launcher was not found. Run this file from a complete VIVOICE repository checkout.
  pause
  exit /b 1
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%LAUNCHER%" -Stop
set "EXIT_CODE=%ERRORLEVEL%"
pause
exit /b %EXIT_CODE%
