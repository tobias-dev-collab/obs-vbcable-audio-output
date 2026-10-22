@echo off
setlocal
set "SCRIPT_DIR=%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath 'powershell.exe' -Verb RunAs -Wait -ArgumentList '-NoProfile -ExecutionPolicy Bypass -NoExit -File \"%SCRIPT_DIR%Uninstall.ps1\"'"
if errorlevel 1 (
    echo.
    echo Abgebrochen oder keine Administratorrechte erhalten.
    pause
    exit /b 1
)
