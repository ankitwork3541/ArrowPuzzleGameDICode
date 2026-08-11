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

    private VisualElement root;
    private VisualElement hudRoot;   // the .gameplay-root element itself, not the panel wrapper
    private Label difficultyLabel;
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
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        livesManager.OnLivesChanged -= OnLivesChanged;
        levelManager.OnArrowCountChanged -= OnArrowCountChanged;
        levelLoader.OnEntranceStart -= OnEntranceStart;
        levelLoader.OnEntranceComplete -= OnEntranceComplete;
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
        difficultyLabel = root.Q<Label>("difficulty-label");
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

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].EnableInClassList("heart--empty", i >= lives);

        float solved = totalArrows > 0 ? (totalArrows - remainingArrows) / (float)totalArrows : 0f;
        progressFill.style.width = new Length(solved * 100f, LengthUnit.Percent);

        gridButton.EnableInClassList("grid-button--on", guideLines.IsVisible);
    }
}
