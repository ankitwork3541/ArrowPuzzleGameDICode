using UnityEngine;
using UnityEngine.UIElements;

// Drives both the Win and the Lose card. One class rather than two because the screens differ only
// in wording, accent colour and which pair of FlowDirector methods the buttons call — the two
// UXML files already carry everything that is genuinely different, and the element names match so
// the wiring below can stay identical.
public class SheetScreenController : MonoBehaviour
{
    public enum Mode { Win, Lose }

    [SerializeField] private Mode mode = Mode.Win;
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private FlowDirector gameFlowManager;
    [SerializeField] private StageLoader levelLoader;

    private VisualElement root;
    private Label levelLabel;
    private PetalBurstView confetti;

    // Versioned overload of the reload callback (the plain one is obsolete). The lambda only drops the
    // version: these handlers rebuild their bindings from scratch on every reload, so a repeat of the
    // same version is harmless. Kept in a field so OnDisable removes the very same delegate.
    private PanelRenderer.VersionedUIReloadCallback reloadCallback;

    private void OnEnable()
    {
        reloadCallback = (reloaded, rootElement, version) => OnUIReload(reloaded, rootElement);
        panelRenderer.RegisterUIReloadCallback(reloadCallback);

        // On a second win the panel is already loaded, so OnUIReload will not fire again and
        // `root` is still the one from last time. Refreshing here is what keeps the level number
        // correct — same reason HallScreenController does it.
        Refresh();
        StartConfetti();
        PlaySheetIn();

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.AddNativeHeightListener(OnNativeHeightChanged);
            adRequested = false;
            HookAdSpace();
        }
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(reloadCallback);
        if (confetti != null) confetti.Stop();

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.RemoveNativeHeightListener(OnNativeHeightChanged);
            PromoManager.Instance.HideNativeAd();
        }

        if (placeRoutine != null)
        {
            StopCoroutine(placeRoutine);
            placeRoutine = null;
        }

        if (settleRoutine != null)
        {
            StopCoroutine(settleRoutine);
            settleRoutine = null;
        }

        UnhookScroll();

        VisualElement space = AdSpace;
        if (space != null)
        {
            space.UnregisterCallback<GeometryChangedEvent>(OnAdSpaceGeometryChanged);
            PromoSpace.Collapse(space);
        }
    }

    private VisualElement AdSpace => root != null ? root.Q<VisualElement>("sheet-sponsor-space") : null;

    // Which ads.placements entry this sheet reads: win_sheet or lose_sheet.
    private PromoScreen SheetPlacement => mode == Mode.Win ? PromoScreen.WinSheet : PromoScreen.LoseSheet;

    // Reserves the box and fills it with the loading skeleton until the overlay paints. Room here is
    // only ever opened for an ad already in hand, so this is a short bridge — a couple of frames plus
    // however long RenderTemplate takes — not an open-ended wait like the bottom slots have.
    //
    // The skeleton needs a box tall enough to read as one. While the ad's real height is still being
    // measured the box is only a sliver, and a skeleton drawn into that comes out as a squashed strip
    // rather than a placeholder.
    private const float MinPlaceholderPixels = 80f;

    private void ApplyAdSpace(VisualElement space, float heightPixels)
    {
        PromoSpace.Apply(space, heightPixels);

        bool pending = PromoManager.Instance != null
                       && PromoManager.Instance.Native != null
                       && !PromoManager.Instance.Native.ShowingAd;

        PromoPlaceholder.SetVisible(space, heightPixels >= MinPlaceholderPixels && pending,
                                 PromoSpace.PixelsToPanel(space, heightPixels));
    }

    private bool adRequested;

    // The ad has to be positioned over the reserved box, but that box has no width until UI Toolkit
    // has laid it out, and worldBound reads NaN until then. Polling for that layout is deliberate:
    // relying on GeometryChangedEvent alone silently never fired, and the ad was simply never asked
    // for.
    private void HookAdSpace()
    {
        VisualElement space = AdSpace;
        if (space == null)
        {
            return;
        }

        if (PromoManager.Instance == null || PromoManager.Instance.Settings == null)
        {
            Debug.Log("[NativeAd] Result card: no PromoManager yet.");
            return;
        }

        // Checked before anything is opened, not after. An ad that was never preloaded is not loaded
        // live here — the player taps Next or Retry long before a request would land — so if none is
        // in hand the card must look exactly as it does with ads off.
        if (!PromoManager.Instance.LevelEndNativeReady(SheetPlacement))
        {
            Debug.Log("[NativeAd] Result card: ads off, no unit ID, unsupported style, or nothing "
                      + "preloaded — leaving no space.");
            PromoSpace.Collapse(space);
            return;
        }

        space.UnregisterCallback<GeometryChangedEvent>(OnAdSpaceGeometryChanged);
        space.RegisterCallback<GeometryChangedEvent>(OnAdSpaceGeometryChanged);

        HookScroll();

        // Opened to a sliver so the box gets a width. Its real height arrives from the measurement.
        space.style.display = DisplayStyle.Flex;
        space.style.height = 1f;

        if (placeRoutine != null)
        {
            StopCoroutine(placeRoutine);
        }

        placeRoutine = StartCoroutine(PlaceAdWhenLaidOut());
    }

    private Coroutine placeRoutine;

    private System.Collections.IEnumerator PlaceAdWhenLaidOut()
    {
        VisualElement space = AdSpace;
        if (space == null || PromoManager.Instance == null)
        {
            placeRoutine = null;
            yield break;
        }

        // No height is predicted here. The box only needs a resolved RECT so the overlay can be
        // placed at its top-left; the height arrives later, measured off the rendered template, and
        // grows the box then. That works because the scroll content is top-aligned: growing the last
        // element does not move its own top edge, so the position handed to the plugin stays correct
        // with nothing to correct afterwards. Guessing a height here instead (which a since-reverted
        // change tried) re-centres the whole card+ad group once the guess is corrected — shifting the
        // CARD, not just the ad box — which is what let the ad's top edge end up underneath the
        // card's bottom border on the session's first render.
        //
        // The sliver is what gives the box a laid-out rect to read. Zero height would still resolve,
        // but a 1px box is also what the skeleton needs to be measurable.
        ApplyAdSpace(space, 1f);

        // Two frames: one for the box's own height, one for the layout around it. Then wait for a
        // resolved rect, because worldBound reads NaN until UI Toolkit has laid the panel out.
        float deadline = Time.realtimeSinceStartup + 2f;
        Vector2Int topLeftDp;

        do
        {
            yield return null;

            if (space == null || PromoManager.Instance == null)
            {
                placeRoutine = null;
                yield break;
            }
        }
        while (!PromoSpace.TryTopLeftDp(space, out topLeftDp)
               && Time.realtimeSinceStartup < deadline);

        if (!PromoSpace.TryTopLeftDp(space, out topLeftDp))
        {
            placeRoutine = null;
            Debug.LogWarning("[NativeAd] Result card never laid out; no ad requested.");
            yield break;
        }

        adRequested = true;
        Debug.Log("[NativeAd] Result card ad box laid out, ad at "
                  + topLeftDp.x + "," + topLeftDp.y + "dp"
                  + " (screen " + Screen.width + "x" + Screen.height
                  + ", density " + PromoSpace.DisplayDensity + "). Height follows the measurement.");

        // Consumes the ad preloaded when the level started, so the card does not wait on a network
        // round trip.
        PromoManager.Instance.ShowLevelEndNative(SheetPlacement,
                                                 PromoNativePlacement.At(topLeftDp.x, topLeftDp.y));

        placeRoutine = null;
    }

    // The overlay is an Android view at a fixed screen position, so scrolling the card moves the box
    // out from under it unless the ad is moved too. Scrolling is a TRANSFORM on the content
    // container, not a layout change, so GeometryChangedEvent never fires for it — the scroller's own
    // value is the only signal.
    private void HookScroll()
    {
        ScrollView scroll = root != null ? root.Q<ScrollView>("sheet-scroll") : null;
        if (scroll == null)
        {
            return;
        }

        scroll.verticalScroller.valueChanged -= OnScrolled;
        scroll.verticalScroller.valueChanged += OnScrolled;
    }

    private void UnhookScroll()
    {
        ScrollView scroll = root != null ? root.Q<ScrollView>("sheet-scroll") : null;
        if (scroll != null)
        {
            scroll.verticalScroller.valueChanged -= OnScrolled;
        }
    }

    private void OnScrolled(float value)
    {
        NudgeAdToBox();
    }

    // Every later layout pass — including the one caused by the ad's measured height — nudges the
    // ad to the box's new position.
    private void OnAdSpaceGeometryChanged(GeometryChangedEvent evt)
    {
        if (!adRequested || PromoManager.Instance == null)
        {
            return;
        }

        NudgeAdToBox();
    }

    private void NudgeAdToBox()
    {
        if (!adRequested || PromoManager.Instance == null)
        {
            return;
        }

        if (PromoSpace.TryTopLeftDp(AdSpace, out Vector2Int topLeftDp))
        {
            PromoManager.Instance.MoveNativeAd(topLeftDp.x, topLeftDp.y);
        }
    }

    // Reserves exactly the ad's measured height. Zero collapses the box completely, so no ad leaves
    // no trace of one.
    private void OnNativeHeightChanged(float heightPixels)
    {
        VisualElement space = AdSpace;
        if (space == null) return;

        ApplyAdSpace(space, heightPixels);

        // Growing the box re-centres the card-and-ad group, which moves the box. GeometryChangedEvent
        // is supposed to catch that, but it fires only when UI Toolkit decides the rect changed — and
        // on a second visit the panel can hand back last visit's rect for a frame or two, so the
        // event either arrives with stale numbers or not at all. Nudging the ad for a short run of
        // frames converges no matter when the layout actually settles.
        if (settleRoutine != null)
        {
            StopCoroutine(settleRoutine);
        }

        settleRoutine = StartCoroutine(SettlePosition());
    }

    private Coroutine settleRoutine;

    private System.Collections.IEnumerator SettlePosition()
    {
        for (int frame = 0; frame < 12; frame++)
        {
            yield return null;

            if (!adRequested || PromoManager.Instance == null)
            {
                settleRoutine = null;
                yield break;
            }

            if (PromoSpace.TryTopLeftDp(AdSpace, out Vector2Int topLeftDp))
            {
                PromoManager.Instance.MoveNativeAd(topLeftDp.x, topLeftDp.y);
            }
        }

        settleRoutine = null;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;
        levelLabel = root.Q<Label>("stage-label");

        Bind(root.Q<Button>("go-button"),
             mode == Mode.Win ? gameFlowManager.OnNextLevelClicked
                              : gameFlowManager.OnRetryLevelClicked);

        Bind(root.Q<Button>("hall-button"),
             mode == Mode.Win ? gameFlowManager.OnWinHomeClicked
                              : gameFlowManager.OnLoseHomeClicked);

        if (mode == Mode.Win && confetti == null)
        {
            // Added last so it sits above the card. It is picking-mode Ignore, so falling confetti
            // never steals a tap from the NEXT button underneath it.
            confetti = new PetalBurstView();
            (root.Q<VisualElement>("page-root") ?? root).Add(confetti);
        }

        Refresh();
        StartConfetti();
        PlaySheetIn();

        // On the first show this fires after OnEnable, which ran with no root to query yet.
        HookAdSpace();
    }

    // The card is a bottom sheet: park it below the screen, then let it slide up. The hidden class
    // has a zero-length transition so it snaps down at once; removing it a beat later is what runs
    // the slow slide in ResultScreen.uss. The beat is needed because both class changes in one frame
    // would be resolved together and no transition would play.
    private void PlaySheetIn()
    {
        if (root == null) return;

        VisualElement card = root.Q<VisualElement>("sheet-card");
        if (card == null) return;

        // Reduce Motion: the sheet is simply there, no slide.
        if (OptionsManager.ReduceMotionOn)
        {
            card.RemoveFromClassList("sheet-card--hidden");
            return;
        }

        card.AddToClassList("sheet-card--hidden");
        card.schedule.Execute(() => card.RemoveFromClassList("sheet-card--hidden")).StartingIn(60);
    }

    // Unregister before registering so a second UI reload cannot stack duplicate handlers.
    private static void Bind(Button button, System.Action handler)
    {
        TapSound.Attach(button);

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

        // Reduce Motion, or ui.confetti off in the game config: no falling confetti at all (a burst
        // already in flight is stopped).
        if (OptionsManager.ReduceMotionOn || !GameConfigService.Current.ui.confetti)
        {
            confetti.Stop();
            return;
        }

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
