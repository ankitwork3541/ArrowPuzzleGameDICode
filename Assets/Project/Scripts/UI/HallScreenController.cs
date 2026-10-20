using UnityEngine;
using UnityEngine.UIElements;

public class HallScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private FlowDirector gameFlowManager;
    [SerializeField] private StageLoader levelLoader;

    private VisualElement root;
    private Label levelLabel;
    private Label difficultyText;
    private Button playButton;
    private Button levelsButton;
    private Button settingsButton;
    private Label cardProgressText;
    private TrailMapController levelMap;

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

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.AddAdHeightListener(OnAdHeightChanged);
            PromoManager.Instance.ShowHomeAd();
        }
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(reloadCallback);

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.RemoveAdHeightListener(OnAdHeightChanged);

            // Deferred: Settings opens later in this same frame and wants the same ad, which
            // cancels this. Hiding outright would destroy the banner and pay for a fresh request
            // every time the player touched the nav.
            PromoManager.Instance.HideScreenAdsDeferred();
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

        PromoSpace.Apply(root.Q<VisualElement>("sponsor-space"), heightPixels,
                            PromoManager.Instance != null && PromoManager.Instance.AdPlaceholderVisible);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;

        levelLabel = root.Q<Label>("stage-label");
        difficultyText = root.Q<Label>("rank-text");
        playButton = root.Q<Button>("start-button");

        TapSound.Attach(playButton);

        // Unregister first so a second UI reload can't stack a duplicate handler.
        playButton.clicked -= gameFlowManager.OnPlayButtonClicked;
        playButton.clicked += gameFlowManager.OnPlayButtonClicked;

        // Rebuilt on every reload: the map is bound to elements of THIS document, which a reload
        // replaces, so a kept instance would be driving a tree that no longer exists.
        levelsButton = root.Q<Button>("trail-button");
        levelMap = new TrailMapController(root.Q<VisualElement>("trail-map-slot"),
                                          root.Q<VisualElement>("stage-body"),
                                          levelLoader, gameFlowManager.OnLevelSelected);

        TapSound.Attach(levelsButton);
        levelsButton.clicked -= OpenLevelMap;
        levelsButton.clicked += OpenLevelMap;

        // The round gear at the top right does the same as the nav's Settings item.
        settingsButton = root.Q<Button>("options-button");
        TapSound.Attach(settingsButton);
        settingsButton.clicked -= gameFlowManager.OnSettingsNavClicked;
        settingsButton.clicked += gameFlowManager.OnSettingsNavClicked;

        cardProgressText = root.Q<Label>("ledger-text");

        // Home is the current screen, so its own nav item only highlights.
        TabDockBinder.Bind(root, "tab-hall", null, OnSettingsNavClicked);

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
        difficultyText.text = FlowDirector.GetDifficultyLabel(levelLoader.CurrentLevel());

        // Levels beaten so far. The campaign size is deliberately never shown anywhere.
        cardProgressText.text = Mathf.Max(0, ProgressStore.Data.highestLevelCompleted) + " CLEARED";

        TabDockBinder.RefreshMilestones(root);

        if (levelMap != null && levelMap.IsOpen) levelMap.Refresh();

        OnAdHeightChanged(PromoManager.Instance != null ? PromoManager.Instance.AdHeightPixels : 0f);
    }
}
