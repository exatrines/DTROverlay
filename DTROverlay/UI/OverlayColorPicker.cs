using Dalamud.Interface;
using DTROverlay.Services;

namespace DTROverlay.UI;

internal static class OverlayColorPicker
{
    public static void DrawTextColumn(string layoutKey, string idPrefix, bool rowEnabled)
    {
        ImGui.BeginDisabled(!rowEnabled);

        var textColorEnabled = EntryFixedWidth.IsTextColorEnabled(layoutKey);
        if (ImGui.Checkbox($"##textColor_{idPrefix}", ref textColorEnabled))
            EntryFixedWidth.SetTextColorEnabled(layoutKey, textColorEnabled);

        ImGui.SameLine();
        ImGui.BeginDisabled(!textColorEnabled);

        var textColor = EntryFixedWidth.GetStoredTextColor(layoutKey);
        if (ImGui.ColorEdit4($"Text##{idPrefix}", ref textColor, DtrStyle.ColorEditFlags) && textColorEnabled)
            SaveTextColor(layoutKey, textColor);

        ImGui.EndDisabled();
        DrawColumnResetButton(layoutKey, idPrefix, "Text", () => EntryFixedWidth.ResetTextStyleToDefault(layoutKey));
        ImGui.EndDisabled();
    }

    public static void DrawEdgeColumn(string layoutKey, string idPrefix, bool rowEnabled)
    {
        ImGui.BeginDisabled(!rowEnabled);

        var edgeStyleEnabled = EntryFixedWidth.IsEdgeStyleEnabled(layoutKey);
        if (ImGui.Checkbox($"##edgeStyle_{idPrefix}", ref edgeStyleEnabled))
            EntryFixedWidth.SetEdgeStyleEnabled(layoutKey, edgeStyleEnabled);

        ImGui.SameLine();
        ImGui.BeginDisabled(!edgeStyleEnabled);

        var edgeColor = EntryFixedWidth.GetStoredOutlineColor(layoutKey);
        if (ImGui.ColorEdit4($"Edge##{idPrefix}", ref edgeColor, DtrStyle.ColorEditFlags) && edgeStyleEnabled)
            SaveEdgeColor(layoutKey, edgeColor);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip("SeString edge (outline) color.");

        ImGui.SameLine();
        var edgeStrength = EntryFixedWidth.GetStoredEdgeStrength(layoutKey);
        DrawStrengthControl(layoutKey, idPrefix, edgeStyleEnabled, ref edgeStrength, isEdge: true);

        ImGui.EndDisabled();
        DrawColumnResetButton(layoutKey, idPrefix, "Edge", () => EntryFixedWidth.ResetEdgeStyleToDefault(layoutKey));
        ImGui.EndDisabled();
    }

    public static void DrawShadowColumn(string layoutKey, string idPrefix, bool rowEnabled)
    {
        ImGui.BeginDisabled(!rowEnabled);

        var shadowStyleEnabled = EntryFixedWidth.IsShadowStyleEnabled(layoutKey);
        if (ImGui.Checkbox($"##shadowStyle_{idPrefix}", ref shadowStyleEnabled))
            EntryFixedWidth.SetShadowStyleEnabled(layoutKey, shadowStyleEnabled);

        ImGui.SameLine();
        ImGui.BeginDisabled(!shadowStyleEnabled);

        var shadowColor = EntryFixedWidth.GetStoredShadowColor(layoutKey);
        if (ImGui.ColorEdit4($"Shadow##{idPrefix}", ref shadowColor, DtrStyle.ColorEditFlags) && shadowStyleEnabled)
            SaveShadowColor(layoutKey, shadowColor);

        ImGui.SameLine();
        var shadowThickness = EntryFixedWidth.GetStoredShadowThickness(layoutKey);
        DrawStrengthControl(layoutKey, idPrefix, shadowStyleEnabled, ref shadowThickness, isEdge: false);

        ImGui.EndDisabled();
        DrawColumnResetButton(layoutKey, idPrefix, "Shadow", () => EntryFixedWidth.ResetShadowStyleToDefault(layoutKey));
        ImGui.EndDisabled();
    }

    private static void DrawColumnResetButton(string layoutKey, string idPrefix, string column, Action reset)
    {
        ImGui.SameLine();
        if (DtrImGui.SmallIconButton(FontAwesomeIcon.Undo, $"##reset{column}_{idPrefix}"))
        {
            reset();
            EzConfig.Save();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(GetResetTooltip(layoutKey, column));
    }

    private static void DrawStrengthControl(
        string layoutKey,
        string idPrefix,
        bool effectEnabled,
        ref float strength,
        bool isEdge)
    {
        ImGui.BeginDisabled(!effectEnabled);
        ImGui.SetNextItemWidth(40f);

        if (isEdge)
        {
            if (ImGui.DragFloat($"E+##{idPrefix}", ref strength, 0.01f, 0f, DtrStyle.MaxEdgeStrength, "%.2f")
                && effectEnabled)
                SaveEdgeStrength(layoutKey, strength);

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                ImGui.SetTooltip("Edge strength (0–1).");
        }
        else
        {
            if (ImGui.DragFloat($"S+##{idPrefix}", ref strength, 0.05f, 0f, DtrStyle.MaxShadowThickness, "%.1f")
                && effectEnabled)
                SaveShadowThickness(layoutKey, strength);

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                ImGui.SetTooltip("Soft shadow radius in pixels (0 = off, fractional values allowed).");
        }

        ImGui.EndDisabled();
    }

    private static string GetResetTooltip(string layoutKey, string column)
    {
        var col = column.ToLowerInvariant();
        if (OverlayEntryIds.IsDefaultText(layoutKey) || OverlayEntryIds.IsDefaultSeparator(layoutKey))
            return $"Copy Origin {col} into this row's stored parameters.";

        if (GroupStyleKeys.IsOverrideKey(layoutKey))
            return $"Copy Default Style {col} stored values into this override row.";

        if (GroupStyleKeys.IsPluginEntryKey(layoutKey))
            return $"Copy group Override Text {col} stored values into this plugin row.";

        return $"Copy group Override Text {col} stored values into this row.";
    }

    private static void SaveTextColor(string layoutKey, Vector4 color)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.TextColor = color;
        else
            C.FixedWidthTextColors[layoutKey] = color;
    }

    private static void SaveEdgeColor(string layoutKey, Vector4 color)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.OutlineColor = color;
        else
            C.FixedWidthOutlineColors[layoutKey] = color;
    }

    private static void SaveShadowColor(string layoutKey, Vector4 color)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.ShadowColor = color;
        else
            C.FixedWidthShadowColors[layoutKey] = color;
    }

    private static void SaveEdgeStrength(string layoutKey, float strength)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.EdgeStrength = strength;
        else
            C.FixedWidthEdgeStrengths[layoutKey] = strength;
    }

    private static void SaveShadowThickness(string layoutKey, float thickness)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            C.ShadowThickness = thickness;
        else
            C.FixedWidthShadowThicknesses[layoutKey] = thickness;
    }
}
