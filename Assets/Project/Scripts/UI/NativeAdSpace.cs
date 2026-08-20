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

    // AdSize is specified in density-independent pixels, which is a third coordinate space on top
    // of panel units and screen pixels.
    public static int PanelToDp(VisualElement element, float panelUnits)
    {
        float pixels = panelUnits * PanelScale(element);
        float dpi = Screen.dpi;

        if (dpi <= 0f || float.IsNaN(dpi))
        {
            return Mathf.RoundToInt(pixels);
        }

        return Mathf.RoundToInt(pixels * 160f / dpi);
    }

    // Top-left of an element in screen pixels, for positioning an overlay over it.
    public static Vector2Int ScreenTopLeft(VisualElement element)
    {
        if (element == null)
        {
            return Vector2Int.zero;
        }

        Rect bounds = element.worldBound;
        if (float.IsNaN(bounds.x) || float.IsNaN(bounds.y))
        {
            return Vector2Int.zero;
        }

        float scale = PanelScale(element);
        return new Vector2Int(Mathf.RoundToInt(bounds.x * scale), Mathf.RoundToInt(bounds.y * scale));
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
