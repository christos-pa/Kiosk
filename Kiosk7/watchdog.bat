@echo off
setlocal EnableExtensions EnableDelayedExpansion

:: ----------------------------------------------------
:: Base folder = where this .bat lives (portable)
:: ----------------------------------------------------
set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%.") do set "APP_DIR=%%~fI"

set "APP_NAME=Kiosk7.exe"
set "APP_PATH=%APP_DIR%\%APP_NAME%"
set "LOCK_FILE=%APP_DIR%\watchdog.lock"
set "STOP_FILE=%APP_DIR%\watchdog.stop"

:: ----------------------------------------------------
:: Exit immediately if uninstall requested
:: ----------------------------------------------------
if exist "%STOP_FILE%" exit /b

:: ----------------------------------------------------
:: Ensure fresh lock per session
:: ----------------------------------------------------
if exist "%LOCK_FILE%" del /f /q "%LOCK_FILE%" >nul 2>&1
echo running > "%LOCK_FILE%"

:loop
:: If uninstall requested, exit cleanly
if exist "%STOP_FILE%" exit /b

:: If app is not running, start it
tasklist /FI "IMAGENAME eq %APP_NAME%" | find /I "%APP_NAME%" >nul
if errorlevel 1 (
    if exist "%APP_PATH%" (
        pushd "%APP_DIR%"
        start "" "%APP_PATH%"
        popd
    )
)

timeout /t 30 /nobreak >nul
goto loop
