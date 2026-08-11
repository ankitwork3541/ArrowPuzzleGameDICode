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

    private VisualElement root;
    private Label difficultyLabel;
    private VisualElement progressFill;
    private List<VisualElement> hearts;

    // Cached state: the managers can fire their events before the UI has finished
    // loading, so we remember the latest values and (re)apply them once root exists.
    private int lives, maxLives;
    private int remainingArrows, totalArrows;

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
        livesManager.OnLivesChanged += OnLivesChanged;
        levelManager.OnArrowCountChanged += OnArrowCountChanged;

        lives = livesManager.lives;
        maxLives = livesManager.maxLives;
        Apply();
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        livesManager.OnLivesChanged -= OnLivesChanged;
        levelManager.OnArrowCountChanged -= OnArrowCountChanged;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;

        difficultyLabel = root.Q<Label>("difficulty-label");
        progressFill = root.Q<VisualElement>("progress-fill");
        hearts = root.Query<VisualElement>(className: "heart").ToList();

        Bind(root.Q<Button>("back-button"), gameFlowManager.OnBackToHomeClicked);
        Bind(root.Q<Button>("restart-button"), gameFlowManager.OnRestartLevelClicked);
        Bind(root.Q<Button>("hint-button"), gameFlowManager.OnHintButtonClicked);

        Apply();
    }

    // Unregister before registering so a second UI reload can't stack duplicates.
    private static void Bind(Button button, System.Action handler)
    {
        button.clicked -= handler;
        button.clicked += handler;
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

        difficultyLabel.text = GameFlowManager.GetDifficultyLabel(levelLoader.CurrentLevel());

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].EnableInClassList("heart--empty", i >= lives);

        float solved = totalArrows > 0 ? (totalArrows - remainingArrows) / (float)totalArrows : 0f;
        progressFill.style.width = new Length(solved * 100f, LengthUnit.Percent);
    }
}
