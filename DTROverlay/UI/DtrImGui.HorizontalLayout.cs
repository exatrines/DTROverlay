using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class DtrImGui
{
    private static void DrawHorizontalEntriesCore(IReadOnlyList<VisibleDtrEntry> entries)
    {
        SplitNativeAndPluginEntries(entries, out var nativeEntries, out var pluginEntries);

        if (nativeEntries.Count == 0)
        {
            DrawHorizontalPluginRow(InsertPluginSeparators(pluginEntries));
            return;
        }

        if (pluginEntries.Count == 0)
        {
            DrawHorizontalRowLeftToRight(nativeEntries);
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
        var showDivisionSeparator = ShouldShowDivisionSeparator(true, true);
        var continuePluginsOnSameLine = showDivisionSeparator;

        if (OverlayPluginFlow.UseHorizontalLeftToRight || UsesNewLineDivision)
        {
            DrawHorizontalRowLeftToRight(nativeEntries);
            if (showDivisionSeparator)
            {
                ImGui.SameLine(0f, DtrStyle.EntrySpacing);
                DrawEntry(CreateDivisionSeparator());
            }

            DrawHorizontalPluginRow(pluginEntries, sameLineBefore: continuePluginsOnSameLine);
            return;
        }

        DrawHorizontalPluginRow(pluginEntries);
        if (showDivisionSeparator)
        {
            ImGui.SameLine(0f, DtrStyle.EntrySpacing);
            DrawEntry(CreateDivisionSeparator());
        }

        DrawHorizontalRowLeftToRight(nativeEntries, sameLineBefore: continuePluginsOnSameLine);
    }

    private static void DrawHorizontalPluginRow(
        IReadOnlyList<VisibleDtrEntry> pluginEntries,
        bool sameLineBefore = false)
    {
        if (OverlayPluginFlow.UseHorizontalLeftToRight)
            DrawHorizontalRowLeftToRight(pluginEntries, sameLineBefore);
        else
            DrawHorizontalRowRightToLeft(pluginEntries, sameLineBefore);
    }

    private static void DrawHorizontalRow(IReadOnlyList<VisibleDtrEntry> entries) =>
        DrawHorizontalPluginRow(entries);

    private static void DrawHorizontalRowLeftToRight(
        IReadOnlyList<VisibleDtrEntry> entries,
        bool sameLineBefore = false)
    {
        var needsSpacing = sameLineBefore;

        for (var i = 0; i < entries.Count;)
        {
            if (TryGetPluginEntryGroupForward(entries, i, out var groupStart, out _, out var groupEnd))
            {
                ImGui.PushID(groupStart);
                DrawHorizontalEntryGroup(entries, groupStart, groupEnd, needsSpacing);
                ImGui.PopID();
                needsSpacing = true;
                i = groupEnd + 1;
                continue;
            }

            ImGui.PushID(i);

            if (needsSpacing)
            {
                var spacing = entries[i].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;
                ImGui.SameLine(0f, spacing);
            }

            DrawEntry(entries[i]);
            ImGui.PopID();
            needsSpacing = true;
            i++;
        }
    }

    private static void DrawHorizontalRowRightToLeft(
        IReadOnlyList<VisibleDtrEntry> entries,
        bool sameLineBefore = false)
    {
        var needsSpacing = sameLineBefore;

        for (var i = entries.Count - 1; i >= 0;)
        {
            if (TryGetPluginEntryGroupAt(entries, i, out var groupStart, out _, out var groupEnd))
            {
                ImGui.PushID(groupStart);
                DrawHorizontalEntryGroup(entries, groupStart, groupEnd, needsSpacing);
                ImGui.PopID();
                needsSpacing = true;
                i = groupStart - 1;
                continue;
            }

            ImGui.PushID(i);

            if (needsSpacing)
            {
                var spacing = entries[i].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;
                ImGui.SameLine(0f, spacing);
            }

            DrawEntry(entries[i]);
            ImGui.PopID();
            needsSpacing = true;
            i--;
        }
    }

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
        OverlayPluginFlow.UseHorizontalLeftToRight
            ? MeasureHorizontalRowWidthLeftToRight(entries)
            : MeasureHorizontalRowWidthRightToLeft(entries);

    private static float MeasureHorizontalRowWidthLeftToRight(IReadOnlyList<VisibleDtrEntry> entries)
    {
        if (entries.Count == 0)
            return 0f;

        var width = 0f;
        var needsSpacing = false;

        for (var i = 0; i < entries.Count;)
        {
            if (TryGetPluginEntryGroupForward(entries, i, out var groupStart, out _, out var groupEnd))
            {
                if (needsSpacing)
                    width += entries[groupStart].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;

                width += MeasurePluginEntryGroupWidth(entries, groupStart, groupEnd);
                needsSpacing = true;
                i = groupEnd + 1;
                continue;
            }

            if (needsSpacing)
                width += entries[i].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;

            width += MeasureEntry(entries[i]).X;
            needsSpacing = true;
            i++;
        }

        return width;
    }

    private static float MeasureHorizontalRowWidthRightToLeft(IReadOnlyList<VisibleDtrEntry> entries)
    {
        if (entries.Count == 0)
            return 0f;

        var width = 0f;
        var needsSpacing = false;

        for (var i = entries.Count - 1; i >= 0;)
        {
            if (TryGetPluginEntryGroupAt(entries, i, out var groupStart, out _, out var groupEnd))
            {
                if (needsSpacing)
                    width += entries[groupStart].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;

                width += MeasurePluginEntryGroupWidth(entries, groupStart, groupEnd);
                needsSpacing = true;
                i = groupStart - 1;
                continue;
            }

            if (needsSpacing)
                width += entries[i].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;

            width += MeasureEntry(entries[i]).X;
            needsSpacing = true;
            i--;
        }

        return width;
    }
}
