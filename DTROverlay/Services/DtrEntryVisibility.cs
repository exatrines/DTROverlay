using Dalamud.Game.Gui.Dtr;



namespace DTROverlay.Services;



public static class DtrEntryVisibility

{

    public static bool ShouldShowInOverlay(IReadOnlyDtrBarEntry entry)

    {

        if (!entry.Shown || entry.Text == null)

            return false;



        if (C.HiddenEntryTitles.Contains(entry.Title))

            return false;



        return !string.IsNullOrEmpty(entry.Text.TextValue);

    }



    public static bool ShouldShowInNative(IReadOnlyDtrBarEntry entry)

    {

        if (!entry.Shown || entry.UserHidden || entry.Text == null)

            return false;



        return !string.IsNullOrEmpty(entry.Text.TextValue);

    }

}


