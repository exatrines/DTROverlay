<p align="center">
  <img src="DTROverlay/Data/plugin-icon.png" alt="DTR Overlay icon" width="128" height="128">
</p>

<h1 align="center">DTR Overlay</h1>

<p align="center">
  English | <a href="docs/README.ja.md">日本語</a>
</p>

<p align="center">
  <a href="https://github.com/exatrines/DTROverlay/releases/latest">
    <img src="https://img.shields.io/github/v/release/exatrines/DTROverlay?label=Release&amp;labelColor=F280B6&amp;color=FFFFFF&amp;style=flat&amp;sort=date&amp;display_name=tag" alt="Release">
  </a>
  <a href="CHANGELOG.md">
    <img src="https://img.shields.io/badge/Changelog-view-FFFFFF?labelColor=F280B6&amp;style=flat" alt="Changelog">
  </a>
  <a href="LICENSE">
    <img src="https://img.shields.io/badge/License-AGPL--3.0--or--later-FFFFFF?labelColor=F280B6&amp;style=flat" alt="AGPL-3.0-or-later">
  </a>
</p>

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

1. `git submodule update --init --recursive`
2. Build: `dotnet build DTROverlay.sln -c Release -p:Platform=x64`
3. Point Dalamud’s **dev plugin** path at `DTROverlay/bin/Release/`
4. Enable **DTR Overlay** in the plugin installer (dev)

[MirageUI](https://github.com/exatrines/MirageUI) is included as a git submodule for the shared UI kit.

## Contributing

Contributions are always welcome! Please see the [contribution guide](CONTRIBUTING.md).
