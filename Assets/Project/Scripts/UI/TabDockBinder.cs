using UnityEngine.UIElements;

// Every screen instances the same nav template, so per-screen differences live here rather than in
// each controller. The dock has two items, Home and Settings.
public static class TabDockBinder
{
    // Pass null for whichever item is the current screen — it should not navigate to itself.
    public static void Bind(VisualElement root,
                            string activeItemName,
                            EventCallback<ClickEvent> onHome,
                            EventCallback<ClickEvent> onSettings)
    {
        Setup(root.Q<VisualElement>("tab-hall"), activeItemName == "tab-hall", onHome);
        Setup(root.Q<VisualElement>("tab-options"), activeItemName == "tab-options", onSettings);
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
