# Changelog

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
