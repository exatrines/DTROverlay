# DTR Overlay

FF14 Dalamud plugin that replaces the Server Info Bar with a customizable ImGui overlay.

## Plugin Repository URL

```
https://raw.githubusercontent.com/exatrines/DalamudPlugins/refs/heads/main/pluginmaster.json
```

## In-game

- `/dtroverlay` — open settings
- `/dtroverlay on|off|toggle` — enable or disable overlay
- `/dtroverlay edit` — toggle edit mode (drag position)

**Settings tab highlights**

- Layout: Horizontal / Vertical, Follow Vanilla DTR, plugin order, native/plugin division
- Appearance: font scale, colors, separators
- Plugin entries: order, visibility, prefix/suffix, min width, per-entry colors

See [spec.md](./spec.md) for behavior details.

## Build

```bash
git submodule update --init --recursive
dotnet build DTROverlay.sln -c Release
```

## Release (maintainers)

```bash
bash .github/scripts/bump-version.sh 1.0.0.0
git add DTROverlay/DTROverlay.json DTROverlay/DTROverlay.csproj CHANGELOG.md
git commit -m "Release 1.0.0.0"
git tag v1.0.0.0
git push origin main
git push origin v1.0.0.0
```

Pushing a `v*` tag (or running the Release workflow manually) builds `DTROverlay.zip` and publishes a GitHub Release.

## License

AGPL-3.0-or-later
