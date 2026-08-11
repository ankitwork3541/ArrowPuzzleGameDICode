using UnityEngine;
using UnityEngine.UIElements;

public class HomeScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private float gridSpacing = 40f;
    [SerializeField] private Color gridLineColor = new Color(0f, 0.898f, 1f, 0.03f);

    private VisualElement gridBackground;

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        gridBackground = rootElement.Q<VisualElement>("grid-background");
        gridBackground.RegisterCallback<GeometryChangedEvent>(OnGridGeometryChanged);
    }

    private void OnGridGeometryChanged(GeometryChangedEvent evt)
    {
        DrawGrid(evt.newRect.width, evt.newRect.height);
    }

    private void DrawGrid(float width, float height)
    {
        gridBackground.Clear();

        for (float x = 0; x < width; x += gridSpacing)
        {
            gridBackground.Add(CreateLine(x, 0, 1, height));
        }

        for (float y = 0; y < height; y += gridSpacing)
        {
            gridBackground.Add(CreateLine(0, y, width, 1));
        }
    }

    private VisualElement CreateLine(float left, float top, float width, float height)
    {
        var line = new VisualElement();
        line.style.position = Position.Absolute;
        line.style.left = left;
        line.style.top = top;
        line.style.width = width;
        line.style.height = height;
        line.style.backgroundColor = gridLineColor;
        return line;
    }
}
