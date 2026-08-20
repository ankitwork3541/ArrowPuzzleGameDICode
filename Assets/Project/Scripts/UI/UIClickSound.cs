using UnityEngine.UIElements;

// Click sound for every UI control.
public static class UIClickSound
{
    public static void Attach(VisualElement element)
    {
        if (element == null) return;

        // TrickleDown: a Button's Clickable manipulator swallows PointerDownEvent, so a bubble-phase
        // callback never runs.
        element.UnregisterCallback<PointerDownEvent>(Play, TrickleDown.TrickleDown);
        element.RegisterCallback<PointerDownEvent>(Play, TrickleDown.TrickleDown);
    }

    static void Play(PointerDownEvent evt)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
    }
}
