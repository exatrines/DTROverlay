using MirageUI.Layout;

namespace DTROverlay.UI;

internal sealed class SettingsWindow : Window
{
    private const string PageStyle = "page:style";
    private const string PageFollowVanilla = "page:follow-vanilla";
    private const string PageOptions = "page:options";

    private ImRaii.ColorDisposable _themeScope;
    private string _selectedId = PageFollowVanilla;
    private string _sidebarSearch = string.Empty;

    public SettingsWindow()
        : base(
            "DTR Overlay Settings###dtroverlaySettings",
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = MirageWindowDefaults.DefaultSize;
        SizeCondition = ImGuiCond.Always;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = MirageWindowDefaults.DefaultSize,
            MaximumSize = MirageWindowDefaults.DefaultSize,
        };
        Flags |= ImGuiWindowFlags.NoResize;

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
        if (_selectedId is not (PageStyle or PageFollowVanilla or PageOptions))
            _selectedId = PageFollowVanilla;

        var state = CreateTwoColumnState();
        MirageUi.TwoColumn.Draw(state, DrawMainContent);

        if (!string.IsNullOrEmpty(state.SelectedId))
            _selectedId = state.SelectedId;
    }

    private MirageTwoColumnState CreateTwoColumnState()
    {
        return new MirageTwoColumnState
        {
            ShowSidebarHeader = false,
            ShowSidebarFooter = false,
            ShowSearch = true,
            SearchHint = "Search…",
            SearchFilter = _sidebarSearch,
            AllowDeselect = false,
            Entries =
            [
                new MirageTwoColumnEntry { Id = PageFollowVanilla, Label = "Mode" },
                new MirageTwoColumnEntry { Id = PageStyle, Label = "Default Style" },
                new MirageTwoColumnEntry { Id = PageOptions, Label = "Options" },
            ],
            SelectedId = _selectedId,
            OnSelectionChanged = id =>
            {
                if (!string.IsNullOrEmpty(id))
                    _selectedId = id;
            },
            OnSearchFilterChanged = filter => _sidebarSearch = filter ?? string.Empty,
        };
    }

    private void DrawMainContent()
    {
        switch (_selectedId)
        {
            case PageFollowVanilla:
                SettingsTab.DrawModePage();
                break;
            case PageOptions:
                SettingsTab.DrawOptionsPage();
                break;
            default:
                SettingsTab.DrawStylePage();
                break;
        }
    }
}
