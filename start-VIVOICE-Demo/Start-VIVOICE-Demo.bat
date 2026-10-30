@echo off
REM Purpose: One-click Windows entry point for the tracked VIVOICE local demo and verified Cloudflare Quick Tunnel.
REM Responsibility: Delegates to the safe shared launcher, opens a verified new public URL, and keeps this window visible for the team.
setlocal
set "LAUNCHER=%~dp0..\tools\Start-VIVOICE-Demo.ps1"

if not exist "%LAUNCHER%" (
  echo [VIVOICE] Shared launcher was not found. Run this file from a complete VIVOICE repository checkout.
  pause
  exit /b 1
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%LAUNCHER%" -OpenBrowser
set "EXIT_CODE=%ERRORLEVEL%"
echo.
if "%EXIT_CODE%"=="0" (
  echo [VIVOICE] Demo launcher completed. Keep the web and cloudflared processes running while sharing the URL.
) else (
  echo [VIVOICE] Demo launcher failed. Read the message above; no database or Docker data was changed by this launcher.
)
pause
exit /b %EXIT_CODE%
