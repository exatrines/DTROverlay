using System.Text;
using System.Text.RegularExpressions;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace DTROverlay.Services;

internal static partial class DtrNetworkInfoReader
{
    [GeneratedRegex(@"Send:\s*([^\r\n,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex SendPattern();

    [GeneratedRegex(@"Receive:\s*([^\r\n,]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ReceivePattern();

    public static unsafe bool TryReadDisplayText(AddonDtr* dtr, out string text)
    {
        text = string.Empty;
        if (dtr == null)
            return false;

        if (TryReadTooltip(dtr, out text))
            return true;

        return TryReadFromContainer(dtr, out text);
    }

    private static unsafe bool TryReadTooltip(AddonDtr* dtr, out string text)
    {
        text = string.Empty;
        var tooltip = dtr->NetworkInfoTooltip;
        if (tooltip.IsEmpty)
            return false;

        text = tooltip.ToString().Trim();
        if (string.IsNullOrEmpty(text))
            return false;

        text = FormatSendReceive(text);
        return !string.IsNullOrEmpty(text);
    }

    private static unsafe bool TryReadFromContainer(AddonDtr* dtr, out string text)
    {
        text = string.Empty;
        if (dtr->NetworkStrengthContainer == null || !dtr->NetworkStrengthContainer->IsVisible())
            return false;

        var lines = new List<string>();
        CollectTextNodes(dtr->NetworkStrengthContainer, lines);
        if (lines.Count == 0)
            return false;

        text = FormatSendReceive(string.Join("\n", lines));
        return !string.IsNullOrEmpty(text);
    }

    private static unsafe void CollectTextNodes(AtkResNode* node, List<string> lines)
    {
        if (node == null)
            return;

        if (node->Type == NodeType.Text
            && NativeDtrReader.TryReadText(node->GetAsAtkTextNode(), out var line)
            && !string.IsNullOrWhiteSpace(line))
        {
            lines.Add(line.Trim());
        }

        CollectTextNodes(node->ChildNode, lines);
        CollectTextNodes(node->NextSiblingNode, lines);
    }

    private static string FormatSendReceive(string source)
    {
        var send = SendPattern().Match(source);
        var receive = ReceivePattern().Match(source);
        if (!send.Success && !receive.Success)
            return source.ReplaceLineEndings(", ").Trim();

        var builder = new StringBuilder();
        if (send.Success)
            builder.Append($"Send:{send.Groups[1].Value.Trim()}");

        if (receive.Success)
        {
            if (builder.Length > 0)
                builder.Append(", ");

            builder.Append($"Receive:{receive.Groups[1].Value.Trim()}");
        }

        return builder.ToString();
    }
}
