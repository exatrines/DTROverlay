# DTR Overlay

[English](../README.md)

DTR Overlay は、Server Info Bar を自分のオーバーレイとして描く Dalamud プラグインです。

メイン画面がオーバーレイの編集です。追加し、出すプラグイン行と並び、位置を決めます。設定は歯車から開き、Follow native DTR、既定のスタイル、ショートカットを扱います。Follow native DTR では Default がゲームのバーの脇に付きます。それ以外のオーバーレイは、それぞれの配置のままです。

## インストール

1. `/xlsettings` を実行し、**試験的機能** タブを開く
2. **カスタムプラグインリポジトリ** に次の URL を追加する:

```
https://raw.githubusercontent.com/exatrines/DalamudPlugins/refs/heads/main/pluginmaster.json
```

3. `/xlplugins` を実行し、**DTR Overlay** をインストールする

## 機能

- **オーバーレイ** — 追加し、名前を付け、それぞれオン・オフする
- **Follow native DTR** — Default をゲームのバーの脇に置く。Manual では自分で配置する
- **Split Native DTR** — Manual のとき、サーバー情報を別のオーバーレイに出す
- **DTR entries** — 並び、表示、接頭・接尾、最小幅、プラグインごとの色
- **スタイル** — 既定の文字、区切り、ツールチップ。オーバーレイごとに上書きできる
- **ショートカット** — ゲームのバーや Dalamud 側の DTR 行を隠し、中クリックでプラグイン UI を開く

## コマンド

| コマンド | 説明 |
| --- | --- |
| `/dtroverlay` | オーバーレイ編集の表示切替 |

## 開発者向け

1. ビルド: `dotnet build DTROverlay.sln -c Release -p:Platform=x64`
2. Dalamud の **dev plugin** パスを `DTROverlay/bin/Release/` にする
3. プラグインインストーラ（dev）で **DTR Overlay** を有効にする

共有 UI キットの [MirageUI](https://github.com/exatrines/MirageUI) を git サブモジュールとして含みます。

## ライセンス

[AGPL-3.0-or-later](../LICENSE)
