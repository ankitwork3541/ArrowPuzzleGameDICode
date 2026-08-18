using UnityEngine.UIElements;

// Every screen instances the same nav template, so per-screen differences live here rather than
// in each controller. Only Home and Settings navigate; Lvl 10/20/30 are progress badges.
public static class BottomNavBinder
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
        Setup(root.Q<VisualElement>("nav-home"), activeItemName == "nav-home", onHome);
        Setup(root.Q<VisualElement>("nav-settings"), activeItemName == "nav-settings", onSettings);

        RefreshMilestones(root);
    }

    // Split out of Bind so a screen can re-run it every time it becomes visible. Bind only fires on
    // a UI reload, which does NOT happen on the way back from a beaten level.
    public static void RefreshMilestones(VisualElement root)
    {
        int beaten = SaveSystem.Data.highestLevelCompleted;

        foreach ((string name, int level) in Milestones)
        {
            VisualElement item = root.Q<VisualElement>(name);
            if (item == null) continue;

            bool unlocked = beaten >= level;

            item.EnableInClassList("nav-item--locked", !unlocked);
            item.EnableInClassList("nav-item--unlocked", unlocked);

            VisualElement icon = item.Q<VisualElement>(className: "nav-icon");
            if (icon == null) continue;

            icon.EnableInClassList("nav-icon--lock", !unlocked);
            icon.EnableInClassList("nav-icon--star", unlocked);
        }
    }

    private static void Setup(VisualElement item, bool active, EventCallback<ClickEvent> handler)
    {
        if (item == null) return;

        // Both toggled explicitly: equal-specificity USS selectors resolve by source order, which
        // reordering the stylesheet would silently break.
        item.EnableInClassList("nav-item--active", active);
        item.EnableInClassList("nav-item--dim", !active);

        if (handler == null) return;   // the current screen's own item: highlight, no sound, no move

        UIClickSound.Attach(item);

        // Unregister first so a second UI reload cannot stack a duplicate. Only works because the
        // caller passes a method group — a fresh lambda would never match.
        item.UnregisterCallback(handler);
        item.RegisterCallback(handler);
    }
}
