@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Install the .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0 then run this file again.
  goto failed
)
dotnet --list-runtimes | findstr /c:"Microsoft.WindowsDesktop.App 8." >nul
if errorlevel 1 (
  echo Install the .NET 8 Desktop Runtime or .NET 8 SDK, then run this file again.
  echo https://dotnet.microsoft.com/download/dotnet/8.0
  goto failed
)
echo Building PRG272. Details are in build.log.
dotnet build PRG272.sln --configuration Release --nologo --verbosity minimal >build.log 2>&1
if errorlevel 1 (
  type build.log
  goto failed
)
if /i "%~1"=="--check" exit /b 0
echo Starting animal records...
"%~dp0src\WildlifeRehabilitation\bin\Release\net8.0-windows\WildlifeRehabilitation.exe"
if errorlevel 1 (
  echo The application could not start. Check the .NET 8 Desktop Runtime and run from a writable folder.
  goto failed
)
exit /b 0
:failed
if /i not "%~1"=="--check" pause
exit /b 1
