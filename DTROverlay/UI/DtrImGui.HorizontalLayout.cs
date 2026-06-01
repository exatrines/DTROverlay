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
            if (TryGetPluginEntryGroupAtIndex(entries, index, leftToRight, out var groupStart, out _, out var groupEnd))
            {
                ImGui.PushID(groupStart);
                DrawHorizontalEntryGroup(entries, groupStart, groupEnd, needsSpacing);
                ImGui.PopID();
                needsSpacing = true;
                index = leftToRight ? groupEnd + 1 : groupStart - 1;
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

    private static bool TryGetPluginEntryGroupAtIndex(
        IReadOnlyList<VisibleDtrEntry> entries,
        int index,
        bool leftToRight,
        out int groupStart,
        out int contentIndex,
        out int groupEnd) =>
        leftToRight
            ? TryGetPluginEntryGroupForward(entries, index, out groupStart, out contentIndex, out groupEnd)
            : TryGetPluginEntryGroupAt(entries, index, out groupStart, out contentIndex, out groupEnd);

    private static void DrawHorizontalEntryGroup(
        IReadOnlyList<VisibleDtrEntry> entries,
        int groupStart,
        int groupEnd,
        bool sameLineBefore)
    {
        var needsSpacing = sameLineBefore;

        for (var i = groupStart; i <= groupEnd; i++)
        {
            if (needsSpacing)
                ImGui.SameLine(0f, entries[i].SameLineSpacingBefore ?? DtrStyle.EntrySpacing);

            DrawEntry(entries[i]);
            needsSpacing = true;
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
            if (TryGetPluginEntryGroupAtIndex(entries, index, leftToRight, out var groupStart, out _, out var groupEnd))
            {
                if (needsSpacing)
                    width += entries[groupStart].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;

                width += MeasurePluginEntryGroupWidth(entries, groupStart, groupEnd);
                needsSpacing = true;
                index = leftToRight ? groupEnd + 1 : groupStart - 1;
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
