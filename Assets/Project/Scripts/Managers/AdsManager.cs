using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

// Owns the ad stack: gathers consent, initialises the SDK, and keeps AdSettings fed from the
// backend. Nothing else in the game touches the Google Mobile Ads API directly.
[RequireComponent(typeof(AdConfigFetcher))]
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    [Tooltip("Leave empty to load Resources/AdConfig.")]
    public AdConfig adConfig;

    public AdSettings Settings { get; private set; }
    public bool Ready { get; private set; }

    // Null until the matching component is added to this GameObject.
    public InterstitialAdUnit Interstitial { get; private set; }
    public BannerAdUnit Banner { get; private set; }
    public NativeAdUnit Native { get; private set; }

    // Ad formats wait on this instead of polling Ready.
    public event System.Action OnReady;

    AdConfigFetcher fetcher;

    void Awake()
    {
        Instance = this;

        if (adConfig == null)
        {
            adConfig = LoadConfigFromResources();
        }

        if (adConfig == null)
        {
            Debug.LogError("[Ads] No AdConfig — create one at Resources/AdConfig. Ads disabled.");
            enabled = false;
            return;
        }

        Settings = new AdSettings(adConfig);
        fetcher = GetComponent<AdConfigFetcher>();
        Interstitial = GetComponent<InterstitialAdUnit>();
        Banner = GetComponent<BannerAdUnit>();
        Native = GetComponent<NativeAdUnit>();

        // Last launch's payload, so an offline session still runs on real settings rather than
        // whatever shipped in the build months ago.
        if (Settings.ApplyRemote(fetcher.LoadCached()))
        {
            Debug.Log("[Ads] Cached config applied.");
        }
    }

    void Start()
    {
        StartCoroutine(fetcher.Fetch(config =>
        {
            if (Settings.ApplyRemote(config))
            {
                Debug.Log("[Ads] Fresh config applied.");
            }
        }));

        RequestConsent();
    }

    // Resources.Load needs the exact file name, and Unity names a new asset after the menu entry
    // ("Ad Config"), not after CreateAssetMenu's fileName. Scan by type so the file can be called
    // anything.
    static AdConfig LoadConfigFromResources()
    {
        AdConfig named = Resources.Load<AdConfig>("AdConfig");
        if (named != null)
        {
            return named;
        }

        AdConfig[] found = Resources.LoadAll<AdConfig>(string.Empty);
        if (found.Length > 1)
        {
            Debug.LogWarning("[Ads] " + found.Length + " AdConfig assets in Resources — using " + found[0].name);
        }

        return found.Length > 0 ? found[0] : null;
    }

    // UMP raises its callbacks on a background JNI thread, and MobileAds.Initialize creates Unity
    // objects, which only the main thread may do. Calling it straight from a consent callback dies
    // inside the plugin's static setup as a TypeInitializationException — with no usable stack,
    // because the failure is in a static constructor. So callbacks only raise a flag; Update acts
    // on it. MobileAdsEventExecutor cannot do this job here: it is itself part of the SDK that has
    // not been initialised yet.
    volatile bool consentResolved;
    volatile bool initializationReported;
    volatile bool initializationSucceeded;
    bool initializeRequested;

    void RequestConsent()
    {
        ConsentInformation.Update(new ConsentRequestParameters(), consentError =>
        {
            if (consentError != null)
            {
                Debug.LogWarning("[Ads] Consent update failed: " + consentError.Message);
                consentResolved = true;
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null)
                {
                    Debug.LogWarning("[Ads] Consent form failed: " + formError.Message);
                }

                consentResolved = true;
            });
        });
    }

    void Update()
    {
        if (consentResolved && !initializeRequested)
        {
            consentResolved = false;
            initializeRequested = true;
            InitializeIfAllowed();
        }

        if (initializationReported)
        {
            initializationReported = false;
            FinishInitialization();
        }
    }

    // Reached even when consent errored: a player outside the EEA needs no form, and a returning
    // player already has a stored answer, so CanRequestAds can still be true.
    void InitializeIfAllowed()
    {
        if (!ConsentInformation.CanRequestAds())
        {
            Debug.Log("[Ads] Consent withheld — ads stay off this session.");
            return;
        }

        Debug.Log("[Ads] Consent resolved. Initialising SDK on the main thread.");

        MobileAds.Initialize(status =>
        {
            // Still a background thread, so record the outcome and let Update report it.
            initializationSucceeded = status != null;
            initializationReported = true;
        });
    }

    void FinishInitialization()
    {
        if (!initializationSucceeded)
        {
            Debug.LogError("[Ads] SDK initialisation failed.");
            return;
        }

        Ready = true;
        Debug.Log("[Ads] SDK ready. remote=" + Settings.HasRemote
                  + " test=" + Settings.UsingTestIds
                  + " interstitial=" + Settings.InterstitialId
                  + " levelsPerAd=" + Settings.LevelsPerInterstitial);
        OnReady?.Invoke();
    }

    // Null-safe one-liner for call sites. False means nothing was shown and the caller should
    // carry on immediately.
    public bool ShowInterstitial()
    {
        return Interstitial != null && Interstitial.Show();
    }

    public void ShowBanner()
    {
        if (Banner != null)
        {
            Banner.Show();
        }
    }

    public void HideBanner()
    {
        if (Banner != null)
        {
            Banner.Hide();
        }
    }

    // Home and gameplay put their reserved box at the very bottom of the screen, so the anchored
    // placement lines up with no coordinate conversion needed.
    public void ShowHomeNative()
    {
        if (Native != null && Settings != null)
        {
            Native.Show(Settings.NativeId, Settings.MainScreenStyle, NativeAdPlacement.Bottom());
        }
    }

    public void ShowGameplayNative()
    {
        if (Native != null && Settings != null)
        {
            Native.Show(Settings.NativeId, Settings.PlayScreenStyle, NativeAdPlacement.Bottom());
        }
    }

    public void PreloadLevelEndNative()
    {
        if (Native != null && Settings != null)
        {
            Native.Preload(Settings.PreloadNativeId);
        }
    }

    // The win and lose cards are centred, so their ad is placed over the card's own rect.
    // ResultScreenController owns that rect and calls this once its UI has laid out.
    public void ShowLevelEndNative(NativeAdPlacement placement)
    {
        if (Native != null && Settings != null)
        {
            Native.ShowPreloaded(Settings.PreloadNativeId, Settings.SuccessScreenStyle, placement);
        }
    }

    // What the level-end ad will measure, so a screen can open its reserved box at the right height
    // first and lay out only once.
    public int ExpectedLevelEndNativeHeightPx()
    {
        return Settings != null ? NativeAdUnit.ExpectedHeightPx(Settings.SuccessScreenStyle) : 0;
    }

    // Called again after the ad's height has grown the card and shifted the reserved box.
    public void MoveNativeAd(int x, int y)
    {
        if (Native != null)
        {
            Native.MoveTo(x, y);
        }
    }

    public void HideNativeAd()
    {
        if (Native != null)
        {
            Native.Hide();
        }
    }

    // Screens subscribe to size their reserved space from the ad's measured height instead of a
    // hardcoded guess.
    public void AddNativeHeightListener(System.Action<float> listener)
    {
        if (Native != null)
        {
            Native.OnHeightChanged += listener;
            listener(Native.HeightPixels);
        }
    }

    public void RemoveNativeHeightListener(System.Action<float> listener)
    {
        if (Native != null)
        {
            Native.OnHeightChanged -= listener;
        }
    }

    public float NativeHeightPixels => Native != null ? Native.HeightPixels : 0f;

    // Wire to a Settings-screen button. Only meaningful where a privacy entry point is required.
    public void ShowPrivacyOptions()
    {
        ConsentForm.ShowPrivacyOptionsForm(showError =>
        {
            if (showError != null)
            {
                Debug.LogWarning("[Ads] Privacy form failed: " + showError.Message);
            }
        });
    }

    public bool PrivacyOptionsRequired =>
        ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
}
