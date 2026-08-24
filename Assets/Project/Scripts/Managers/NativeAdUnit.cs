using System;
using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

// Where Android should draw the overlay: anchored to a screen edge, or at an explicit offset so it
// can be lined up with a box the game's own UI reserved for it.
//
// x and y are DP, not pixels. The plugin runs convertDpToPixel over the offsets it is handed, so
// passing pixels multiplies them by the display density and puts the ad off screen. Undocumented;
// established by measuring on device.
//
// There is deliberately no width here. Requesting a custom size through the AdSize overloads is a
// path Google has acknowledged as broken — a developer reporting
// RenderTemplate(style, new AdSize(600, 500), AdPosition.Bottom) rendering "with the bottom part cut
// off" was answered with "we are able to reproduce the issue", and the thread was never resolved.
// Measurements here agree: with no AdSize the template comes out cleanly at full width, and with one
// it comes out the requested pixel box and clips its own contents.
public struct NativeAdPlacement
{
    public bool anchored;
    public AdPosition anchor;
    public int x;
    public int y;

    public static NativeAdPlacement Bottom()
    {
        return new NativeAdPlacement { anchored = true, anchor = AdPosition.Bottom };
    }

    public static NativeAdPlacement At(int xDp, int yDp)
    {
        return new NativeAdPlacement { anchored = false, x = xDp, y = yDp };
    }

    // Compared field by field rather than with Equals(): this is a struct with no override, so the
    // default comparison boxes and reflects, which is not what a per-screen-change check wants.
    public static bool Same(NativeAdPlacement a, NativeAdPlacement b)
    {
        if (a.anchored != b.anchored) return false;
        if (a.anchored) return a.anchor == b.anchor;

        return a.x == b.x && a.y == b.y;
    }
}

// Owns the native overlay ad. Android draws it on top of the game, in screen pixels, while the
// game's own layout lives in panel units — so room has to be opened before the overlay appears and
// corrected from the template's real height once it exists. Nothing is reserved without an ad.
//
// Takes an AdSlotKind, never the backend's raw digit. Digits 1 and 2 ask for banners, which
// BannerAdUnit owns; passing one here used to be possible and came out as a runtime "unsupported
// style" log. Now it cannot be expressed.
public class NativeAdUnit : MonoBehaviour
{
    const float MeasureTimeoutSeconds = 2f;

    // How long to let a request settle before deciding whether it failed. Only used to pace the
    // retry loop; the load callback itself is not on a timer.
    const float LoadSettleTimeoutSeconds = 30f;

    // Read off the template layouts in googlemobileads-unity.aar, not chosen.
    const int MediumTemplateHeightDp = 350;
    const int SmallTemplateMarginDp = 10;

    // Retry floor and ceiling. The floor is deliberately independent of refresh_native_sec: a
    // backend asking for a 10s refresh must not turn a no-fill region into 360 failed requests an
    // hour, which is exactly the request-to-impression ratio that earns an ad serving limit.
    const int MinBackoffSeconds = 30;
    const int MaxBackoffSeconds = 300;

    NativeOverlayAd currentAd;
    NativeOverlayAd preloadedAd;
    bool isPreloading;

    // Realtime stamp, used for staleness. Google treats a cached ad as stale after roughly an hour.
    // Only the preloaded ad needs one: the visible ad is destroyed on every screen exit and replaced
    // on every refresh, so it never gets old enough to matter.
    float preloadedAt = -1f;

    bool wantVisible;
    string activeUnitId;
    AdSlotKind activeSlot;
    NativeAdPlacement activePlacement;
    bool activeAutoRefresh;

    int consecutiveFailures;
    bool loadInFlight;

    Coroutine loadRoutine;
    Coroutine measureRoutine;
    Coroutine presentRoutine;

    // Session counters. Requests versus impressions is the ratio that predicts a serving limit, and
    // there is no way to see it from inside the game without counting it here.
    public int RequestCount { get; private set; }
    public int FillCount { get; private set; }
    public int ImpressionCount { get; private set; }
    public int ClickCount { get; private set; }
    public double RevenueMicros { get; private set; }

