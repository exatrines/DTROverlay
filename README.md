# DTR Overlay

[日本語](docs/README.ja.md)

DTR Overlay is a Dalamud plugin that draws your own Server Info Bar overlays.

The main window is the overlay editor: add overlays, choose which plugin entries they show, and set order and placement. Settings, opened from the gear icon, cover Follow native DTR, default style, and shortcuts. In Follow native DTR, the Default overlay sits beside the game bar. Other overlays keep their own layout.

## Install

1. Run `/xlsettings` and open the **Experimental** tab
2. Add this URL under **Custom Plugin Repositories**:

```
https://raw.githubusercontent.com/exatrines/DalamudPlugins/refs/heads/main/pluginmaster.json
```

3. Run `/xlplugins` and install **DTR Overlay**

## Features

- **Overlays** — add overlays, name them, and turn each one on or off
- **Follow native DTR** — sit the Default overlay beside the game bar, or place overlays yourself in Manual
- **Split Native DTR** — in Manual, show server info on its own overlay
- **DTR entries** — order, visibility, prefix and suffix, minimum width, and colors per plugin
- **Style** — default text, separators, and tooltips, with per-overlay overrides
- **Shortcuts** — hide the game bar or Dalamud’s own DTR entries, and open a plugin UI with a middle-click

## Commands

| Command | Description |
| --- | --- |
| `/dtroverlay` | Toggle the overlay editor |

## For developers

1. Build: `dotnet build DTROverlay.sln -c Release -p:Platform=x64`
2. Point Dalamud’s **dev plugin** path at `DTROverlay/bin/Release/`
3. Enable **DTR Overlay** in the plugin installer (dev)

[MirageUI](https://github.com/exatrines/MirageUI) is included as a git submodule for the shared UI kit.

## License

[AGPL-3.0-or-later](LICENSE)
