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
        return new Vector2(EntryFixedWidth.ResolveWidth(entry.LayoutKey, content.X), content.Y);
    }

    private static Vector2 MeasureEntryContent(VisibleDtrEntry entry) =>
        entry.Kind switch
        {
            VisibleDtrEntryKind.SeString => MeasureSeString(entry.SeStringData),
            VisibleDtrEntryKind.Image => MeasureImage(entry.Image, entry.ImageScale),
            _ => MeasureText(entry.Text),
        };

    private static Vector2 MeasureSeString(byte[] data)
    {
        if (data.Length == 0)
            return Vector2.Zero;

        var drawParams = CreateSeStringDrawParams();
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
        var colorLayoutKey = ResolveColorLayoutKey(entry);

        switch (entry.Kind)
        {
            case VisibleDtrEntryKind.SeString:
                DrawStyledSeString(
                    entry.SeStringData,
                    entry.OnClick,
                    entry.DtrEntryTitle,
                    entry.Opacity,
                    entry.LayoutKey,
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
                DrawStyledText(
                    entry.Text,
                    entry.Opacity,
                    entry.LayoutKey,
                    colorLayoutKey,
                    entry.HoverTooltip,
                    entry.OnClick,
                    entry.DtrEntryTitle,
                    entry.HoverTooltipSeStringData);
                break;
        }
    }

    private static SeStringDrawParams CreateSeStringDrawParams()
    {
        return new SeStringDrawParams
        {
            Font = ImGui.GetFont(),
            FontSize = ImGui.GetFontSize(),
            LineHeight = 1f,
            Edge = false,
            Color = EntryFixedWidth.GetDefaultTextColor().ToUint(),
        };
    }

    private static SeStringDrawParams CreateTooltipSeStringDrawParams() =>
        new SeStringDrawParams
        {
            Font = ImGui.GetFont(),
            FontSize = ImGui.GetFontSize(),
            LineHeight = 1f,
            Edge = false,
            Color = ImGui.ColorConvertFloat4ToU32(C.TooltipTextColor),
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

        var drawParams = CreateSeStringDrawParams();
        drawParams.TargetDrawList = ImGui.GetWindowDrawList();
        drawParams.ScreenOffset = new Vector2(-10000f, -10000f);

        var measured = ImGuiHelpers.SeStringWrapped(data, drawParams);
        var contentSize = measured.Size;
        if (contentSize == Vector2.Zero)
            return;

        var slotWidth = EntryFixedWidth.ResolveWidth(layoutKey, contentSize.X);
        var slotSize = new Vector2(slotWidth, LineHeight);

        if (opacity > 0f)
        {
            var pos = GetAlignedPos(contentSize, layoutKey);
            var outlineColor = ApplyOpacity(EntryFixedWidth.GetOutlineColor(colorLayoutKey), opacity);
            var textColor = ApplyOpacity(EntryFixedWidth.GetTextColor(colorLayoutKey), opacity);

            foreach (var offset in DtrStyle.OutlineOffsets)
            {
                drawParams.ScreenOffset = pos + offset;
                drawParams.Color = outlineColor;
                ImGuiHelpers.SeStringWrapped(data, drawParams);
            }

            drawParams.ScreenOffset = pos;
            drawParams.Color = textColor;
            ImGuiHelpers.SeStringWrapped(data, drawParams);
        }

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
            var outlineColor = ApplyOpacity(EntryFixedWidth.GetOutlineColor(colorLayoutKey), opacity);
            var textColor = ApplyOpacity(EntryFixedWidth.GetTextColor(colorLayoutKey), opacity);

            foreach (var offset in DtrStyle.OutlineOffsets)
                drawList.AddText(font, fontSize, pos + offset, outlineColor, text);

            drawList.AddText(font, fontSize, pos, textColor, text);
        }

        AdvanceEntry(slotSize, onClick, dtrEntryTitle, hoverTooltip, hoverTooltipSeStringData);
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
        if (C.OverlayEditMode)
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
        ImGui.PushStyleColor(ImGuiCol.PopupBg, C.TooltipBackgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Text, C.TooltipTextColor);
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
        switch (C.TooltipPosition)
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
