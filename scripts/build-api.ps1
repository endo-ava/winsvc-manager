$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$sdkRoot = "C:\Program Files\dotnet\sdk\9.0.312"
$msbuildDll = Join-Path $sdkRoot "MSBuild.dll"
$projectPath = Join-Path $repoRoot "src\Winsvc.Api\Winsvc.Api.csproj"

if (-not (Test-Path $msbuildDll)) {
    throw "MSBuild.dll not found: $msbuildDll"
}

$env:MSBuildEnableWorkloadResolver = "false"

& "C:\Program Files\dotnet\dotnet.exe" exec $msbuildDll `
    $projectPath `
    /restore `
    /t:Build `
    /p:Configuration=Debug `
    /v:minimal
