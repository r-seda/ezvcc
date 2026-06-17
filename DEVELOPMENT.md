# Development Guide — ezvcc

ezvcc を開発するための環境構築・ビルド・デバッグ手順。エンドユーザー向けの利用方法は [README.md](./README.md)、プロジェクト全体の設計は [PLANNING.md](./PLANNING.md) を参照。

---

## 1. 前提と全体像

- **ターゲット**: Windows 10/11 上で動作する .NET 8 WPF アプリ
- **言語**: C# 12 (`net8.0-windows`)
- **主要依存**: NAudio (WASAPI入出力)、CommunityToolkit.Mvvm (MVVM)
- **ビルド・実行可能なOS**: Windows のみ(WPFはWindows専用)
- **macOSでの作業範囲**: コード編集とコミット/プッシュは可能。`dotnet restore` / `build` は不可

ビルドと音声I/Oの動作確認は実機の Windows でのみ可能なため、Macで書き溜めたコードを定期的に Windows に持ち込んで検証するワークフローを取る。

---

## 2. Windows 開発環境のセットアップ

### 2.1 必須ツール

| ツール | バージョン | 入手元 / 備考 |
|-------|-----------|--------------|
| **.NET 8 SDK** (Windows Desktop ワークロード込み) | 8.0.x | https://dotnet.microsoft.com/download/dotnet/8.0 → "SDK 8.0.x (x64)" をインストール。WPF対応のためSDKでDesktopが入る |
| **Git for Windows** | 任意の新しめ | https://git-scm.com/download/win |
| **Visual Studio 2022 Community** (推奨) | 17.8+ | https://visualstudio.microsoft.com/ja/vs/community/ <br>インストール時に **「.NET デスクトップ開発」** ワークロードを選択 |

### 2.2 任意ツール

