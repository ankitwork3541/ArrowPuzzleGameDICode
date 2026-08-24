using System.Collections.Generic;
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
    public RewardedAdUnit Rewarded { get; private set; }

    // Ad formats wait on this instead of polling Ready.
    public event System.Action OnReady;

    // Fired after a cached or fresh backend payload is applied. Hint count follows this.
    public event System.Action OnSettingsChanged;

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
        Rewarded = GetComponent<RewardedAdUnit>() ?? gameObject.AddComponent<RewardedAdUnit>();

        if (Native != null)
        {
            Native.OnHeightChanged += OnUnitHeightChanged;
        }

        if (Banner != null)
        {
            Banner.OnHeightChanged += OnUnitHeightChanged;
        }

        // Last launch's payload, so an offline session still runs on real settings rather than
        // whatever shipped in the build months ago. Applied before HintManager so the first
        // hint sync already sees free_hint_count.
        if (Settings.ApplyRemote(fetcher.LoadCached()))
        {
            Debug.Log("[Ads] Cached config applied.");
        }

        if (GetComponent<HintManager>() == null)
        {
            gameObject.AddComponent<HintManager>();
        }
    }

    void Start()
    {
        StartCoroutine(fetcher.Fetch(config =>
        {
            if (Settings.ApplyRemote(config))
            {
                Debug.Log("[Ads] Fresh config applied.");
                OnSettingsChanged?.Invoke();
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

        RegisterTestDevices();

        MobileAds.Initialize(status =>
        {
            // Still a background thread, so record the outcome and let Update report it.
            initializationSucceeded = status != null;
            initializationReported = true;
        });
    }

    // Registered before Initialize, so even the first request is a test request. A device that is
    // not on this list gets live ads: requesting them, and above all clicking them, is invalid
    // activity by AdMob's policy and is attributed to this account.
    void RegisterTestDevices()
    {
        List<string> ids = Settings.TestDeviceIds;
        if (ids == null || ids.Count == 0)
        {
            if (!Settings.UsingTestIds)
            {
                Debug.LogWarning("[Ads] Live ad unit IDs with no test devices registered. Add this "
                                 + "device's ID to AdConfig.testDeviceIds before testing on it.");
            }

            return;
        }

        MobileAds.SetRequestConfiguration(new RequestConfiguration { TestDeviceIds = ids });
        Debug.Log("[Ads] Registered " + ids.Count + " test device(s).");
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
                  + " levelsPerAd=" + Settings.LevelsPerInterstitial
                  + " native=" + Settings.NativeId
                  + " home=" + Settings.MainScreenStyle + "(" + Settings.MainScreenSlot + ")"
                  + " play=" + Settings.PlayScreenStyle + "(" + Settings.PlayScreenSlot + ")"
                  + " result=" + Settings.SuccessScreenStyle + "(" + Settings.SuccessScreenSlot + ")"
                  + " refresh=" + Settings.NativeRefreshSeconds + "s");

        // LogError, not LogWarning: shipping with this on serves Google's test inventory to real
        // players and earns nothing, and it also bypasses the "no backend config, no ads" rule.
        if (Settings.UsingTestIds)
        {
            Debug.LogError("[Ads] TEST AD UNITS ARE ON. Turn off AdConfig.useTestIds before release "
                           + "— the backend's ad unit IDs and its ad_enable flag are both ignored "
                           + "while this is set.");
        }

        OnReady?.Invoke();
    }

    // Null-safe one-liner for call sites. False means nothing was shown and the caller should
    // carry on immediately.
    public bool ShowInterstitial()
    {
        return Interstitial != null && Interstitial.Show();
    }

    public void HideBanner()
    {
        if (Banner != null)
        {
            Banner.Hide();
        }
    }

    // Home and gameplay reserve their box at the very bottom of the screen, so both an anchored
    // banner and an anchored native line up with no coordinate conversion needed. The backend's digit
    // decides which of the two the screen gets; the position is the screen's, not the format's.
    public void ShowHomeAd()
    {
        ShowScreenAd(Settings != null ? Settings.MainScreenSlot : AdSlotKind.None,
                     Settings != null ? Settings.MainScreenStyle : 0, "home");
    }

    public void ShowGameplayAd()
    {
        ShowScreenAd(Settings != null ? Settings.PlayScreenSlot : AdSlotKind.None,
                     Settings != null ? Settings.PlayScreenStyle : 0, "gameplay");
    }

    // digit is carried purely for the log: a slot of None prints as "None" and would not say which
    // number the backend actually sent, which is the one thing worth knowing when a screen is bare.
    void ShowScreenAd(AdSlotKind slot, int digit, string screen)
    {
        if (Settings == null)
        {
            return;
        }

        // Whichever format is not wanted has to be torn down, or the ad from the previous screen
        // keeps drawing over this one — an overlay follows the player until something destroys it.
        if (!AdSettings.IsBanner(slot))
        {
            HideBanner();
        }

        if (!AdSettings.IsNative(slot))
        {
            HideNativeAd();
        }

        if (!Settings.CanShow(slot))
        {
            Debug.Log("[Ads] " + screen + " slot " + digit + " (" + slot + ") shows nothing — "
                      + "ads off, unmapped digit, or no unit ID.");
            return;
        }

        if (AdSettings.IsBanner(slot) && Banner != null)
        {
            Banner.Show(slot == AdSlotKind.BannerLarge);
            return;
        }

        if (AdSettings.IsNative(slot) && Native != null)
        {
            Native.Show(Settings.NativeId, slot, NativeAdPlacement.Bottom());
        }
    }

    // Both formats, for a screen on its way out.
    public void HideScreenAds()
    {
        HideBanner();
        HideNativeAd();
    }

    public void PreloadLevelEndNative()
    {
        if (Native != null && Settings != null)
        {
            Native.Preload(Settings.PreloadNativeId, Settings.SuccessScreenSlot);
        }
    }

    // The win and lose cards are centred, so their ad is placed over the card's own rect.
    // ResultScreenController owns that rect and calls this once its UI has laid out.
    public void ShowLevelEndNative(NativeAdPlacement placement)
    {
        if (Native != null && Settings != null)
        {
            Native.ShowPreloaded(Settings.PreloadNativeId, Settings.SuccessScreenSlot, placement);
        }
    }

    // What the level-end ad will measure, so a screen can open its reserved box at the right height
    // first and lay out only once.
    public int ExpectedLevelEndNativeHeightPx()
    {
        return Settings != null ? NativeAdUnit.ExpectedHeightPx(Settings.SuccessScreenSlot) : 0;
    }

    // True only when an ad preloaded at level start is actually in hand. The result cards never load
    // one live — nobody waits on a win screen — so this is what decides whether the card reserves
    // any room at all.
    public bool LevelEndNativeReady =>
        Native != null
        && Settings != null
        && Settings.CanShowPreloadNative
        && Native.HasPreloadedAd;

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
    // hardcoded guess. Deliberately format-agnostic: a screen reserves room for "the ad", and which
    // format the backend picked is not its business.
    public void AddAdHeightListener(System.Action<float> listener)
    {
        adHeightListeners += listener;
        listener(AdHeightPixels);
    }

    public void RemoveAdHeightListener(System.Action<float> listener)
    {
        adHeightListeners -= listener;
    }

    // Only one format is ever on screen at a time, so the larger of the two is the live one.
    public float AdHeightPixels =>
        Mathf.Max(Native != null ? Native.HeightPixels : 0f,
                  Banner != null ? Banner.HeightPixels : 0f);

    System.Action<float> adHeightListeners;

    void OnUnitHeightChanged(float ignored)
    {
        adHeightListeners?.Invoke(AdHeightPixels);
    }

    // Kept for the result cards, which are native-only.
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
