# Development

## 概要

このドキュメントは `winsvc-manager` の保守者・コントリビューター向けです。
ビルド、テスト、CI、リリースの手順を一つにまとめています。

## 前提条件

- Windows
- .NET SDK 10.0.201（`global.json` で固定。別バージョンが入っていてもこのリポジトリでは 10.0.201 が使われる）
- PowerShell
- （ローカル実行時）WinSW 補助ファイルを `scripts/bootstrap.ps1` で取得済みであること

確認コマンド:

```powershell
dotnet --version
# 10.0.201 が表示されることを確認
```

## ビルドとテスト

ソリューションのビルド:

```powershell
dotnet restore winsvc-manager.sln
dotnet build winsvc-manager.sln --configuration Release -m:1
```

テストの実行:

```powershell
dotnet test winsvc-manager.sln --configuration Release -m:1
```

ソリューション構成:

| プロジェクト | 内容 |
|---|---|
| `src/Winsvc.Cli` | CLI エントリポイント |
| `src/Winsvc.Hosting` | API エンドポイント・DI |
| `src/Winsvc.Core` | ビジネスロジックの抽象・検証 |
| `src/Winsvc.Contracts` | 共通データ型 |
| `src/Winsvc.Infrastructure` | 具体実装 |
| `tests/Winsvc.Core.Tests` | 単体テスト |

## ローカルでの実行

bootstrap（初回のみ）:

```powershell
.\scripts\bootstrap.ps1
# tools/winsw/WinSW.exe をダウンロード
```

CLI の実行:

```powershell
dotnet run --project src\Winsvc.Cli -- --help
dotnet run --project src\Winsvc.Cli -- render <service-id>
dotnet run --project src\Winsvc.Cli -- list managed
```

`winsvc.bat` を使う方法:

```powershell
.\winsvc.bat --help
.\winsvc.bat render sample-service
```

`winsvc.bat` は `WINSVC_MANIFEST_DIR` を自動設定し、`dotnet run` をラップします。

API の起動:

```powershell
dotnet run --project src\Winsvc.Cli -- api serve
curl http://127.0.0.1:8011/services/managed
```

manifest ディレクトリの変更:

```powershell
$env:WINSVC_MANIFEST_DIR = "D:\svc\manifests"
dotnet run --project src\Winsvc.Cli -- list managed
```

API バインドの変更:

```powershell
$env:Winsvc__Api__Urls = "http://localhost:9011"
dotnet run --project src\Winsvc.Cli -- api serve --urls http://localhost:9011
```

## CI

GitHub Actions で `main` への push と PR ごとに実行されます。

- ワークフローファイル: `.github/workflows/ci.yml`
- ランナー: `windows-latest`

ステップ:

1. コードのチェックアウト
2. .NET SDK のセットアップ（`global.json` ベース）
3. `dotnet restore`
4. `dotnet build --configuration Release --no-restore -m:1`
5. `dotnet test --configuration Release --no-build -m:1`

## リリース

タグ `v*` の push でリリースワークフローが発火します。手動実行（`workflow_dispatch`）も可能です。

- ワークフローファイル: `.github/workflows/release.yml`

対象プラットフォーム:

- `win-x64`
- `win-arm64`

生成物:

| ファイル | 説明 |
|---|---|
| `winsvc-<tag>-win-x64.zip` | x64 向け配布 ZIP |
| `winsvc-<tag>-win-x64.zip.sha256` | SHA256 チェックサム |
| `winsvc-<tag>-win-arm64.zip` | ARM64 向け配布 ZIP |
| `winsvc-<tag>-win-arm64.zip.sha256` | SHA256 チェックサム |

ZIP の中身:

- `winsvc.exe`（自己完結型シングルファイル）
- `winsw.exe`（WinSW バイナリ）
- `appsettings.json`（API 設定）
- `manifests/service.template.yaml`（テンプレート）

リリース手順:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

ローカルでの配布物生成:

```powershell
.\scripts\publish-release.ps1 -Runtime win-x64 -Version v1.0.0
.\scripts\publish-release.ps1 -Runtime win-arm64 -Version v1.0.0
```

## スクリプト

### bootstrap.ps1

初回セットアップ用スクリプトです。

- WinSW バイナリ（`WinSW-net461.exe`）を GitHub からダウンロード
- 配置先: `tools/winsw/WinSW.exe`
- 既に存在する場合はスキップ

### publish-release.ps1

配布物生成用スクリプトです。

パラメータ:

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `Runtime` | はい | なし | `win-x64` または `win-arm64` |
| `Version` | いいえ | `dev` | バージョンタグ |
| `Configuration` | いいえ | `Release` | ビルド構成 |
| `OutputRoot` | いいえ | `artifacts/` | 出力先ディレクトリ |

処理内容:

1. `dotnet publish` で自己完結型バイナリを生成
2. `Winsvc.Cli.exe` を `winsvc.exe` にリネーム
3. WinSW バイナリを同梱
4. manifest テンプレートを同梱
5. ZIP アーカイブを作成
6. SHA256 チェックサムを生成

## 補足

- 配布 ZIP の実行ファイル名は `winsvc.exe` です
- `appsettings.json` は API の既定設定を含みます
- Git タグは `v0.1.0` 形式（semver）を想定しています
- `global.json` の `rollForward: latestPatch` により、パッチバージョンの更新を許容します
