using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class DtrImGui
{
    public static void DrawVerticalLayout(
        IReadOnlyList<VisibleDtrEntry> nativeEntries,
        IReadOnlyList<VisibleDtrEntry> pluginEntries,
        OverlayVerticalAlignment alignment)
    {
        using var _ = BeginEntryDrawScope();

        var rowWidths = new List<float>();
        if (nativeEntries.Count > 0)
            rowWidths.Add(MeasureHorizontalRowWidth(nativeEntries, leftToRight: true));

        var orderedPluginEntries = OverlayPluginFlow.OrderForVertical(pluginEntries);

        for (var i = 0; i < orderedPluginEntries.Count;)
        {
            float rowWidth;
            var groupLength = 1;
            if (PluginEntryRowLayout.TryGetPluginRowSpan(orderedPluginEntries, i, out var groupStart, out var groupEnd))
            {
                rowWidth = MeasurePluginEntryGroupWidth(orderedPluginEntries, groupStart, groupEnd);
                groupLength = groupEnd - i + 1;
            }
            else
            {
                rowWidth = MeasureEntry(orderedPluginEntries[i]).X;
            }

            rowWidths.Add(rowWidth);
            i += groupLength;
        }

        if (rowWidths.Count == 0)
            return;

        var maxWidth = 0f;
        foreach (var rowWidth in rowWidths)
        {
            if (rowWidth > maxWidth)
                maxWidth = rowWidth;
        }
        var contentMin = ImGui.GetWindowContentRegionMin();
        var y = ImGui.GetCursorPosY();
        var rowIndex = 0;

        if (nativeEntries.Count > 0)
        {
            ImGui.SetCursorPos(new Vector2(
                GetRowStartX(contentMin.X, maxWidth, rowWidths[rowIndex], alignment),
                y));
            DrawHorizontalRow(nativeEntries, leftToRight: true);
            y = ImGui.GetCursorPosY();
            rowIndex++;

            if (pluginEntries.Count > 0)
            {
                y += DtrStyle.EntrySpacing;
                ImGui.SetCursorPosY(y);
            }
        }

        for (var i = 0; i < orderedPluginEntries.Count;)
        {
            ImGui.SetCursorPos(new Vector2(
                GetRowStartX(contentMin.X, maxWidth, rowWidths[rowIndex], alignment),
                y));

            if (PluginEntryRowLayout.TryGetPluginRowSpan(orderedPluginEntries, i, out var drawGroupStart, out var drawGroupEnd))
            {
                DrawPluginEntryRow(orderedPluginEntries, drawGroupStart, drawGroupEnd, sameLineBefore: false);
                i = drawGroupEnd + 1;
            }
            else
            {
                DrawEntry(orderedPluginEntries[i]);
                i++;
            }

            y += LineHeight;
            rowIndex++;

            if (i < orderedPluginEntries.Count)
            {
                y += DtrStyle.EntrySpacing;
                ImGui.SetCursorPosY(y);
            }
        }

        ImGui.SetCursorPos(new Vector2(contentMin.X, y));
        ImGui.Dummy(new Vector2(maxWidth, 0f));
    }

    private static float GetRowStartX(
        float contentMinX,
        float maxWidth,
        float rowWidth,
        OverlayVerticalAlignment alignment) =>
        alignment == OverlayVerticalAlignment.Right
            ? contentMinX + maxWidth - rowWidth
            : contentMinX;
}
