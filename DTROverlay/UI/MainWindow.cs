using System.IO;
using MirageUI.Layout;

namespace DTROverlay.UI;

internal sealed class MainWindow : Window
{
    private readonly Action _toggleSettings;
    private readonly string _pluginIconPath;
    private ImRaii.ColorDisposable _themeScope;
    private string _sidebarSearch = string.Empty;

    public MainWindow(Action toggleSettings)
        : base(
            "DTR Overlay###dtroverlayMain",
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        _toggleSettings = toggleSettings;
        var size = new Vector2(1300f, MirageWindowDefaults.DefaultSize.Y);
        Size = size;
        SizeCondition = ImGuiCond.Always;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = size,
            MaximumSize = size,
        };
        Flags |= ImGuiWindowFlags.NoResize;

        _pluginIconPath = ResolvePluginIconPath();
        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.Heart,
            IconOffset = new Vector2(2.5f, 1f),
            Click = _ => MirageUi.OpenPluginPage(),
        });
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        MirageTheme.EnsureDefaultsCaptured();
        _themeScope = MirageTheme.PushCustom(MirageTheme.ResolveAppliedColors());
    }

    public override void PostDraw()
    {
        MirageTheme.Pop(_themeScope);
        _themeScope = null;
        ImGui.PopStyleVar();
    }

    public override void Draw()
    {
        var state = CreateTwoColumnState();
        MirageUi.TwoColumn.Draw(state, SettingsTab.DrawOverlaysPage);
    }

    private MirageTwoColumnState CreateTwoColumnState()
    {
        var version = PluginServices.PluginInterface.Manifest.AssemblyVersion?.ToString() ?? "0.0.0";
        var selected = DtrOverlayGroups.GetSelected();
        var canDelete = DtrOverlayGroups.CanRemoveOverlay(selected);

        return new MirageTwoColumnState
        {
            ShowSidebarHeader = true,
            ShowSidebarFooter = false,
            ShowSearch = true,
            SearchHint = "Search…",
            SearchFilter = _sidebarSearch,
            AllowDeselect = false,
            SidebarHeader = new MirageTwoColumnSidebarHeader
            {
                ImagePath = File.Exists(_pluginIconPath) ? _pluginIconPath : null,
                ImageWidth = 48f,
                ImageHeight = 48f,
                Title = PluginServices.PluginInterface.Manifest.Name ?? "DTR Overlay",
                Subtitle = $"v{version}",
                TrailingActions =
                [
                    new MirageTwoColumnTrailingAction
                    {
                        Id = "settings",
                        Icon = FontAwesomeIcon.Cog,
                        Tooltip = "Settings",
                        OnClick = _toggleSettings,
                    },
                ],
            },
            SearchTrailingActions =
            [
                new MirageTwoColumnTrailingAction
                {
                    Id = "add",
                    Icon = FontAwesomeIcon.Plus,
                    Tooltip = "Add overlay",
                    OnClick = () => DtrOverlayGroups.TryAddOverlay(),
                },
                new MirageTwoColumnTrailingAction
                {
                    Id = "delete",
                    Icon = FontAwesomeIcon.Trash,
                    Tooltip = canDelete ? "Delete selected overlay" : "Default and Native cannot be deleted",
                    OnClick = () =>
                    {
                        if (canDelete)
                            DtrOverlayGroups.TryRemoveOverlay(C.SelectedOverlayId);
                    },
                },
            ],
            Entries = BuildGroupEntries(),
            SelectedId = selected.Id,
            OnSelectionChanged = id =>
            {
                if (!string.IsNullOrEmpty(id))
                    DtrOverlayGroups.Select(id);
            },
            OnSearchFilterChanged = filter => _sidebarSearch = filter ?? string.Empty,
        };
    }

    private static List<MirageTwoColumnEntry> BuildGroupEntries()
    {
        var entries = new List<MirageTwoColumnEntry>();
        foreach (var group in DtrOverlayGroups.EnumerateGroupsForSettings())
        {
            entries.Add(new MirageTwoColumnEntry
            {
                Id = group.Id,
                Label = group.Name,
            });
        }

        return entries;
    }

    private static string ResolvePluginIconPath()
    {
        var dir = PluginServices.PluginInterface.AssemblyLocation.DirectoryName
                  ?? AppContext.BaseDirectory;
        return Path.Combine(dir, "Data", "plugin-icon.png");
    }
}
