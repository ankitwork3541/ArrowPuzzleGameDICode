using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

// Owns the ad stack: initialises the SDK and keeps AdSettings fed from the backend. Nothing
// else in the game touches the Google Mobile Ads API directly.
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

        if (Interstitial != null)
        {
            Interstitial.OnOpened += PauseAudioForAd;
            Interstitial.OnClosed += ResumeAudioAfterAd;
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

        if (GetComponent<InterstitialManager>() == null)
        {
            gameObject.AddComponent<InterstitialManager>();
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

                // The screen the player is already on asked for its ad using whatever settings were
                // in hand at the time — last launch's cached payload, or the baked fallbacks on a
                // first ever launch. Home opens in well under a second and the fetch has a five
                // second timeout, so that is the normal case, not an edge one: without this the
                // screen keeps a slot the backend has since changed for the rest of the session.
                ReapplyScreenAd();
            }
        }));

        Initialize();
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

    // MobileAds.Initialize creates Unity objects, so its callback (which fires on a background
    // JNI thread) can only raise a flag here; Update acts on it on the main thread.
    volatile bool initializationReported;
    volatile bool initializationSucceeded;

    void Update()
    {
        if (initializationReported)
        {
            initializationReported = false;
            FinishInitialization();
        }
    }

    void Initialize()
    {
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
                  + " levelsPerAd=" + Settings.AdLevelCount
                  + " clickCount=" + Settings.AdClickCount
                  + " clickTime=" + Settings.AdClickTimeSec + "s"
                  + " backAd=" + Settings.BackAdEnabled
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

    // A full-screen ad takes over the screen but not Unity's audio, so the win jingle and gameplay
    // SFX would otherwise keep playing underneath it. Google guarantees one of Closed / Failed for
    // every presentation, and both land in InterstitialAdUnit.Finish(), so this always pairs up.
    static void PauseAudioForAd()
    {
        AudioListener.pause = true;
    }

    static void ResumeAudioAfterAd()
    {
        AudioListener.pause = false;
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
        activeScreenAd = ScreenAd.Home;
        ShowScreenAd(Settings != null ? Settings.MainScreenSlot : AdSlotKind.None,
                     Settings != null ? Settings.MainScreenStyle : 0, "home");
    }

    public void ShowGameplayAd()
    {
        activeScreenAd = ScreenAd.Gameplay;
        ShowScreenAd(Settings != null ? Settings.PlayScreenSlot : AdSlotKind.None,
                     Settings != null ? Settings.PlayScreenStyle : 0, "gameplay");
    }

    // Which screen's ad is currently up, so a settings change can re-ask for it. The screen itself
    // cannot do this: it has no idea its slot digit just changed underneath it.
    enum ScreenAd
    {
        None,
        Home,
        Gameplay
    }

    ScreenAd activeScreenAd;

    void ReapplyScreenAd()
    {
        switch (activeScreenAd)
        {
            case ScreenAd.Home:
                ShowHomeAd();
                break;
            case ScreenAd.Gameplay:
                ShowGameplayAd();
                break;
        }
    }

    // digit is carried purely for the log: a slot of None prints as "None" and would not say which
    // number the backend actually sent, which is the one thing worth knowing when a screen is bare.
    void ShowScreenAd(AdSlotKind slot, int digit, string screen)
    {
        if (Settings == null)
        {
            return;
        }

        // The screen we are replacing may have queued a teardown on its way out. It did not know
        // this screen wants the same ad; now that we do, call it off.
        CancelPendingHide();

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
            Banner.Show(slot == AdSlotKind.BannerLarge, screen);
            return;
        }

        if (AdSettings.IsNative(slot) && Native != null)
        {
            Native.Show(Settings.NativeId, slot, NativeAdPlacement.Bottom(), screen);
        }
    }

    // Both formats, for a screen on its way out.
    public void HideScreenAds()
    {
        CancelPendingHide();
        activeScreenAd = ScreenAd.None;
        HideBanner();
        HideNativeAd();
    }

    // Screens hand over inside a single frame: the outgoing screen's OnDisable runs before the
    // incoming screen's OnEnable. Hiding right there tears down a banner the next screen is about
    // to ask for again — and Banner.Hide() DESTROYS the view, so that costs a whole fresh ad
    // request and a visible gap. Waiting until the end of the frame lets a Show that arrived in the
    // meantime cancel the teardown, which is what keeps the ad steady across Home <-> Settings.
    public void HideScreenAdsDeferred()
    {
        if (pendingHide != null)
        {
            return;
        }

        pendingHide = StartCoroutine(HideAtEndOfFrame());
    }

    System.Collections.IEnumerator HideAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        pendingHide = null;

        // Cleared here rather than when the teardown was queued: a Show that arrived in the meantime
        // cancels this coroutine, and it has already recorded which screen now owns the ad.
        activeScreenAd = ScreenAd.None;
        HideBanner();
        HideNativeAd();
    }

    void CancelPendingHide()
    {
        if (pendingHide == null)
        {
            return;
        }

        StopCoroutine(pendingHide);
        pendingHide = null;
    }

    Coroutine pendingHide;

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

    // Room is reserved but no ad has painted into it yet, so the screen should fill it with a
    // skeleton. Both units reserve their box the moment the request goes out and only flip ShowingAd
    // once the ad is on screen, which is what makes this a single answer for either format.
    public bool AdPlaceholderVisible =>
        AdHeightPixels > 0f
        && !(Banner != null && Banner.ShowingAd)
        && !(Native != null && Native.ShowingAd);

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
}
