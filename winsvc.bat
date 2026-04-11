@echo off
dotnet run --project "%~dp0src\Winsvc.Cli\Winsvc.Cli.csproj" -- %*
