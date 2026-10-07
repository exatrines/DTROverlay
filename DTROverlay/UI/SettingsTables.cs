namespace DTROverlay.UI;

internal static class SettingsTables
{
    private const ImGuiTableFlags DefaultFlags =
        ImGuiTableFlags.NoSavedSettings
        | ImGuiTableFlags.RowBg
        | ImGuiTableFlags.Borders
        | ImGuiTableFlags.SizingFixedFit;

    public static bool BeginDefaultTable(string id, string[] headers)
    {
        var outer = RequestsStretch(headers)
            ? new Vector2(MathF.Max(1f, ImGui.GetContentRegionAvail().X), 0f)
            : default;
        if (!ImGui.BeginTable(id, headers.Length, DefaultFlags, outer))
            return false;

        foreach (var header in headers)
        {
            var flags = ImGuiTableColumnFlags.None;
            var start = 0;
            while (start < header.Length && header[start] is '~' or '^')
            {
                if (header[start] == '~')
                    flags |= ImGuiTableColumnFlags.WidthStretch;
                else
                    flags |= ImGuiTableColumnFlags.NoSort;
                start++;
            }

            ImGui.TableSetupColumn(start > 0 ? header[start..] : header, flags);
        }

        ImGui.TableHeadersRow();
        return true;
    }

    private static bool RequestsStretch(string[] headers)
    {
        foreach (var header in headers)
        {
            for (var i = 0; i < header.Length && header[i] is '~' or '^'; i++)
            {
                if (header[i] == '~')
                    return true;
            }
        }

        return false;
    }
}
