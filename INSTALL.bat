@echo off
setlocal EnableExtensions EnableDelayedExpansion

title Kiosk7 - Install

net session >nul 2>&1
if errorlevel 1 (
    echo ERROR: Please run this file as Administrator.
    pause
    exit /b 1
)

echo.
echo Disabling Windows touch keyboard auto-invoke (TabletTip)...
REG ADD "HKLM\SOFTWARE\Microsoft\TabletTip\1.7" ^
 /v EnableDesktopModeAutoInvoke ^
 /t REG_DWORD ^
 /d 0 ^
 /f >nul 2>&1

for /f "tokens=2,*" %%A in ('REG QUERY "HKLM\SOFTWARE\Microsoft\TabletTip\1.7" /v EnableDesktopModeAutoInvoke 2^>nul ^| find /i "EnableDesktopModeAutoInvoke"') do (
    echo TabletTip setting applied: EnableDesktopModeAutoInvoke=%%B
)

set "APP_NAME=Kiosk7"
for /f "usebackq delims=" %%U in (`powershell -NoProfile -Command "(Get-CimInstance Win32_ComputerSystem).UserName"`) do set "RUN_USER=%%U"

if "%RUN_USER%"=="" (
    echo ERROR: Could not detect logged-in user.
    pause
    exit /b 1
)

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%.") do set "BASE_DIR=%%~fI"

set "VBS_PATH=%BASE_DIR%\watchdog.vbs"
set "TASK_NAME=Kiosk7_Watchdog"
set "STOP_FILE=%BASE_DIR%\watchdog.stop"

echo ====================================
echo Installing %APP_NAME% kiosk watchdog
echo Base folder: %BASE_DIR%
echo Scheduled task user: %RUN_USER%
echo ====================================

del /f /q "%STOP_FILE%" >nul 2>&1

if not exist "%BASE_DIR%\watchdog.bat" (
    echo ERROR: watchdog.bat not found in %BASE_DIR%
    pause
    exit /b 1
)

if not exist "%VBS_PATH%" (
    echo ERROR: watchdog.vbs not found in %BASE_DIR%
    pause
    exit /b 1
)

schtasks /delete /tn "%TASK_NAME%" /f >nul 2>&1

schtasks /create ^
 /tn "%TASK_NAME%" ^
 /tr "wscript.exe \"%VBS_PATH%\"" ^
 /sc onlogon ^
 /ru "%RUN_USER%" ^
 /it ^
 /rl highest ^
 /f

if errorlevel 1 (
    echo ERROR: Failed to create scheduled task
    pause
    exit /b 1
)

start "" wscript.exe "%VBS_PATH%"

echo.
echo INSTALL COMPLETE
echo - Watchdog starts immediately
echo - Watchdog starts at %RUN_USER% logon
echo - Windows touch keyboard auto-invoke disabled (reboot recommended)
pause

endlocal
