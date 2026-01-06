@echo off
setlocal EnableExtensions EnableDelayedExpansion

title Kiosk7 - Uninstall

:: ----------------------------------------------------
:: Admin check
:: ----------------------------------------------------
net session >nul 2>&1 || (
    echo ERROR: Please run this file as Administrator.
    pause
    exit /b 1
)

:: ----------------------------------------------------
:: Base directory = folder this script is running from
:: ----------------------------------------------------
set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%.") do set "BASE_DIR=%%~fI"

set "TASK_NAME=Kiosk7_Watchdog"
set "LOCK_FILE=%BASE_DIR%\watchdog.lock"
set "STOP_FILE=%BASE_DIR%\watchdog.stop"

:: ----------------------------------------------------
:: Detect currently logged-in user (for info/debug)
:: ----------------------------------------------------
for /f "usebackq delims=" %%U in (`powershell -NoProfile -Command "(Get-CimInstance Win32_ComputerSystem).UserName"`) do set "RUN_USER=%%U"

echo ====================================
echo Uninstalling Kiosk7 watchdog
echo Base folder: %BASE_DIR%
if not "%RUN_USER%"=="" echo Logged-in user: %RUN_USER%
echo ====================================

:: ----------------------------------------------------
:: Signal watchdog to stop
:: ----------------------------------------------------
echo stop > "%STOP_FILE%"

:: ----------------------------------------------------
:: Remove scheduled task
:: ----------------------------------------------------
schtasks /delete /tn "%TASK_NAME%" /f >nul 2>&1

:: ----------------------------------------------------
:: Kill watchdog-related processes only
:: ----------------------------------------------------
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -and (($_.CommandLine -like '*watchdog.vbs*') -or ($_.CommandLine -like '*watchdog.bat*')) } | Invoke-CimMethod -MethodName Terminate | Out-Null" ^
  >nul 2>&1

timeout /t 2 /nobreak >nul

:: ----------------------------------------------------
:: Cleanup runtime files
:: ----------------------------------------------------
del /f /q "%LOCK_FILE%" >nul 2>&1
del /f /q "%STOP_FILE%" >nul 2>&1

:: ----------------------------------------------------
:: Restore Windows touch keyboard auto-invoke (TabletTip)
:: ----------------------------------------------------
echo.
echo Restoring Windows touch keyboard auto-invoke (TabletTip)...
REG ADD "HKLM\SOFTWARE\Microsoft\TabletTip\1.7" ^
 /v EnableDesktopModeAutoInvoke ^
 /t REG_DWORD ^
 /d 1 ^
 /f >nul 2>&1

:: Optional quick verify
for /f "tokens=2,*" %%A in ('REG QUERY "HKLM\SOFTWARE\Microsoft\TabletTip\1.7" /v EnableDesktopModeAutoInvoke 2^>nul ^| find /i "EnableDesktopModeAutoInvoke"') do (
    echo TabletTip setting now: EnableDesktopModeAutoInvoke=%%B
)

:: Optional verify task removal
schtasks /query /tn "%TASK_NAME%" >nul 2>&1
if %ERRORLEVEL%==0 (
    echo WARNING: Task "%TASK_NAME%" still appears to exist.
) else (
    echo Task "%TASK_NAME%" removed.
)

echo.
echo UNINSTALL COMPLETE
echo - Watchdog stopped
echo - Scheduled task removed
echo - Touch keyboard restored
pause

endlocal
