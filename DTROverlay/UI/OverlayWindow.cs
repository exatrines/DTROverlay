using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using DTROverlay.Services;

namespace DTROverlay.UI;

public sealed class OverlayWindow : Window
{
    private const ImGuiWindowFlags BaseFlags =
        ImGuiWindowFlags.NoDecoration
            | ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoNav;

    private readonly string _groupId;
    private DtrOverlayGroup _group;
    private DtrOverlayContent _content;
    private OverlayPositionOrigin? _appliedOrigin;
    private bool _followVanillaPaddingPushed;
    private float _lastWindowWidth;
    private IDisposable _styleScope;

    public string GroupId => _groupId;

    private static readonly Dictionary<string, float> LastWidthsByGroup = [];

    public static float GetLastWidthForGroup(string groupId) =>
        LastWidthsByGroup.GetValueOrDefault(groupId);

    public OverlayWindow(string groupId)
        : base($"DTR Overlay##dtroverlayHud_{groupId}", BaseFlags | ImGuiWindowFlags.NoBackground, true)
    {
        _groupId = groupId;
        IsOpen = true;
        RespectCloseHotkey = false;
    }

    public override bool DrawConditions()
    {
        _group = DtrOverlayGroups.GetById(_groupId);
        if (_group == null)
            return false;

        if (C.FollowVanillaDtr
            && _groupId != DtrOverlayGroups.GetDefaultGroup().Id
            && _groupId != DtrOverlayGroups.GetNativeGroup().Id)
            return false;

        if (DtrOverlayGroups.IsNativeGroup(_group) && !DtrOverlayGroups.IsSplitNativeMode())
            return false;

        return (_group.Enabled && C.OverlayEnabled) || _group.OverlayEditMode;
    }

    public override void PreDraw()
    {
        if (_group == null)
            return;

        _styleScope?.Dispose();
        _styleScope = OverlayStyleContext.Push(_group);
        FollowVanillaDtrMode.EnforceLayoutConstraints();
        UpdateEditModeState();

        if (!DrawConditions())
            return;

        if (FollowVanillaDtrMode.IsActive && !FollowVanillaDtrMode.IsVanillaDtrVisible)
            return;

        _content = DtrOverlayCollector.Collect(_group);
        if (_content.IsEmpty && !_group.OverlayEditMode)
            return;

        if (FollowVanillaDtrMode.IsActive)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            _followVanillaPaddingPushed = true;
        }
        else
        {
            _followVanillaPaddingPushed = false;
        }

        if (!FollowVanillaDtrMode.IsActive)
        {
            OverlayPositioning.MigrateLegacyTopLeftAnchor(_group);
            if (_appliedOrigin is { } previousOrigin)
                OverlayPositioning.OnOriginChanged(_group, previousOrigin, _lastWindowWidth);
            _appliedOrigin = _group.OverlayPositionOrigin;
        }

        OverlayPositioning.ApplyWindowPosition(_group);
    }

    public override void Draw()
    {
        try
        {
            if (_group == null || !DrawConditions())
                return;

            if (FollowVanillaDtrMode.IsActive && !FollowVanillaDtrMode.IsVanillaDtrVisible)
                return;

            if (_content.IsEmpty && _group.OverlayEditMode)
                DrawEditModePlaceholder();
            else if (!_content.IsEmpty)
                DrawContent();

            var size = ImGui.GetWindowSize();
            if (size.X > 0f)
            {
                _lastWindowWidth = size.X;
                LastWidthsByGroup[_groupId] = size.X;
            }

            if (_group.OverlayEditMode && !FollowVanillaDtrMode.IsActive)
                HandleEditModeDrag(_group);
        }
        finally
        {
            _styleScope?.Dispose();
            _styleScope = null;

            if (_followVanillaPaddingPushed)
            {
                ImGui.PopStyleVar();
                _followVanillaPaddingPushed = false;
            }
        }
    }

    private void UpdateEditModeState()
    {
        if (_group == null)
            return;

        Flags = _group.OverlayEditMode ? BaseFlags : BaseFlags | ImGuiWindowFlags.NoBackground;
        BgAlpha = _group.OverlayEditMode ? DtrStyle.EditModeBackgroundAlpha : 0f;
        AllowClickthrough = !_group.OverlayEditMode;
    }

    private void DrawContent()
    {
        if (FollowVanillaDtrMode.IsActive)
        {
            var entries = _content.NativeEntries.Count > 0
                ? _content.NativeEntries
                : _content.PluginEntries;
            DtrImGui.DrawHorizontalEntries(entries);
            return;
        }

        if (_group.LayoutMode == OverlayLayoutMode.Vertical)
        {
            DtrImGui.DrawVerticalLayout(
                _content.NativeEntries,
                _content.PluginEntries,
                _group.VerticalAlignment);
            return;
        }

        DtrImGui.DrawHorizontalEntries(_content.OrderedEntries);
    }

    private static void DrawEditModePlaceholder()
    {
        using var fontPush = DtrOverlayFonts.PushActive();
        ImGui.TextUnformatted("Edit Mode");
    }

    private static void HandleEditModeDrag(DtrOverlayGroup group)
    {
        if (ImGui.IsWindowHovered() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            OverlayPositioning.ApplyDragDelta(group, ImGui.GetIO().MouseDelta);

        if (ImGui.IsWindowHovered())
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
    }
}
