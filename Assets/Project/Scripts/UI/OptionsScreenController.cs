using UnityEngine;
using UnityEngine.UIElements;

public class OptionsScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private FlowDirector gameFlowManager;
    [SerializeField] private OptionsManager settingsManager;

    // The Rate / Privacy / Terms links and which rows show at all come from the game config
    // (links.*, ui.options_rows.*), so nothing about them is hard-coded or set in the Inspector.

    private VisualElement root;
    private VisualElement soundSwitch, vibrationSwitch, refreshSwitch, motionSwitch;

    // Versioned overload of the reload callback (the plain one is obsolete). The lambda only drops the
    // version: these handlers rebuild their bindings from scratch on every reload, so a repeat of the
    // same version is harmless. Kept in a field so OnDisable removes the very same delegate.
    private PanelRenderer.VersionedUIReloadCallback reloadCallback;

    private void OnEnable()
    {
        reloadCallback = (reloaded, rootElement, version) => OnUIReload(reloaded, rootElement);
        panelRenderer.RegisterUIReloadCallback(reloadCallback);
        GameConfigService.Changed += OnConfigChanged;
        Apply();
        ApplyConfig();

        // Settings has its own placement (ads.placements.options); Home -> Settings hands over inside
        // one frame, and when both ask for the same slot the banner is kept rather than rebuilt.
        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.AddAdHeightListener(OnAdHeightChanged);
            PromoManager.Instance.ShowOptionsAd();
        }
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(reloadCallback);
        GameConfigService.Changed -= OnConfigChanged;

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.RemoveAdHeightListener(OnAdHeightChanged);

            // Deferred: going back to Home, that screen's OnEnable lands later in this same frame
            // and asks for the very same ad, which cancels this.
            PromoManager.Instance.HideScreenAdsDeferred();
        }
    }

    // Android draws the ad over the bottom of the screen; this only reserves matching room above
    // it. A height of zero collapses the box, so no ad means no gap.
    private void OnAdHeightChanged(float heightPixels)
    {
        if (root == null) return;

        PromoSpace.Apply(root.Q<VisualElement>("sponsor-space"), heightPixels,
                            PromoManager.Instance != null && PromoManager.Instance.AdPlaceholderVisible);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;

        soundSwitch = root.Q<VisualElement>("toggle-bell");
        vibrationSwitch = root.Q<VisualElement>("toggle-tremor");
        refreshSwitch = root.Q<VisualElement>("toggle-pulse");
        motionSwitch = root.Q<VisualElement>("toggle-calm");

        BindSwitch(soundSwitch, OnSoundClicked);
        BindSwitch(vibrationSwitch, OnVibrationClicked);
        BindSwitch(refreshSwitch, OnRefreshClicked);
        BindSwitch(motionSwitch, OnMotionClicked);

        BindLink("link-rate");
        BindLink("link-privacy");
        BindLink("link-terms");

        // The round back button at the top left does the same as the nav's Home item.
        Button backButton = root.Q<Button>("options-return-button");
        TapSound.Attach(backButton);
        backButton.clicked -= gameFlowManager.OnHomeNavClicked;
        backButton.clicked += gameFlowManager.OnHomeNavClicked;

        // Settings is the current screen, so its own nav item only highlights.
        TabDockBinder.Bind(root, "tab-options", OnHomeNavClicked, null);

        Apply();
        ApplyConfig();

        // The reload can land after OnEnable, which ran with no root to size the box in.
        OnAdHeightChanged(PromoManager.Instance != null ? PromoManager.Instance.AdHeightPixels : 0f);
    }

    // Same unregister-then-register guard as everywhere else: OnUIReload can fire
    // more than once, and these are plain VisualElements, so there is no Button
    // `clicked` event to lean on — a raw ClickEvent callback it is.
    //
    // The callback goes on the whole row, not on the 56px switch: the row is 132px high and full
    // width, which meets the 96px touch target. A tap on the switch itself bubbles up to the row,
    // so it still fires exactly once.
    private static void BindSwitch(VisualElement element, EventCallback<ClickEvent> handler)
    {
        VisualElement hitArea = element.parent ?? element;
        TapSound.Attach(hitArea);
        hitArea.UnregisterCallback(handler);
        hitArea.RegisterCallback(handler);
    }

    private void BindLink(string elementName)
    {
        VisualElement element = root.Q<VisualElement>(elementName);
        if (element == null) return;

        // One shared handler, unregistered by method group. The URL is looked up from the config at
        // the moment of the tap, so a fresh payload takes effect without rebinding.
        TapSound.Attach(element);
        element.UnregisterCallback<ClickEvent>(OnLinkClicked);
        element.RegisterCallback<ClickEvent>(OnLinkClicked);
    }

    private static string LinkFor(string elementName)
    {
        LinksSection links = GameConfigService.Current.links;

        switch (elementName)
        {
            case "link-rate": return links.rate_us;
            case "link-privacy": return links.privacy;
            case "link-terms": return links.terms;
            default: return null;
        }
    }

    private void OnLinkClicked(ClickEvent evt)
    {
        VisualElement element = (VisualElement)evt.currentTarget;
        string url = LinkFor(element.name);

        if (string.IsNullOrEmpty(url))
        {
            Debug.Log($"[Settings] '{element.name}' has no link in the game config.");
            return;
        }

        Application.OpenURL(url);
    }

    private void OnConfigChanged()
    {
        ApplyConfig();
    }

    // Shows or hides each row from ui.options_rows, and hides a link row whose link is empty. The
    // first row that stays visible loses its divider, so hiding the top rows never leaves a stray line.
    private void ApplyConfig()
    {
        if (root == null) return;

        GameConfig config = GameConfigService.Current;
        OptionsRows rows = config.ui.options_rows;

        SetRowVisible(vibrationSwitch?.parent, rows.vibration);
        SetRowVisible(soundSwitch?.parent, rows.sound);
        SetRowVisible(refreshSwitch?.parent, rows.native_refresh_rate);
        SetRowVisible(motionSwitch?.parent, rows.reduce_motion);
        SetRowVisible(root.Q<VisualElement>("link-rate"), GameConfigParser.LinkRowVisible(config, "rate_us"));
        SetRowVisible(root.Q<VisualElement>("link-privacy"), GameConfigParser.LinkRowVisible(config, "privacy"));
        SetRowVisible(root.Q<VisualElement>("link-terms"), GameConfigParser.LinkRowVisible(config, "terms"));

        VisualElement card = root.Q<VisualElement>("settings-card");
        if (card == null) return;

        bool first = true;
        foreach (VisualElement row in card.Children())
        {
            if (row.ClassListContains("entry--hidden")) continue;

            row.EnableInClassList("entry--no-sep", first);
            first = false;
        }

        // The Reduce Motion switch may be showing the config default rather than a saved choice.
        Apply();
    }

    private static void SetRowVisible(VisualElement row, bool visible)
    {
        if (row == null) return;

        row.EnableInClassList("entry--hidden", !visible);
    }

    private void OnSoundClicked(ClickEvent evt)
    {
        settingsManager.SetSound(!settingsManager.SoundEnabled);
        Apply();
    }

    private void OnVibrationClicked(ClickEvent evt)
    {
        settingsManager.SetVibration(!settingsManager.VibrationEnabled);
        Apply();
    }

    private void OnRefreshClicked(ClickEvent evt)
    {
        settingsManager.SetNativeRefreshRate(!settingsManager.NativeRefreshRateEnabled);
        Apply();
    }

    private void OnMotionClicked(ClickEvent evt)
    {
        settingsManager.SetReduceMotion(!settingsManager.ReduceMotion);
        Apply();
    }

    private void OnHomeNavClicked(ClickEvent evt)
    {
        gameFlowManager.OnHomeNavClicked();
    }

    // All the colour lives in USS; C# only ever toggles the class.
    private void Apply()
    {
        if (root == null) return;

        soundSwitch.EnableInClassList("toggle--on", settingsManager.SoundEnabled);
        vibrationSwitch.EnableInClassList("toggle--on", settingsManager.VibrationEnabled);
        refreshSwitch.EnableInClassList("toggle--on", settingsManager.NativeRefreshRateEnabled);
        motionSwitch.EnableInClassList("toggle--on", settingsManager.ReduceMotion);
    }
}
