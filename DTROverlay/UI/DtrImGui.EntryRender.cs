using System.Linq;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class DtrImGui
{
    private static Vector2 MeasureEntry(VisibleDtrEntry entry)
    {
        var content = MeasureEntryContent(entry);
        return new Vector2(EntryFixedWidth.ResolveWidth(ResolveLayoutKey(entry), content.X), content.Y);
    }

    private static string ResolveLayoutKey(VisibleDtrEntry entry) =>
        !string.IsNullOrEmpty(entry.LayoutKey)
            ? entry.LayoutKey
            : entry.ColorLayoutKey;

    private static Vector2 MeasureEntryContent(VisibleDtrEntry entry)
    {
        var colorLayoutKey = ResolveColorLayoutKey(entry);
        return entry.Kind switch
        {
            VisibleDtrEntryKind.SeString => MeasureSeString(entry.SeStringData, colorLayoutKey),
            VisibleDtrEntryKind.Image => MeasureImage(entry.Image, entry.ImageScale),
            _ => MeasureText(entry.Text),
        };
    }

    private static Vector2 MeasureSeString(byte[] data, string colorLayoutKey)
    {
        if (data.Length == 0)
            return Vector2.Zero;

        var drawParams = CreateSeStringDrawParams(colorLayoutKey, 1f);
        drawParams.TargetDrawList = ImGui.GetWindowDrawList();
        drawParams.ScreenOffset = new Vector2(-10000f, -10000f);

        var measured = ImGuiHelpers.SeStringWrapped(data, drawParams);
        return measured.Size == Vector2.Zero
            ? Vector2.Zero
            : new Vector2(measured.Size.X, LineHeight);
    }

    private static Vector2 MeasureImage(DtrImageInfo image, float imageScale)
    {
        var size = DtrNativeImage.GetDisplaySize(image, IconHeight * (imageScale > 0f ? imageScale : 1f));
        return new Vector2(size.X, LineHeight);
    }

    private static Vector2 MeasureText(string text) =>
        string.IsNullOrEmpty(text) ? Vector2.Zero : new Vector2(ImGui.CalcTextSize(text).X, LineHeight);

    private static string ResolveColorLayoutKey(VisibleDtrEntry entry) =>
        !string.IsNullOrEmpty(entry.ColorLayoutKey) ? entry.ColorLayoutKey : entry.LayoutKey;

    private static void DrawEntry(VisibleDtrEntry entry)
    {
        var layoutKey = ResolveLayoutKey(entry);
        var colorLayoutKey = ResolveColorLayoutKey(entry);

        switch (entry.Kind)
        {
            case VisibleDtrEntryKind.SeString:
                DrawStyledSeString(
                    entry.SeStringData,
                    entry.OnClick,
                    entry.DtrEntryTitle,
                    entry.Opacity,
                    layoutKey,
                    colorLayoutKey,
                    entry.HoverTooltipSeStringData);
                break;
            case VisibleDtrEntryKind.Image:
                DrawImage(
                    entry.Image,
                    entry.Opacity,
                    entry.ImageScale,
                    entry.LayoutKey,
                    entry.HoverTooltip,
                    entry.OnClick,
                    entry.DtrEntryTitle,
                    entry.HoverTooltipSeStringData);
                break;
            default:
                if (DtrSeparatorStyle.IsSeparatorKey(layoutKey))
                {
                    DrawSeparatorSlot(
                        layoutKey,
                        colorLayoutKey,
                        entry.Opacity,
                        entry.HoverTooltip,
                        entry.OnClick,
                        entry.DtrEntryTitle,
                        entry.HoverTooltipSeStringData);
                    break;
                }

                DrawStyledText(
                    entry.Text,
                    entry.Opacity,
                    layoutKey,
                    colorLayoutKey,
                    entry.HoverTooltip,
                    entry.OnClick,
                    entry.DtrEntryTitle,
                    entry.HoverTooltipSeStringData);
                break;
        }
    }

    private static SeStringDrawParams CreateSeStringDrawParams(string colorLayoutKey, float opacity)
    {
        var edgeEnabled = EntryFixedWidth.IsEdgeEnabled(colorLayoutKey);
        return new SeStringDrawParams
        {
            Font = ImGui.GetFont(),
            FontSize = ImGui.GetFontSize(),
            LineHeight = 1f,
            Edge = edgeEnabled,
            Shadow = false,
            ForceEdgeColor = edgeEnabled,
            EdgeStrength = EntryFixedWidth.GetEdgeStrength(colorLayoutKey),
            Color = ApplyOpacity(EntryFixedWidth.GetTextColor(colorLayoutKey), opacity),
            EdgeColor = ApplyOpacity(EntryFixedWidth.GetOutlineColor(colorLayoutKey), opacity),
            ShadowColor = ApplyOpacity(EntryFixedWidth.GetShadowColor(colorLayoutKey), opacity),
        };
    }

    private static void DrawSeStringWithEffects(byte[] data, string colorLayoutKey, float opacity, Vector2 pos)
    {
        var drawParams = CreateSeStringDrawParams(colorLayoutKey, opacity);
        drawParams.TargetDrawList = ImGui.GetWindowDrawList();

        if (EntryFixedWidth.IsShadowEnabled(colorLayoutKey))
        {
            SeStringSoftShadow.Draw(
                data,
                drawParams,
                pos,
                EntryFixedWidth.GetShadowThickness(colorLayoutKey));
        }

        var finalParams = drawParams;
        finalParams.ScreenOffset = pos;
        ImGuiHelpers.SeStringWrapped(data, finalParams);
    }

    private static DtrOverlayGroup TooltipGroup => OverlayStyleContext.Group;

    private static SeStringDrawParams CreateTooltipSeStringDrawParams() =>
        new SeStringDrawParams
        {
            Font = ImGui.GetFont(),
            FontSize = ImGui.GetFontSize(),
            LineHeight = 1f,
            Edge = false,
            Color = ImGui.ColorConvertFloat4ToU32(OverlayTooltipResolver.GetEffectiveTextColor(TooltipGroup)),
        };

    private static void DrawStyledSeString(
        byte[] data,
        Action<DtrInteractionEvent> onClick,
        string dtrEntryTitle,
        float opacity,
        string layoutKey,
        string colorLayoutKey,
        byte[] hoverTooltipSeStringData)
    {
        if (data.Length == 0)
            return;

        var measureParams = CreateSeStringDrawParams(colorLayoutKey, 1f);
        measureParams.TargetDrawList = ImGui.GetWindowDrawList();
        measureParams.ScreenOffset = new Vector2(-10000f, -10000f);

        var measured = ImGuiHelpers.SeStringWrapped(data, measureParams);
        var contentSize = measured.Size;
        if (contentSize == Vector2.Zero)
            return;

        var slotWidth = EntryFixedWidth.ResolveWidth(layoutKey, contentSize.X);
        var slotSize = new Vector2(slotWidth, LineHeight);

        if (opacity > 0f)
            DrawSeStringWithEffects(data, colorLayoutKey, opacity, GetAlignedPos(contentSize, layoutKey));

        AdvanceEntry(slotSize, onClick, dtrEntryTitle, hoverTooltipSeStringData: hoverTooltipSeStringData);
    }

    private static void DrawImage(
        DtrImageInfo image,
        float opacity,
        float imageScale,
        string layoutKey,
        string hoverTooltip,
        Action<DtrInteractionEvent> onClick,
        string dtrEntryTitle,
        byte[] hoverTooltipSeStringData)
    {
        var wrap = Svc.Texture.GetFromGame(image.TexturePath).GetWrapOrDefault();
        var size = DtrNativeImage.GetDisplaySize(image, IconHeight * (imageScale > 0f ? imageScale : 1f));
        var slotWidth = EntryFixedWidth.ResolveWidth(layoutKey, size.X);
        var slotSize = new Vector2(slotWidth, LineHeight);

        if (wrap == null)
        {
            AdvanceEntry(slotSize, onClick, dtrEntryTitle, hoverTooltip, hoverTooltipSeStringData);
            return;
        }

        if (opacity > 0f)
        {
            var (uv0, uv1) = DtrNativeImage.GetUvCoords(image);
            var pos = GetAlignedPos(size, layoutKey);
            var tint = ApplyOpacity(Vector4.One, opacity);
            ImGui.GetWindowDrawList().AddImage(wrap.Handle, pos, pos + size, uv0, uv1, tint);
        }

        AdvanceEntry(slotSize, onClick, dtrEntryTitle, hoverTooltip, hoverTooltipSeStringData);
    }

    private static void DrawSeparatorSlot(
        string layoutKey,
        string colorLayoutKey,
        float opacity,
        string hoverTooltip,
        Action<DtrInteractionEvent> onClick,
        string dtrEntryTitle,
        byte[] hoverTooltipSeStringData)
    {
        var displayText = DtrSeparatorStyle.GetDisplayGlyph(layoutKey);
        var contentSize = string.IsNullOrEmpty(displayText)
            ? Vector2.Zero
            : ImGui.CalcTextSize(displayText);
        var slotWidth = EntryFixedWidth.ResolveWidth(layoutKey, contentSize.X);
        var slotSize = new Vector2(slotWidth, LineHeight);

        if (opacity > 0f && !string.IsNullOrEmpty(displayText))
        {
            var pos = GetAlignedPos(contentSize, layoutKey);
            var drawList = ImGui.GetWindowDrawList();
            var font = ImGui.GetFont();
            var fontSize = ImGui.GetFontSize();
            var textColor = ApplyOpacity(EntryFixedWidth.GetTextColor(colorLayoutKey), opacity);

            DrawManualSoftShadow(drawList, font, fontSize, pos, colorLayoutKey, opacity, displayText);
            DrawManualOutline(drawList, font, fontSize, pos, colorLayoutKey, opacity, displayText);

            drawList.AddText(font, fontSize, pos, textColor, displayText);
        }

        AdvanceEntry(slotSize, onClick, dtrEntryTitle, hoverTooltip, hoverTooltipSeStringData);
    }

    private static void DrawStyledText(
        string text,
        float opacity,
        string layoutKey,
        string colorLayoutKey,
        string hoverTooltip,
        Action<DtrInteractionEvent> onClick,
        string dtrEntryTitle,
        byte[] hoverTooltipSeStringData)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var font = ImGui.GetFont();
        var fontSize = ImGui.GetFontSize();
        var contentSize = ImGui.CalcTextSize(text);
        var slotWidth = EntryFixedWidth.ResolveWidth(layoutKey, contentSize.X);
        var slotSize = new Vector2(slotWidth, LineHeight);

        if (opacity > 0f)
        {
            var pos = GetAlignedPos(contentSize, layoutKey);
            var drawList = ImGui.GetWindowDrawList();
            var textColor = ApplyOpacity(EntryFixedWidth.GetTextColor(colorLayoutKey), opacity);

            DrawManualSoftShadow(drawList, font, fontSize, pos, colorLayoutKey, opacity, text);
            DrawManualOutline(drawList, font, fontSize, pos, colorLayoutKey, opacity, text);

            drawList.AddText(font, fontSize, pos, textColor, text);
        }

        AdvanceEntry(slotSize, onClick, dtrEntryTitle, hoverTooltip, hoverTooltipSeStringData);
    }

    private static void DrawManualSoftShadow(
        ImDrawListPtr drawList,
        ImFontPtr font,
        float fontSize,
        Vector2 pos,
        string colorLayoutKey,
        float opacity,
        string text)
    {
        if (!EntryFixedWidth.IsShadowEnabled(colorLayoutKey))
            return;

        var radius = EntryFixedWidth.GetShadowThickness(colorLayoutKey);
        if (radius <= 0f)
            return;

        var shadowColor = ApplyOpacity(EntryFixedWidth.GetShadowColor(colorLayoutKey), opacity);
        SeStringSoftShadow.DrawPlainText(drawList, font, fontSize, pos, shadowColor, radius, text);
    }

    /// <summary>
    /// ImGui text outline ignores <see cref="SeStringDrawParams.EdgeStrength"/>; scale alpha from strength (memo: global edge 0.02).
    /// </summary>
    private static void DrawManualOutline(
        ImDrawListPtr drawList,
        ImFontPtr font,
        float fontSize,
        Vector2 pos,
        string colorLayoutKey,
        float opacity,
        string text)
    {
        if (!EntryFixedWidth.IsEdgeEnabled(colorLayoutKey))
            return;

        var strength = EntryFixedWidth.GetEdgeStrength(colorLayoutKey);
        if (strength <= 0f)
            return;

        var outlineColor = ApplyOpacity(EntryFixedWidth.GetOutlineColor(colorLayoutKey), opacity * strength);
        foreach (var offset in DtrStyle.OutlineOffsets)
            drawList.AddText(font, fontSize, pos + offset, outlineColor, text);
    }

    private static Vector2 GetAlignedPos(Vector2 contentSize, string layoutKey)
    {
        var pos = ImGui.GetCursorScreenPos();
        var slotWidth = EntryFixedWidth.ResolveWidth(layoutKey, contentSize.X);
        pos.X = MathF.Floor(pos.X + EntryFixedWidth.GetContentOffsetX(layoutKey, slotWidth, contentSize.X));
        pos.Y = MathF.Floor(pos.Y + (LineHeight - contentSize.Y) * 0.5f);
        return pos;
    }

    private static uint ApplyOpacity(Vector4 color, float opacity) =>
        ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, color.W * opacity));

    private static void AdvanceEntry(
        Vector2 size,
        Action<DtrInteractionEvent> onClick = null,
        string dtrEntryTitle = null,
        string hoverTooltip = null,
        byte[] hoverTooltipSeStringData = null)
    {
        if (C.OverlayGroups != null && C.OverlayGroups.Any(g => g.OverlayEditMode))
        {
            ImGui.Dummy(size);
            return;
        }

        var hasOnClick = onClick != null;
        var hasMiddleClickUi = C.OpenPluginUiOnMiddleClick && !string.IsNullOrEmpty(dtrEntryTitle);
        var hasHoverTooltip = !string.IsNullOrEmpty(hoverTooltip)
            || hoverTooltipSeStringData is { Length: > 0 };

        if (!hasOnClick && !hasMiddleClickUi && !hasHoverTooltip)
        {
            ImGui.Dummy(size);
            return;
        }

        ImGui.InvisibleButton("##dtrClick", size);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && hasOnClick)
            InvokeDtrClick(onClick, MouseClickType.Left);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && hasOnClick)
            InvokeDtrClick(onClick, MouseClickType.Right);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Middle) && hasMiddleClickUi)
            DtrPluginUiOpener.TryOpen(dtrEntryTitle);

        if (!ImGui.IsItemHovered())
            return;

        var itemMin = ImGui.GetItemRectMin();
        var itemMax = ImGui.GetItemRectMax();

        if (hoverTooltipSeStringData is { Length: > 0 })
            DrawStyledSeStringTooltip(hoverTooltipSeStringData, itemMin, itemMax);
        else if (!string.IsNullOrEmpty(hoverTooltip))
            DrawStyledTooltip(hoverTooltip, itemMin, itemMax);
        else if (hasOnClick)
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
    }

    private static void PushTooltipStyle()
    {
        var group = TooltipGroup;
        ImGui.PushStyleColor(ImGuiCol.PopupBg, OverlayTooltipResolver.GetEffectiveBackgroundColor(group));
        ImGui.PushStyleColor(ImGuiCol.Text, OverlayTooltipResolver.GetEffectiveTextColor(group));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, DtrStyle.TooltipWindowRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, DtrStyle.TooltipWindowPadding);
    }

    private static void PopTooltipStyle()
    {
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(2);
    }

    private static void BeginEntryTooltip(Vector2 itemMin, Vector2 itemMax)
    {
        switch (OverlayTooltipResolver.GetEffectivePosition(TooltipGroup))
        {
            case TooltipPosition.Lower:
            {
                var centerX = (itemMin.X + itemMax.X) * 0.5f;
                var belowY = itemMax.Y + DtrStyle.TooltipEntrySpacing;
                ImGui.SetNextWindowPos(new Vector2(centerX, belowY), ImGuiCond.Always, new Vector2(0.5f, 0f));
                break;
            }
            case TooltipPosition.Upper:
            {
                var centerX = (itemMin.X + itemMax.X) * 0.5f;
                var aboveY = itemMin.Y - DtrStyle.TooltipEntrySpacing;
                ImGui.SetNextWindowPos(new Vector2(centerX, aboveY), ImGuiCond.Always, new Vector2(0.5f, 1f));
                break;
            }
        }

        ImGui.BeginTooltip();
    }

    private static void DrawStyledSeStringTooltip(byte[] data, Vector2 itemMin, Vector2 itemMax)
    {
        if (data.Length == 0)
            return;

        PushTooltipStyle();
        using var fontPush = DtrOverlayFonts.PushTooltip();

        var drawParams = CreateTooltipSeStringDrawParams();
        drawParams.TargetDrawList = ImGui.GetWindowDrawList();
        drawParams.ScreenOffset = new Vector2(-10000f, -10000f);

        var measured = ImGuiHelpers.SeStringWrapped(data, drawParams);
        if (measured.Size == Vector2.Zero)
        {
            PopTooltipStyle();
            return;
        }

        BeginEntryTooltip(itemMin, itemMax);

        var pos = ImGui.GetCursorScreenPos();
        ImGui.Dummy(measured.Size);
        drawParams.ScreenOffset = pos;
        drawParams.TargetDrawList = ImGui.GetWindowDrawList();
        ImGuiHelpers.SeStringWrapped(data, drawParams);

        ImGui.EndTooltip();
        PopTooltipStyle();
    }

    private static void DrawStyledTooltip(string text, Vector2 itemMin, Vector2 itemMax)
    {
        PushTooltipStyle();
        BeginEntryTooltip(itemMin, itemMax);
        using var fontPush = DtrOverlayFonts.PushTooltip();
        ImGui.TextUnformatted(text);
        ImGui.EndTooltip();
        PopTooltipStyle();
    }

    private static void InvokeDtrClick(Action<DtrInteractionEvent> onClick, MouseClickType clickType)
    {
        onClick(new DtrInteractionEvent
        {
            ClickType = clickType,
            ModifierKeys = GetModifierKeys(),
            ScrollDirection = MouseScrollDirection.None,
            Position = ImGui.GetIO().MousePos,
        });
    }

    private static ClickModifierKeys GetModifierKeys()
    {
        var io = ImGui.GetIO();
        var modifiers = ClickModifierKeys.None;
        if (io.KeyCtrl)
            modifiers |= ClickModifierKeys.Ctrl;
        if (io.KeyShift)
            modifiers |= ClickModifierKeys.Shift;
        if (io.KeyAlt)
            modifiers |= ClickModifierKeys.Alt;
        return modifiers;
    }

    private static void AdvanceEntry(float width)
    {
        AdvanceEntry(new Vector2(width, LineHeight));
    }
}
