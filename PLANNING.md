# ezvcc — Easy Voice Changer (Planning)

Windows向けリアルタイムボイスチェンジャーアプリケーションの企画・要件・構成をまとめるドキュメント。

---

## 1. プロジェクト概要

マイク入力を受け取り、波形(まずはピッチ)をリアルタイムに加工して別アプリケーション(Discord, OBS, ゲーム等)へ「マイク入力」として渡すWindowsアプリ。

- **プロジェクト名**: ezvcc (Easy Voice Changer)
- **対象OS**: Windows 10 / 11 (64bit)
- **配布形態**: スタンドアロンの.exe(将来的にインストーラ同梱)
- **ライセンス想定**: 未定(SoundTouchがLGPLのため、リンク形態に注意)

---

## 2. 機能要件

### 2.1 必須要件 (Must)

| ID | 要件 |
|----|------|
| FR-01 | PCに接続された入力デバイス(マイク)を一覧から選択できる |
| FR-02 | 出力先デバイスを一覧から選択できる(物理スピーカー / 仮想オーディオケーブル両対応) |
| FR-03 | マイク入力をリアルタイムに出力デバイスへ流す(パススルー) |
| FR-04 | 入力音声のピッチを変更して出力できる(半音単位、例: -12 〜 +12) |
| FR-05 | 開始 / 停止 / バイパス(原音そのまま)操作ができる |
| FR-06 | 入力レベルメーターを表示する |
| FR-07 | 他アプリケーションが本アプリの出力を「マイク入力」として認識できる(VB-CABLE経由) |

### 2.2 任意要件 (Should / Nice to have)

- 設定の保存・復元(選択デバイス、ピッチ量)
- プリセット(男声化/女声化など)
- ノイズゲート / 簡易EQ
- ホットキーで開始・停止・バイパス
- 起動時に自動開始

### 2.3 非要件 (Out of scope, 少なくともv1.0では対象外)

- 自前の仮想オーディオドライバ開発
- macOS / Linux対応
- 録音・ファイル出力
- 高度なエフェクト(リバーブ、ロボットボイス、AI声質変換 等)

---

## 3. 非機能要件

| 項目 | 目標 |
|------|------|
| **レイテンシ** | マイク入力 → 出力デバイスまで **約50ms以下** (ゲーム/VoIPで違和感がない水準) |
| **CPU使用率** | アイドル時1%以下、稼働時シングルコア20%以下を目標 |
| **対応サンプリングレート** | 内部48kHz/16bit (NAudioデフォルト)、入出力デバイス側で自動変換 |
| **チャンネル** | モノラル入力 → モノラル/ステレオ出力 |
| **稼働環境** | Windows 10 (1903+) / Windows 11、.NET 8 ランタイム |

---

## 4. 技術スタック

### 4.1 確定事項

| カテゴリ | 採用 | 理由 |
|---------|------|------|
| 言語 / ランタイム | **C# / .NET 8** | Windows GUI開発の生産性、NAudioエコシステム |
| UIフレームワーク | **WPF** | Windows標準で広く使われ、デザイン自由度も高い |
| オーディオI/O | **NAudio** (WASAPI shared) | デバイス列挙・入出力・WASAPIラッパーが揃う |
| ピッチシフト | **SoundTouch** (.NET binding) | LGPL、リアルタイム志向、品質と速度のバランス良好 |
| 仮想オーディオ | **VB-CABLE (ユーザー導入)** | 無料・実績あり、ドライバ自作回避 |

### 4.2 依存関係(予定)

- `NAudio` (MIT)
- `SoundTouch.Net` または `SoundTouch` ネイティブ + P/Invokeラッパー (LGPL)
- `CommunityToolkit.Mvvm` (MVVMの省力化、任意)

### 4.3 ライセンス上の留意

- SoundTouchはLGPL。**動的リンク**(DLL分離)で利用し、ユーザーがDLL差し替え可能な形にすればクローズドソース配布も可能。本プロジェクトの公開形態は別途決定する。

---

## 5. アーキテクチャ概要

