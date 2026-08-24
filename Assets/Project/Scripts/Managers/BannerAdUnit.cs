using System;
using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

// Owns the anchored banner. Says nothing about which screens should carry one — callers decide by
// calling Show() and Hide().
//
// Deliberately never refreshed from code: a banner ad unit carries its own auto-refresh setting in
// the AdMob dashboard, and driving a second refresh from here would mean two schedules fighting over
// the same unit. The native ad is the opposite case — it has no dashboard refresh, so
// refresh_native_sec drives it.
public class BannerAdUnit : MonoBehaviour
{
    const float MeasureTimeoutSeconds = 2f;

    BannerView banner;
    bool wantVisible;
    bool wantLarge;

    // A failed BannerView is kept rather than nulled, so this is what tells "still loading" apart
    // from "gave up" when deciding whether an existing banner can be reused.
    bool loadFailed;

    Coroutine presentRoutine;

    public bool IsLoaded { get; private set; }

    // Screen pixels, published for the UI to reserve matching room. Zero means nothing is on screen,
    // which is the signal to collapse that room completely rather than leave a hole.
    public float HeightPixels { get; private set; }
    public event Action<float> OnHeightChanged;

    // Requests versus impressions is the ratio that predicts an ad serving limit.
    public int RequestCount { get; private set; }
    public int FillCount { get; private set; }
    public int ImpressionCount { get; private set; }
    public int ClickCount { get; private set; }
    public double RevenueMicros { get; private set; }

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

