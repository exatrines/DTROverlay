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

    private DtrOverlayContent _content;
    private OverlayPositionOrigin? _appliedOrigin;
    private bool _followVanillaPaddingPushed;

    public OverlayWindow()
        : base("DTR Overlay##dtroverlayHud", BaseFlags | ImGuiWindowFlags.NoBackground, true)
    {
        IsOpen = true;
        RespectCloseHotkey = false;
    }

    public override bool DrawConditions() => C.OverlayEnabled || C.OverlayEditMode;

    public override void PreDraw()
    {
        FollowVanillaDtrMode.EnforceLayoutConstraints();
        UpdateEditModeState();

        if (!DrawConditions())
            return;

        if (FollowVanillaDtrMode.IsActive && !FollowVanillaDtrMode.IsVanillaDtrVisible)
            return;

        _content = DtrOverlayCollector.Collect();
        if (_content.IsEmpty && !C.OverlayEditMode)
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
            OverlayPositioning.MigrateLegacyTopLeftAnchor();
            if (_appliedOrigin is { } previousOrigin)
                OverlayPositioning.OnOriginChanged(previousOrigin);
            _appliedOrigin = C.OverlayPositionOrigin;
        }

        OverlayPositioning.ApplyWindowPosition();
    }

    public override void Draw()
    {
        try
        {
            if (!DrawConditions())
                return;

            if (FollowVanillaDtrMode.IsActive && !FollowVanillaDtrMode.IsVanillaDtrVisible)
                return;

            if (_content.IsEmpty && C.OverlayEditMode)
                DrawEditModePlaceholder();
            else if (!_content.IsEmpty)
                DrawContent();

            OverlayPositioning.SetLastWindowSize(ImGui.GetWindowSize());

            if (C.OverlayEditMode && !FollowVanillaDtrMode.IsActive)
                HandleEditModeDrag();
        }
        finally
        {
            if (_followVanillaPaddingPushed)
            {
                ImGui.PopStyleVar();
                _followVanillaPaddingPushed = false;
            }
        }
    }

    private void UpdateEditModeState()
    {
        Flags = C.OverlayEditMode ? BaseFlags : BaseFlags | ImGuiWindowFlags.NoBackground;
        BgAlpha = C.OverlayEditMode ? DtrStyle.EditModeBackgroundAlpha : 0f;
        AllowClickthrough = !C.OverlayEditMode;
    }

    private void DrawContent()
    {
        if (FollowVanillaDtrMode.IsActive)
        {
            DtrImGui.DrawHorizontalEntries(_content.PluginEntries);
            return;
        }

        if (C.OverlayLayoutMode == OverlayLayoutMode.Vertical)
        {
            DtrImGui.DrawVerticalLayout(
                _content.NativeEntries,
                _content.PluginEntries,
                C.OverlayVerticalAlignment);
            return;
        }

        DtrImGui.DrawHorizontalEntries(_content.OrderedEntries);
    }

    private static void DrawEditModePlaceholder()
    {
        using var fontPush = DtrOverlayFonts.PushActive();
        ImGui.TextUnformatted("Edit Mode");
    }

    private static void HandleEditModeDrag()
    {
        if (ImGui.IsWindowHovered() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            OverlayPositioning.ApplyDragDelta(ImGui.GetIO().MouseDelta);

        if (ImGui.IsWindowHovered())
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
    }
}
