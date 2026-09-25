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
        if (!ImGui.BeginTable(id, headers.Length, DefaultFlags))
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
}
