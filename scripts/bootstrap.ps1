<#
.SYNOPSIS
    mlsvc-manager の事前準備スクリプト
.DESCRIPTION
    WinSW バイナリをダウンロードし、tools/winsw/ に配置します。
#>

param(
    [string]$WinSwVersion = "v3.0.0-alpha.11",
    [string]$ToolsDir = "$PSScriptRoot\..\tools\winsw"
)

$ErrorActionPreference = "Stop"

# WinSW ダウンロード
$winswExe = Join-Path $ToolsDir "WinSW.exe"

if (Test-Path $winswExe) {
    Write-Host "[bootstrap] WinSW already exists at $winswExe" -ForegroundColor Green
} else {
    $url = "https://github.com/winsw/winsw/releases/download/$WinSwVersion/WinSW-net462.exe"
    Write-Host "[bootstrap] Downloading WinSW $WinSwVersion ..." -ForegroundColor Cyan

    New-Item -ItemType Directory -Path $ToolsDir -Force | Out-Null

    try {
        Invoke-WebRequest -Uri $url -OutFile $winswExe -UseBasicParsing
        Write-Host "[bootstrap] WinSW downloaded to $winswExe" -ForegroundColor Green
    } catch {
        Write-Error "Failed to download WinSW: $_"
        exit 1
    }
}

# 確認
Write-Host ""
Write-Host "[bootstrap] Setup complete!" -ForegroundColor Green
Write-Host "  WinSW: $winswExe"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  dotnet run --project src\Mlsvc.Cli -- --help"
