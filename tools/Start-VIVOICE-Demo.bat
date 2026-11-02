@echo off
REM Purpose: Starts or stops the tracked VIVOICE PowerShell demo launcher without embedding credentials.
REM Responsibility: Preserves the PowerShell launcher exit code for a double-click or terminal invocation.
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Start-VIVOICE-Demo.ps1" %*
exit /b %ERRORLEVEL%
