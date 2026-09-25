namespace DTROverlay.Services;

internal static class DtrEntryOrder
{
    public static void MoveUp(IList<string> order, int index)
    {
        if (index <= 0 || index >= order.Count)
            return;

        (order[index - 1], order[index]) = (order[index], order[index - 1]);
        C.Save();
    }

    public static void MoveDown(IList<string> order, int index)
    {
        if (index < 0 || index >= order.Count - 1)
            return;

        (order[index + 1], order[index]) = (order[index], order[index + 1]);
        C.Save();
    }

    public static IReadOnlyList<string> GetOrderedPluginIdsForDisplay(DtrOverlayGroup group)
    {
        DtrOverlayGroups.SyncOverlayOrder(group);
        return group.EntryOrder;
    }
}
