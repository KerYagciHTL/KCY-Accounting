@echo off
title License Server Build Script
color 0A

echo ================================================
echo         License Server Build Script
echo ================================================
echo.

:: Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK is not installed or not in PATH!
    echo Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [INFO] .NET SDK Version:
dotnet --version
echo.

:: Create project if it doesn't exist
if not exist "LicenseServer.csproj" (
    echo [INFO] Creating new console project...
    dotnet new console -n LicenseServer -f net8.0
    if errorlevel 1 (
        echo [ERROR] Failed to create project!
        pause
        exit /b 1
    )
    echo [SUCCESS] Project created successfully!
    echo.
)

:: Add required NuGet packages
echo [INFO] Adding required NuGet packages...
dotnet add package Raylib-cs --version 5.0.0
if errorlevel 1 (
    echo [ERROR] Failed to add Raylib-cs package!
    pause
    exit /b 1
)

dotnet add package System.Text.Json --version 8.0.0
if errorlevel 1 (
    echo [ERROR] Failed to add System.Text.Json package!
    pause
    exit /b 1
)

dotnet add package DotNetEnv --version 2.4.0
if errorlevel 1 (
    echo [ERROR] Failed to add DotNetEnv!
    pause
    exit /b 1
)

echo [SUCCESS] NuGet packages added successfully!
echo.

:: Restore packages
echo [INFO] Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo [ERROR] Failed to restore packages!
    pause
    exit /b 1
)
echo [SUCCESS] Packages restored successfully!
echo.

:: Build the project
echo [INFO] Building project in Release mode...
dotnet build --configuration Release --no-restore
if errorlevel 1 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)
echo [SUCCESS] Build completed successfully!
echo.

:: Publish self-contained executable
echo [INFO] Publishing self-contained executable for Windows x64...
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output "./publish" -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 (
    echo [ERROR] Publish failed!
    pause
    exit /b 1
)
echo [SUCCESS] Self-contained executable published to ./publish/
echo.

:: Copy config files to publish directory
echo [INFO] Copying configuration files...
if exist "config.json" (
    copy "config.json" ".\publish\" >nul
    echo [SUCCESS] config.json copied to publish directory
)

if exist "licenses.json" (
    copy "licenses.json" ".\publish\" >nul
    echo [SUCCESS] licenses.json copied to publish directory
) else (
    echo [INFO] licenses.json not found - will be created on first run
)

cls

echo.
echo ================================================
echo                BUILD COMPLETE!
echo ================================================
echo.
echo Executable location: .\publish\LicenseServer.exe
echo.
echo Files in publish directory:
dir ".\publish\" /B
echo.
echo [INFO] You can now run the application by executing:
echo        .\publish\LicenseServer.exe
echo.
echo [INFO] The application requires the following files:
echo        - config.json (configuration)
echo        - licenses.json (license database)
echo.
