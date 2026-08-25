using UnityEngine;
using UnityEngine.UIElements;

// Drives the ad-loading skeleton inside a reserved ad box: shows or hides it, and sweeps its sheen
// while it is up. USS has no keyframe animations, so the sweep has to be moved from code.
public static class AdPlaceholder
{
    // How long one pass across the slot takes, and how long it waits off the right edge before
    // starting over. The rest is what keeps it reading as a slow pulse rather than a running belt.
    const float SweepSeconds = 1.15f;
    const float RestSeconds = 0.55f;

    // Panel units. Above this a slot has room for a media block; a banner never does.
    const float TallSlotPanelUnits = 380f;

    public static void SetVisible(VisualElement space, bool visible, float panelHeight)
    {
        VisualElement placeholder = space != null ? space.Q<VisualElement>("ad-placeholder") : null;
        if (placeholder == null)
        {
            return;
        }

        placeholder.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        placeholder.EnableInClassList("ad-ph--tall", panelHeight >= TallSlotPanelUnits);

        VisualElement sheen = placeholder.Q<VisualElement>("ad-placeholder-sheen");
        if (sheen == null)
        {
            return;
        }

        IVisualElementScheduledItem sweep = sheen.userData as IVisualElementScheduledItem;

        if (!visible)
        {
            if (sweep != null)
            {
                sweep.Pause();
            }

            return;
        }

        if (sweep == null)
        {
            // The panel's own scheduler, not a coroutine: it stops on its own whenever the panel
            // goes away with the screen, which is exactly when the sweep should stop.
            sweep = sheen.schedule.Execute(() => Sweep(sheen)).Every(16);
            sheen.userData = sweep;
        }

        sweep.Resume();
    }

    static void Sweep(VisualElement sheen)
    {
        VisualElement track = sheen.parent;
        if (track == null)
        {
            return;
        }

        float trackWidth = track.layout.width;
        float sheenWidth = sheen.layout.width;

        // Not laid out yet. Moving it now would park it at a position measured against a zero width.
        if (float.IsNaN(trackWidth) || float.IsNaN(sheenWidth) || trackWidth <= 0f)
        {
            return;
        }

        // Unscaled: a paused game still shows the skeleton, and a frozen sweep reads as a hang.
        float phase = Time.unscaledTime % (SweepSeconds + RestSeconds) / SweepSeconds;
        float x = phase >= 1f ? trackWidth : Mathf.Lerp(-sheenWidth, trackWidth, phase);

        sheen.style.translate = new Translate(x, 0f);
    }
}
