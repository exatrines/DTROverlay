using System.Numerics;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class DtrImGui
{
    private static DrawScope BeginEntryDrawScope()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

        if (FollowVanillaDtrMode.IsActive && DtrVanillaBounds.TryGet(out var bounds, useScreenCoordinates: true))
            FollowVanillaFontScale.UpdateFromBounds(bounds);

        var fontPush = DtrOverlayFonts.PushActive();

        return new DrawScope(fontPush);
    }

    private readonly ref struct DrawScope(IDisposable fontPush)
    {
        public void Dispose()
        {
            fontPush.Dispose();
            ImGui.PopStyleVar(3);
        }
    }

    private static float LineHeight => ImGui.GetFontSize();

    private static float IconHeight
    {
        get
        {
            if (FollowVanillaDtrMode.IsActive)
            {
                var rowHeight = FollowVanillaFontScale.NativeRowHeight;
                if (rowHeight > 0f)
                    return rowHeight;

                return LineHeight * DtrStyle.FollowVanillaIconHeightRatio;
            }

            return LineHeight * DtrStyle.IconHeightRatio;
        }
    }

    private static float CachedLineHeight = -1f;

    private static float ManualEstimateTextDrawTopInset()
    {
        var lineHeight = UiBuilder.DefaultFont.FontSize * C.OverlayFontSizeScale;
        var contentHeight = lineHeight * 0.86f;
        return (lineHeight - contentHeight) * 0.5f;
    }
}
