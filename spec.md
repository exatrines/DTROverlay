# DTR Overlay 仕様

Dalamud の Server Info Bar（DTR）を ImGui オーバーレイで置き換え・拡張するプラグイン。  
本書は **現在の実装** に基づく挙動の要約である。

## 概要

- ログイン中、設定に応じて画面上に ImGui ウィンドウで DTR 相当の情報を描画する。
- **ネイティブグループ**（ワールド名・接続・時計など）と **プラグイングループ**（各 DTR プラグインの `IDtrBar` エントリ）を分けて扱う。
- `OverlayEnabled` が false でも `OverlayEditMode` が true ならウィンドウは表示される（プレースホルダーまたはドラッグ用）。

## コマンド

`/dtroverlay`（エイリアスなし）

| 引数 | 動作 |
|------|------|
| （なし） | 設定 UI の表示/非表示をトグル |
| `on` / `enable` | オーバーレイ有効 |
| `off` / `disable` | オーバーレイ無効 |
| `toggle` | オーバーレイの有効/無効切替 |

## 設定 UI

- タブ: **General**（説明のみ）、**Settings**（本体）
- ウィンドウ: 初回 900×500、最小 900×500
- フッター: 左にバージョン、右に GitHub / OFUSE / Ko-fi リンク

### Settings タブ構成

| セクション | 内容 |
|------------|------|
| Enable | `OverlayEnabled` |
| Shortcuts | バニラ DTR トグル（`/hud dtr`）、Dalamud DTR 設定の全エントリ無効化（`DtrIgnore`） |
| Layout | レイアウト・追従・並び（下記） |
| Appearance | フォントサイズ・色（Follow Vanilla 時はフォントサイズ UI 非表示） |
| Position | Follow Vanilla **無効** 時のみ。編集モード・原点・XY |
| Native Group | Follow Vanilla **無効** 時のみ。表示・モード・パーツ |
| Plugin entries | 順序・表示・prefix/suffix・Min Width・色 |
| Option | 右クリックでプラグイン UI を開く |

## データ収集

- プラグインエントリ: `Svc.DtrBar.Entries` を `EntryOrder` の順で収集。
  - `Shown` かつ `Text` あり、`HiddenEntryTitles` に含まれないもののみ。
  - 各エントリは **prefix（任意）→ SeString 本体 → suffix（任意）** の 0〜3 セグメントに展開。
- ネイティブ: `_DTR` アドオンから読み取り（Follow Vanilla 時はオーバーレイには載せない）。
- Follow Vanilla 有効時: 収集対象はプラグインエントリのみ。

## Follow Vanilla DTR

`FollowVanillaDtr && OverlayEnabled` のとき有効。

- レイアウトは常に **Horizontal** に強制。編集モードは強制オフ。
- ゲームの `_DTR` 上の Dalamud プラグインノード（NodeId ≥ 1000）を非表示にし、ネイティブはバニラバーに残す。
- オーバーレイはプラグイン行のみを水平描画。位置はバニラ DTR の境界に追従（左右選択・XY・フォントスケールオフセット）。
- 設定の Plugin order（Left/Right to left）は Follow Vanilla 時も利用可能。

## レイアウト（Follow Vanilla 無効時）

### Horizontal

ネイティブとプラグインを水平に配置。プラグイン間には `|` 区切り（`ShowPluginEntrySeparators`）を挿入可能。

**ネイティブとプラグインの関係**（`NativePluginDivision`）

| モード | 配置 |
|--------|------|
| New line | ネイティブ行の次行にプラグイン行 |
| Separator | 同一行。区切り `|`（`ShowNativeEntrySeparators` 相当の division 色）の後にプラグイン |

**Plugin order**（`HorizontalPluginFlow`）

| 設定 | ネイティブ行 | プラグイン行 |
|------|--------------|--------------|
| Left to right | 左→右 | 左→右 |
| Right to left + **New line** | 1 行目・左→右 | 2 行目・**右→左**（逆順） |
| Right to left + **Separator** | 同一行・プラグインが先（右→左）→ 区切り → ネイティブ（左→右） |

ネイティブセグメント間の `|` は `ShowNativeEntrySeparators` で制御（`NativeDtrReader` 内）。

### Vertical

- **1 行目**: ネイティブグループ全体を 1 行（左→右）
- **2 行目以降**: プラグインエントリを **1 エントリ 1 行**（prefix+本体+suffix は同一行のグループ）
- **Plugin order**: Top to bottom / Bottom to top（エントリリストの順序反転）
- **Plugin alignment**: 行単位で左揃え / 右揃え（最長行幅基準）
- プラグイン間 `|` 区切りは **Horizontal のみ**（Vertical では未使用）

## 見た目

### フォント

- 通常: `OverlayFontSizeScale`（0.5〜3）
- Follow Vanilla: `FollowVanillaFontSizeScale`

### 色・Min Width

| 対象 | 設定キー | 備考 |
|------|----------|------|
| デフォルト文言 | `@appearance:text` | 全エントリの既定色 |
| ネイティブグループ | `@native:server-info-text-group` | Text モード時など |
| 区切り（division / plugin / native） | `@separator:*` | 表示 ON/OFF は Appearance テーブル |
| 各プラグインエントリ | エントリ `Title` | Min Width・色は **本体のみ**（prefix/suffix は `ColorLayoutKey=Title`、幅は空 `LayoutKey`） |

**Min Width 有効時のスロット内配置**

| レイアウト | 配置 |
|------------|------|
| Horizontal | 中央 |
| Vertical + Left align | 左 |
| Vertical + Right align | 右 |

## ネイティブグループ

- `ShowServerInfo` でオーバーレイへの掲載を制御（Follow Vanilla 時はバニラ側表示のため Appearance のネイティブ色は無効扱い）。
- **Icon mode**: ワールドアイコン・名前、歩行、ネットワーク画像、ET/LT 時計（各パーツ個別に表示 ON/OFF）。
- **Text mode**: 上記をテキスト化した 1 グループ（色は `server-info-text-group`）。

## プラグインエントリ設定

- **表示**: テーブルのチェックボックス → `HiddenEntryTitles`
- **順序**: 上下ボタン → `EntryOrder`（リセットは Dalamud 登録順）
- **prefix / suffix**: `PluginEntryAffixesByTitle`（空なら出力しない）
- **Min Width / Color**: エントリ `Title` を `layoutKey` に使用

## 操作

| 操作 | 条件 |
|------|------|
| 左クリック | エントリの `OnClick` あり時（DTR 標準動作） |
| 右クリック | `OpenPluginUiOnRightClick` かつエントリタイトルあり → プラグイン UI を開く |
| ドラッグ | `OverlayEditMode` かつ Follow Vanilla 無効 → `OverlayPosition` 更新（原点に応じて X の符号反転） |

## 位置（Follow Vanilla 無効）

- 原点: Top left / Top right（Top right が既定。レガシー座標はマイグレーションあり）
- `OverlayPosition` は原点からのオフセット（Top right 時は X は右端からの距離）

## 永続化・マイグレーション

起動時・順序同期時に以下を自動移行: プラグイン affix、フォントスケール分割、Server Info 行、レガシー native ID、色設定、区切り表示フラグ分割 など（`DtrEntryOrder.SyncOrder` 経由）。
