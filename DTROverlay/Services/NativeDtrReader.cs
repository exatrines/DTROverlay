using System.Text.RegularExpressions;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

using DTROverlay.UI;

namespace DTROverlay.Services;

internal static partial class NativeDtrReader
{
    [GeneratedRegex(@"^\d{1,2}:\d{2}$")]
    private static partial Regex ClockTextPattern();

    public static unsafe IReadOnlyList<VisibleDtrEntry> BuildServerInfoSegments()
    {
        var segments = new List<VisibleDtrEntry>();

        var addon = Svc.GameGui.GetAddonByName("_DTR");
        if (addon == null)
            return segments;

        var atk = (AtkUnitBase*)addon.Address;
        if (atk == null || atk->UldManager.NodeList == null)
            return segments;

        var dtr = (AddonDtr*)atk;
        const float opacity = 1f;

        if (OverlayGroupSettings.GetServerInfoDisplayMode() == ServerInfoDisplayMode.Text)
        {
            BuildServerInfoTextSegments(dtr, atk, segments, opacity);
            return segments;
        }

        var hasNetworkImage = TryGetNetworkImage(dtr, out var networkImage);

        AddWorldSegments(dtr, segments, opacity);

        if (OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.WalkMode))
            AddWalkSegment(dtr, segments, opacity);

