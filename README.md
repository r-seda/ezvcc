# ezvcc — Easy Voice Changer

Windows向けのシンプルなリアルタイムボイスチェンジャー。マイク入力をリアルタイムにピッチシフトし、Discord / OBS / ゲーム等の他アプリへ「マイク入力」として渡せます。

> **Status**: 🚧 開発初期 (Phase 0 — 設計中)。コード本体はまだありません。本READMEは完成イメージと、現在着手中の流れを示します。詳細は [PLANNING.md](./PLANNING.md) を参照してください。

---

## ✨ 特徴

- 🎤 PCに接続したマイクをリアルタイムに処理
- 🔊 出力先デバイスを自由に選択(物理スピーカーや仮想オーディオケーブル)
- 🎚️ ピッチを半音単位で変更(-12〜+12 半音)
- 🪶 軽量・低レイテンシ(目標 ~50ms)
- 🪟 Windows 10 / 11 ネイティブアプリ(WPF)

---

## 動作環境

| 項目 | 要件 |
|------|------|
| OS | Windows 10 (1903以降) / Windows 11 (64bit) |
| ランタイム | .NET 8 デスクトップランタイム |
| 仮想オーディオ | (任意)他アプリへ「マイク入力」として渡す場合は **VB-CABLE** が必要 |
| ハードウェア | 入力デバイス(マイク)1つ以上 |

---

## エンドユーザー向け: インストールと起動

### 1. .NET 8 デスクトップランタイムを導入

https://dotnet.microsoft.com/download/dotnet/8.0 から **".NET Desktop Runtime 8.x (x64)"** をダウンロード・インストール。

### 2. (任意) VB-CABLE を導入

他アプリ(Discord, OBS, ゲーム等)に「マイク入力」として ezvcc の出力を渡したい場合のみ必要です。

1. https://vb-audio.com/Cable/ から VB-CABLE をダウンロード
2. zipを展開し、`VBCABLE_Setup_x64.exe` を **管理者権限で実行**
3. インストール後、PCを再起動

導入後、Windowsの録音デバイスに `CABLE Output (VB-Audio Virtual Cable)` が現れます。

### 3. ezvcc を取得

> ⚠️ 現時点ではリリース成果物はありません。下記は実装完了後の手順です。

1. [Releases](https://github.com/r-seda/ezvcc/releases) ページから最新の `ezvcc-x.y.z.zip` をダウンロード
2. 任意のフォルダに展開
3. `ezvcc.exe` をダブルクリックで起動

### 4. 使い方

1. ezvcc を起動
2. **入力デバイス**: 使うマイクを選択
3. **出力デバイス**:
   - ヘッドホン等で自分の声を確認したい → 物理出力デバイスを選択
   - Discord等の他アプリへ流したい → `CABLE Input (VB-Audio Virtual Cable)` を選択
4. **ピッチ**スライダを動かして声の高さを調整
5. **開始** ボタンを押して動作開始
6. (3でCABLE Inputを選んだ場合) 他アプリ側のマイク設定で `CABLE Output (VB-Audio Virtual Cable)` を選択

---

## 開発者向け: ビルドと実行

### 必要なもの

| ツール | バージョン | 用途 |
|-------|-----------|------|
| Windows | 10 (1903+) / 11 | 実行環境(WPFはWindows専用) |
| .NET SDK | 8.0.x | ビルド |
| Git | 任意 | ソース取得 |
| Visual Studio 2022 (Community可) | 任意 | デバッグ・GUIデザイナ |

### 取得

```sh
git clone git@github.com:r-seda/ezvcc.git
cd ezvcc
```

### ビルド

> ⚠️ 現時点では `src/` 配下にプロジェクトファイルがまだありません。Phase 1着手後に下記コマンドが有効になります。

```powershell
dotnet restore
dotnet build -c Release
```

### 実行

```powershell
dotnet run --project src/ezvcc.App -c Release
```

または Visual Studio で `ezvcc.sln` を開き、F5 で起動。

### Macからの開発について

本プロジェクトはmacOS上でもコード編集は可能ですが、WPFのビルド/実行はできません。Mac上の作業は設計・実装まで、実機検証はWindows側で行う運用です。詳細は [PLANNING.md §9](./PLANNING.md#9-開発環境とワークフロー) を参照。

---

## ロードマップ(MVP)

- [ ] **Phase 1**: デフォルトデバイスのパススルー(マイク → スピーカー)
- [ ] **Phase 2**: ピッチ変更機能
- [ ] **Phase 3**: 入出力デバイス選択UI + VB-CABLE連携
- [ ] **Phase 4**: 設定保存、エラー処理、配布パッケージ

最新の進捗は [PLANNING.md §6](./PLANNING.md#6-マイルストーンmvp--拡張) を参照してください。

---

## トラブルシューティング

> ⚠️ 実装完了後に充実させます。

- **音が出ない**: 入力/出力デバイスがWindowsの「サウンド設定」で有効になっているか確認
- **他アプリで音が拾われない**: VB-CABLEが正しくインストールされ、出力デバイスを `CABLE Input` に設定しているか、また他アプリ側のマイク設定が `CABLE Output` になっているか確認
- **遅延が大きい**: 出力先デバイスをWASAPI Exclusive対応のものに変更、またはバッファサイズ設定を下げる(設定UIは将来追加予定)

---

## ライセンス

未定。決定次第ここに記載します。

依存ライブラリ:
- [NAudio](https://github.com/naudio/NAudio) — MIT License
- [SoundTouch](https://www.surina.net/soundtouch/) — LGPL v2.1(動的リンクで利用予定)
- [VB-CABLE](https://vb-audio.com/Cable/) — ドネーションウェア(本アプリには同梱せず、ユーザーが各自インストール)

---

## 開発

- 設計・要件: [PLANNING.md](./PLANNING.md)
- Issue / PR: GitHub Issues / Pull Requests にて
