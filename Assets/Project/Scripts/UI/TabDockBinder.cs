using UnityEngine.UIElements;

// Every screen instances the same nav template, so per-screen differences live here rather than
// in each controller. Only Home and Settings navigate; Lvl 10/20/30 are progress badges.
public static class TabDockBinder
{
    // Element name -> the level number that unlocks it.
    private static readonly (string name, int level)[] Milestones =
    {
        ("nav-lvl10", 10),
        ("nav-lvl20", 20),
        ("nav-lvl30", 30),
    };

    // Pass null for whichever item is the current screen — it should not navigate to itself.
    public static void Bind(VisualElement root, string activeItemName,
                            EventCallback<ClickEvent> onHome,
                            EventCallback<ClickEvent> onSettings)
    {
        Setup(root.Q<VisualElement>("tab-hall"), activeItemName == "tab-hall", onHome);
        Setup(root.Q<VisualElement>("tab-options"), activeItemName == "tab-options", onSettings);

        RefreshMilestones(root);
    }

    // Split out of Bind so a screen can re-run it every time it becomes visible. Bind only fires on
    // a UI reload, which does NOT happen on the way back from a beaten level.
    public static void RefreshMilestones(VisualElement root)
    {
        int beaten = ProgressStore.Data.highestLevelCompleted;

        foreach ((string name, int level) in Milestones)
        {
            VisualElement item = root.Q<VisualElement>(name);
            if (item == null) continue;

            bool unlocked = beaten >= level;

            item.EnableInClassList("tab-item--locked", !unlocked);
            item.EnableInClassList("tab-item--unlocked", unlocked);

            VisualElement icon = item.Q<VisualElement>(className: "tab-icon");
            if (icon == null) continue;

            icon.EnableInClassList("tab-icon--lock", !unlocked);
            icon.EnableInClassList("tab-icon--star", unlocked);
        }
    }

    private static void Setup(VisualElement item, bool active, EventCallback<ClickEvent> handler)
    {
        if (item == null) return;

        // Both toggled explicitly: equal-specificity USS selectors resolve by source order, which
        // reordering the stylesheet would silently break.
        item.EnableInClassList("tab-item--active", active);
        item.EnableInClassList("tab-item--dim", !active);

        if (handler == null) return;   // the current screen's own item: highlight, no sound, no move

        TapSound.Attach(item);

        // Unregister first so a second UI reload cannot stack a duplicate. Only works because the
        // caller passes a method group — a fresh lambda would never match.
        item.UnregisterCallback(handler);
        item.RegisterCallback(handler);
    }
}
