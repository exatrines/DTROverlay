using Dalamud.Interface.Utility;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class DtrImGui
{
    public static void SectionHeader(string label) =>
        SectionHeader(label, null, null);

    public static void SectionHeader(string label, Action onTrailingButton, string trailingButtonId, string trailingButtonTooltip = null)
    {
        ImGui.Spacing();

        var rowY = ImGui.GetCursorPosY();
        var rowHeight = onTrailingButton != null && !string.IsNullOrEmpty(trailingButtonId)
            ? SquareIconButtonSize()
            : ImGui.GetTextLineHeight();
        var labelX = ImGui.GetCursorPosX();

        if (onTrailingButton != null && !string.IsNullOrEmpty(trailingButtonId))
        {
            var buttonSize = SquareIconButtonSize();
            ImGui.SetCursorPos(new Vector2(ImGui.GetContentRegionMax().X - buttonSize, rowY));
            if (SmallIconButton(FontAwesomeIcon.Undo, trailingButtonId))
                onTrailingButton();

            if (!string.IsNullOrEmpty(trailingButtonTooltip) && ImGui.IsItemHovered())
                ImGui.SetTooltip(trailingButtonTooltip);
        }

        ImGui.SetCursorPos(new Vector2(labelX, rowY + rowHeight - ImGui.GetTextLineHeight()));
        ImGui.TextColored(ImGuiColors.DalamudGrey, label);
        ImGui.SetCursorPosY(rowY + rowHeight);

        ImGui.Separator();
        ImGui.Spacing();
    }

    public static bool SmallIconButton(FontAwesomeIcon icon, string id) =>
        ImGuiEx.IconButton(icon, id, new Vector2(SquareIconButtonSize(), SquareIconButtonSize()));

    private static float SquareIconButtonSize() => ImGui.GetFrameHeight();

    public static void DrawHorizontalEntries(IReadOnlyList<VisibleDtrEntry> entries)
    {
        if (entries.Count == 0)
            return;

        using var _ = BeginEntryDrawScope();
        UpdateCachedLineHeight();
        OverlayPositioning.RefineFollowVanillaPositionInFrame();
        DrawHorizontalEntriesCore(entries);
    }

    public static float GetHorizontalRowLineHeight()
    {
        var groupId = OverlayStyleContext.Group?.Id;
        if (!string.IsNullOrEmpty(groupId)
            && CachedLineHeightByGroupId.TryGetValue(groupId, out var cached)
            && cached > 0f)
            return cached;

        return ImGui.GetFontSize() > 0f
            ? ImGui.GetFontSize()
            : UiBuilder.DefaultFont.FontSize * FollowVanillaFontScale.ActiveScale;
    }

    public static void UpdateCachedLineHeight()
    {
        var groupId = OverlayStyleContext.Group?.Id;
        if (string.IsNullOrEmpty(groupId))
            return;

        CachedLineHeightByGroupId[groupId] = ImGui.GetFontSize();
    }

    /// <summary>Matches vertical offset applied in <see cref="GetAlignedPos"/> for drawn text.</summary>
    public static float GetTextDrawTopInset()
    {
        var lineHeight = ImGui.GetFontSize();
        var contentHeight = GetOverlayContentHeight();
        return (lineHeight - contentHeight) * 0.5f;
    }

    public static float GetOverlayContentHeight() =>
        ImGui.CalcTextSize("ET 00:00").Y;

    public static float EstimateTextDrawTopInset() =>
        FollowVanillaDtrMode.IsActive
            ? FollowVanillaFontScale.EstimateTextDrawTopInset()
            : ManualEstimateTextDrawTopInset();

    public static float EstimateOverlayContentHeight() =>
        FollowVanillaDtrMode.IsActive
            ? FollowVanillaFontScale.EstimateContentHeight()
            : UiBuilder.DefaultFont.FontSize * OverlayStyleResolver.GetEffectiveOverlayFontScale() * 0.86f;
}
