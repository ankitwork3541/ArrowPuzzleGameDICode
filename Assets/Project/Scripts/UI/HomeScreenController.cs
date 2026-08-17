using UnityEngine;
using UnityEngine.UIElements;

public class HomeScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private LevelLoader levelLoader;

    private VisualElement root;
    private Label levelLabel;
    private Label difficultyText;
    private Button playButton;

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
        Refresh();
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;

        levelLabel = root.Q<Label>("level-label");
        difficultyText = root.Q<Label>("difficulty-text");
        playButton = root.Q<Button>("play-button");

        UIClickSound.Attach(playButton);

        // Unregister first so a second UI reload can't stack a duplicate handler.
        playButton.clicked -= gameFlowManager.OnPlayButtonClicked;
        playButton.clicked += gameFlowManager.OnPlayButtonClicked;

        // Home is the current screen, so its own nav item only highlights.
        BottomNavBinder.Bind(root, "nav-home", null, OnSettingsNavClicked);

        Refresh();
    }

    private void OnSettingsNavClicked(ClickEvent evt)
    {
        gameFlowManager.OnSettingsNavClicked();
    }

    // Called every time Home becomes visible, so the level number and difficulty
    // stay correct after advancing a level.
    public void Refresh()
    {
        if (root == null) return;

        levelLabel.text = "LEVEL " + levelLoader.CurrentLevelNumber().ToString("00");
        difficultyText.text = GameFlowManager.GetDifficultyLabel(levelLoader.CurrentLevel());
    }
}
