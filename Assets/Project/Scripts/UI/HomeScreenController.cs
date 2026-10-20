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
    private Button levelsButton;
    private Button settingsButton;
    private Label cardProgressText;
    private LevelMapController levelMap;

    // Versioned overload of the reload callback (the plain one is obsolete). The lambda only drops the
    // version: these handlers rebuild their bindings from scratch on every reload, so a repeat of the
    // same version is harmless. Kept in a field so OnDisable removes the very same delegate.
    private PanelRenderer.VersionedUIReloadCallback reloadCallback;

    private void OnEnable()
    {
        reloadCallback = (reloaded, rootElement, version) => OnUIReload(reloaded, rootElement);
        panelRenderer.RegisterUIReloadCallback(reloadCallback);

        // Home always opens on its main column. Its document survives being deactivated, so a map
        // left open when the player started a level would still be open on the way back.
        levelMap?.Close();
        Refresh();

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.AddAdHeightListener(OnAdHeightChanged);
            AdsManager.Instance.ShowHomeAd();
        }
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(reloadCallback);

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.RemoveAdHeightListener(OnAdHeightChanged);

            // Deferred: Settings opens later in this same frame and wants the same ad, which
            // cancels this. Hiding outright would destroy the banner and pay for a fresh request
            // every time the player touched the nav.
            AdsManager.Instance.HideScreenAdsDeferred();
        }
    }

    // The ad — banner or native, whichever the backend asked for — is drawn by Android over the
    // bottom of the screen; this only reserves matching room above it. A height of zero collapses the
    // box entirely, so no ad means no gap.
    //
    // The room opens as soon as the ad is REQUESTED, not when it arrives, and carries a skeleton in
    // the meantime — otherwise the nav snapped upwards the instant the ad painted.
    private void OnAdHeightChanged(float heightPixels)
    {
        if (root == null) return;

        NativeAdSpace.Apply(root.Q<VisualElement>("native-ad-space"), heightPixels,
                            AdsManager.Instance != null && AdsManager.Instance.AdPlaceholderVisible);
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

        // Rebuilt on every reload: the map is bound to elements of THIS document, which a reload
        // replaces, so a kept instance would be driving a tree that no longer exists.
        levelsButton = root.Q<Button>("levels-button");
        levelMap = new LevelMapController(root.Q<VisualElement>("level-map-slot"),
                                          root.Q<VisualElement>("content"),
                                          levelLoader, gameFlowManager.OnLevelSelected);

        UIClickSound.Attach(levelsButton);
        levelsButton.clicked -= OpenLevelMap;
        levelsButton.clicked += OpenLevelMap;

        // The round gear at the top right does the same as the nav's Settings item.
        settingsButton = root.Q<Button>("settings-button");
        UIClickSound.Attach(settingsButton);
        settingsButton.clicked -= gameFlowManager.OnSettingsNavClicked;
        settingsButton.clicked += gameFlowManager.OnSettingsNavClicked;

        cardProgressText = root.Q<Label>("card-progress-text");

        // Home is the current screen, so its own nav item only highlights.
        BottomNavBinder.Bind(root, "nav-home", null, OnSettingsNavClicked);

        Refresh();
    }

    private void OnSettingsNavClicked(ClickEvent evt)
    {
        gameFlowManager.OnSettingsNavClicked();
    }

    private void OpenLevelMap()
    {
        levelMap?.Open();
    }

    // Android back while the map is up closes the map instead of leaving the app. Returns true when
    // it consumed the press.
    public bool TryCloseMap()
    {
        if (levelMap == null || !levelMap.IsOpen) return false;

        levelMap.Close();
        return true;
    }

    // Called every time Home becomes visible, so the level number and difficulty
    // stay correct after advancing a level.
    public void Refresh()
    {
        if (root == null) return;

        levelLabel.text = "LEVEL " + levelLoader.CurrentLevelNumber().ToString("00");
        difficultyText.text = GameFlowManager.GetDifficultyLabel(levelLoader.CurrentLevel());

        // Levels beaten so far. The campaign size is deliberately never shown anywhere.
        cardProgressText.text = Mathf.Max(0, SaveSystem.Data.highestLevelCompleted) + " CLEARED";

        BottomNavBinder.RefreshMilestones(root);

        if (levelMap != null && levelMap.IsOpen) levelMap.Refresh();

        OnAdHeightChanged(AdsManager.Instance != null ? AdsManager.Instance.AdHeightPixels : 0f);
    }
}
