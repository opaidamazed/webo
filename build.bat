@echo off
chcp 65001 >nul
where dotnet >nul 2>nul || (echo Install .NET 8 SDK first: winget install Microsoft.DotNet.SDK.8 & pause & exit /b 1)
dotnet publish WeboIconSpike.csproj -c Release -r win-x64 -p:Platform=x64 -o out
if errorlevel 1 (echo BUILD FAILED - send me the text above & pause & exit /b 1)
echo Done: out\WeboIconSpike.exe
pause
