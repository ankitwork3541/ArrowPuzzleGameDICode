using UnityEngine;
using UnityEngine.UIElements;

// Drives both the Win and the Lose card. One class rather than two because the screens differ only
// in wording, accent colour and which pair of GameFlowManager methods the buttons call — the two
// UXML files already carry everything that is genuinely different, and the element names match so
// the wiring below can stay identical.
public class ResultScreenController : MonoBehaviour
{
    public enum Mode { Win, Lose }

    [SerializeField] private Mode mode = Mode.Win;
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private LevelLoader levelLoader;

    private VisualElement root;
    private Label levelLabel;
    private ConfettiView confetti;

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        // On a second win the panel is already loaded, so OnUIReload will not fire again and
        // `root` is still the one from last time. Refreshing here is what keeps the level number
        // correct — same reason HomeScreenController does it.
        Refresh();
        StartConfetti();

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.AddNativeHeightListener(OnNativeHeightChanged);
            adRequested = false;
            HookAdSpace();
        }
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        if (confetti != null) confetti.Stop();

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.RemoveNativeHeightListener(OnNativeHeightChanged);
            AdsManager.Instance.HideNativeAd();
        }

        VisualElement space = AdSpace;
        if (space != null)
        {
            space.UnregisterCallback<GeometryChangedEvent>(OnAdSpaceGeometryChanged);
            NativeAdSpace.Collapse(space);
        }
    }

    private VisualElement AdSpace => root != null ? root.Q<VisualElement>("dialog-ad-space") : null;

    private bool adRequested;

    // The ad has to be positioned over the reserved box, but that box has no width until it is laid
    // out and no height until the ad has been measured. So the box is opened to a sliver first,
    // which resolves its width, and every later layout pass nudges the ad to the box's new position.
    private void HookAdSpace()
    {
        VisualElement space = AdSpace;
        if (space == null || AdsManager.Instance == null || AdsManager.Instance.Settings == null) return;

        if (!AdsManager.Instance.Settings.CanShowPreloadNative)
        {
            NativeAdSpace.Collapse(space);
            return;
        }

        space.UnregisterCallback<GeometryChangedEvent>(OnAdSpaceGeometryChanged);
        space.RegisterCallback<GeometryChangedEvent>(OnAdSpaceGeometryChanged);

        float known = AdsManager.Instance.NativeHeightPixels;
        space.style.display = DisplayStyle.Flex;
        space.style.height = known > 0f ? NativeAdSpace.PixelsToPanel(space, known) : 1f;

        PlaceOrMoveAd();
    }

    private void OnAdSpaceGeometryChanged(GeometryChangedEvent evt)
    {
        PlaceOrMoveAd();
    }

    private void PlaceOrMoveAd()
    {
        VisualElement space = AdSpace;
        if (space == null || AdsManager.Instance == null) return;

        Rect bounds = space.worldBound;
        if (float.IsNaN(bounds.width) || bounds.width <= 0f) return;

        Vector2Int topLeft = NativeAdSpace.ScreenTopLeft(space);

        if (adRequested)
        {
            AdsManager.Instance.MoveNativeAd(topLeft.x, topLeft.y);
            return;
        }

        adRequested = true;

        // Consumes the ad preloaded when the level started, so the card does not wait on a network
        // round trip. Width is the card's own content width, or the ad would overhang the card.
        int widthDp = NativeAdSpace.PanelToDp(space, bounds.width);
        AdsManager.Instance.ShowLevelEndNative(NativeAdPlacement.At(topLeft.x, topLeft.y, widthDp));
    }

    // Reserves exactly the ad's measured height. Zero collapses the box completely, so no ad leaves
    // no trace of one.
    private void OnNativeHeightChanged(float heightPixels)
    {
        VisualElement space = AdSpace;
        if (space == null) return;

        NativeAdSpace.Apply(space, heightPixels);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;
        levelLabel = root.Q<Label>("level-label");

        Bind(root.Q<Button>("primary-button"),
             mode == Mode.Win ? gameFlowManager.OnNextLevelClicked
                              : gameFlowManager.OnRetryLevelClicked);

        Bind(root.Q<Button>("home-button"),
             mode == Mode.Win ? gameFlowManager.OnWinHomeClicked
                              : gameFlowManager.OnLoseHomeClicked);

        if (mode == Mode.Win && confetti == null)
        {
            // Added last so it sits above the card. It is picking-mode Ignore, so falling confetti
            // never steals a tap from the NEXT button underneath it.
            confetti = new ConfettiView();
            (root.Q<VisualElement>("screen-root") ?? root).Add(confetti);
        }

        Refresh();
        StartConfetti();

        // On the first show this fires after OnEnable, which ran with no root to query yet.
        HookAdSpace();
    }

    // Unregister before registering so a second UI reload cannot stack duplicate handlers.
    private static void Bind(Button button, System.Action handler)
    {
        UIClickSound.Attach(button);

        if (button == null) return;
        button.clicked -= handler;
        button.clicked += handler;
    }

    private void Refresh()
    {
        if (root == null || levelLabel == null) return;

        // Win shows the level just beaten, which is NOT the current one: the campaign advances the
        // instant the level is won, so that quitting on this card cannot undo it.
        int number = mode == Mode.Win ? gameFlowManager.LastCompletedLevelNumber
                                      : levelLoader.CurrentLevelNumber();

        levelLabel.text = "LEVEL " + number.ToString("00");
    }

    private void StartConfetti()
    {
        if (confetti == null) return;

        // Burst() lays the pieces out across the element's width, so it needs a real width to work
        // with. On the very first frame the panel has not been laid out yet and everything would
        // spawn in a stack at x=0 — hence the wait for geometry.
        if (confetti.resolvedStyle.width > 0f)
        {
            confetti.Burst(gameFlowManager.LastCompletedLevelNumber);
            return;
        }

        confetti.UnregisterCallback<GeometryChangedEvent>(OnConfettiGeometry);
        confetti.RegisterCallback<GeometryChangedEvent>(OnConfettiGeometry);
    }

    private void OnConfettiGeometry(GeometryChangedEvent evt)
    {
        confetti.UnregisterCallback<GeometryChangedEvent>(OnConfettiGeometry);
        confetti.Burst(gameFlowManager.LastCompletedLevelNumber);
    }

    // Stepped from here rather than from a UI scheduler so the simulation advances exactly once
    // per rendered frame. Costs nothing once the burst has fallen out of view — IsRunning goes
    // false and the element stops being marked dirty.
    private void Update()
    {
        if (confetti != null && confetti.IsRunning) confetti.Advance(Time.deltaTime);
    }
}
