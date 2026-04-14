# API Integration Plan

## Goal

`Winsvc.Api` を独立実行物として配布する構成をやめ、`winsvc` 単体に統合する。
最終的には `winsvc api serve` でローカル API を起動できる状態を目指す。

## Target State

- 配布物は `winsvc.exe` のみ
- CLI 操作用コマンドと API 起動コマンドが同じ実行ファイルに入る
- 既存 API endpoint は可能な限り互換維持
- README は `winsvc` 中心で説明できる構成にする
- release workflow は単一成果物の publish に寄せる

## Main Idea

いきなり `Winsvc.Api` を消すのではなく、まず API 定義と host 構築処理を再利用可能な形へ切り出す。
そのうえで `Winsvc.Cli` から `api serve` を呼べるようにする。
最後に配布・ドキュメント・不要 project の整理を行う。

## Work Plan

### 1. Separate reusable API hosting code

- 新設 `Winsvc.Hosting` project を作成する（`Microsoft.NET.Sdk.Web`）
- `Winsvc.Api` にある endpoint 定義を `Winsvc.Hosting` へ移動する
- `WebApplicationBuilder` の service registration を再利用できる形へ寄せる
- API response record 型を `Winsvc.Contracts` へ移動する
- `winsvc` 側と `Winsvc.Api` 側の両方から同じ API 初期化コードを呼べるようにする

期待する着地点:

- API endpoint 定義が `Winsvc.Hosting` にまとまる
- `Winsvc.Api` は薄い host project になる
- `Winsvc.Cli` からも同じ API を起動できる
- 依存関係: `Contracts ← Core ← Infrastructure ← Hosting ← Api` / `Hosting ← Cli`

### 2. Add `winsvc api serve`

- CLI に `api` コマンドグループを追加する
- `winsvc api serve` で ASP.NET Core host を起動する
- `--urls` と `--manifest-dir` 引数を受け付ける
- CLI 引数が指定された場合のみ `Configuration` を上書きする

最低限ほしい UX:

```powershell
winsvc api serve
winsvc api serve --urls http://localhost:9011
winsvc api serve --manifest-dir ./my-services
```

### 3. Delete `Winsvc.Api` project

- `winsvc api serve` が安定動作を確認したら `Winsvc.Api` を削除する
- 削除判断基準はすべて満たす見込み:
  - API 定義が `Winsvc.Hosting` に統合済み
  - 外部利用者にとって別 exe が不要
  - 開発・デバッグ上のメリットが薄い（`winsvc api serve` で代替可能）
  - CI と release が単純になる

### 4. Update packaging

- release workflow を `winsvc.exe` 単一配布前提へ整理する
- README の配布説明を `winsvc` のみで完結させる
- `publish-release.ps1` の成果物構成が統合後も妥当か確認する
- `appsettings.json` を zip に含めるよう publish script を更新する

確認ポイント:

- `winsvc.exe` に CLI と API 起動機能の両方が入るか
- ZIP 名はそのままでよいか
- `appsettings.json` が配布物に含まれているか

### 5. Update docs

- README の API 起動手順を `winsvc api serve` に変更する
- `docs/release-and-ci.md` を統合後の release 手順に合わせる
- `Commands` 一覧に `api serve` を追加する
- `API` 一覧は endpoint が変わらない限り維持する

## Resolved Decisions

### 1. `Winsvc.Api` project の最終処置 → 削除

- `Winsvc.Api` は Program.cs 1ファイルのみの薄い host で独自ロジックを持たない
- API 定義を `Winsvc.Hosting` に共通化した時点で残す価値はゼロ
- 別 exe による外部利用者は想定されていない

### 2. `api serve` の引数 → 最小限

| 引数 | 必須 | 説明 |
|------|------|------|
| `--urls` | 任意 | リッスン URL（`Winsvc:Api:Urls` を上書き） |
| `--manifest-dir` | 任意 | マニフェストディレクトリ（`Winsvc:ManifestDirectory` を上書き） |

持たせないもの:

- `--port` → `--urls http://localhost:9011` で十分。競合と優先順位の複雑化を避けるため不要
- `--environment` → `ASPNETCORE_ENVIRONMENT` 環境変数で設定するのが ASP.NET Core 標準

### 3. API 用設定の置き場所 → 既存キー互換 + CLI 引数マージ

- `appsettings.json` の `Winsvc:Api:Urls` / `Winsvc:ManifestDirectory` 構造を維持
- `Winsvc.Hosting` から `Configuration` を読む（appsettings / 環境変数 / CLI 注入の順で優先）
- CLI 引数が指定された場合、`WebApplicationBuilder` の Configuration に上書き注入する

### 4. 新設 `Winsvc.Hosting` project

- `Microsoft.NET.Sdk.Web` 使用
- `Winsvc.Infrastructure` を参照
- API endpoint 定義 + DI 登録 + host 構築拡張メソッドを収容
- `Winsvc.Cli.csproj` を `Microsoft.NET.Sdk.Web` に変更して Minimal API を利用可能にする

### 5. Response record 型の配置 → `Winsvc.Contracts`

- `ApiInfoResponse` 等を `Winsvc.Contracts.Api` namespace へ移動
- API レスポンスの型は公開契約として Contracts に属する

## Risks

- CLI project に ASP.NET Core host 依存を持ち込むため、構成が少し重くなる（成果物サイズ増大）
- API と CLI の起動責務が混ざるので、初期化コードの整理が甘いと見通しが悪くなる
- `System.CommandLine` と host 起動の組み合わせで終了制御を丁寧に扱う必要がある
- README と release の想定を変えるため、途中状態のまま出すと利用者が混乱する

## Recommended Execution Order

1. `Winsvc.Contracts` に API response 型を移動
2. `Winsvc.Hosting` project を新設、API 定義 + host 構築処理を移動
3. `Winsvc.Api` を `Winsvc.Hosting` を利用する薄い host へ差し替え
4. `Winsvc.Cli` を `Microsoft.NET.Sdk.Web` に変更、`api serve` コマンドを追加
5. README と docs を更新
6. release 構成を整理（appsettings.json を zip に含める）
7. `Winsvc.Api` project を削除

## Definition of Done

- `winsvc api serve` で既存 API が起動する
- `--urls` / `--manifest-dir` で設定を上書きできる
- 主要 endpoint が従来どおり動く
- release 配布物は `winsvc.exe` + `appsettings.json` 中心で説明可能
- README が CLI/API 統合後の利用方法を正しく説明している
- `Winsvc.Api` project が削除されている
