# アーキテクチャ

## コンポーネント構成

```
┌─────────────────┐     ┌──────────────┐     ┌────────────────────┐
│  mlsvc CLI      │────▶│  WinSW       │────▶│  ACE-Step API      │
│  (C# .NET 9)    │     │  (exe + XML) │     │  (FastAPI/uvicorn) │
└─────────────────┘     └──────────────┘     └────────────────────┘
        │                       │
        ▼                       ▼
  manifests/*.yaml      Windows Service Manager
  (真実のソース)          (SCM)
```

## データフロー

1. **manifest** (`manifests/acestep.yaml`) が唯一の設定源
2. `mlsvc render` が manifest を読み、WinSW XML を生成
3. `mlsvc install` が WinSW exe を使ってサービスを登録
4. Windows SCM がサービスのライフサイクル（起動・停止・障害復旧）を管理
5. `mlsvc status/health` が SCM と HTTP エンドポイントを問い合わせ

## パス設計

| 用途 | パス |
|------|------|
| Git リポジトリ | `C:\Users\ryuto\src\mlsvc-manager` |
| ランタイム | `C:\svc\runtimes\<service-id>\` |
| WinSW 実体 | `C:\svc\services\<service-id>\` |
| ログ | `C:\svc\services\<service-id>\logs\` |
| 状態 | `C:\svc\state\<service-id>\` |

## CLI サブコマンド

| コマンド | 説明 |
|----------|------|
| `render <id>` | manifest → WinSW XML 生成 |
| `install <id>` | サービス登録 |
| `uninstall <id>` | サービス削除 |
| `start <id>` | サービス開始 |
| `stop <id>` | サービス停止 |
| `restart <id>` | サービス再起動 |
| `status <id>` | サービス状態表示 |
| `health <id>` | HTTP ヘルスチェック |

## サービス設定

- **startMode**: `delayed-auto` (他の自動サービスの後に起動)
- **onFailure**: `restart` (異常終了時に自動再起動)
- **resetFailure**: `1 hour` (1時間後に障害カウンタリセット)
- **バインド**: `127.0.0.1:8010` (localhost のみ)
