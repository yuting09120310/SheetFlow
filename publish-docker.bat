@echo off
setlocal enabledelayedexpansion

REM ===============================
REM Docker Publish Script for StoreFlow
REM ===============================

REM Docker Hub 帳號與 Image 名稱
set DOCKER_USER=yuting09120310
set IMAGE_NAME=sheetflow
set REPO=%DOCKER_USER%/%IMAGE_NAME%

REM 版本檔案
set VERSION_FILE=version.txt

REM 檢查 Docker 是否可用
docker version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker is not running or not installed.
    echo Please start Docker Desktop first.
    pause
    exit /b 1
)

REM 如果 version.txt 不存在，建立初始版本
if not exist %VERSION_FILE% (
    echo 1.0.0>%VERSION_FILE%
)

REM 讀取目前版本
set /p VERSION=<%VERSION_FILE%

REM 拆版本號：major.minor.patch
for /f "tokens=1,2,3 delims=." %%a in ("%VERSION%") do (
    set MAJOR=%%a
    set MINOR=%%b
    set PATCH=%%c
)

REM patch + 1
set /a PATCH+=1
set NEW_VERSION=%MAJOR%.%MINOR%.%PATCH%

REM 寫回 version.txt
echo %NEW_VERSION%>%VERSION_FILE%

echo.
echo ===============================
echo Publishing Docker Image
echo ===============================
echo Repository : %REPO%
echo Version    : %NEW_VERSION%
echo Tags       : %NEW_VERSION%, latest
echo ===============================
echo.

REM Build image
echo [1/3] Building Docker image...
docker build -t %REPO%:%NEW_VERSION% -t %REPO%:latest .
if errorlevel 1 (
    echo.
    echo [ERROR] Docker build failed.
    pause
    exit /b 1
)

REM Push version tag
echo.
echo [2/3] Pushing version tag: %NEW_VERSION%...
docker push %REPO%:%NEW_VERSION%
if errorlevel 1 (
    echo.
    echo [ERROR] Docker push version tag failed.
    echo Please check Docker Hub login, repository name, or token permission.
    pause
    exit /b 1
)

REM Push latest tag
echo.
echo [3/3] Pushing latest tag...
docker push %REPO%:latest
if errorlevel 1 (
    echo.
    echo [ERROR] Docker push latest tag failed.
    pause
    exit /b 1
)

echo.
echo ===============================
echo Publish Success!
echo ===============================
echo Image:
echo   %REPO%:%NEW_VERSION%
echo   %REPO%:latest
echo.
echo Next Portainer image:
echo   %REPO%:latest
echo ===============================
echo.

pause
endlocal