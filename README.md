# mlsvc-manager

Windows ML サービスを manifest 駆動で一元管理する C# CLI ツール。

## 概要

WinSW を実行器として利用し、YAML manifest からサービス定義を生成・管理します。
現在の第1対象は **ACE-Step** (音楽生成 FastAPI サーバー) です。

## アーキテクチャ

```
manifests/*.yaml  →  mlsvc render  →  WinSW XML + exe  →  Windows Service
                     mlsvc install/start/stop/status/health
```

- **manifest** (`manifests/`): 人間が編集する真実のサービス定義
- **WinSW XML** (`C:\svc\services\`): manifest から生成されるデプロイ生成物
- **CLI** (`mlsvc`): manifest を読み、WinSW を制御するインターフェース

## ディレクトリ構成

```
mlsvc-manager/          ← このリポジトリ (Git 管理)
  src/Mlsvc.Cli/        ← C# CLI 本体
  manifests/             ← サービス定義 (YAML)
  templates/             ← WinSW XML テンプレート
  scripts/               ← セットアップスクリプト
  docs/                  ← ドキュメント

C:\svc\                  ← 実運用デプロイ先 (Git 管理外)
  runtimes/              ← Python ランタイム・venv
  services/              ← WinSW exe + XML + ログ
  state/                 ← 状態ファイル
```

## クイックスタート

```powershell
# 事前準備: WinSW をダウンロード
.\scripts\bootstrap.ps1

# CLI をビルド & ヘルプ表示
dotnet run --project src\Mlsvc.Cli -- --help

# サービスを管理
dotnet run --project src\Mlsvc.Cli -- render acestep
dotnet run --project src\Mlsvc.Cli -- install acestep
dotnet run --project src\Mlsvc.Cli -- start acestep
dotnet run --project src\Mlsvc.Cli -- status acestep
dotnet run --project src\Mlsvc.Cli -- health acestep
```

## 設計方針

- WinSW は実行器、C# CLI は制御面
- Git で管理するのは source と manifest。WinSW XML は生成物
- localhost バインドで始め、Tailscale Serve は後段で追加