| ツール | 用途 |
|-------|------|
| **VS Code + C# Dev Kit** | 軽量編集が好みの場合の代替 |
| **VB-CABLE** | 他アプリへ「マイク入力」として渡すテスト用 ([README](./README.md#インストール手順) 参照) |
| **Windows Terminal** | PowerShell / cmd の使い勝手向上 |

### 2.3 インストール確認

PowerShell またはコマンドプロンプトで:

```powershell
dotnet --info
```

下記が確認できればOK:

- `.NET SDK > Version: 8.0.x`
- `.NET workloads installed` に `wasm-tools` などではなく、最低限 SDK が入っていること
- `Host > Version: 8.0.x`

`git --version` も併せて確認しておく。

---

## 3. リポジトリの取得

```powershell
# 任意の場所(例: C:\dev\)に clone
git clone git@github.com:r-seda/ezvcc.git C:\dev\ezvcc
cd C:\dev\ezvcc
```

HTTPSで取得する場合:

```powershell
git clone https://github.com/r-seda/ezvcc.git C:\dev\ezvcc
```

---

## 4. ビルド

### 4.1 コマンドラインから

```powershell
cd C:\dev\ezvcc

# パッケージ復元(初回 or csproj変更時)
dotnet restore

# Debugビルド
dotnet build -c Debug

# Releaseビルド
dotnet build -c Release
```

成果物: `src/ezvcc.App/bin/Debug/net8.0-windows/ezvcc.exe`

### 4.2 Visual Studio 2022 から

1. `C:\dev\ezvcc\ezvcc.sln` をダブルクリック
2. ソリューション エクスプローラで `ezvcc.App` をスタートアッププロジェクトに設定(右クリック → 「スタートアッププロジェクトに設定」)
3. ビルド構成は `Debug | Any CPU` 推奨
4. `Ctrl+Shift+B` でビルド

---

## 5. 実行とデバッグ

### 5.1 コマンドラインから起動

```powershell
dotnet run --project src/ezvcc.App -c Debug
```

ビルド済みexeを直接起動する場合:

```powershell
.\src\ezvcc.App\bin\Debug\net8.0-windows\ezvcc.exe
```

### 5.2 Visual Studio でのデバッグ

| 操作 | キー | 用途 |
|------|------|------|
| デバッグ開始 | `F5` | ブレークポイント有効でアプリ起動 |
| デバッグなしで開始 | `Ctrl+F5` | 通常起動(高速) |
| ブレークポイント切り替え | `F9` | 現在行に設定/解除 |
| ステップオーバー | `F10` | 次の行へ |
| ステップイン | `F11` | 関数の中に入る |
| ステップアウト | `Shift+F11` | 現在の関数を抜けるまで実行 |
| 続行 | `F5` | 次のブレークポイントまで実行 |

**推奨ブレークポイント位置**(初回検証時):

- `AudioEngine.Start()` の冒頭(デバイス取得が走るか)
- `AudioEngine.OnDataAvailable(...)` の冒頭(キャプチャが流れているか)
- `MainViewModel.OnLevelUpdated(...)`(UIスレッドへの marshalling が動いているか)

### 5.3 動作確認の手順

アプリ起動後:

1. ステータスに「停止中」、ProgressBarは0、「開始」ボタンが有効、「停止」ボタンが無効になっていること
2. **開始** をクリック
   - ステータスが「稼働中」に変わる
   - マイクに向かって話すと ProgressBar が動く
   - Windowsの既定の出力デバイス(スピーカー/ヘッドホン)から自分の声が遅延少なく聞こえる
3. **停止** をクリック
   - ステータスが「停止中」に戻り、ProgressBarが0に戻る
4. もう一度 **開始** → **停止** を繰り返しても落ちないこと
5. ウィンドウの「×」で閉じる → デバッガで例外が出ないこと
6. (Phase 3完了後) 出力デバイスとして `CABLE Input (VB-Audio Virtual Cable)` を選び、別アプリのマイク入力に `CABLE Output` を設定して動作確認

### 5.4 ログ・出力の確認

- **出力ウィンドウ**(VS2022): `表示` → `出力` または `Ctrl+Alt+O`
- **デバッグ出力**: `System.Diagnostics.Debug.WriteLine(...)` でメッセージを出力ウィンドウに送れる
- **例外ヘルパー**: `デバッグ` → `ウィンドウ` → `例外設定` で、特定の例外で必ず止めることが可能

### 5.5 ホットリロード

VS2022 はデバッグ実行中に C# コードの編集を反映できる(XAMLは特に良くサポートされる)。
- XAML編集: 保存と同時にウィンドウに反映
- C#編集: ツールバーの 🔥 アイコン または `Alt+F10` で適用

ただし、フィールド追加やメソッドシグネチャ変更など構造的な変更は再起動が必要。

---

## 6. macOS 側での編集ワークフロー

Macではビルド・実行はできないが、Claude Code や任意エディタでコードを書き、コミット/プッシュは可能。

```sh
cd /Users/starx105/MyDev/ezvcc

# 変更を確認
git status
git diff

# コミット
git add <files>
git commit -m "feat: add ..."

# プッシュ
git push
```

**注意**:
- Mac側のコミットはビルド未検証であることを前提とする。Windows側で `dotnet build` が通って初めて検証完了。
- パッケージのバージョン解決(`dotnet restore`)も Mac では実行しないため、`*.csproj` の編集は慎重に。
- WPFのXAMLデザイナはMacにはない。ビルド可否を確かめるにはWindowsに持ち込むしかない。

---

## 7. プロジェクト構造

```
ezvcc/
├── PLANNING.md              プロジェクト設計・要件・マイルストーン
├── README.md                エンドユーザー向け利用ガイド
├── DEVELOPMENT.md           本ファイル
├── .gitignore
├── ezvcc.sln                Visual Studio ソリューション
└── src/
    └── ezvcc.App/           WPFアプリ本体プロジェクト
        ├── ezvcc.App.csproj
        ├── App.xaml / App.xaml.cs     Applicationエントリ
        ├── MainWindow.xaml / .cs      メインウィンドウ
        ├── ViewModels/
        │   └── MainViewModel.cs       MVVM の View Model
        └── Audio/
            ├── AudioEngine.cs         WASAPIキャプチャ・再生エンジン
            └── AudioLevelEventArgs.cs レベル通知用イベント引数
```

将来のフェーズで以下が追加予定(PLANNING.md §10 参照):

- `src/ezvcc.App/Services/` — `SettingsService`, `AudioDeviceService` 等
- `src/ezvcc.App/Audio/PitchProcessor.cs` — SoundTouch ラッパー (Phase 2)
- `src/ezvcc.Core/` — 必要に応じてロジックをライブラリ化 (Phase 2 以降)
- `tests/ezvcc.Tests/` — ユニットテスト (Core 分離時)

---

## 8. コミット運用

- ブランチ: 開発初期は `main` 直push可。安定したらPRベースに移行
- コミットメッセージ: Conventional Commits を緩めに準拠
  - `feat:` 機能追加
  - `fix:` 不具合修正
  - `chore:` ビルド・設定・雑務
  - `docs:` ドキュメント変更
  - `refactor:` 機能を変えないリファクタ
- 粒度: 1コミット = 1論理変更を目安に小さく

---

## 9. トラブルシューティング(開発時)

### `dotnet restore` で `NU1100` / パッケージが見つからない
- NuGet ソースが設定されているか確認: `dotnet nuget list source`
- 既定の `https://api.nuget.org/v3/index.json` が enabled になっていればOK

### `dotnet build` で WPF 関連 SDK が見つからない
- SDK インストール時に Windows Desktop ワークロードが入っていない可能性
- VS インストーラーから「.NET デスクトップ開発」ワークロードを追加

### `WasapiCapture` の初期化で例外
- Windows の「サウンド設定」でマイクが既定の録音デバイスとして有効になっているか確認
- マイクへのアクセス許可: `Windows 設定` → `プライバシーとセキュリティ` → `マイク` → デスクトップアプリへのアクセスをON

### 音が出ない
- 既定の再生デバイスにスピーカー/ヘッドホンが設定されているか
- 別アプリで音量を取り合っていないか
- `WasapiOut` の例外が出ていないか(出力ウィンドウ確認)

### ProgressBar が動かない
- `AudioEngine.LevelUpdated` が発火しているか(ブレークポイント)
- `MainViewModel.OnLevelUpdated` が呼ばれているか
- UIスレッドへの marshalling(`Dispatcher.BeginInvoke`)が走っているか

### ビルドはできるが起動直後に落ちる
- VS2022 で例外設定をすべて有効にしてF5すれば、最初に投げられた例外で止まる
- 出力ウィンドウに `[Managed]` / `[CLR]` のメッセージがあれば確認

---

## 10. リリースビルド・配布(将来)

Phase 4 で本格的なリリース手順を整えるが、現時点で試したい場合:

```powershell
dotnet publish src/ezvcc.App -c Release -r win-x64 --self-contained false -o publish/
```

- `--self-contained false`: .NET 8 Desktop ランタイムがユーザー側に必要
- `--self-contained true`: ランタイム同梱(配布物が大きくなる代わりにユーザーは追加インストール不要)

`publish/` 配下に `ezvcc.exe` と依存DLLが出力される。

---

## 関連ドキュメント

- [README.md](./README.md) — エンドユーザー向け使い方
- [PLANNING.md](./PLANNING.md) — 設計・要件・マイルストーン
