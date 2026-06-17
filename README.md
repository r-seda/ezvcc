# ezvcc — Easy Voice Changer

Windows向けのシンプルなリアルタイムボイスチェンジャー。マイク入力をリアルタイムにピッチシフトし、Discord / OBS / ゲーム等の他アプリへ「マイク入力」として渡せます。

> **Status**: 🚧 開発中。リリース成果物はまだ公開されていません。最新の進捗は [PLANNING.md](./PLANNING.md) を参照してください。

---

## 特徴

- 🎤 PCに接続したマイクをリアルタイムに処理
- 🔊 出力先デバイスを自由に選択(物理スピーカー / 仮想オーディオケーブル)
- 🎚️ ピッチを半音単位で変更(-12〜+12 半音、Phase 2 以降)
- 🪶 軽量・低レイテンシ(目標 ~50ms)
- 🪟 Windows 10 / 11 ネイティブアプリ(WPF)

---

## 動作環境

| 項目 | 要件 |
|------|------|
| OS | Windows 10 (1903以降) / Windows 11 (64bit) |
| ランタイム | **不要**(exe 内に同梱) |
| 仮想オーディオ | (任意)他アプリへ「マイク入力」として渡す場合は **VB-CABLE** が必要 |
| ハードウェア | 入力デバイス(マイク)1つ以上 |

---

## インストール手順

### 1. ezvcc を取得

> ⚠️ 現時点ではリリース成果物はまだ公開されていません。下記はリリース後の手順です。

1. [Releases](https://github.com/r-seda/ezvcc/releases) ページから最新の `ezvcc.exe` をダウンロード
2. 任意のフォルダ(例: `C:\Tools\ezvcc\` や `Documents\ezvcc\`)に保存
3. `ezvcc.exe` をダブルクリックで起動

> 💡 .NET ランタイム等の事前インストールは不要です(exe内に同梱されています)。
>
> 💡 初回起動時に SmartScreen の警告が出ることがあります。「詳細情報」→「実行」で起動できます(コード署名は今後対応予定)。

### 2. (任意) VB-CABLE を導入

他アプリ(Discord / OBS / ゲーム等)に「マイク入力」として ezvcc の出力を渡したい場合のみ必要です。スピーカー/ヘッドホンで自分の声を確認するだけなら不要です。

1. https://vb-audio.com/Cable/ から VB-CABLE をダウンロード(無料)
2. zip を展開し、`VBCABLE_Setup_x64.exe` を **管理者権限で実行**
3. インストール後、PCを再起動

導入が成功すると、Windowsのサウンド設定の「録音」タブに `CABLE Output (VB-Audio Virtual Cable)`、「再生」タブに `CABLE Input (VB-Audio Virtual Cable)` が現れます。

---

## アンインストール

`ezvcc.exe` を削除するだけで完了します。.NETランタイム等は同梱されているため、PC全体の環境に影響しません。

完全に綺麗にしたい場合、初回起動時に展開される一時ファイルが `%LOCALAPPDATA%\Temp\.net\ezvcc\` 配下に残ることがあります。気になる場合はこのフォルダも削除してください。

VB-CABLE はWindowsの「設定 > アプリ > インストールされているアプリ」から個別にアンインストール可能です。

---

## 使い方

1. ezvcc を起動
2. **入力デバイス**: 使用するマイクを選択(Phase 3 以降)
3. **出力デバイス**:
   - ヘッドホン等で自分の声を確認したい → 物理出力デバイスを選択
   - Discord 等の他アプリへ流したい → `CABLE Input (VB-Audio Virtual Cable)` を選択
4. **ピッチ**スライダを動かして声の高さを調整(Phase 2 以降)
5. **開始** ボタンで動作開始
6. 出力デバイスとして CABLE Input を選んだ場合は、他アプリ側のマイク設定で `CABLE Output (VB-Audio Virtual Cable)` を選択

---

## トラブルシューティング

### 音が出ない
- Windows のサウンド設定で、入力 / 出力デバイスが有効になっているか確認
- ezvcc 上で正しい入出力デバイスが選択されているか確認
- 他アプリがマイクを占有していないか確認

### 他アプリで自分の声が拾われない
- VB-CABLE が正しくインストールされているか確認(サウンド設定に CABLE が出ているか)
- ezvcc の出力先が `CABLE Input` になっているか確認
- 他アプリ(Discord 等)のマイク入力が `CABLE Output` になっているか確認

### 遅延が大きい
- 出力デバイスを変更してみる(Bluetooth より有線が低遅延)
- 他アプリの音声処理(ノイズ抑制等)を無効化してみる
- バッファサイズ設定での調整(将来追加予定)

### 起動しない / エラーが出る
- .NET 8 デスクトップランタイムが導入されているか確認
- アンチウイルスソフトがブロックしていないか確認

それでも解決しない場合は [GitHub Issues](https://github.com/r-seda/ezvcc/issues) に報告してください。

---

## ライセンス

未定。決定次第ここに記載します。

依存ライブラリ:
- [NAudio](https://github.com/naudio/NAudio) — MIT License
- [SoundTouch](https://www.surina.net/soundtouch/) — LGPL v2.1(動的リンクで利用予定、Phase 2 以降)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) — MIT License
- [VB-CABLE](https://vb-audio.com/Cable/) — ドネーションウェア(本アプリには同梱せず、ユーザーが各自インストール)

---

## 開発者向け情報

ezvcc の開発に参加したい / ソースからビルドしたい方は以下を参照:

- [DEVELOPMENT.md](./DEVELOPMENT.md) — 開発環境のセットアップ、ビルド、デバッグ手順
- [PLANNING.md](./PLANNING.md) — プロジェクト設計、要件、マイルストーン
- Issue / PR: [GitHub Issues](https://github.com/r-seda/ezvcc/issues) / [Pull Requests](https://github.com/r-seda/ezvcc/pulls)
