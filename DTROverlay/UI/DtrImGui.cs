using Dalamud.Interface.Utility;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class DtrImGui
{
    public static void DrawHorizontalEntries(IReadOnlyList<VisibleDtrEntry> entries)
    {
        if (entries.Count == 0)
            return;

        using var _ = BeginEntryDrawScope();
        UpdateCachedLineHeight();
        OverlayPositioning.RefineFollowNativePositionInFrame();
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
            : UiBuilder.DefaultFont.FontSize * FollowNativeFontScale.ActiveScale;
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
        FollowNativeDtrMode.AppliesTo(OverlayStyleContext.Group)
            ? FollowNativeFontScale.EstimateTextDrawTopInset()
            : ManualEstimateTextDrawTopInset();

    public static float EstimateOverlayContentHeight() =>
        FollowNativeDtrMode.AppliesTo(OverlayStyleContext.Group)
            ? FollowNativeFontScale.EstimateContentHeight()
            : UiBuilder.DefaultFont.FontSize * OverlayStyleResolver.GetEffectiveOverlayFontScale() * 0.86f;
}