```
┌─────────────────────────────────────────────────────────────────┐
│                          ezvcc (WPF App)                        │
│                                                                 │
│  ┌──────────┐    ┌──────────────┐    ┌────────────────────┐    │
│  │ View     │◀──▶│ ViewModel    │◀──▶│ AudioEngine        │    │
│  │ (WPF)    │    │ (MVVM)       │    │ (Service)          │    │
│  └──────────┘    └──────────────┘    └────────────────────┘    │
│                                              │                  │
│                                              ▼                  │
│                                  ┌───────────────────────┐      │
│                                  │ Audio Pipeline        │      │
│                                  │  WasapiCapture        │      │
│                                  │   → BufferedProvider  │      │
│                                  │   → PitchProcessor    │      │
│                                  │     (SoundTouch)      │      │
│                                  │   → WasapiOut         │      │
│                                  └───────────────────────┘      │
└─────────────────────────────────────────────────────────────────┘
        ▲                                              │
        │ Mic                                          │ Audio
        │                                              ▼
   [Physical Mic]                          [Output Device]
                                                  │
                                  ┌───────────────┴───────────────┐
                                  ▼                               ▼
                          [Speaker / Headphones]          [VB-CABLE Input]
                                                                  │
                                                                  ▼
                                                       [Discord / OBS / Game]
                                                       (CABLE Output をマイクに設定)
```

### 5.1 主要コンポーネント

| コンポーネント | 役割 |
|--------------|------|
| `MainWindow` (View) | デバイス選択UI、ピッチスライダ、開始/停止ボタン、レベルメーター |
| `MainViewModel` | デバイス一覧バインド、設定値の保持、AudioEngineの操作 |
| `AudioDeviceService` | NAudio経由で入出力デバイスを列挙・参照する |
| `AudioEngine` | キャプチャ・処理・再生のライフサイクル管理 |
| `IAudioProcessor` | 音声処理パイプラインの抽象。実装に `PitchProcessor` (SoundTouch) |
| `PitchProcessor` | SoundTouchを用いて半音単位のピッチ変更 |
| `SettingsService` | 設定の保存/読込(JSON、`%AppData%/ezvcc/`) |

### 5.2 オーディオフロー(MVPの想定)

1. `WasapiCapture` で選択マイクから16bit/48kHz/モノラルでキャプチャ。
2. キャプチャイベントから `BufferedWaveProvider` に書き込み。
3. `PitchProcessor` がバッファから読み出してSoundTouchに投入、処理後サンプルを取得。
4. `WasapiOut` (出力デバイスをVB-CABLE Inputに設定可) で再生。
5. 他アプリはWindowsの録音デバイスとして "CABLE Output (VB-Audio Virtual Cable)" をマイクに指定。

### 5.3 レイテンシ設計

- WASAPI Sharedモードのバッファ:約10-20ms × 2〜3段(キャプチャ/処理/再生) → 合計30〜50msに収まる想定。
- バッファサイズは設定可能にし、低レイテンシモード(Exclusive)は後段で検討。

---

## 6. マイルストーン(MVP → 拡張)

### Phase 1: 最小パススルー (MVP-1)
- [ ] WPFプロジェクト雛形 (.NET 8)
- [ ] NAudio導入・OSデフォルトの入力→出力をパススルー
- [ ] 開始/停止ボタン、入力レベルメーター
- **完了基準**: マイクに向けて話した声が、ヘッドホンから遅延少なく聞こえる

### Phase 2: ピッチ変更
- [ ] SoundTouch導入、`PitchProcessor` 実装
- [ ] ピッチスライダ(-12〜+12 半音)を追加
- [ ] バイパストグル
- **完了基準**: スライダ操作で声の高さが滑らかに変化する

### Phase 3: デバイス選択 & 仮想ケーブル連携
- [ ] 入力デバイス選択UI(NAudioで列挙)
- [ ] 出力デバイス選択UI(VB-CABLE Input含む)
- [ ] README に VB-CABLE 導入手順を記載
- **完了基準**: 出力先を CABLE Input に設定 → Discord等で本アプリ加工後の声がマイク入力として聞こえる

### Phase 4: 仕上げ
- [ ] 設定の永続化(`%AppData%\ezvcc\settings.json`)
- [ ] エラー処理(デバイス切断、起動失敗等)
- [ ] 簡易プリセット
- [ ] **単一exe (self-contained) 配布** — `dotnet publish` で `ezvcc.exe` 1ファイル生成し GitHub Releases へ
- [ ] (任意) Inno Setup で `Setup.exe` も並行配布(スタートメニュー登録 / アンインストーラ付き)
- [ ] (将来) コード署名で SmartScreen 警告を解消

---

## 7. リスクと検討事項

| リスク | 影響 | 対応方針 |
|--------|------|---------|
| WASAPI Sharedでレイテンシ目標未達 | 体験悪化 | Exclusiveモードへの切替UIを用意。ASIO対応は将来検討 |
| SoundTouchのLGPL条項 | 配布方法に制約 | 動的リンク(DLL分離)で配布。ライセンス文書同梱 |
| VB-CABLEがユーザー環境に未インストール | 仮想マイク機能が動かない | 起動時に検出・案内ダイアログを表示 |
| デバイスのフォーマット不一致 | 音が出ない/歪む | NAudioのResampler経由で48kHzに統一 |
| マルチチャンネル/サンプルレート組合せ | 想定外の挙動 | MVPはモノラル48kHz固定。設定UIは将来追加 |

