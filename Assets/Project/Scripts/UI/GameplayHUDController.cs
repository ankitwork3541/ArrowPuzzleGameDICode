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

        // GameFlowManager activates this panel just before LoadLevel(), so start hidden
        // rather than flashing the controls for the frames before the entrance begins.
        hudVisible = false;
        lives = livesManager.lives;
        maxLives = livesManager.maxLives;
        Apply();

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.AddNativeHeightListener(OnNativeHeightChanged);
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

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.RemoveNativeHeightListener(OnNativeHeightChanged);
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

        Bind(root.Q<Button>("back-button"), gameFlowManager.OnBackToHomeClicked);
        Bind(root.Q<Button>("restart-button"), gameFlowManager.OnRestartLevelClicked);
        Bind(root.Q<Button>("hint-button"), gameFlowManager.OnHintButtonClicked);
        Bind(gridButton, ToggleGuideLines);

        Apply();
    }

    // Unregister before registering so a second UI reload can't stack duplicates.
    private static void Bind(Button button, System.Action handler)
    {
        UIClickSound.Attach(button);

        button.clicked -= handler;
        button.clicked += handler;
    }

    private void ToggleGuideLines()
    {
        guideLines.Toggle();
        gridButton.EnableInClassList("grid-button--on", guideLines.IsVisible);
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

        OnNativeHeightChanged(AdsManager.Instance != null ? AdsManager.Instance.NativeHeightPixels : 0f);
    }

    // bottom-bar is absolutely positioned 70px up in USS. The ad is drawn beneath it, so the bar
    // lifts by exactly the measured ad height — and by nothing when there is no ad.
    private const float BottomBarBaseOffset = 70f;

    private void OnNativeHeightChanged(float heightPixels)
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
    //
    // Deliberately tested against the known blockers' worldBound instead of IPanel.Pick(): Pick
    // depends on the picking-mode of every ancestor being right, including the panel wrapper this
    // controller does not own, and one wrong ancestor silently blocks the entire board instead of
    // failing loudly. The blockers are few and explicit here — extend this when the HUD gains new
    // controls.
    //
    // `.top-panel` is checked as a whole, not just its buttons, because it is deliberately opaque
    // (it hides arrows panned up behind the progress line), so there is no board to tap up there.
    //
    // `.bottom-bar` is NOT opaque, so its worldBound is wider than what the player can actually
    // see — it spans the full screen width to keep its three buttons centred. Blocking that whole
    // strip is still the right call: it is only ~250px tall at the bottom of the screen, and the
    // alternative is that a tap next to Hint slides whichever arrow happens to sit behind it.
    public bool IsPointerOverUI(Vector2 screenPos)
    {
        // A hidden HUD must not eat board taps — during the entrance the whole root is
        // `visibility: hidden`, so the board really is visible under the top bar.
        if (root == null || root.panel == null || !hudVisible) return false;

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
