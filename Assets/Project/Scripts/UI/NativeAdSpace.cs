using UnityEngine;
using UnityEngine.UIElements;

// Keeps a UI Toolkit box exactly as tall as the native ad Android is drawing, and collapsed to
// nothing when there is no ad. The two live in different coordinate spaces — Android in screen
// pixels, UI Toolkit in panel units — so a measured height has to be converted or the reserved box
// and the ad never line up.
public static class NativeAdSpace
{
    public static void Apply(VisualElement space, float adHeightPixels)
    {
        if (space == null)
        {
            return;
        }

        if (adHeightPixels <= 0f)
        {
            space.style.display = DisplayStyle.None;
            space.style.height = 0f;
            return;
        }

        space.style.display = DisplayStyle.Flex;
        space.style.height = PixelsToPanel(space, adHeightPixels);
    }

    // What a box was last asked to reserve, kept on the element so it can be converted again once
    // the panel is able to answer. See Apply() below.
    class Reservation
    {
        public float pixels;
        public bool placeholder;
        public float appliedPanelHeight = float.NaN;

        // Runs only while the box has not settled on its final height, and pauses itself once it
        // has. A GeometryChangedEvent is not enough on its own: a panel that lays out to exactly
        // the rect it had last visit raises none, and the box would then keep whatever height that
        // visit left on it.
        public IVisualElementScheduledItem retry;
    }

    // The variant the anchored bottom slots use: room is opened the moment the ad is requested and
    // filled with a skeleton, so the layout settles once instead of jumping when the ad paints.
    //
    // Unlike Apply(space, height) this never touches `display`. The height is animated by USS, and
    // flipping display to None cancels a transition mid-flight — which is the jolt this exists to
    // remove. A zero-height box with `flex-shrink: 0` and `overflow: hidden` takes up nothing, so
    // leaving it laid out costs the screen nothing.
    //
    // The reservation is REMEMBERED rather than applied once. Pixels to panel units needs the
    // panel's laid-out height, and a screen that was only just re-activated has not been through a
    // layout pass yet — PanelScale then falls back to 1, which reserves the raw pixel count and
    // comes out visibly too tall on any scaled panel. The ad's own height never changes again after
    // that (the banner is reused across screens), so nothing would ever correct it. Replaying the
    // reservation each frame until the panel can convert it properly is what fixes that.
    public static void Apply(VisualElement space, float adHeightPixels, bool showPlaceholder)
    {
        if (space == null)
        {
            return;
        }

        if (!(space.userData is Reservation state))
        {
            state = new Reservation();
            space.userData = state;
            state.retry = space.schedule.Execute(() => Reapply(space)).Every(16);
        }

        state.pixels = adHeightPixels;
        state.placeholder = showPlaceholder;
        state.retry.Resume();

        Reapply(space);
    }

    static void Reapply(VisualElement space)
    {
        if (!(space.userData is Reservation state))
        {
            return;
        }

        space.style.display = DisplayStyle.Flex;

        // Zero needs no conversion, so it can settle straight away even on an unlaid-out panel.
        if (state.pixels <= 0f)
        {
            if (float.IsNaN(state.appliedPanelHeight)
                || !Mathf.Approximately(state.appliedPanelHeight, 0f))
            {
                state.appliedPanelHeight = 0f;
                space.style.height = 0f;
            }

            AdPlaceholder.SetVisible(space, false, 0f);
            state.retry.Pause();
            return;
        }

        // Panel not laid out yet. Converting now would write the raw pixel count and the retry would
        // correct it a frame later — visible as an oversized gap that shrinks back. Whatever height
        // is already on the box is the better thing to show until the panel can answer.
        if (!TryPanelScale(space, out float scale))
        {
            if (!float.IsNaN(state.appliedPanelHeight))
            {
                AdPlaceholder.SetVisible(space, state.placeholder && state.appliedPanelHeight > 0f,
                                         state.appliedPanelHeight);
            }

            return;
        }

        float panelHeight = state.pixels / scale;

        // Compared against the value last written, NOT against resolvedStyle: mid-transition the
        // resolved height is wherever the animation has got to, so it would never match and this
        // would keep rewriting — restarting nothing, but never settling either.
        if (float.IsNaN(state.appliedPanelHeight)
            || !Mathf.Approximately(state.appliedPanelHeight, panelHeight))
        {
            state.appliedPanelHeight = panelHeight;
            space.style.height = panelHeight;
        }

        AdPlaceholder.SetVisible(space, state.placeholder && panelHeight > 0f, panelHeight);
        state.retry.Pause();
    }