        StopPresent();
        DestroyBanner();
    }

    // Show() may well have been called before the SDK finished initialising, so honour the request
    // that was parked rather than dropping it.
    void OnAdsReady()
    {
        if (wantVisible)
        {
            Show(wantLarge);
        }
    }

    public void Show(bool large)
    {
        // Already carrying exactly this banner — leave it alone. Create() destroys before it
        // builds, so falling through would tear down a good ad and pay for a fresh request every
        // time the player crossed between two screens that both want the same banner. A failed
        // load is the one case worth rebuilding for, and loadFailed is what lets that through.
        if (wantVisible && wantLarge == large && banner != null && !loadFailed)
        {
            return;
        }

        wantVisible = true;
        wantLarge = large;

        if (!IsAllowed())
        {
            Debug.Log("[Banner] Show refused — ads disabled or no banner unit ID.");
            Hide();
            return;
        }

        AdsManager manager = AdsManager.Instance;
        if (manager == null || !manager.Ready)
        {
            Debug.Log("[Banner] Show parked until the SDK is ready.");
            return;
        }

        Create(large);
    }

    public void Hide()
    {
        wantVisible = false;
        StopPresent();
        DestroyBanner();
        SetHeight(0f);
    }

    void Create(bool large)
    {
        DestroyBanner();
        loadFailed = false;

        AdSize size = large ? LargeAnchoredAdaptiveSize() : AnchoredAdaptiveSize();
        string unitId = AdsManager.Instance.Settings.BannerId;

        banner = new BannerView(unitId, size, AdPosition.Bottom);
        BannerView created = banner;
        RequestCount++;

        // Hidden from the moment it exists. A BannerView puts itself on screen as soon as it loads,
        // and a banner that appears before the game's layout has opened room for it lands on top of
        // whatever is at the bottom of the screen — on the gameplay screen, the Hint and Restart
        // buttons. A tap meant for a button then registers on the ad, and an accidental click is
        // invalid traffic. PresentRoutine shows it only once the room exists.
        created.Hide();

        created.OnBannerAdLoaded += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (banner != created)
            {
                return;
            }

            IsLoaded = true;
            FillCount++;

            if (!wantVisible)
            {
                created.Hide();
                return;
            }

            StartPresent(created);
        });

        created.OnBannerAdLoadFailed += error => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (banner != created)
            {
                return;
            }

            IsLoaded = false;
            loadFailed = true;
            SetHeight(0f);

            // No retry from here. The ad unit's own auto-refresh owns that; without it enabled in
            // the AdMob dashboard this screen simply carries no banner for this visit, which is the
            // agreed behaviour.
            Debug.LogWarning("[Banner] Load failed: "
                             + (error != null ? error.GetMessage() : "unknown error"));
            LogCounters("load failed");
        });

        created.OnAdImpressionRecorded += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            ImpressionCount++;
            LogCounters("impression");
        });

        created.OnAdClicked += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            ClickCount++;
            LogCounters("click");
        });

        created.OnAdPaid += value => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (value != null)
            {
                RevenueMicros += value.Value;
            }
        });

        created.LoadAd(new AdRequest());
    }

    void StartPresent(BannerView target)
    {
        StopPresent();
        presentRoutine = StartCoroutine(PresentRoutine(target));
    }

    void StopPresent()
    {
        if (presentRoutine != null)
        {
            StopCoroutine(presentRoutine);
            presentRoutine = null;
        }
    }

    // Publishes the banner's real height, waits for the UI to open that much room, and only then puts
    // the banner on screen. The height is read from the loaded banner rather than guessed: an
    // adaptive banner's height is chosen by Google per device, so there is no formula to predict it.
    IEnumerator PresentRoutine(BannerView target)
    {
        float heightPx = 0f;
        float deadline = Time.realtimeSinceStartup + MeasureTimeoutSeconds;

        while (Time.realtimeSinceStartup < deadline)
        {
            if (banner != target || !wantVisible)
            {
                presentRoutine = null;
                yield break;
            }

            heightPx = target.GetHeightInPixels();
            if (heightPx > 0f)
            {
                break;
            }

            yield return null;
        }

        if (heightPx <= 0f)
        {
            // Never shown rather than shown over the UI: without a height there is no way to know
            // how much room to open, and an unplaced banner is exactly the accidental-click case.
            Debug.LogWarning("[Banner] Never reported a height; not shown.");
            presentRoutine = null;
            SetHeight(0f);
            yield break;
        }

        SetHeight(heightPx);

        // Two frames: one for the reserved box's own height, one for the layout that follows it.
        yield return null;
        yield return null;

        if (banner != target || !wantVisible)
        {
            presentRoutine = null;
            yield break;
        }

        target.Show();
        presentRoutine = null;

        Debug.Log("[Banner] Showing " + (wantLarge ? "large" : "standard") + " adaptive, "
                  + target.GetWidthInPixels() + " x " + heightPx + " px, room reserved first.");
        LogCounters("shown");
    }

    void SetHeight(float pixels)
    {
        if (Mathf.Approximately(HeightPixels, pixels))
        {
            return;
        }

        HeightPixels = pixels;
        OnHeightChanged?.Invoke(pixels);
    }

    void LogCounters(string what)
    {
        float matchRate = RequestCount > 0 ? 100f * FillCount / RequestCount : 0f;

        Debug.Log("[Banner] " + what + " — requests " + RequestCount
                  + ", filled " + FillCount + " (" + matchRate.ToString("F0") + "% match)"
                  + ", impressions " + ImpressionCount
                  + ", clicks " + ClickCount
                  + ", est. revenue " + (RevenueMicros / 1000000d).ToString("F4"));
    }

    static bool IsAllowed()
    {
        AdsManager manager = AdsManager.Instance;

        return manager != null
               && manager.Settings != null
               && manager.Settings.AdsEnabled
               && !string.IsNullOrEmpty(manager.Settings.BannerId);
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

    // The taller variant: up to 20% of screen height, between 50 and 150dp. Google intends it for
    // non-scrolling layouts, which is what every screen in this game is.
    public static AdSize LargeAnchoredAdaptiveSize()
    {
        return AdSize.GetCurrentOrientationLargeAnchoredAdaptiveBannerAdSizeWithWidth(ScreenWidthDp);
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
