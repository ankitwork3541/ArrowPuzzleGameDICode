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
public class PromoBannerUnit : MonoBehaviour
{
    const float MeasureTimeoutSeconds = 2f;

    // Backstop for the reserved room. Google's load callbacks always fire, but a skeleton left up
    // forever because one of them did not is a visible bug, not a missing ad.
    const float ReserveTimeoutSeconds = 12f;

    // The two fixed sizes, in dp, so the room can be opened before any ad exists.
    const int StandardHeightDp = 50;
    const int LargeHeightDp = 100;

    BannerView banner;
    bool wantVisible;
    bool wantLarge;

    // Which screen the ad on screen belongs to. Part of the reuse test, not just the size: the
    // backend gives main_screen and play_screen their own digits, so they are separate placements
    // even when both digits happen to name the same size. Home and Settings share one owner because
    // they share one digit.
    string wantOwner;

    // A failed BannerView is kept rather than nulled, so this is what tells "still loading" apart
    // from "gave up" when deciding whether an existing banner can be reused.
    bool loadFailed;

    Coroutine presentRoutine;
    Coroutine reserveRoutine;

    public bool IsLoaded { get; private set; }

    // True only once the ad is actually painted. Until then the reserved room is holding a skeleton,
    // which is what the UI needs to know to show one.
    public bool ShowingAd { get; private set; }

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
        PromoManager manager = PromoManager.Instance;
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
        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.OnReady -= OnAdsReady;
        }

        StopPresent();
        StopReserveTimeout();
        DestroyBanner();
    }

    // Show() may well have been called before the SDK finished initialising, so honour the request
    // that was parked rather than dropping it.
    void OnAdsReady()
    {
        if (wantVisible)
        {
            Show(wantLarge, wantOwner);
        }
    }

    public void Show(bool large, string owner)
    {
        // Already carrying exactly this screen's banner — leave it alone. Create() destroys before
        // it builds, so falling through would tear down a good ad and pay for a fresh request every
        // time the player crossed between two screens sharing one digit, which is Home and Settings.
        // A failed load is the one case worth rebuilding for, and loadFailed is what lets that
        // through.
        if (wantVisible && wantLarge == large && wantOwner == owner
            && banner != null && !loadFailed)
        {
            Debug.Log("[Banner] Reusing the " + owner + " banner already on screen ("
                      + (large ? "LARGE_BANNER 320x100" : "BANNER 320x50") + ") — no new request.");
            return;
        }

        wantVisible = true;
        wantLarge = large;
        wantOwner = owner;

        if (!IsAllowed())
        {
            Debug.Log("[Banner] Show refused — ads disabled or no banner unit ID.");
            Hide();
            return;
        }

        PromoManager manager = PromoManager.Instance;
        if (manager == null || !manager.Ready)
        {
            Debug.Log("[Banner] Show parked until the SDK is ready.");
            return;
        }

        Create(large);
    }

    public void Hide()
    {
        if (wantVisible || banner != null)
        {
            Debug.Log("[Banner] Hidden and destroyed.");
        }

        wantVisible = false;
        wantOwner = null;
        StopPresent();
        StopReserveTimeout();
        DestroyBanner();
        SetShowingAd(false);
        SetHeight(0f);
    }

    void Create(bool large)
    {
        DestroyBanner();
        loadFailed = false;

        AdSize size = large ? LargeSize() : StandardSize();
        string unitId = PromoManager.Instance.Settings.BannerIdFor(large);

        // Room first, ad second. The size is fixed, so the final height is known before the request
        // is even sent — the UI can open the box to exactly that and fill it with a skeleton, and
        // nothing has to move again when the ad paints. Opening it afterwards is what shoved the
        // bottom nav upwards in one frame.
        SetShowingAd(false);
        SetHeight(ExpectedHeightPx(large));
        StartReserveTimeout();

        banner = new BannerView(unitId, size, AdPosition.Bottom);
        BannerView created = banner;
        RequestCount++;

        Debug.Log("[Banner] Requesting " + (large ? "LARGE_BANNER 320x100" : "BANNER 320x50")
                  + " for " + wantOwner + " — unit " + unitId + ", "
                  + ExpectedHeightPx(large) + "px reserved.");

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
            Debug.Log("[Banner] Loaded.");

            if (!wantVisible)
            {
                Debug.Log("[Banner] Loaded after the screen was left — held back, not shown.");
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
            StopReserveTimeout();
            SetShowingAd(false);
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

        // created.OnAdPaid += value => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        // {
        //     if (value != null)
        //     {
        //         RevenueMicros += value.Value;
        //     }
        // });

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
    // the banner on screen. The height is read from the loaded banner rather than converted from dp
    // here, so the reserved room always matches what the SDK actually placed.
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
            StopReserveTimeout();
            SetShowingAd(false);
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
        StopReserveTimeout();
        SetShowingAd(true);

        Debug.Log("[Banner] Showing " + (wantLarge ? "LARGE_BANNER 320x100" : "BANNER 320x50") + ", "
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

    // Raises OnHeightChanged too, even though no height moved. A fixed-size banner loads at exactly
    // the height that was reserved for it, so without this the one event that matters — "the
    // skeleton can come down now" — would never reach the UI.
    void SetShowingAd(bool value)
    {
        if (ShowingAd == value)
        {
            return;
        }

        ShowingAd = value;
        OnHeightChanged?.Invoke(HeightPixels);
    }

    void StartReserveTimeout()
    {
        StopReserveTimeout();
        reserveRoutine = StartCoroutine(ReserveTimeout());
    }

    void StopReserveTimeout()
    {
        if (reserveRoutine != null)
        {
            StopCoroutine(reserveRoutine);
            reserveRoutine = null;
        }
    }

    IEnumerator ReserveTimeout()
    {
        yield return new WaitForSecondsRealtime(ReserveTimeoutSeconds);
        reserveRoutine = null;

        if (ShowingAd)
        {
            yield break;
        }

        Debug.LogWarning("[Banner] No answer in " + ReserveTimeoutSeconds
                         + "s — giving the reserved room back.");
        SetHeight(0f);
    }

    // What the banner will measure, in screen pixels, before one exists. Both sizes are fixed, so
    // this is exact rather than an estimate.
    public static int ExpectedHeightPx(bool large)
    {
        int dp = large ? LargeHeightDp : StandardHeightDp;
        return Mathf.RoundToInt(dp * PromoSpace.DisplayDensity);
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
        PromoManager manager = PromoManager.Instance;

        return manager != null
               && manager.Settings != null
               && manager.Settings.AdsEnabled
               && manager.Settings.BannerFormatEnabled
               && !string.IsNullOrEmpty(manager.Settings.BannerIdFor(true));
    }

    // The two fixed IAB sizes from Google's standard banner table: BANNER (320x50 dp) for style 1
    // and LARGE_BANNER (320x100 dp) for style 2. Fixed, not adaptive — the creative is always
    // exactly this size on every device, so the two styles stay visually distinct.
    public static AdSize StandardSize()
    {
        return AdSize.Banner;
    }

    public static AdSize LargeSize()
    {
        return AdSize.LargeBanner;
    }

    // Same density source the native ad's placement maths uses, so dp means the same thing on both
    // sides of the plugin boundary.
    public static int ScreenWidthDp =>
        Mathf.Max(1, Mathf.RoundToInt(Screen.width / PromoSpace.DisplayDensity));

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
