using UnityEngine;
using UnityEngine.UIElements;

// The one "loading ad" modal for the whole game. It sits on its own always-active PanelRenderer
// above every screen, because interstitials are triggered from home, gameplay and both result
// cards, and a modal living inside one screen's UXML cannot cover the others.
public class WaitSheet : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;

    public static WaitSheet Instance;

    private VisualElement root;
    private VisualElement modal;
    private VisualElement spinner;
    private bool visible;
    private float spinnerTime;

    public static bool Showing => Instance != null && Instance.visible;

    public static void Show()
    {
        if (Instance != null) Instance.SetVisible(true);
    }

    public static void Hide()
    {
        if (Instance != null) Instance.SetVisible(false);
    }

    // Versioned overload of the reload callback (the plain one is obsolete). The lambda only drops the
    // version: these handlers rebuild their bindings from scratch on every reload, so a repeat of the
    // same version is harmless. Kept in a field so OnDisable removes the very same delegate.
    private PanelRenderer.VersionedUIReloadCallback reloadCallback;

    private void OnEnable()
    {
        Instance = this;
        reloadCallback = (reloaded, rootElement, version) => OnUIReload(reloaded, rootElement);
        panelRenderer.RegisterUIReloadCallback(reloadCallback);
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
        panelRenderer.UnregisterUIReloadCallback(reloadCallback);
    }

    // Unscaled: an interstitial or rewarded ad is usually shown with the game paused, and a spinner
    // frozen at Time.timeScale 0 reads as a hang.
    private void Update()
    {
        if (!visible || spinner == null) return;

        spinnerTime += Time.unscaledDeltaTime;
        spinner.MarkDirtyRepaint();
    }

    // Panel units. The element is 90x90 in USS.
    private const float StrokeWidth = 8f;
    private const float RotationDegreesPerSecond = 260f;
    private const float SweepSeconds = 1.4f;

    private static readonly Color TrackColor = new Color(0.227f, 0.141f, 0.094f, 0.12f);
    private static readonly Color ArcColor = new Color(0.769f, 0.204f, 0.282f, 1f);

    // Drawn rather than assembled from a sprite. A ring image can only be spun, and spinning a
    // radially symmetric one changes nothing on screen — which is why the old spinner read as a
    // static dot. Painter2D can draw a partial arc, so the head's LENGTH can move too, and that is
    // what makes it read as progress.
    private void DrawSpinner(MeshGenerationContext ctx)
    {
        Rect rect = ctx.visualElement.contentRect;
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f - StrokeWidth * 0.5f;

        // Not laid out yet.
        if (radius <= 0f || float.IsNaN(radius)) return;

        Vector2 centre = rect.center;
        Painter2D painter = ctx.painter2D;
        painter.lineWidth = StrokeWidth;
        painter.lineCap = LineCap.Round;

        // Full ring underneath, so the arc's gap reads as progress rather than as a broken circle.
        painter.strokeColor = TrackColor;
        painter.BeginPath();
        painter.Arc(centre, radius, Angle.Degrees(0f), Angle.Degrees(360f));
        painter.Stroke();

        float start = spinnerTime * RotationDegreesPerSecond % 360f;
        float sweep = Mathf.Lerp(25f, 290f,
                                 (Mathf.Sin(spinnerTime * Mathf.PI * 2f / SweepSeconds) + 1f) * 0.5f);

        painter.strokeColor = ArcColor;
        painter.BeginPath();
        painter.Arc(centre, radius, Angle.Degrees(start), Angle.Degrees(start + sweep));
        painter.Stroke();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;
        modal = root.Q<VisualElement>("wait-sheet");
        spinner = root.Q<VisualElement>("wait-spinner");

        if (spinner != null)
        {
            // Unregister first: this reload callback can fire more than once and the handler would
            // otherwise stack, drawing the same ring several times over.
            spinner.generateVisualContent -= DrawSpinner;
            spinner.generateVisualContent += DrawSpinner;
        }

        Apply();
    }

    private void SetVisible(bool value)
    {
        if (value) spinnerTime = 0f;
        visible = value;
        Apply();
    }

    // This panel never turns off, so while the modal is down its root has to be taken out of both
    // layout and picking — otherwise an always-on transparent panel swallows every tap meant for
    // the screen underneath it.
    private void Apply()
    {
        if (root == null) return;

        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        root.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;

        if (modal != null)
        {
            modal.EnableInClassList("wait-sheet--hidden", !visible);
        }
    }
}
