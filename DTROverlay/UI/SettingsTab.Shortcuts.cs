using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawShortcutsSection()
    {
        MirageUi.SubHeader(I18n.Get("shortcuts.header"));

        if (MirageUi.SecondaryButton(I18n.Get("shortcuts.vanilla"), id: "toggleVanillaDtr"))
            VanillaDtrHud.Toggle();
        MirageUi.Tooltip(I18n.Get("shortcuts.vanilla_hint"));

        if (MirageUi.SecondaryButton(I18n.Get("shortcuts.dalamud"), id: "hideDalamudDtr"))
            DtrDalamudConfig.HideAllEntriesInDalamudSettings();
        MirageUi.Tooltip(I18n.Get("shortcuts.dalamud_hint"));
    }

    private static void DrawOptionSection()
    {
        MirageUi.SubHeader(I18n.Get("option.header"));
        DrawLanguageSetting();

        if (MirageUi.Checkbox(I18n.Get("option.middle_click"), ref C.OpenPluginUiOnMiddleClick))
            C.Save();
    }

    private static void DrawLanguageSetting()
    {
        var current = C.UiLanguage is "en" or "ja" ? C.UiLanguage : I18n.FollowClient;
        string[] codes = [I18n.FollowClient, "en", "ja"];
        string[] labels =
        [
            I18n.Get("option.language.client"),
            I18n.Get("option.language.en"),
            I18n.Get("option.language.ja"),
        ];
        var index = Array.IndexOf(codes, current);
        if (index < 0)
            index = 0;

        var selected = labels[index];
        if (!MirageUi.Dropdown(I18n.Get("option.language"), ref selected, labels, allowClear: false, id: "uiLanguage"))
            return;

        var next = Array.IndexOf(labels, selected);
        if (next < 0 || codes[next] == current)
            return;

        C.UiLanguage = codes[next];
        C.Save();
        I18n.Apply();
    }
}
