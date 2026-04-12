# Development

## Prerequisites

開発機に必要なもの:
- Windows
- `.NET SDK 9`
- PowerShell

現在のローカル target を動かすために必要なもの:
- ACE-Step runtime が `C:\svc\runtimes\acestep\` に準備済みであること
- WinSW 補助ファイルを `.\scripts\bootstrap.ps1` で取得していること

## Commands

CLI:

```powershell
dotnet run --project src\Winsvc.Cli -- --help
dotnet run --project src\Winsvc.Cli -- render acestep
dotnet run --project src\Winsvc.Cli -- install acestep
dotnet run --project src\Winsvc.Cli -- start acestep
dotnet run --project src\Winsvc.Cli -- status acestep
dotnet run --project src\Winsvc.Cli -- health acestep
```

API:

```powershell
dotnet run --project src\Winsvc.Api
curl http://127.0.0.1:8011/services/managed
curl http://127.0.0.1:8011/services/acestep/health
```

## Build Issue

この開発機では、通常の `dotnet build` がコンパイル前に失敗することがあります。

観測しているエラー:
- `MSB4276`
- `Microsoft.NET.SDK.WorkloadAutoImportPropsLocator` を解決できない
- `Microsoft.NET.SDK.WorkloadManifestTargetsLocator` を解決できない

意味:
- `Winsvc.Api` のソースコードのコンパイルエラーではない
- ローカル `.NET SDK` の workload resolver 側の問題

## Workaround

暫定回避:

```powershell
.\scripts\build-api.ps1
```

この script がやっていること:
- `MSBuildEnableWorkloadResolver=false` を付ける
- `.NET 9 SDK` 同梱の `MSBuild.dll` を `dotnet exec` で直接実行する

script を使わない等価コマンド:

```powershell
$env:MSBuildEnableWorkloadResolver = "false"
& "C:\Program Files\dotnet\dotnet.exe" exec "C:\Program Files\dotnet\sdk\9.0.312\MSBuild.dll" `
    .\src\Winsvc.Api\Winsvc.Api.csproj `
    /restore `
    /t:Build `
    /p:Configuration=Debug `
    /v:minimal
```

この workaround は開発機固有です。標準手順として固定する意図ではありません。

## Serve

現行の Serve 例:
- `8443 -> 127.0.0.1:8010` for ACE-Step
- `8444 -> 127.0.0.1:8011` for winsvc-manager API

確認:

```powershell
tailscale serve status
```

注意:
- このホスト自身からの HTTPS 確認は、proxy 設定や Schannel の都合で失敗する場合がある
- Serve の実利用確認は別の tailnet 端末から行う方が確実