---

## 8. 未決定 / 今後決める事項

- [ ] アプリアイコン・ブランディング
- [ ] 公開リポジトリのライセンス(MIT? GPL?)
- [ ] 配布チャネル(GitHub Releases / Microsoft Store)
- [ ] テレメトリ・エラーレポートの有無
- [ ] 自動アップデート機構

---

## 9. 開発環境とワークフロー

### 9.1 マシン構成と当面の方針

| マシン | 当面の役割 |
|-------|----------|
| **Mac (macOS, 現在の作業機)** | **コード実装の主軸**。設計・PLANNING・コード記述まで全てここで行う。WPFビルドや音声検証は不可。 |
| **Windows PC** | **ビルド・テスト・実機検証の場**。利用可能なタイミングでまとめてpullし、ビルド/動作確認/デバッグを実施。 |

> **当面のスタンス**: Macで書いて、Windowsが使えるタイミングでビルド・テストする運用。Windows側にClaude Codeを入れたり、WSL2をセットアップするのは「Macで書き溜めたコードを最初にWindowsで検証する」段階で必要になったら判断する。

### 9.2 リポジトリ配置

- **リモート**: GitHub (リポジトリ名 `ezvcc`、初期はプライベート想定)
- **Mac**: `/Users/starx105/MyDev/ezvcc` (既存。当面はここがメイン)
- **Windows**: 初回検証時に `C:\dev\ezvcc` などへ `git clone`。Visual Studio 2022 でソリューションを開いてビルド・F5実行。

### 9.3 ループ(現時点)

```
[ Mac で実装 ]
  Claude Code で C#/XAML を書く
    │
    │ コミット & push
    ▼
  GitHub (ezvcc)
    │
    │ Windows利用可能タイミングで pull
    ▼
[ Windows でビルド・実行・検証 ]
  Visual Studio 2022 で開く → ビルド → F5
  実マイク + 実出力デバイス(必要に応じVB-CABLE)で挙動確認
    │
    │ 問題があればフィードバックをMacに持ち帰り修正
    ▼
[ Mac で再実装 ]
```

- **Mac側のコミットはビルド未検証である前提** で進める。WPF/NAudio/SoundTouchはMac上ではコンパイル不可なため、コード品質はレビュー・静的解析・既知のAPIシグネチャ準拠で担保する。
- 検証フェーズで判明した修正は、Macに戻ってから対応する(または、その場でWindows側でも直接編集してpushする)。

### 9.4 ツールセットアップ概要

| マシン | 必要なもの | 導入タイミング |
|-------|-----------|--------------|
| Mac | Claude Code (導入済)、git | 済 |
| Windows | .NET 8 SDK (WPFワークロード込み)、Git for Windows、Visual Studio 2022 Community、VB-CABLE | 初回ビルド検証時にまとめて |
| GitHub | リポジトリ `ezvcc` (private)、当面ブランチ保護なし | リモート化したくなったタイミングで作成 |

### 9.5 ブランチ・コミット運用(初期)

- 開発初期は `main` 直push可。Phase 2以降、機能単位で `feature/xxx` ブランチを切る運用に移行。
- コミット粒度はタスク単位。`docs:` / `feat:` / `fix:` / `refactor:` のプレフィックスを付ける(Conventional Commits 緩めに準拠)。
- **Mac側でのコミットはビルド未検証**になる点を許容する。コミットメッセージに `(unverified on windows)` 等の注記は不要だが、Windows検証後にバグ修正コミットが続くことを前提に扱う。

### 9.6 Docker等について

本プロジェクトはWindowsネイティブAPI(WASAPI)とWPFに強く依存するため、**Docker / コンテナは使用しない**。CI/CDが必要になった段階で、GitHub ActionsのWindowsランナーを用いる方針。

---

## 10. リポジトリ構成 (予定)

```
ezvcc/
├── PLANNING.md              # 本ファイル
├── README.md                # 利用者向け説明
├── ezvcc.sln
├── src/
│   ├── ezvcc.App/           # WPFアプリ本体
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── Services/        # AudioDeviceService, SettingsService
│   │   └── Audio/           # AudioEngine, PitchProcessor
│   └── ezvcc.Core/          # 音声処理ロジック(必要に応じ分離)
└── tests/
    └── ezvcc.Tests/
```
