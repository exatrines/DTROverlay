using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class DtrImGui
{
    private static void DrawHorizontalEntriesCore(IReadOnlyList<VisibleDtrEntry> entries)
    {
        SplitNativeAndPluginEntries(entries, out var nativeEntries, out var pluginEntries);

        if (nativeEntries.Count == 0)
        {
            DrawHorizontalPluginRow(
                InsertPluginSeparators(InsertDivisionSeparatorIfNeeded(pluginEntries)));
            return;
        }

        if (pluginEntries.Count == 0)
        {
            DrawHorizontalRow(nativeEntries, leftToRight: true);
            return;
        }

        DrawHorizontalNativeAndPlugins(nativeEntries, InsertPluginSeparators(pluginEntries));
    }

    private static void SplitNativeAndPluginEntries(
        IReadOnlyList<VisibleDtrEntry> entries,
        out List<VisibleDtrEntry> nativeEntries,
        out List<VisibleDtrEntry> pluginEntries)
    {
        nativeEntries = [];
        pluginEntries = [];

        foreach (var entry in entries)
        {
            if (IsPluginOverlayEntry(entry))
                pluginEntries.Add(entry);
            else
                nativeEntries.Add(entry);
        }
    }

    private static void DrawHorizontalNativeAndPlugins(
        IReadOnlyList<VisibleDtrEntry> nativeEntries,
        IReadOnlyList<VisibleDtrEntry> pluginEntries)
    {
        var insertDivisionSeparator = DtrSeparators.ShouldInsertDivision(true, true);
        var continuePluginsOnSameLine = insertDivisionSeparator;
        var pluginsLeftToRight = OverlayPluginFlow.UseHorizontalLeftToRight();

        if (pluginsLeftToRight)
        {
            DrawHorizontalRow(nativeEntries, leftToRight: true);
            if (insertDivisionSeparator)
            {
                ImGui.SameLine(0f, DtrStyle.EntrySpacing);
                DrawEntry(DtrSeparators.CreateDivision());
            }

            DrawHorizontalPluginRow(pluginEntries, sameLineBefore: continuePluginsOnSameLine);
            return;
        }

        DrawHorizontalPluginRow(pluginEntries);
        if (insertDivisionSeparator)
        {
            ImGui.SameLine(0f, DtrStyle.EntrySpacing);
            DrawEntry(DtrSeparators.CreateDivision());
        }

        DrawHorizontalRow(nativeEntries, leftToRight: true, sameLineBefore: continuePluginsOnSameLine);
    }

    private static void DrawHorizontalPluginRow(
        IReadOnlyList<VisibleDtrEntry> pluginEntries,
        bool sameLineBefore = false) =>
        DrawHorizontalRow(
            pluginEntries,
            OverlayPluginFlow.UseHorizontalLeftToRight(),
            sameLineBefore);

    private static void DrawHorizontalRow(
        IReadOnlyList<VisibleDtrEntry> entries,
        bool leftToRight,
        bool sameLineBefore = false)
    {
        var needsSpacing = sameLineBefore;
        var index = leftToRight ? 0 : entries.Count - 1;

        while (leftToRight ? index < entries.Count : index >= 0)
        {
            if (PluginEntryRowLayout.TryGetPluginRowSpan(entries, index, out var rowStart, out var rowEnd))
            {
                ImGui.PushID(rowStart);
                DrawPluginEntryRow(entries, rowStart, rowEnd, needsSpacing);
                ImGui.PopID();
                needsSpacing = true;
                index = leftToRight ? rowEnd + 1 : rowStart - 1;
                continue;
            }

            ImGui.PushID(index);

            if (needsSpacing)
            {
                var spacing = entries[index].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;
                ImGui.SameLine(0f, spacing);
            }

            DrawEntry(entries[index]);
            ImGui.PopID();
            needsSpacing = true;
            index += leftToRight ? 1 : -1;
        }
    }

    private static float MeasureHorizontalRowWidth(IReadOnlyList<VisibleDtrEntry> entries) =>
        MeasureHorizontalRowWidth(entries, OverlayPluginFlow.UseHorizontalLeftToRight());

    private static float MeasureHorizontalRowWidth(IReadOnlyList<VisibleDtrEntry> entries, bool leftToRight)
    {
        if (entries.Count == 0)
            return 0f;

        var width = 0f;
        var needsSpacing = false;
        var index = leftToRight ? 0 : entries.Count - 1;

        while (leftToRight ? index < entries.Count : index >= 0)
        {
            if (PluginEntryRowLayout.TryGetPluginRowSpan(entries, index, out var rowStart, out var rowEnd))
            {
                if (needsSpacing)
                    width += entries[rowStart].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;

                width += MeasurePluginEntryGroupWidth(entries, rowStart, rowEnd);
                needsSpacing = true;
                index = leftToRight ? rowEnd + 1 : rowStart - 1;
                continue;
            }

            if (needsSpacing)
                width += entries[index].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;

            width += MeasureEntry(entries[index]).X;
            needsSpacing = true;
            index += leftToRight ? 1 : -1;
        }

        return width;
    }
}
