namespace DTROverlay.Services;

internal static class OverlayPluginFlow
{
    public static bool UseHorizontalLeftToRight =>
        C.OverlayLayoutMode == OverlayLayoutMode.Horizontal
        && C.HorizontalPluginFlow == OverlayHorizontalFlow.LeftToRight;

    public static bool UseVerticalBottomToTop =>
        C.OverlayLayoutMode == OverlayLayoutMode.Vertical
        && C.VerticalPluginFlow == OverlayVerticalFlow.BottomToTop;

    public static IReadOnlyList<VisibleDtrEntry> OrderForVertical(IReadOnlyList<VisibleDtrEntry> entries)
    {
        if (!UseVerticalBottomToTop || entries.Count <= 1)
            return entries;

        var reversed = new VisibleDtrEntry[entries.Count];
        for (var i = 0; i < entries.Count; i++)
            reversed[i] = entries[entries.Count - 1 - i];

        return reversed;
    }
}
