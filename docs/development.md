# Development

## Prerequisites

開発機に必要なもの:
- Windows
- `.NET SDK 10.0.201`
- PowerShell

現在のローカル target を動かすために必要なもの:
- WinSW 補助ファイルを `.\scripts\bootstrap.ps1` で取得していること
- 管理対象サービスに対応する runtime や実行ファイルがホスト側に配置済みであること

## Commands

solution build:

```powershell
dotnet build winsvc-manager.sln
```

CLI:

```powershell
dotnet run --project src\Winsvc.Cli -- --help
dotnet run --project src\Winsvc.Cli -- render <service-id>
dotnet run --project src\Winsvc.Cli -- install <service-id>
dotnet run --project src\Winsvc.Cli -- start <service-id>
dotnet run --project src\Winsvc.Cli -- status <service-id>
dotnet run --project src\Winsvc.Cli -- health <service-id>
```

API:

```powershell
dotnet run --project src\Winsvc.Api
curl http://127.0.0.1:8011/services/managed
curl http://127.0.0.1:8011/services/<service-id>/health
```

## Build Issue

この repository は `global.json` で SDK を固定しています。

確認:

```powershell
dotnet --version
dotnet build winsvc-manager.sln
```

もし別の SDK が既に入っていても、`global.json` によりこの repository では `10.0.201` が選ばれます。

## Serve

Tailscale Serve を使う場合は、managed service 側と winsvc-manager API 側の公開ポートを分けて扱います。

例:
- managed service 側の HTTPS port
- winsvc-manager API 側の HTTPS port

確認:

```powershell
tailscale serve status
```

注意:
- このホスト自身からの HTTPS 確認は、proxy 設定や Schannel の都合で失敗する場合がある
- Serve の実利用確認は別の tailnet 端末から行う方が確実
