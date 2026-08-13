using UnityEngine;
using UnityEngine.UIElements;

public class SettingsScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private SettingsManager settingsManager;

    // Left empty on purpose: the store listing and the legal pages do not exist yet.
    // An empty field logs instead of opening a broken link, so filling these in later
    // is an Inspector change with no code change.
    [Header("Links (fill in when the real pages exist)")]
    [SerializeField] private string rateUsUrl;
    [SerializeField] private string writeUsUrl;
    [SerializeField] private string privacyUrl;
    [SerializeField] private string termsUrl;

    private VisualElement root;
    private VisualElement soundSwitch, vibrationSwitch, refreshSwitch;

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
        Apply();
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;

        soundSwitch = root.Q<VisualElement>("switch-sound");
        vibrationSwitch = root.Q<VisualElement>("switch-vibration");
        refreshSwitch = root.Q<VisualElement>("switch-refresh");

        BindSwitch(soundSwitch, OnSoundClicked);
        BindSwitch(vibrationSwitch, OnVibrationClicked);
        BindSwitch(refreshSwitch, OnRefreshClicked);

        BindLink("link-rate", rateUsUrl);
        BindLink("link-write", writeUsUrl);
        BindLink("link-privacy", privacyUrl);
        BindLink("link-terms", termsUrl);

        // Settings is the current screen, so its own nav item only highlights.
        BottomNavBinder.Bind(root, "nav-settings", OnHomeNavClicked, null);

        Apply();
    }

    // Same unregister-then-register guard as everywhere else: OnUIReload can fire
    // more than once, and these are plain VisualElements, so there is no Button
    // `clicked` event to lean on — a raw ClickEvent callback it is.
    private static void BindSwitch(VisualElement element, EventCallback<ClickEvent> handler)
    {
        element.UnregisterCallback(handler);
        element.RegisterCallback(handler);
    }

    private void BindLink(string elementName, string url)
    {
        VisualElement element = root.Q<VisualElement>(elementName);
        if (element == null) return;

        // The URL rides on the element itself, so all four rows can share one
        // handler and still be unregistered by method group.
        element.userData = url;
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

    private void OnHomeNavClicked(ClickEvent evt)
    {
        gameFlowManager.OnHomeNavClicked();
    }

    // All the colour lives in USS; C# only ever toggles the class.
    private void Apply()
    {
        if (root == null) return;

        soundSwitch.EnableInClassList("switch--on", settingsManager.SoundEnabled);
        vibrationSwitch.EnableInClassList("switch--on", settingsManager.VibrationEnabled);
        refreshSwitch.EnableInClassList("switch--on", settingsManager.NativeRefreshRateEnabled);
    }
}