    // Lets the result card decide whether to reserve room before it opens anything. Without this it
    // would open the box, discover on the next frame that no ad was preloaded, and collapse again —
    // a visible jump in the card's layout.
    public bool HasPreloadedAd => preloadedAd != null && !IsStale(preloadedAt);

    // Screen pixels. Zero means nothing is on screen, which is the signal for the UI to collapse
    // its reserved space completely rather than leave a hole.
    public float HeightPixels { get; private set; }
    public event Action<float> OnHeightChanged;

    static AdSettings Settings => AdsManager.Instance != null ? AdsManager.Instance.Settings : null;

    static float AdMaxAgeSeconds => Settings != null ? Settings.AdMaxAgeSeconds : 1800f;

    static int RefreshSeconds => Settings != null ? Settings.NativeRefreshSeconds : 0;

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

        StopLoadLoop();
        StopMeasure();
        StopPresent();
        DestroyActive();
        DestroyPreloaded();
    }

    // A request that arrived before the SDK finished initialising was parked, not dropped.
    void OnAdsReady()
    {
        if (wantVisible && !string.IsNullOrEmpty(activeUnitId))
        {
            Show(activeUnitId, activeSlot, activePlacement, activeAutoRefresh);
        }
    }

    public void Show(string adUnitId, AdSlotKind slot, NativeAdPlacement placement, bool autoRefresh = true)
    {
        // Nothing has changed and an ad is already on screen — reloading here would throw away a
        // live impression and start a new request every time the player crossed between two
        // screens that carry the same slot. Same reasoning as BannerAdUnit.Show().
        if (wantVisible && currentAd != null
            && activeUnitId == adUnitId && activeSlot == slot
            && NativeAdPlacement.Same(activePlacement, placement))
        {
            return;
        }

        wantVisible = true;
        activeUnitId = adUnitId;
        activeSlot = slot;
        activePlacement = placement;
        activeAutoRefresh = autoRefresh;

        if (!IsAllowed(adUnitId, slot))
        {
            Debug.Log("[NativeAd] Show refused for " + slot
                      + " — ads disabled, unit ID blank, or style unsupported.");
            Hide();
            return;
        }

        AdsManager manager = AdsManager.Instance;
        if (manager == null || !manager.Ready)
        {
            Debug.Log("[NativeAd] Show for " + slot + " parked until the SDK is ready.");
            return;
        }

        // A fresh, player-driven show starts the backoff ladder over. Only a failing retry loop is
        // meant to back off; opening a screen is not evidence that fill is unavailable.
        consecutiveFailures = 0;
        StartLoadLoop(0f);
    }

    // Called when a level starts, so the win or lose card has an ad in hand instead of a wait. This
    // is the only place a result-card ad is ever requested — see ShowPreloaded.
    public void Preload(string adUnitId, AdSlotKind slot)
    {
        AdsManager manager = AdsManager.Instance;
        if (manager == null || !manager.Ready || !IsAllowed(adUnitId, slot))
        {
            return;
        }

        // A preloaded ad that has gone stale is not worth holding on to, and holding it would also
        // block this fresh attempt.
        if (preloadedAd != null && IsStale(preloadedAt))
        {
            Debug.Log("[NativeAd] Dropping a preloaded ad that went stale before it was shown.");
            DestroyPreloaded();
        }

        if (isPreloading || preloadedAd != null)
        {
            return;
        }

        isPreloading = true;
        RequestCount++;

        NativeOverlayAd.Load(adUnitId, new AdRequest(), BuildOptions(), (ad, error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                isPreloading = false;

                if (error != null || ad == null)
                {
                    Debug.LogWarning("[NativeAd] Preload failed: "
                                     + (error != null ? error.GetMessage() : "no ad returned"));
                    return;
                }

                DestroyPreloaded();
                FillCount++;
                preloadedAd = ad;
                preloadedAt = Time.realtimeSinceStartup;
                WireEvents(ad);
                Debug.Log("[NativeAd] Preloaded and waiting.");
            });
        });
    }

    // Shows only what is already in hand. A live load here is deliberately not attempted: nobody
    // waits on a result card, so a request started now would arrive after the player has already
    // tapped Next or Retry — paid for, never seen. No preloaded ad means no ad and no reserved space.
    //
    // The next ad is not requested here either. Preloading happens once per level, at StartLevel,
    // so a player who quits from the card never triggers a request that goes to waste.
    public void ShowPreloaded(string adUnitId, AdSlotKind slot, NativeAdPlacement placement)
    {
        wantVisible = true;
        activeUnitId = adUnitId;
        activeSlot = slot;
        activePlacement = placement;
        activeAutoRefresh = false;
        StopLoadLoop();

        if (!IsAllowed(adUnitId, slot))
        {
            Hide();
            return;
        }

        if (preloadedAd == null)
        {
            Debug.Log("[NativeAd] No preloaded ad in hand — result card shows no ad.");
            Hide();
            return;
        }

        if (IsStale(preloadedAt))
        {
            Debug.Log("[NativeAd] Preloaded ad is "
                      + Mathf.RoundToInt(Time.realtimeSinceStartup - preloadedAt)
                      + "s old — too stale to show. Result card shows no ad.");
            DestroyPreloaded();
            Hide();
            return;
        }

        DestroyActive();
        currentAd = preloadedAd;
        preloadedAd = null;

        Debug.Log("[NativeAd] Using the preloaded ad for " + slot + ".");
        StartPresent(currentAd, slot, placement, needsTemplate: true);
    }

    // Full teardown. The ad object goes with it, so the next Show pays for a new request.
    public void Hide()
    {
        wantVisible = false;
        StopLoadLoop();
        StopMeasure();
        StopPresent();
        DestroyActive();
        SetHeight(0f);
    }

    static bool IsStale(float loadedAt)
    {
        return loadedAt < 0f || Time.realtimeSinceStartup - loadedAt >= AdMaxAgeSeconds;
    }

    void Load(string adUnitId, AdSlotKind slot, NativeAdPlacement placement)
    {
        loadInFlight = true;
        RequestCount++;

        NativeOverlayAd.Load(adUnitId, new AdRequest(), BuildOptions(), (ad, error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                loadInFlight = false;

                if (error != null || ad == null)
                {
                    consecutiveFailures++;
                    Debug.LogWarning("[NativeAd] Load failed (" + consecutiveFailures + " in a row): "
                                     + (error != null ? error.GetMessage() : "no ad returned"));

                    // Counted here too, not only on impression: a unit that requests plenty and
                    // never fills would otherwise produce no ratio to look at at all, and that is
                    // the exact shape of a serving-limit problem.
                    LogCounters("load failed");

                    // Only collapse when there is nothing on screen to keep room for. A failed
                    // refresh must not pull the floor out from under an ad that is still showing.
                    if (currentAd == null)
                    {
                        SetHeight(0f);
                    }

                    return;
                }

                FillCount++;
                consecutiveFailures = 0;

                // The player may have left the screen while the request was in flight.
                if (!wantVisible || !IsAllowed(adUnitId, slot))
                {
                    ad.Destroy();
                    return;
                }

                DestroyActive();
                currentAd = ad;
                WireEvents(ad);
                StartPresent(ad, slot, placement, needsTemplate: true);
            });
        });
    }

    // Impression and click are the only honest measure of whether the requests are worth anything.
    // Wrapped in ExecuteInUpdate because the plugin raises these from the JNI thread.
    void WireEvents(NativeOverlayAd ad)
    {
        ad.OnAdImpressionRecorded += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            ImpressionCount++;
            LogCounters("impression");
        });

        ad.OnAdClicked += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            ClickCount++;
            LogCounters("click");
        });

        ad.OnAdPaid += value => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (value != null)
            {
                RevenueMicros += value.Value;
            }
        });
    }

    void LogCounters(string what)
    {
        float matchRate = RequestCount > 0 ? 100f * FillCount / RequestCount : 0f;
        float showRate = FillCount > 0 ? 100f * ImpressionCount / FillCount : 0f;

        Debug.Log("[NativeAd] " + what + " — requests " + RequestCount
                  + ", filled " + FillCount + " (" + matchRate.ToString("F0") + "% match)"
                  + ", impressions " + ImpressionCount + " (" + showRate.ToString("F0") + "% shown)"
                  + ", clicks " + ClickCount
                  + ", est. revenue " + (RevenueMicros / 1000000d).ToString("F4"));
    }

    // Repositions a template that is already on screen. This is what makes an in-card ad possible:
    // the card cannot know where to put the ad until the ad's height has grown the card, and the
    // card's rect is only final after that layout pass.
    public void MoveTo(int x, int y)
    {
        if (currentAd == null)
        {
            return;
        }

        currentAd.SetTemplatePosition(x, y);
    }

    void StartPresent(NativeOverlayAd ad, AdSlotKind slot, NativeAdPlacement placement, bool needsTemplate)
    {
        StopPresent();
        presentRoutine = StartCoroutine(PresentRoutine(ad, slot, placement, needsTemplate));
    }

    void StopPresent()
    {
        if (presentRoutine != null)
        {
            StopCoroutine(presentRoutine);
            presentRoutine = null;
        }
    }

    // Room is opened BEFORE the overlay appears, not after it has been measured. The overlay is an
    // Android view drawn above Unity: showing it while the game's own layout still ends at the
    // bottom of the screen puts it straight over whatever is there — on the gameplay screen, the
    // Hint and Restart buttons. A tap meant for a button then lands on the ad, and an accidental
    // click is invalid traffic, which is the fastest route to an ad serving limit.
    //
    // The expected height comes from the template's own layout, so it is known before the ad exists.
    // The measured height afterwards only corrects it.
    IEnumerator PresentRoutine(NativeOverlayAd ad, AdSlotKind slot, NativeAdPlacement placement, bool needsTemplate)
    {
        NativeStyleMapping mapping = Settings != null ? Settings.NativeStyle(slot) : null;

        // A fresh config can land between the load and here. Rendering an unmapped style would put
        // an ad on screen at a size the backend just withdrew.
        if (mapping == null)
        {
            Debug.Log("[NativeAd] " + slot + " has no native layout — nothing rendered.");
            presentRoutine = null;
            Hide();
            yield break;
        }

        int expectedHeightPx = ExpectedHeightPx(slot);
        if (expectedHeightPx <= 0)
        {
            Debug.Log("[NativeAd] No expected height for " + slot + " — nothing rendered.");
            presentRoutine = null;
            Hide();
            yield break;
        }

        SetHeight(expectedHeightPx);

        // Two frames: one for the reserved box's own height, one for the layout that follows it.
        yield return null;
        yield return null;

        if (ad == null || currentAd != ad || !wantVisible)
        {
            presentRoutine = null;
            yield break;
        }

        if (needsTemplate)
        {
            NativeTemplateStyle style = BuildStyle(mapping);

            if (placement.anchored)
            {
                ad.RenderTemplate(style, placement.anchor);
            }
            else
            {
                ad.RenderTemplate(style, placement.x, placement.y);
            }
        }

        ad.Show();
        presentRoutine = null;

        Debug.Log("[NativeAd] Showing " + slot
                  + (placement.anchored
                      ? " anchored " + placement.anchor
                      : " at " + placement.x + "," + placement.y + "dp")
                  + ", " + expectedHeightPx + "px reserved first.");

        StartMeasure(expectedHeightPx);
    }

    // Colours come from the game's own palette so the ad reads as part of the screen rather than a
    // white slab dropped onto it. The arrangement is fixed by the template and cannot be changed.
    // The card tint from ResultScreen.uss, so an ad beside the result card reads as the same
    // material rather than a lighter or darker slab.
    static readonly Color Panel = new Color(0.047f, 0.071f, 0.165f);
    static readonly Color Ink = new Color(0.02f, 0.03f, 0.086f);
    static readonly Color Muted = new Color(0.65f, 0.70f, 0.80f);
    static readonly Color Accent = new Color(0f, 0.898f, 1f);

    static NativeTemplateStyle BuildStyle(NativeStyleMapping mapping)
    {
        NativeTemplateSize template = mapping != null ? mapping.template : NativeTemplateSize.Small;

        return new NativeTemplateStyle
        {
            TemplateId = template == NativeTemplateSize.Medium
                ? NativeTemplateId.Medium
                : NativeTemplateId.Small,

            MainBackgroundColor = Panel,

            PrimaryText = TextStyle(Color.white, 15, NativeTemplateFontStyle.Bold),
            SecondaryText = TextStyle(Muted, 12, NativeTemplateFontStyle.Normal),
            TertiaryText = TextStyle(Muted, 12, NativeTemplateFontStyle.Normal),

            CallToActionText = new NativeTemplateTextStyle
            {
                BackgroundColor = Accent,
                TextColor = Ink,
                FontSize = 14,
                Style = NativeTemplateFontStyle.Bold
            }
        };
    }

    // BackgroundColor has to be set on every text row, not left at the struct default. The plugin's
    // applyStyles() pushes each row's background onto its TextView unconditionally, so an unset
    // value paints a box in whatever colour the default happens to be — which is what made the
    // headline, the rating row and the body each sit on a different shade.
    static NativeTemplateTextStyle TextStyle(Color color, int size, NativeTemplateFontStyle style)
    {
        return new NativeTemplateTextStyle
        {
            BackgroundColor = Panel,
            TextColor = color,
            FontSize = size,
            Style = style
        };
    }

    // How tall the ad will come out for a given width. Public because the UI needs it BEFORE the ad
    // exists: opening the reserved box at its final height means the layout settles once, and the ad
    // can then be requested at a position that is already correct — no repositioning afterwards.
    // Returns 0 for a non-native slot, which is the caller's signal to reserve nothing at all.
    public static int ExpectedHeightPx(AdSlotKind slot)
    {
        AdSettings settings = Settings;
        NativeStyleMapping mapping = settings != null ? settings.NativeStyle(slot) : null;

        if (mapping == null)
        {
            return 0;
        }

        if (mapping.heightPx > 0)
        {
            return mapping.heightPx;
        }

        return NaturalHeightPx(mapping, Screen.width);
    }

    // A container shorter than the template's own height does not scale the ad down — it clips and
    // overlaps it. Both figures are read off the template layouts inside googlemobileads-unity.aar:
    //
    //   medium  the root NativeAdView is a fixed 350dp tall
    //   small   the inner box is constrained to a 4:1 ratio, inside 10dp of margin each side
    static int NaturalHeightPx(NativeStyleMapping mapping, int widthPx)
    {
        bool medium = mapping != null && mapping.template == NativeTemplateSize.Medium;

        return medium
            ? Mathf.RoundToInt(MediumTemplateHeightDp * Density)
            : Mathf.RoundToInt(widthPx / 4f + SmallTemplateMarginDp * 2f * Density);
    }

    // Same source the placement maths uses, so a dp figure means the same thing on both sides.
    static float Density => NativeAdSpace.DisplayDensity;

    static NativeAdOptions BuildOptions()
    {
        return new NativeAdOptions
        {
            // Any, not Landscape: the media area is a fixed 190dp tall, so demanding a landscape
            // creative just letterboxes square artwork and leaves bands above and below it.
            MediaAspectRatio = MediaAspectRatio.Any,
            AdChoicesPlacement = AdChoicesPlacement.TopRightCorner
        };
    }

    // Three ways a placement can be off, and all three have to leave the screen looking exactly as
    // it does with ads disabled: the master flag, a blank unit ID, and a slot that is not a native
    // one (AdSlotKind.None, or a banner slot that reached here by mistake).
    static bool IsAllowed(string adUnitId, AdSlotKind slot)
    {
        AdSettings settings = Settings;

        return settings != null
               && settings.AdsEnabled
               && !string.IsNullOrEmpty(adUnitId)
               && settings.NativeStyle(slot) != null;
    }

    void StartMeasure(int fallbackHeightPx)
    {
        StopMeasure();
        measureRoutine = StartCoroutine(MeasureRoutine(fallbackHeightPx));
    }

    void StopMeasure()
    {
        if (measureRoutine != null)
        {
            StopCoroutine(measureRoutine);
            measureRoutine = null;
        }
    }

    // The template reports a height only once Android has laid it out, which is not the frame the
    // render call returned on. Polling is the only signal available.
    IEnumerator MeasureRoutine(int fallbackHeightPx)
    {
        float deadline = Time.realtimeSinceStartup + MeasureTimeoutSeconds;

        while (Time.realtimeSinceStartup < deadline)
        {
            if (currentAd == null)
            {
                measureRoutine = null;
                yield break;
            }

            float height = currentAd.GetTemplateHeightInPixels();
            if (height > 0f)
            {
                Debug.Log("[NativeAd] Measured " + currentAd.GetTemplateWidthInPixels()
                          + " x " + height + " px (reserved " + fallbackHeightPx + "px, screen is "
                          + Screen.width + " x " + Screen.height + ").");
                SetHeight(height);
                measureRoutine = null;
                yield break;
            }

            yield return null;
        }

        // The reserved height stays. The ad is on screen whether or not it ever reported a size, so
        // collapsing here would leave it sitting over the UI underneath — the accidental-click case
        // the reserve-first order exists to prevent.
        Debug.LogWarning("[NativeAd] Template never reported a height; keeping the "
                         + fallbackHeightPx + "px reserved for it.");
        measureRoutine = null;
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

    void StartLoadLoop(float firstDelaySeconds)
    {
        StopLoadLoop();
        loadRoutine = StartCoroutine(LoadLoop(firstDelaySeconds));
    }

    void StopLoadLoop()
    {
        if (loadRoutine != null)
        {
            StopCoroutine(loadRoutine);
            loadRoutine = null;
        }
    }

    // One loop owns every request for the visible ad: the first one and each refresh. That is what
    // makes the backoff hold — a retry ladder split across two code paths always ends up with one
    // path ignoring it.
    IEnumerator LoadLoop(float firstDelaySeconds)
    {
        float delay = firstDelaySeconds;

        while (true)
        {
            if (delay > 0f)
            {
                // Realtime, so a dialog that froze Time.timeScale does not stall the cycle.
                yield return new WaitForSecondsRealtime(delay);
            }

            if (!wantVisible || !IsAllowed(activeUnitId, activeSlot))
            {
                break;
            }

            Load(activeUnitId, activeSlot, activePlacement);

            // Waiting for the request to settle is what lets the next delay react to a failure.
            float deadline = Time.realtimeSinceStartup + LoadSettleTimeoutSeconds;
            while (loadInFlight && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            delay = NextDelaySeconds();
            if (delay <= 0f)
            {
                break;
            }
        }

        loadRoutine = null;
    }

    // A healthy cycle refreshes on the backend's interval. A failing one backs off, doubling from a
    // 30s floor up to five minutes, so a device that cannot be filled stops asking every few
    // seconds.
    float NextDelaySeconds()
    {
        if (consecutiveFailures > 0)
        {
            int shift = Mathf.Min(consecutiveFailures - 1, 5);
            int backoff = Mathf.Min(MinBackoffSeconds << shift, MaxBackoffSeconds);

            Debug.Log("[NativeAd] " + consecutiveFailures + " failed load(s) in a row — next attempt in "
                      + backoff + "s.");
            return backoff;
        }

        // Zero or negative disables refreshing entirely, which ends the loop.
        return activeAutoRefresh ? RefreshSeconds : 0f;
    }

    void DestroyActive()
    {
        if (currentAd == null)
        {
            return;
        }

        currentAd.Hide();
        currentAd.Destroy();
        currentAd = null;
    }

    void DestroyPreloaded()
    {
        if (preloadedAd == null)
        {
            return;
        }

        preloadedAd.Destroy();
        preloadedAd = null;
        preloadedAt = -1f;
    }
}
