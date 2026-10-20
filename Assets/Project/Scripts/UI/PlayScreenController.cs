using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private FlowDirector gameFlowManager;
    [SerializeField] private StageLoader levelLoader;
    [SerializeField] private EmberManager livesManager;
    [SerializeField] private StageManager levelManager;
    [SerializeField] private LatticeGuides guideLines;

    // The board reads taps straight from the Input System and knows nothing about the HUD, so
    // BoardCamera asks this whether a tap landed on UI. Singleton to match EmberManager /
    // StageManager / BoardCamera, and so it needs no extra Inspector wiring.
    public static PlayScreenController Instance;

    private VisualElement root;
    private VisualElement topPanel;
    private VisualElement bottomBar;
    private Label difficultyLabel;
    private Label levelNumberLabel;
    private VisualElement progressFill;
    private List<VisualElement> hearts;
    private Button gridButton;
    private Button hintButton;
    private VisualElement hintIcon;
    private VisualElement hintBadge;
    private Label hintBadgeLabel;

    // Cached state: the managers can fire their events before the UI has finished
    // loading, so we remember the latest values and (re)apply them once root exists.
    private int lives, maxLives;
    private int remainingDragons, totalDragons;
    private bool hudVisible;

    // Versioned overload of the reload callback (the plain one is obsolete). The lambda only drops the
    // version: these handlers rebuild their bindings from scratch on every reload, so a repeat of the
    // same version is harmless. Kept in a field so OnDisable removes the very same delegate.
    private PanelRenderer.VersionedUIReloadCallback reloadCallback;

    private void OnEnable()
    {
        Instance = this;
        reloadCallback = (reloaded, rootElement, version) => OnUIReload(reloaded, rootElement);
        panelRenderer.RegisterUIReloadCallback(reloadCallback);
        livesManager.OnLivesChanged += OnLivesChanged;
        levelManager.OnDragonCountChanged += OnDragonCountChanged;
        levelLoader.OnEntranceStart += OnEntranceStart;
        levelLoader.OnEntranceComplete += OnEntranceComplete;

        if (ClueManager.Instance != null)
        {
            ClueManager.Instance.OnHintStateChanged += OnHintStateChanged;
        }

        // FlowDirector activates this panel just before LoadLevel(), so start hidden
        // rather than flashing the controls for the frames before the entrance begins.
        hudVisible = false;
        lives = livesManager.lives;
        maxLives = livesManager.maxLives;
        Apply();

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.AddAdHeightListener(OnAdHeightChanged);
        }
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
        panelRenderer.UnregisterUIReloadCallback(reloadCallback);
        livesManager.OnLivesChanged -= OnLivesChanged;
        levelManager.OnDragonCountChanged -= OnDragonCountChanged;
        levelLoader.OnEntranceStart -= OnEntranceStart;
        levelLoader.OnEntranceComplete -= OnEntranceComplete;

        if (ClueManager.Instance != null)
        {
            ClueManager.Instance.OnHintStateChanged -= OnHintStateChanged;
        }

        HideAdLoadingModal();

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.RemoveAdHeightListener(OnAdHeightChanged);

            // Android draws the overlay above everything, so leaving it up follows the player to
            // whatever screen comes next. Whoever opens that screen shows its own ad.
            PromoManager.Instance.HideScreenAds();
        }
    }

    private void OnEntranceStart()
    {
        hudVisible = false;
        Apply();
    }

    private void OnEntranceComplete()
    {
        hudVisible = true;
        Apply();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;

        topPanel = root.Q<VisualElement>("crown-panel");
        bottomBar = root.Q<VisualElement>("lower-dock");
        difficultyLabel = root.Q<Label>("rank-label");
        levelNumberLabel = root.Q<Label>("stage-number");
        progressFill = root.Q<VisualElement>("glow-fill");
        hearts = root.Query<VisualElement>(className: "ember").ToList();
        gridButton = root.Q<Button>("guide-button");
        hintButton = root.Q<Button>("clue-button");
        hintIcon = root.Q<VisualElement>("clue-icon");
        hintBadge = root.Q<VisualElement>("clue-badge");
        hintBadgeLabel = root.Q<Label>("clue-badge-label");

        Bind(root.Q<Button>("return-button"), gameFlowManager.OnBackToHomeClicked);
        Bind(root.Q<Button>("redo-button"), gameFlowManager.OnRestartLevelClicked);
        Bind(hintButton, gameFlowManager.OnHintButtonClicked);
        Bind(gridButton, ToggleGuideLines);

        Apply();
    }

    // Unregister before registering so a second UI reload can't stack duplicates.
    private static void Bind(Button button, System.Action handler)
    {
        if (button == null) return;
        TapSound.Attach(button);

        button.clicked -= handler;
        button.clicked += handler;
    }

    private void ToggleGuideLines()
    {
        PromoInterstitialManager.ShowClickAd(() =>
        {
            guideLines.Toggle();
            gridButton.EnableInClassList("guide-button--on", guideLines.IsVisible);
        });
    }

    private void OnLivesChanged(int newLives, int newMax)
    {
        lives = newLives;
        maxLives = newMax;
        Apply();
    }

    private void OnDragonCountChanged(int remaining, int total)
    {
        remainingDragons = remaining;
        totalDragons = total;
        Apply();
    }

    private void OnHintStateChanged(int remaining, bool isAdRequired)
    {
        ApplyHintState();
    }

    public void HandleHintRequested()
    {
        if (WaitSheet.Showing)
        {
            return;
        }

        ClueManager hintMgr = ClueManager.Instance ?? FindAnyObjectByType<ClueManager>();
        if (hintMgr == null)
        {
            levelLoader.PlayHint();
            return;
        }

        if (!hintMgr.CanUseHint)
        {
            return;
        }

        hintMgr.RequestHint(
            onHintUnlocked: () =>
            {
                HideAdLoadingModal();
                levelLoader.PlayHint();
            },
            onFailed: () =>
            {
                HideAdLoadingModal();
            },
            onAdLoadingStart: () =>
            {
                ShowAdLoadingModal();
            },
            onAdOpening: () =>
            {
                HideAdLoadingModal();
            }
        );
    }

    // Kept as the hint flow's entry points; the modal itself is now the game-wide overlay.
    public void ShowAdLoadingModal()
    {
        WaitSheet.Show();
    }

    public void HideAdLoadingModal()
    {
        WaitSheet.Hide();
    }

    private void ApplyHintState()
    {
        if (hintButton == null) return;

        ClueManager hintMgr = ClueManager.Instance ?? FindAnyObjectByType<ClueManager>();
        if (hintMgr == null)
        {
            SetHintState(badgeText: null, adCost: false, enabled: true);
            return;
        }

        if (hintMgr.HasFreeHints)
        {
            SetHintState(hintMgr.RemainingHints.ToString(), adCost: false, enabled: true);
        }
        else if (hintMgr.IsRewardedAvailable)
        {
            SetHintState("AD", adCost: true, enabled: true);
        }
        else
        {
            SetHintState(badgeText: null, adCost: false, enabled: false);
        }
    }

    // One place for all three states, so the icon can never disagree with the badge. A null
    // badgeText means no badge at all: nothing left to spend and no ad to earn one with.
    private void SetHintState(string badgeText, bool adCost, bool enabled)
    {
        if (hintIcon != null)
        {
            hintIcon.EnableInClassList("tool-button__icon--hint", !adCost);
            hintIcon.EnableInClassList("tool-button__icon--hint-ad", adCost);
        }

        if (hintBadge != null)
        {
            hintBadge.style.display = badgeText != null ? DisplayStyle.Flex : DisplayStyle.None;
            hintBadge.EnableInClassList("clue-badge--ad", adCost);
        }

        if (hintBadgeLabel != null && badgeText != null)
        {
            hintBadgeLabel.text = badgeText;
        }

        hintButton.SetEnabled(enabled);
        hintButton.EnableInClassList("tool-button--disabled", !enabled);
    }

    private void Apply()
    {
        if (root == null) return;

        // The two bars fade, the ad box does not — see the .hud-hidden note in GameplayScreen.uss.
        if (topPanel != null) topPanel.EnableInClassList("chrome-hidden", !hudVisible);
        if (bottomBar != null) bottomBar.EnableInClassList("chrome-hidden", !hudVisible);

        difficultyLabel.text = FlowDirector.GetDifficultyLabel(levelLoader.CurrentLevel());
        levelNumberLabel.text = levelLoader.CurrentLevelNumber().ToString();

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].EnableInClassList("ember--empty", i >= lives);

        float solved = totalDragons > 0 ? (totalDragons - remainingDragons) / (float)totalDragons : 0f;
        progressFill.style.width = new Length(solved * 100f, LengthUnit.Percent);

        gridButton.EnableInClassList("guide-button--on", guideLines.IsVisible);

        ApplyHintState();

        OnAdHeightChanged(PromoManager.Instance != null ? PromoManager.Instance.AdHeightPixels : 0f);
    }

    // bottom-bar is absolutely positioned 70px up in USS. The ad is drawn beneath it, so the bar
    // lifts by exactly the measured ad height — and by nothing when there is no ad.
    private const float BottomBarBaseOffset = 70f;

    private void OnAdHeightChanged(float heightPixels)
    {
        if (root == null) return;

        VisualElement adSpace = root.Q<VisualElement>("sponsor-space");
        PromoSpace.Apply(adSpace, heightPixels,
                            PromoManager.Instance != null && PromoManager.Instance.AdPlaceholderVisible);

        float lift = heightPixels > 0f && adSpace != null
            ? PromoSpace.PixelsToPanel(adSpace, heightPixels)
            : 0f;

        if (bottomBar != null)
        {
            bottomBar.style.bottom = BottomBarBaseOffset + lift;
        }
    }

    // How much of the camera's view the HUD covers, in SCREEN PIXELS. The board is drawn behind
    // these bars, so BoardCamera treats them as content insets — the same idea as
    // UIScrollView's contentInset. Without it a cell at the very edge of the board can only ever be
    // panned as far as a spot that is already covered.
    //
    // The last good reading is kept because the camera needs these the instant a level loads, and
    // that is exactly when they are not available: FlowDirector reactivates this panel and calls
    // LoadLevel() in the same breath, so PanelRenderer has not re-run its reload callback yet and
    // the tree is not laid out. The bars are the same size on every level, so last level's numbers
    // are the right answer. Only the session's very first level has nothing to fall back on.
    public float TopInsetPixels => Measure(topPanel, true, ref cachedTopInset);
    public float BottomInsetPixels => Measure(bottomBar, false, ref cachedBottomInset);

    private float cachedTopInset;
    private float cachedBottomInset;

    private float Measure(VisualElement element, bool fromTop, ref float cache)
    {
        float measured = InsetPixels(element, fromTop);
        if (measured > 0f) cache = measured;
        return measured > 0f ? measured : cache;
    }

    // Measured from layout, not from hudVisible: the bars occupy that space whether or not they
    // have faded in yet, and letting the inset flip to zero mid-entrance would jolt the pan range.
    private float InsetPixels(VisualElement element, bool fromTop)
    {
        if (root == null || root.panel == null || element == null) return 0f;

        Rect bounds = element.worldBound;
        if (float.IsNaN(bounds.y) || float.IsNaN(bounds.height)) return 0f;

        // UI Toolkit's y grows downward from the top of the panel. bottom-bar is already lifted by
        // the ad's height, so measuring from its top edge down covers the ad along with it.
        float panelHeight = root.panel.visualTree.layout.height;
        float panelUnits = fromTop ? bounds.yMax : panelHeight - bounds.yMin;

        return Mathf.Max(0f, PromoSpace.PanelToPixels(root, panelUnits));
    }

    // Did this tap land on the HUD rather than the board? Without this, tapping Hint or the grid
    // toggle ALSO moves whatever arrow happens to sit underneath it.
    public bool IsPointerOverUI(Vector2 screenPos)
    {
        // A hidden HUD must not eat board taps — during the entrance the whole root is
        // `visibility: hidden`, so the board really is visible under the top bar.
        if (root == null || root.panel == null || !hudVisible) return false;

        // If loading modal is on screen, block all board interaction
        if (WaitSheet.Showing) return true;

        // UI Toolkit uses a top-left origin, screen coordinates are bottom-left, and
        // ScreenToPanel does NOT flip for you — Unity's own example inverts y first.
        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
            root.panel, new Vector2(screenPos.x, Screen.height - screenPos.y));

        return HitTest(topPanel, panelPos) || HitTest(bottomBar, panelPos);
    }

    private static bool HitTest(VisualElement element, Vector2 panelPos)
    {
        return element != null && element.worldBound.Contains(panelPos);
    }
}
