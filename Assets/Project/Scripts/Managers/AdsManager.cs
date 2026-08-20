using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
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

    // Runs every launch: consent can be withdrawn or expire, and the answer decides whether the
    // SDK may be initialised at all.
    void RequestConsent()
    {
        ConsentInformation.Update(new ConsentRequestParameters(), consentError =>
        {
            if (consentError != null)
            {
                Debug.LogWarning("[Ads] Consent update failed: " + consentError.Message);
                InitializeIfAllowed();
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null)
                {
                    Debug.LogWarning("[Ads] Consent form failed: " + formError.Message);
                }

                InitializeIfAllowed();
            });
        });
    }

    // Called even when consent errored: a player outside the EEA needs no form, and a returning
    // player already has a stored answer, so CanRequestAds can still be true.
    void InitializeIfAllowed()
    {
        if (!ConsentInformation.CanRequestAds())
        {
            Debug.Log("[Ads] Consent withheld — ads stay off this session.");
            return;
        }

        MobileAds.Initialize(status =>
        {
            // Plugin callbacks arrive on a background thread; everything below touches Unity state.
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (status == null)
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
            });
        });
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
