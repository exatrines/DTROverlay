using DTROverlay.Services;

namespace DTROverlay.UI;

internal static class OverlayColorPicker
{
    public static void DrawTextColumn(string layoutKey, string idPrefix, bool rowEnabled)
    {
        using (MirageUi.DisabledIf(!rowEnabled))
        {
            var textColorEnabled = EntryFixedWidth.IsTextColorEnabled(layoutKey);
            if (MirageUi.Checkbox($"##textColor_{idPrefix}", ref textColorEnabled))
                EntryFixedWidth.SetTextColorEnabled(layoutKey, textColorEnabled);

            ImGui.SameLine();
            using (MirageUi.DisabledIf(!textColorEnabled))
            {
                var textColor = EntryFixedWidth.GetStoredTextColor(layoutKey);
                if (MirageUi.ColorEdit4("", ref textColor, DtrStyle.ColorEditFlags, $"text_{idPrefix}", 36f)
                    && textColorEnabled)
                    SaveTextColor(layoutKey, textColor);
            }

            DrawColumnResetButton(layoutKey, idPrefix, "Text", () => EntryFixedWidth.ResetTextStyleToDefault(layoutKey));
        }
    }

    public static void DrawEdgeColumn(string layoutKey, string idPrefix, bool rowEnabled)
    {
        using (MirageUi.DisabledIf(!rowEnabled))
        {
            var edgeStyleEnabled = EntryFixedWidth.IsEdgeStyleEnabled(layoutKey);
            if (MirageUi.Checkbox($"##edgeStyle_{idPrefix}", ref edgeStyleEnabled))
                EntryFixedWidth.SetEdgeStyleEnabled(layoutKey, edgeStyleEnabled);

            ImGui.SameLine();
            using (MirageUi.DisabledIf(!edgeStyleEnabled))
            {
                var edgeColor = EntryFixedWidth.GetStoredOutlineColor(layoutKey);
                if (MirageUi.ColorEdit4("", ref edgeColor, DtrStyle.ColorEditFlags, $"edge_{idPrefix}", 36f)
                    && edgeStyleEnabled)
                    SaveEdgeColor(layoutKey, edgeColor);
                MirageUi.Tooltip(I18n.Get("style.edge_color"));

                ImGui.SameLine();
                var edgeStrength = EntryFixedWidth.GetStoredEdgeStrength(layoutKey);
                DrawStrengthControl(layoutKey, idPrefix, edgeStyleEnabled, ref edgeStrength, isEdge: true);
            }

            DrawColumnResetButton(layoutKey, idPrefix, "Edge", () => EntryFixedWidth.ResetEdgeStyleToDefault(layoutKey));
        }
    }

    public static void DrawShadowColumn(string layoutKey, string idPrefix, bool rowEnabled)
    {
        using (MirageUi.DisabledIf(!rowEnabled))
        {
            var shadowStyleEnabled = EntryFixedWidth.IsShadowStyleEnabled(layoutKey);
            if (MirageUi.Checkbox($"##shadowStyle_{idPrefix}", ref shadowStyleEnabled))
                EntryFixedWidth.SetShadowStyleEnabled(layoutKey, shadowStyleEnabled);

            ImGui.SameLine();
            using (MirageUi.DisabledIf(!shadowStyleEnabled))
            {
                var shadowColor = EntryFixedWidth.GetStoredShadowColor(layoutKey);
                if (MirageUi.ColorEdit4("", ref shadowColor, DtrStyle.ColorEditFlags, $"shadow_{idPrefix}", 36f)
                    && shadowStyleEnabled)
                    SaveShadowColor(layoutKey, shadowColor);

                ImGui.SameLine();
                var shadowThickness = EntryFixedWidth.GetStoredShadowThickness(layoutKey);
                DrawStrengthControl(layoutKey, idPrefix, shadowStyleEnabled, ref shadowThickness, isEdge: false);
            }

            DrawColumnResetButton(layoutKey, idPrefix, "Shadow", () => EntryFixedWidth.ResetShadowStyleToDefault(layoutKey));
        }
    }

    private static void DrawColumnResetButton(string layoutKey, string idPrefix, string column, Action reset)
    {
        ImGui.SameLine();
        if (MirageUi.IconButton(
                FontAwesomeIcon.Undo,
                $"reset{column}_{idPrefix}",
                new Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight()),
                tooltip: GetResetTooltip(layoutKey, column)))
            reset();
    }

    private static void DrawStrengthControl(
        string layoutKey,
        string idPrefix,
        bool effectEnabled,
        ref float strength,
        bool isEdge)
    {
        using (MirageUi.DisabledIf(!effectEnabled))
        {
            if (isEdge)
            {
                if (MirageUi.SliderFloat("", ref strength, 0f, DtrStyle.MaxEdgeStrength, "%.2f", $"edgeStr_{idPrefix}", 48f)
                    && effectEnabled)
                    SaveEdgeStrength(layoutKey, strength);
                MirageUi.Tooltip(I18n.Get("style.edge_strength"));
            }
            else
            {
                if (MirageUi.SliderFloat("", ref strength, 0f, DtrStyle.MaxShadowThickness, "%.1f", $"shadowStr_{idPrefix}", 48f)
                    && effectEnabled)
                    SaveShadowThickness(layoutKey, strength);
                MirageUi.Tooltip(I18n.Get("style.shadow_radius"));
            }
        }
    }

    private static string GetResetTooltip(string layoutKey, string column)
    {
        var col = ColumnLabel(column);
        if (OverlayEntryIds.IsDefaultText(layoutKey) || OverlayEntryIds.IsDefaultSeparator(layoutKey))
            return I18n.Format("style.reset.origin", col);

        if (GroupStyleKeys.IsOverrideKey(layoutKey))
            return I18n.Format("style.reset.default", col);

        if (GroupStyleKeys.IsPluginEntryKey(layoutKey))
            return I18n.Format("style.reset.plugin", col);

        return I18n.Format("style.reset.row", col);
    }

    private static string ColumnLabel(string column) => column switch
    {
        "Edge" => I18n.Get("style.column.edge"),
        "Shadow" => I18n.Get("style.column.shadow"),
        _ => I18n.Get("style.column.text"),
    };

    private static void SaveTextColor(string layoutKey, Vector4 color)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.TextColor = color;
        else
            C.FixedWidthTextColors[layoutKey] = color;

        C.Save();
    }

    private static void SaveEdgeColor(string layoutKey, Vector4 color)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.OutlineColor = color;
        else
            C.FixedWidthOutlineColors[layoutKey] = color;

        C.Save();
    }

    private static void SaveShadowColor(string layoutKey, Vector4 color)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.ShadowColor = color;
        else
            C.FixedWidthShadowColors[layoutKey] = color;

        C.Save();
    }

    private static void SaveEdgeStrength(string layoutKey, float strength)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.EdgeStrength = strength;
        else
            C.FixedWidthEdgeStrengths[layoutKey] = strength;

        C.Save();
    }

    private static void SaveShadowThickness(string layoutKey, float thickness)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.ShadowThickness = thickness;
        else
            C.FixedWidthShadowThicknesses[layoutKey] = thickness;

        C.Save();
    }
}
