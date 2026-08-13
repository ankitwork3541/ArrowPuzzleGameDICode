using UnityEngine.UIElements;

// Every screen that shows the bottom nav instances the same UXML template, so the
// per-screen differences — which item is selected, and where its two live items
// navigate to — are applied here rather than duplicated in each screen's controller.
//
// Only Home and Settings navigate. The Lvl 10/20/30 items stay decorative, per the
// locked design decision.
public static class BottomNavBinder
{
    // Pass null as a handler for whichever item is the current screen: an item that
    // navigates to the screen you are already on should do nothing.
    public static void Bind(VisualElement root, string activeItemName,
                            EventCallback<ClickEvent> onHome,
                            EventCallback<ClickEvent> onSettings)
    {
        Setup(root.Q<VisualElement>("nav-home"), activeItemName == "nav-home", onHome);
        Setup(root.Q<VisualElement>("nav-settings"), activeItemName == "nav-settings", onSettings);
    }

    private static void Setup(VisualElement item, bool active, EventCallback<ClickEvent> handler)
    {
        if (item == null) return;

        // Both classes are toggled explicitly rather than letting one override the
        // other: two single-class USS selectors have equal specificity, so the winner
        // is decided by which rule sits later in the stylesheet — far too easy to
        // break by reordering the file.
        item.EnableInClassList("nav-item--active", active);
        item.EnableInClassList("nav-item--dim", !active);

        if (handler == null) return;

        // Unregister first so a second UI reload cannot stack a duplicate handler.
        // This only works because the caller passes a method group, not a lambda —
        // a fresh lambda each call would never match the one already registered.
        item.UnregisterCallback(handler);
        item.RegisterCallback(handler);
    }
}
