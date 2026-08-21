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

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.AddAdHeightListener(OnAdHeightChanged);
            AdsManager.Instance.ShowHomeAd();
        }
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.RemoveAdHeightListener(OnAdHeightChanged);
            AdsManager.Instance.HideScreenAds();
        }
    }

    // The ad — banner or native, whichever the backend asked for — is drawn by Android over the
    // bottom of the screen; this only reserves matching room above it. A height of zero collapses the
    // box entirely, so no ad means no gap.
    private void OnAdHeightChanged(float heightPixels)
    {
        if (root == null) return;

        NativeAdSpace.Apply(root.Q<VisualElement>("native-ad-space"), heightPixels);
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

        BottomNavBinder.RefreshMilestones(root);

        OnAdHeightChanged(AdsManager.Instance != null ? AdsManager.Instance.AdHeightPixels : 0f);
    }
}
