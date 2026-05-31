using Dalamud.Interface;
using DTROverlay.Services;

namespace DTROverlay.UI;

internal static class OverlayColorPicker
{
    public static void DrawRow(string layoutKey, string idPrefix, bool rowEnabled)
    {
        ImGui.BeginDisabled(!rowEnabled);

        var colorEnabled = EntryFixedWidth.IsColorEnabled(layoutKey);
        if (ImGui.Checkbox($"##color_{idPrefix}", ref colorEnabled))
            EntryFixedWidth.SetColorEnabled(layoutKey, colorEnabled);

        ImGui.SameLine();
        var textColor = GetTextColor(layoutKey);
        var outlineColor = GetOutlineColor(layoutKey);

        ImGui.BeginDisabled(!colorEnabled);
        if (ImGui.ColorEdit4($"Text##{idPrefix}", ref textColor, DtrStyle.ColorEditFlags) && colorEnabled)
            SaveTextColor(layoutKey, textColor);

        ImGui.SameLine();
        if (ImGui.ColorEdit4($"Outline##{idPrefix}", ref outlineColor, DtrStyle.ColorEditFlags) && colorEnabled)
            SaveOutlineColor(layoutKey, outlineColor);

        ImGui.SameLine();
        if (DtrImGui.SmallIconButton(FontAwesomeIcon.Undo, $"##colorReset_{idPrefix}"))
        {
            EntryFixedWidth.ResetColorsToDefault(layoutKey);
            textColor = GetTextColor(layoutKey);
            outlineColor = GetOutlineColor(layoutKey);
        }

        ImGui.EndDisabled();
        ImGui.EndDisabled();
    }

    public static void DrawPluginEntryColors(string layoutKey, string idPrefix)
    {
        var colorEnabled = EntryFixedWidth.IsColorEnabled(layoutKey);
        if (ImGui.Checkbox($"##color_{idPrefix}", ref colorEnabled))
            EntryFixedWidth.SetColorEnabled(layoutKey, colorEnabled);

        if (!C.FixedWidthTextColors.TryGetValue(layoutKey, out var textColor))
            textColor = EntryFixedWidth.GetDefaultTextColor();

        if (!C.FixedWidthOutlineColors.TryGetValue(layoutKey, out var outlineColor))
            outlineColor = EntryFixedWidth.GetDefaultOutlineColor();

        ImGui.SameLine();
        ImGui.BeginDisabled(!colorEnabled);
        if (ImGui.ColorEdit4($"Text##{idPrefix}", ref textColor, DtrStyle.ColorEditFlags) && colorEnabled)
            C.FixedWidthTextColors[layoutKey] = textColor;

        ImGui.SameLine();
        if (ImGui.ColorEdit4($"Outline##{idPrefix}", ref outlineColor, DtrStyle.ColorEditFlags) && colorEnabled)
            C.FixedWidthOutlineColors[layoutKey] = outlineColor;

        ImGui.SameLine();
        if (DtrImGui.SmallIconButton(FontAwesomeIcon.Undo, $"##colorReset_{idPrefix}"))
        {
            EntryFixedWidth.ResetColorsToDefault(layoutKey);
            textColor = EntryFixedWidth.GetDefaultTextColor();
            outlineColor = EntryFixedWidth.GetDefaultOutlineColor();
        }

        ImGui.EndDisabled();
    }

    private static Vector4 GetTextColor(string layoutKey) =>
        OverlayEntryIds.IsAppearanceText(layoutKey)
            ? C.TextColor
            : C.FixedWidthTextColors.TryGetValue(layoutKey, out var color)
                ? color
                : EntryFixedWidth.GetDefaultTextColor();

    private static Vector4 GetOutlineColor(string layoutKey) =>
        OverlayEntryIds.IsAppearanceText(layoutKey)
            ? C.OutlineColor
            : C.FixedWidthOutlineColors.TryGetValue(layoutKey, out var color)
                ? color
                : EntryFixedWidth.GetDefaultOutlineColor();

    private static void SaveTextColor(string layoutKey, Vector4 color)
    {
        if (OverlayEntryIds.IsAppearanceText(layoutKey))
            C.TextColor = color;
        else
            C.FixedWidthTextColors[layoutKey] = color;
    }

    private static void SaveOutlineColor(string layoutKey, Vector4 color)
    {
        if (OverlayEntryIds.IsAppearanceText(layoutKey))
            C.OutlineColor = color;
        else
            C.FixedWidthOutlineColors[layoutKey] = color;
    }
}
