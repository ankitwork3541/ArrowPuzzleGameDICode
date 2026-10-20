using UnityEngine;
using UnityEngine.UIElements;

public class OptionsScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private FlowDirector gameFlowManager;
    [SerializeField] private OptionsManager settingsManager;

    // Left empty on purpose: the store listing and the legal pages do not exist yet.
    // An empty field logs instead of opening a broken link, so filling these in later
    // is an Inspector change with no code change.
    [Header("Links (fill in when the real pages exist)")]
    [SerializeField] private string rateUsUrl;
    [SerializeField] private string privacyUrl;
    [SerializeField] private string termsUrl;

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
        Apply();

        // Settings is reached from Home's own bottom nav and is the same shell, so it carries the
        // same ad. There is no settings_screen digit from the backend — main_screen governs both.
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

        BindLink("link-rate", rateUsUrl);
        BindLink("link-privacy", privacyUrl);
        BindLink("link-terms", termsUrl);

        // The round back button at the top left does the same as the nav's Home item.
        Button backButton = root.Q<Button>("options-return-button");
        TapSound.Attach(backButton);
        backButton.clicked -= gameFlowManager.OnHomeNavClicked;
        backButton.clicked += gameFlowManager.OnHomeNavClicked;

        // Settings is the current screen, so its own nav item only highlights.
        TabDockBinder.Bind(root, "tab-options", OnHomeNavClicked, null);

        Apply();

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

    private void BindLink(string elementName, string url)
    {
        VisualElement element = root.Q<VisualElement>(elementName);
        if (element == null) return;

        // The URL rides on the element itself, so all the link rows can share one
        // handler and still be unregistered by method group.
        element.userData = url;
        TapSound.Attach(element);
        element.UnregisterCallback<ClickEvent>(OnLinkClicked);
        element.RegisterCallback<ClickEvent>(OnLinkClicked);
    }

    private void OnLinkClicked(ClickEvent evt)
    {
        VisualElement element = (VisualElement)evt.currentTarget;
        string url = element.userData as string;

        if (string.IsNullOrEmpty(url))
        {
            Debug.Log($"[Settings] '{element.name}' has no URL set yet.");
            return;
        }

        Application.OpenURL(url);
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