        if (OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.Network))
            AddNetworkSegment(dtr, segments, hasNetworkImage, networkImage, opacity);

        AddClockSegments(atk, segments, opacity);

        return segments;
    }

    private static unsafe void BuildServerInfoTextSegments(
        AddonDtr* dtr,
        AtkUnitBase* atk,
        List<VisibleDtrEntry> segments,
        float opacity)
    {
        AddWorldTextSegment(dtr, segments, opacity);

        if (OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.WalkMode))
            AddWalkTextSegment(dtr, segments, opacity);

        if (OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.Network))
            AddNetworkTextSegment(dtr, segments, opacity);

        AddClockSegments(atk, segments, opacity);
    }

    private static void AppendTextPart(
        List<VisibleDtrEntry> segments,
        string text,
        string partId,
        float opacity)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var colorLayoutKey = GetTextModeColorLayoutKey();

        MaybeAddNativeSeparator(segments, opacity);

        segments.Add(VisibleDtrEntry.FromText(
            text,
            opacity: opacity,
            layoutKey: GetNativePartLayoutKey(partId),
            colorLayoutKey: colorLayoutKey));
    }

    private static string GetNativeGroupColorLayoutKey() =>
        OverlayStyleKeys.GetNativeTextColorLayoutKey();

    private static string GetTextModeColorLayoutKey() =>
        OverlayGroupSettings.GetServerInfoDisplayMode() == ServerInfoDisplayMode.Text
            ? GetNativeGroupColorLayoutKey()
            : string.Empty;

    private static string GetNativePartLayoutKey(string partId) =>
        OverlayGroupSettings.GetServerInfoDisplayMode() == ServerInfoDisplayMode.Text
            ? string.Empty
            : partId;

    private static unsafe void AddWorldTextSegment(AddonDtr* dtr, List<VisibleDtrEntry> segments, float opacity)
    {
        var showWorldIcon = OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.WorldIcon);
        var showWorldName = OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.WorldName);

        if (!showWorldIcon && !showWorldName)
            return;

        var hasWorldIcon = DtrNativeImage.TryCreateWorldIcon(dtr, out _);
        TryGetWorldInfo(dtr, out _, out var worldName);

        if (!hasWorldIcon && string.IsNullOrEmpty(worldName))
            return;

        if (showWorldIcon && hasWorldIcon)
            AppendTextPart(segments, GetWorldIconTextLabel(dtr), OverlayEntryIds.WorldIcon, opacity);

        if (showWorldName && !string.IsNullOrEmpty(worldName))
            AppendTextPart(segments, worldName, OverlayEntryIds.WorldName, opacity);
    }

    private static unsafe string GetWorldIconTextLabel(AddonDtr* dtr)
    {
        var character = Player.Character;
        if (character != null)
        {
            return character->CurrentWorld != character->HomeWorld
                ? DtrNativeImage.TravelingIconText
                : DtrNativeImage.HomeworldIconText;
        }

        return dtr->WorldVisitImage != null && dtr->WorldVisitImage->IsVisible()
            ? DtrNativeImage.HomeworldIconText
            : DtrNativeImage.TravelingIconText;
    }

    private static unsafe void AddWalkTextSegment(AddonDtr* dtr, List<VisibleDtrEntry> segments, float opacity)
    {
        if (dtr->WalkModeContainer == null
            || !dtr->WalkModeContainer->IsVisible()
            || !DtrNativeImage.TryCreateFromContainer(dtr->WalkModeContainer, out _))
            return;

        AppendTextPart(segments, "Walking", OverlayEntryIds.WalkMode, opacity);
    }

    private static unsafe void AddNetworkTextSegment(AddonDtr* dtr, List<VisibleDtrEntry> segments, float opacity)
    {
        if (dtr->NetworkStrengthContainer == null || !dtr->NetworkStrengthContainer->IsVisible())
            return;

        var colorLayoutKey = GetTextModeColorLayoutKey();
        var hoverTooltip = TryReadNetworkTooltip(dtr);

        MaybeAddNativeSeparator(segments, opacity);

        segments.Add(VisibleDtrEntry.FromIcon(
            DtrNetworkIcons.TextModeIcon,
            opacity: opacity,
            colorLayoutKey: colorLayoutKey,
            hoverTooltip: hoverTooltip));
    }

    private static unsafe string TryReadNetworkTooltip(AddonDtr* dtr) =>
        DtrNetworkInfoReader.TryReadDisplayText(dtr, out var text) ? text : string.Empty;

    internal static unsafe bool TryReadText(AtkTextNode* node, out string text)
    {
        text = string.Empty;
        if (node == null || !node->IsVisible())
            return false;

        text = GenericHelpers.ReadSeString(&node->NodeText).TextValue.Trim();
        return !string.IsNullOrEmpty(text);
    }

    private static unsafe bool TryGetNetworkImage(AddonDtr* dtr, out DtrImageInfo image)
    {
        image = default;
        if (dtr->NetworkStrengthContainer == null || !dtr->NetworkStrengthContainer->IsVisible())
            return false;

        return DtrNativeImage.TryCreate(dtr->NetworkStrengthImage, out image);
    }

    private static unsafe void AddWorldSegments(AddonDtr* dtr, List<VisibleDtrEntry> segments, float opacity)
    {
        if (!TryGetWorldInfo(dtr, out var worldImage, out var worldName))
            return;

        var showWorldIcon = OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.WorldIcon);
        var showWorldName = OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.WorldName);

        if (showWorldIcon)
        {
            segments.Add(VisibleDtrEntry.FromImage(
                worldImage,
                opacity: opacity,
                layoutKey: OverlayEntryIds.WorldIcon));
        }

        if (showWorldName)
        {
            segments.Add(VisibleDtrEntry.FromText(
                worldName,
                showWorldIcon ? DtrStyle.IconAdjacentSpacing : null,
                opacity,
                OverlayEntryIds.WorldName,
                GetNativeGroupColorLayoutKey()));
        }
    }

    /// <summary>Home/visit world icon and name are only available when native DTR exposes world info (hidden in many instances).</summary>
    private static unsafe bool TryGetWorldInfo(AddonDtr* dtr, out DtrImageInfo worldImage, out string worldName)
    {
        worldImage = default;
        worldName = string.Empty;

        if (!DtrNativeImage.TryCreateWorldIcon(dtr, out worldImage))
            return false;

        return TryReadText(dtr->WorldText, out worldName);
    }

    private static unsafe bool TryGetWorldInfo(AddonDtr* dtr, out string worldName) =>
        TryGetWorldInfo(dtr, out _, out worldName);

    private static unsafe void AddWalkSegment(AddonDtr* dtr, List<VisibleDtrEntry> segments, float opacity)
    {
        if (dtr->WalkModeContainer == null
            || !DtrNativeImage.TryCreateFromContainer(dtr->WalkModeContainer, out var walkImage))
            return;

        MaybeAddNativeSeparator(segments, opacity);

        segments.Add(VisibleDtrEntry.FromImage(
            walkImage,
            DtrStyle.IconAdjacentSpacing,
            opacity,
            imageScale: DtrStyle.WalkIconScale,
            layoutKey: OverlayEntryIds.WalkMode));
    }

    private static unsafe void AddNetworkSegment(
        AddonDtr* dtr,
        List<VisibleDtrEntry> segments,
        bool hasNetworkImage,
        DtrImageInfo networkImage,
        float opacity)
    {
        if (!hasNetworkImage)
            return;

        MaybeAddNativeSeparator(segments, opacity);

        segments.Add(VisibleDtrEntry.FromImage(
            networkImage,
            DtrStyle.IconAdjacentSpacing,
            opacity,
            layoutKey: OverlayEntryIds.Network,
            hoverTooltip: TryReadNetworkTooltip(dtr)));
    }

    private static unsafe void AddClockSegments(
        AtkUnitBase* atk,
        List<VisibleDtrEntry> segments,
        float opacity)
    {
        var settings = DtrClockSettings.Read();
        var nativeTimes = ReadNativeClockTimes(atk);

        if (settings.ShowEorzea && OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.EorzeaClock))
        {
            AddClock(
                segments,
                nativeTimes,
                DtrTimeIcons.EorzeaTime,
                GameClock.GetEorzeaTime(),
                opacity,
                OverlayEntryIds.EorzeaClock);
        }

        if (settings.ShowLocal && OverlayEntryIds.IsServerInfoPartVisible(OverlayEntryIds.LocalClock))
        {
            AddClock(
                segments,
                nativeTimes,
                DtrTimeIcons.LocalTime,
                GameClock.GetLocalTime(),
                opacity,
                OverlayEntryIds.LocalClock);
        }
    }

    private static void AddClock(
        List<VisibleDtrEntry> segments,
        IReadOnlyDictionary<SeIconChar, string> nativeTimes,
        SeIconChar icon,
        DateTime fallbackTime,
        float opacity,
        string layoutKey)
    {
        var partLayoutKey = GetNativePartLayoutKey(layoutKey);
        var colorLayoutKey = GetNativeGroupColorLayoutKey();

        MaybeAddNativeSeparator(segments, opacity);

        segments.Add(VisibleDtrEntry.FromIcon(
            icon,
            opacity: opacity,
            layoutKey: partLayoutKey,
            colorLayoutKey: colorLayoutKey));
        segments.Add(VisibleDtrEntry.FromText(
            GetClockTime(nativeTimes, icon, fallbackTime),
            DtrStyle.ClockIconTimeSpacing,
            opacity,
            partLayoutKey,
            colorLayoutKey));
    }

    private static string GetClockTime(
        IReadOnlyDictionary<SeIconChar, string> nativeTimes,
        SeIconChar icon,
        DateTime fallbackTime) =>
        nativeTimes.TryGetValue(icon, out var nativeTime)
            ? nativeTime
            : GameClock.FormatTime(fallbackTime);

    private static unsafe IReadOnlyDictionary<SeIconChar, string> ReadNativeClockTimes(AtkUnitBase* atk)
    {
        var times = new Dictionary<SeIconChar, string>();
        if (atk == null || atk->UldManager.NodeList == null)
            return times;

        var clockTexts = new List<(float X, string Text)>();

        for (var i = 0; i < atk->UldManager.NodeListCount; i++)
        {
            var resNode = atk->UldManager.NodeList[i];
            if (resNode == null || resNode->Type != NodeType.Text)
                continue;

            var textNode = resNode->GetAsAtkTextNode();
            if (!TryReadText(textNode, out var text))
                continue;

            if (ClockTextPattern().IsMatch(text))
                clockTexts.Add((textNode->ScreenX, text));
        }

        if (clockTexts.Count == 0)
            return times;

        clockTexts.Sort((left, right) => right.X.CompareTo(left.X));

        var settings = DtrClockSettings.Read();
        var expectedCount = (settings.ShowLocal ? 1 : 0) + (settings.ShowEorzea ? 1 : 0);
        var index = clockTexts.Count > expectedCount ? clockTexts.Count - expectedCount : 0;

        if (settings.ShowLocal && index < clockTexts.Count)
            times[DtrTimeIcons.LocalTime] = clockTexts[index++].Text;

        if (settings.ShowEorzea && index < clockTexts.Count)
            times[DtrTimeIcons.EorzeaTime] = clockTexts[index].Text;

        return times;
    }

    private static void MaybeAddNativeSeparator(List<VisibleDtrEntry> segments, float opacity)
    {
        if (segments.Count > 0)
            segments.Add(DtrSeparators.CreateNative(opacity));
    }
}