    public static void Collapse(VisualElement space)
    {
        Apply(space, 0f);
        AdPlaceholder.SetVisible(space, false, 0f);
    }

    public static float PixelsToPanel(VisualElement element, float pixels)
    {
        float scale = PanelScale(element);
        return scale > 0f ? pixels / scale : pixels;
    }

    public static float PanelToPixels(VisualElement element, float panelUnits)
    {
        return panelUnits * PanelScale(element);
    }

    // DP is a third coordinate space on top of panel units and screen pixels.
    public static int PanelToDp(VisualElement element, float panelUnits)
    {
        return Mathf.RoundToInt(panelUnits * PanelScale(element) / DisplayDensity);
    }

    static float density = -1f;

    // Android's own DisplayMetrics.density, deliberately not Screen.dpi. The plugin converts dp
    // through DisplayMetrics.density, which is bucketed (1.0, 1.5, 2.0, 3.0...), while Screen.dpi
    // reports the panel's physical dpi. Those two disagree on most phones, and using the wrong one
    // offsets the ad by a fixed fraction — which reads as the ad being narrower than its box and
    // pulled to one side.
    public static float DisplayDensity
    {
        get
        {
            if (density > 0f)
            {
                return density;
            }

            density = ReadAndroidDensity();
            Debug.Log("[NativeAd] DisplayMetrics.density = " + density
                      + " (Screen.dpi reports " + Screen.dpi + ")");
            return density;
        }
    }

    static float ReadAndroidDensity()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var resources = new AndroidJavaClass("android.content.res.Resources"))
            using (var system = resources.CallStatic<AndroidJavaObject>("getSystem"))
            using (var metrics = system.Call<AndroidJavaObject>("getDisplayMetrics"))
            {
                float value = metrics.Get<float>("density");
                if (value > 0f)
                {
                    return value;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[NativeAd] Could not read display density: " + e.Message);
        }
#endif
        return 1f;
    }

    // Top-left of an element in dp, which is what the plugin's positioning expects: it runs
    // PluginUtils.convertDpToPixel over whatever x and y it is handed. Passing screen pixels puts
    // the ad off the bottom of the display on any device denser than 160dpi — it renders, reports a
    // height, and is never seen.
    //
    // Returns false while UI Toolkit has yet to lay the element out, rather than a zero that would
    // silently fling the ad into the corner.
    public static bool TryTopLeftDp(VisualElement element, out Vector2Int topLeftDp)
    {
        topLeftDp = Vector2Int.zero;

        if (element == null)
        {
            return false;
        }

        Rect bounds = element.worldBound;
        if (float.IsNaN(bounds.x) || float.IsNaN(bounds.y))
        {
            return false;
        }

        topLeftDp = new Vector2Int(PanelToDp(element, bounds.x), PanelToDp(element, bounds.y));
        return true;
    }

    // Screen pixels per panel unit. PanelSettings scales the whole UI on most devices, so this can
    // never be assumed to be 1.
    static float PanelScale(VisualElement element)
    {
        return TryPanelScale(element, out float scale) ? scale : 1f;
    }

    // Says whether the panel can answer at all, instead of handing back a 1 that reads as a real
    // answer. A screen that was only just re-activated has no layout yet, and a caller that treats
    // 1 as the scale reserves the raw pixel count — too tall by whatever the panel is scaled by.
    static bool TryPanelScale(VisualElement element, out float scale)
    {
        scale = 1f;

        if (element == null || element.panel == null)
        {
            return false;
        }

        float panelHeight = element.panel.visualTree.layout.height;
        if (panelHeight <= 0f || float.IsNaN(panelHeight) || Screen.height <= 0)
        {
            return false;
        }

        scale = Screen.height / panelHeight;
        return true;
    }
}
