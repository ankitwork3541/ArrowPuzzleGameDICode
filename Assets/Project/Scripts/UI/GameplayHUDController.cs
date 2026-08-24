using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameplayHUDController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private LivesManager livesManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GuideLines guideLines;

    // The board reads taps straight from the Input System and knows nothing about the HUD, so
    // CameraController asks this whether a tap landed on UI. Singleton to match LivesManager /
    // LevelManager / CameraController, and so it needs no extra Inspector wiring.
    public static GameplayHUDController Instance;

    private VisualElement root;
    private VisualElement hudRoot;   // the .gameplay-root element itself, not the panel wrapper
    private VisualElement topPanel;
    private VisualElement bottomBar;
    private Label difficultyLabel;
    private Label levelNumberLabel;
    private VisualElement progressFill;
    private List<VisualElement> hearts;
    private Button gridButton;
    private Button hintButton;
    private VisualElement hintBadge;
    private Label hintBadgeLabel;

    // Cached state: the managers can fire their events before the UI has finished
    // loading, so we remember the latest values and (re)apply them once root exists.
    private int lives, maxLives;
    private int remainingArrows, totalArrows;
    private bool hudVisible;

    private void OnEnable()
    {
        Instance = this;
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
        livesManager.OnLivesChanged += OnLivesChanged;
        levelManager.OnArrowCountChanged += OnArrowCountChanged;
        levelLoader.OnEntranceStart += OnEntranceStart;
        levelLoader.OnEntranceComplete += OnEntranceComplete;

        if (HintManager.Instance != null)
        {
            HintManager.Instance.OnHintStateChanged += OnHintStateChanged;
        }

        // GameFlowManager activates this panel just before LoadLevel(), so start hidden
        // rather than flashing the controls for the frames before the entrance begins.
        hudVisible = false;
        lives = livesManager.lives;
        maxLives = livesManager.maxLives;
        Apply();

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.AddAdHeightListener(OnAdHeightChanged);
        }
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        livesManager.OnLivesChanged -= OnLivesChanged;
        levelManager.OnArrowCountChanged -= OnArrowCountChanged;
        levelLoader.OnEntranceStart -= OnEntranceStart;
        levelLoader.OnEntranceComplete -= OnEntranceComplete;

        if (HintManager.Instance != null)
        {
            HintManager.Instance.OnHintStateChanged -= OnHintStateChanged;
        }

        HideAdLoadingModal();

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.RemoveAdHeightListener(OnAdHeightChanged);

            // Android draws the overlay above everything, so leaving it up follows the player to
            // whatever screen comes next. Whoever opens that screen shows its own ad.
            AdsManager.Instance.HideScreenAds();
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

        hudRoot = root.Q<VisualElement>("gameplay-root") ?? root;
        topPanel = root.Q<VisualElement>("top-panel");
        bottomBar = root.Q<VisualElement>("bottom-bar");
        difficultyLabel = root.Q<Label>("difficulty-label");
        levelNumberLabel = root.Q<Label>("level-number");
        progressFill = root.Q<VisualElement>("progress-fill");
        hearts = root.Query<VisualElement>(className: "heart").ToList();
        gridButton = root.Q<Button>("grid-button");
        hintButton = root.Q<Button>("hint-button");
        hintBadge = root.Q<VisualElement>("hint-badge");
        hintBadgeLabel = root.Q<Label>("hint-badge-label");

        Bind(root.Q<Button>("back-button"), gameFlowManager.OnBackToHomeClicked);
        Bind(root.Q<Button>("restart-button"), gameFlowManager.OnRestartLevelClicked);
        Bind(hintButton, gameFlowManager.OnHintButtonClicked);
        Bind(gridButton, ToggleGuideLines);

        Apply();
    }

    // Unregister before registering so a second UI reload can't stack duplicates.
    private static void Bind(Button button, System.Action handler)
    {
        if (button == null) return;
        UIClickSound.Attach(button);

        button.clicked -= handler;
        button.clicked += handler;
    }

    private void ToggleGuideLines()
    {
        InterstitialManager.ShowClickAd(() =>
        {
            guideLines.Toggle();
            gridButton.EnableInClassList("grid-button--on", guideLines.IsVisible);
        });
    }

    private void OnLivesChanged(int newLives, int newMax)
    {
        lives = newLives;
        maxLives = newMax;
        Apply();
    }

    private void OnArrowCountChanged(int remaining, int total)
    {
        remainingArrows = remaining;
        totalArrows = total;
        Apply();
    }

    private void OnHintStateChanged(int remaining, bool isAdRequired)
    {
        ApplyHintState();
    }

    public void HandleHintRequested()
    {
        if (AdLoadingOverlay.Showing)
        {
            return;
        }

        HintManager hintMgr = HintManager.Instance ?? FindFirstObjectByType<HintManager>();
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
        AdLoadingOverlay.Show();
    }

    public void HideAdLoadingModal()
    {
        AdLoadingOverlay.Hide();
    }

    private void ApplyHintState()
    {
        if (hintButton == null) return;

        HintManager hintMgr = HintManager.Instance ?? FindFirstObjectByType<HintManager>();
        if (hintMgr == null)
        {
            if (hintBadge != null) hintBadge.style.display = DisplayStyle.None;
            hintButton.SetEnabled(true);
            hintButton.EnableInClassList("oct-button--disabled", false);
            return;
        }

        if (hintMgr.HasFreeHints)
        {
            if (hintBadge != null)
            {
                hintBadge.style.display = DisplayStyle.Flex;
                hintBadge.EnableInClassList("hint-badge--ad", false);
            }
            if (hintBadgeLabel != null)
            {
                hintBadgeLabel.text = hintMgr.RemainingHints.ToString();
            }
            hintButton.SetEnabled(true);
            hintButton.EnableInClassList("oct-button--disabled", false);
        }
        else if (hintMgr.IsRewardedAvailable)
        {
            if (hintBadge != null)
            {
                hintBadge.style.display = DisplayStyle.Flex;
                hintBadge.EnableInClassList("hint-badge--ad", true);
            }
            if (hintBadgeLabel != null)
            {
                hintBadgeLabel.text = "AD";
            }
            hintButton.SetEnabled(true);
            hintButton.EnableInClassList("oct-button--disabled", false);
        }
        else
        {
            if (hintBadge != null)
            {
                hintBadge.style.display = DisplayStyle.None;
            }
            hintButton.SetEnabled(false);
            hintButton.EnableInClassList("oct-button--disabled", true);
        }
    }

    private void Apply()
    {
        if (root == null) return;

        hudRoot.EnableInClassList("gameplay-root--hidden", !hudVisible);

        difficultyLabel.text = GameFlowManager.GetDifficultyLabel(levelLoader.CurrentLevel());
        levelNumberLabel.text = levelLoader.CurrentLevelNumber().ToString();

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].EnableInClassList("heart--empty", i >= lives);

        float solved = totalArrows > 0 ? (totalArrows - remainingArrows) / (float)totalArrows : 0f;
        progressFill.style.width = new Length(solved * 100f, LengthUnit.Percent);

        gridButton.EnableInClassList("grid-button--on", guideLines.IsVisible);

        ApplyHintState();

        OnAdHeightChanged(AdsManager.Instance != null ? AdsManager.Instance.AdHeightPixels : 0f);
    }

    // bottom-bar is absolutely positioned 70px up in USS. The ad is drawn beneath it, so the bar
    // lifts by exactly the measured ad height — and by nothing when there is no ad.
    private const float BottomBarBaseOffset = 70f;

    private void OnAdHeightChanged(float heightPixels)
    {
        if (root == null) return;

        VisualElement adSpace = root.Q<VisualElement>("native-ad-space");
        NativeAdSpace.Apply(adSpace, heightPixels);

        float lift = heightPixels > 0f && adSpace != null
            ? NativeAdSpace.PixelsToPanel(adSpace, heightPixels)
            : 0f;

        if (bottomBar != null)
        {
            bottomBar.style.bottom = BottomBarBaseOffset + lift;
        }
    }

    // Did this tap land on the HUD rather than the board? Without this, tapping Hint or the grid
    // toggle ALSO moves whatever arrow happens to sit underneath it.
    public bool IsPointerOverUI(Vector2 screenPos)
    {
        // A hidden HUD must not eat board taps — during the entrance the whole root is
        // `visibility: hidden`, so the board really is visible under the top bar.
        if (root == null || root.panel == null || !hudVisible) return false;

        // If loading modal is on screen, block all board interaction
        if (AdLoadingOverlay.Showing) return true;

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
