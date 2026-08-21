using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

// Owns the anchored banner. Deliberately says nothing about which screens should carry one —
// callers decide by calling Show() and Hide().
public class BannerAdUnit : MonoBehaviour
{
    [Tooltip("Screen edge the banner sticks to.")]
    public AdPosition position = AdPosition.Bottom;

    BannerView banner;
    bool wantVisible;

    public bool IsLoaded { get; private set; }

    void Start()
    {
        AdsManager manager = AdsManager.Instance;
        if (manager == null)
        {
            return;
        }

        if (manager.Ready)
        {
            OnAdsReady();
        }
        else
        {
            manager.OnReady += OnAdsReady;
        }
    }

    void OnDestroy()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.OnReady -= OnAdsReady;
        }

        DestroyBanner();
    }

    // Show() may well have been called before the SDK finished initialising, so honour the request
    // that was parked rather than dropping it.
    void OnAdsReady()
    {
        if (wantVisible)
        {
            Create();
        }
    }

    public void Show()
    {
        wantVisible = true;

        if (banner == null)
        {
            Create();
            return;
        }

        banner.Show();
    }

    public void Hide()
    {
        wantVisible = false;

        if (banner != null)
        {
            banner.Hide();
        }
    }

    void Create()
    {
        AdsManager manager = AdsManager.Instance;
        if (manager == null || !manager.Ready)
        {
            return;
        }

        if (!manager.Settings.AdsEnabled)
        {
            Debug.Log("[Ads] Banner skipped — ads disabled by config.");
            return;
        }

        string unitId = manager.Settings.BannerId;
        if (string.IsNullOrEmpty(unitId))
        {
            Debug.LogWarning("[Ads] No banner unit available — nothing to create.");
            return;
        }

        DestroyBanner();

        banner = new BannerView(unitId, AnchoredAdaptiveSize(), position);

        banner.OnBannerAdLoaded += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            IsLoaded = true;

            // A BannerView puts itself on screen the moment it loads. Without this, a player who
            // navigated away mid-load ends up with a banner sitting over the wrong screen.
            if (!wantVisible)
            {
                banner.Hide();
            }

            Debug.Log("[Ads] Banner loaded.");
        });

        banner.OnBannerAdLoadFailed += error => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            IsLoaded = false;
            Debug.LogWarning("[Ads] Banner load failed: " + error.GetMessage());
        });

        banner.LoadAd(new AdRequest());
    }

    // Anchored adaptive rather than the fixed 320x50: it takes the device's real width, so wide
    // phones do not get a small banner floating in a bar of background.
    //
    // The width is the screen's real dp width, NOT AdSize.FullWidth. FullWidth is -1, and the
    // next-generation Ads SDK this plugin now depends on (ads-mobile-sdk) rejects it outright —
    // "java.lang.IllegalArgumentException: Invalid width for AdSize -1" the moment the BannerView is
    // constructed. Google's own Unity sample still shows FullWidth; it no longer works here.
    public static AdSize AnchoredAdaptiveSize()
    {
        return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(ScreenWidthDp);
    }

    // Same density source the native ad's placement maths uses, so dp means the same thing on both
    // sides of the plugin boundary.
    public static int ScreenWidthDp =>
        Mathf.Max(1, Mathf.RoundToInt(Screen.width / NativeAdSpace.DisplayDensity));

    void DestroyBanner()
    {
        if (banner == null)
        {
            return;
        }

        banner.Destroy();
        banner = null;
        IsLoaded = false;
    }
}
