using UnityEngine;
using UnityEngine.UIElements;

// The one "loading ad" modal for the whole game. It sits on its own always-active PanelRenderer
// above every screen, because interstitials are triggered from home, gameplay and both result
// cards, and a modal living inside one screen's UXML cannot cover the others.
public class AdLoadingOverlay : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;

    public static AdLoadingOverlay Instance;

    private VisualElement root;
    private VisualElement modal;
    private VisualElement spinner;
    private bool visible;
    private float spinnerAngle;

    public static bool Showing => Instance != null && Instance.visible;

    public static void Show()
    {
        if (Instance != null) Instance.SetVisible(true);
    }

    public static void Hide()
    {
        if (Instance != null) Instance.SetVisible(false);
    }

    private void OnEnable()
    {
        Instance = this;
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void Update()
    {
        if (!visible || spinner == null) return;

        spinnerAngle = (spinnerAngle + Time.unscaledDeltaTime * 360f) % 360f;
        spinner.style.rotate = new Rotate(Angle.Degrees(spinnerAngle));
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;
        modal = root.Q<VisualElement>("ad-loading-modal");
        spinner = root.Q<VisualElement>("ad-loading-spinner");
        Apply();
    }

    private void SetVisible(bool value)
    {
        if (value) spinnerAngle = 0f;
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
            modal.EnableInClassList("ad-loading-modal--hidden", !visible);
        }
    }
}
