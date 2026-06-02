# Changelog

## 1.2.0.4 — 2026-06-02

### 修正

- ログイン直後に `EntryOrder` から未登録の DTR タイトルを削除していたため、Follow Vanilla 等で保存済みのプラグイン順序の大半が読み込まれない不具合を修正（`SyncGroupOrder` は未登録を保持し、新規 DTR のみ末尾に追加）
- 設定 UI で変更した項目（有効化、Follow Vanilla DTR、ショートカット、オフセット、フォントスケール、色・スタイルなど）が即座に `DefaultConfig.json` へ保存されない不具合を修正

## 1.2.0.3 — 2026-06-02

### 修正

- prefix のみ設定・suffix なし時に RTL レイアウトで本文が二重表示される不具合を修正
- 空の prefix/suffix を `""` に正規化し、余分な列・間隔を出さないよう整理
- プラグイン行の描画順を prefix → 本文 → suffix に統一（`PluginEntryRowLayout`）

## 1.2.0.2 — 2026-06-01

### 修正

- Follow Vanilla DTR 有効時に毎フレーム設定保存・ウィンドウ再構築が走り FPS が大幅に低下する不具合を修正（@rioriopu）
- Follow Vanilla 時の `DtrVanillaBounds` 走査を同一フレーム内でキャッシュし、ATK ノード走査の重複を削減

## 1.2.0.1 — 2026-06-01

### 修正

- 初回起動・Native グループ未作成の設定でプラグインが読み込めない不具合を修正（`MigrateMergedDefaultOverrideStyles` をシステムグループ作成後に実行）

## 1.2.0.0 — 2026-06-01

### スタイル・表示

- スタイル階層を整理（Default Style / Override Style / プラグイン個別色、グループ別フォントスケール）
- Default Style に Font Colors 表（Text / Separator / Division）を統合
- Split Native OFF 時、Default グループ名を **Default+Native** に変更
- Default+Native の Override Style に Plugin / Native / Division の5行 Font Colors を追加
- セパレーターバー OFF 時は `|` の代わりに**空白スロット**（幅は Separator width 設定を使用）
- Division separator はバー OFF でも挿入し、改行しない（統合レイアウト・Follow Vanilla）
- Native テキストモードの各パーツ（トラベル状況・ワールド名・歩行・WiDi 等）に Native 用オーバーライド色を適用する不具合を修正

### Follow Vanilla DTR

- Groups タブの **Default** に Division 表示チェック、Division 色、Override の Font Size 非表示を集約（Follow Vanilla セクション直下から移動）
- Division separator の位置を DTR 左右（Left/Right side）と Plugin order（LTR/RTL）の組み合わせで決定（バニラ DTR 隣接側に配置）

### 設定 UI

- 「Between native and plugins」ラジオを削除（Division はチェックボックスのみ）
- Follow Vanilla 時は Default グループの Native Group 設定を非表示（バニラ DTR を使用）
- Font Colors 表の列ずれ・未使用列を修正

### 内部

- `OverlayStyleKeys` 等で色 layout key 選択を集約、水平 LTR/RTL 描画の重複を統合
- 未使用 API・設定 UI ヘルパー整理（挙動変更なし）

## 1.1.0.1 - 2026-06-01

- アイコンを変更

## 1.1.0.0 — 2026-05-30

- Follow Vanilla DTR の位置・横幅追従を改善（Left/Right 切替時の Y ずれ、コンテンツ内外のフォントサイズ差を修正）
- バニラ DTR 非表示時にオーバーレイも非表示（例: 美容師の呼び鈴使用時など）
- ツールチップ設定を追加（サイズ・色・表示位置: Follow cursor / Upper / Lower）

## 1.0.3.0 — 2026-05-30

- Dalamud のメイン UI コールバックを登録（`OpenMainUi` 警告の解消）
- `/dtroverlay` で設定 UI の表示/非表示をトグル
- `/dtroverlay edit` サブコマンドを削除（編集モードは設定 UI から操作）

## 1.0.2.0 — 2026-05-30

- プラグイン manifest の文言を更新（Author / Punchline / Description）

## 1.0.1.0 — 2026-05-30

- 軽微な不具合修正

## 1.0.0.0 — 2026-05-30

初回リリース。

- Server Info Bar を ImGui オーバーレイで置き換え
- Horizontal / Vertical レイアウト、Follow Vanilla DTR
- プラグインエントリの順序・表示・prefix/suffix・Min Width・色
- ネイティブグループ（ワールド・接続・時計）の表示とスタイル
- 設定 UI（General / Settings）、ショートカット（バニラ DTR / Dalamud DTR 一括無効化）
