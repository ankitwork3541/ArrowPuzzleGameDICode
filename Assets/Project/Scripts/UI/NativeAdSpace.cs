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

    public static void Collapse(VisualElement space)
    {
        Apply(space, 0f);
    }

    public static float PixelsToPanel(VisualElement element, float pixels)
    {
        float scale = PanelScale(element);
        return scale > 0f ? pixels / scale : pixels;
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
        if (element == null || element.panel == null)
        {
            return 1f;
        }

        float panelHeight = element.panel.visualTree.layout.height;
        if (panelHeight <= 0f || float.IsNaN(panelHeight) || Screen.height <= 0)
        {
            return 1f;
        }

        return Screen.height / panelHeight;
    }
}
