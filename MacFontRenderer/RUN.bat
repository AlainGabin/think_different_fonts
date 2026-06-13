@echo off
REM MacFontRenderer - Quick Launch Batch
REM This batch file runs the application with admin privileges

setlocal enabledelayedexpansion

set "EXE_PATH=%~dp0bin\Release\net8.0-windows\MacFontRenderer.exe"

if not exist "!EXE_PATH!" (
    color 0C
    echo.
    echo  ERROR: Executable not found at:
    echo  !EXE_PATH!
    echo.
    echo  Please build the project first:
    echo  dotnet build -c Release
    echo.
    pause
    exit /b 1
)

REM Check for admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    color 0E
    echo.
    echo  MacFontRenderer requires Administrator privileges.
    echo.
    powershell -Command "Start-Process '!EXE_PATH!' -Verb RunAs"
    exit /b
)

REM Run as admin
color 0A
echo.
echo  Launching MacFontRenderer...
echo.
"!EXE_PATH!"
