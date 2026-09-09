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

    // Which screen the ad on screen belongs to. Part of the reuse test for the same reason the
    // banner has one — see the note on BannerAdUnit.wantOwner.
    string activeOwner;

    // Bumped by Show(), ShowPreloaded() and Hide() — anything that changes which screen owns the
    // slot. Load()'s callback captures this at request time and only acts if it is still current.
    // wantVisible alone is not enough: a fast level (won in a couple of seconds, well within the
    // gameplay screen's still-in-flight ad request) hands the slot to the result screen's
    // ShowPreloaded() while that request is outstanding, which sets wantVisible = true again for
    // the RESULT ad. The stale gameplay-ad callback would then read wantVisible as true, pass
    // IsAllowed() (still generically true), and overwrite the result ad with itself at the
    // gameplay screen's placement — a genuine "wrong ad, wrong position" glitch that AdMob's
    // validator can catch as an out-of-bounds render on that one occasion.
    int requestToken;

    int consecutiveFailures;
    bool loadInFlight;

    // What each template last measured on THIS device, so the next request for the same slot can
    // open its box straight away and carry the loading skeleton. Deliberately learned rather than
    // hardcoded: the template's real height depends on the device's font scale, its dp width and the
    // creative's own text, so no constant can be right everywhere. Empty on the session's first
    // request for a slot, which simply means no skeleton that once.
    readonly System.Collections.Generic.Dictionary<AdSlotKind, int> measuredHeights =
        new System.Collections.Generic.Dictionary<AdSlotKind, int>();

    Coroutine loadRoutine;
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

    // True only once the overlay is actually painted. Until then any reserved room is holding a
    // skeleton, which is what the UI needs to know to show one.
    public bool ShowingAd { get; private set; }

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
        StopPresent();
        DestroyActive();
        DestroyPreloaded();
    }

    // A request that arrived before the SDK finished initialising was parked, not dropped.
    void OnAdsReady()
    {
        if (wantVisible && !string.IsNullOrEmpty(activeUnitId))
        {
            Show(activeUnitId, activeSlot, activePlacement, activeOwner, activeAutoRefresh);
        }
    }

    public void Show(string adUnitId, AdSlotKind slot, NativeAdPlacement placement, string owner,
                     bool autoRefresh = true)
    {
        // Nothing has changed and this screen's ad is already up — reloading here would throw away a
        // live impression and start a new request every time the player crossed between two screens
        // sharing one digit. Same reasoning as BannerAdUnit.Show().
        if (wantVisible && currentAd != null
            && activeUnitId == adUnitId && activeSlot == slot && activeOwner == owner
            && NativeAdPlacement.Same(activePlacement, placement))
        {
            Debug.Log("[NativeAd] Reusing the " + owner + " " + slot
                      + " already on screen — no new request.");
            return;
        }

        wantVisible = true;
        activeUnitId = adUnitId;
        activeSlot = slot;
        activePlacement = placement;
        activeOwner = owner;
        activeAutoRefresh = autoRefresh;
        requestToken++;

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

        // Room first, request second. The template's height is known from its own layout before any
        // ad exists, so the screen can open the box to that and fill it with a skeleton — and
        // nothing has to move again when the overlay paints. Opening it only in PresentRoutine is
        // what shoved the bottom nav upwards in one frame.
        if (currentAd == null)
        {
            SetShowingAd(false);
            SetHeight(LastMeasuredHeightPx(slot));
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
        Debug.Log("[NativeAd] Preloading " + slot + " for the result card — unit " + adUnitId + ".");

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
        activeOwner = "result";
        activeAutoRefresh = false;
        requestToken++;
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
        if (wantVisible || currentAd != null)
        {
            Debug.Log("[NativeAd] Hidden and destroyed.");
        }

        wantVisible = false;
        activeOwner = null;
        requestToken++;
        StopLoadLoop();
        StopPresent();
        DestroyActive();
        SetShowingAd(false);
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

        // Captured now, checked in the callback: if Show()/ShowPreloaded()/Hide() moved the slot
        // on to a different owner in the meantime, wantVisible alone would read true again and let
        // this stale result through — see the comment on requestToken's declaration.
        int token = requestToken;

        int reserved = LastMeasuredHeightPx(slot);
        Debug.Log("[NativeAd] Requesting " + slot + " for " + activeOwner + " — unit " + adUnitId
                  + (reserved > 0
                      ? ", " + reserved + "px reserved from the last measurement."
                      : ", nothing reserved yet — first " + slot + " of this session."));

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
                        SetShowingAd(false);
                        SetHeight(0f);
                    }

                    return;
                }

                FillCount++;
                consecutiveFailures = 0;
                Debug.Log("[NativeAd] Loaded " + slot + ".");

                // The player may have left the screen while the request was in flight, or handed
                // the slot to a different owner (e.g. the result screen's ShowPreloaded()) that
                // also wants an ad — token catches that second case, which wantVisible alone cannot
                // since ShowPreloaded() sets it straight back to true for itself.
                if (!wantVisible || token != requestToken || !IsAllowed(adUnitId, slot))
                {
                    Debug.Log("[NativeAd] Loaded after the slot moved on — discarded, not shown.");
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

        ad.OnAdPaid += (AdValue adValue) =>
        {
            AndroidDataBridge.SendDataToAndroid(adValue.Value,adValue.CurrencyCode,"Native");
        };

        // ad.OnAdPaid += value => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        // {
        //     if (value != null)
        //     {
        //         RevenueMicros += value.Value;
        //     }
        // });
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
    //
    // Confirmed on device (logcat): ResultScreenController's settle-and-nudge callbacks call this
    // with the SAME x,y up to a dozen times in one ~250ms window while the card's layout is
    // settling — none of them are wrong, they just keep re-confirming a position that already
    // landed. SetTemplatePosition re-triggers Android's own layout pass on the template each time,
    // so this was a dozen redundant relayouts for nothing. Worth keeping on its own merits, but
    // confirmed (also via logcat) NOT to be what makes AdMob's validator flag the session's first
    // native ad — that still happens with this dedup in place, from a cause not yet identified.
    int? lastMoveX;
    int? lastMoveY;

    public void MoveTo(int x, int y)
    {
        if (currentAd == null)
        {
            return;
        }

        if (lastMoveX == x && lastMoveY == y)
        {
            return;
        }

        lastMoveX = x;
        lastMoveY = y;
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

    // Renders the template, opens whatever room this slot is known to need, shows the ad, and then
    // corrects the room from the height Android actually reports.
    //
    // The measurement has to happen AFTER Show(), not before. The plugin's hide() sets the template
    // view to View.GONE (bipush 8 in UnityNativeTemplateAd$10), and Android never measures or lays
    // out a GONE view — so getHeight(), which is all getTemplateHeightInPixels() returns, reads 0 for
    // as long as the ad is hidden. There is no way to measure a native overlay off screen. That is
    // the one place it differs from BannerAdUnit, which can measure before showing.
    //
    // The height is still never PREDICTED. It used to be computed from the template's layout (350dp
    // for medium), but that is only the NativeAdView's own bound — the rendered height moves with the
    // device's font scale, its dp width and the creative's text length, so a constant was wrong on
    // exactly the devices where it mattered. What is reserved up front is the last figure this slot
    // actually measured on THIS device; the session's first render of a slot has none, and takes a
    // couple of frames with the ad on screen before its room opens.
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

        int known = LastMeasuredHeightPx(slot);
        if (known > 0)
        {
            SetHeight(known);

            // Two frames: one for the reserved box's own height, one for the layout that follows it.
            yield return null;
            yield return null;
        }

        if (ad == null || currentAd != ad || !wantVisible)
        {
            presentRoutine = null;
            yield break;
        }

        ad.Show();
        SetShowingAd(true);

        Debug.Log("[NativeAd] Showing " + slot
                  + (placement.anchored
                      ? " anchored " + placement.anchor
                      : " at " + placement.x + "," + placement.y + "dp")
                  + (known > 0
                      ? ", " + known + "px reserved first from the last measurement."
                      : ", nothing reserved yet — measuring now."));

        // Polling is the only signal available: the template reports a height once Android has laid
        // it out, which is not the frame Show() returned on.
        float measured = 0f;
        float deadline = Time.realtimeSinceStartup + MeasureTimeoutSeconds;

        while (Time.realtimeSinceStartup < deadline)
        {
            if (ad == null || currentAd != ad || !wantVisible)
            {
                presentRoutine = null;
                yield break;
            }

            measured = ad.GetTemplateHeightInPixels();
            if (measured > 0f)
            {
                break;
            }

            yield return null;
        }

        presentRoutine = null;

        if (measured <= 0f)
        {
            // Nothing to reserve and an overlay already on screen — which is the accidental-click
            // case the reserve-first order exists to prevent. One filled ad is worth less than
            // invalid traffic on the account, so it comes down.
            Debug.LogWarning("[NativeAd] " + slot + " never reported a height even after Show(); "
                             + "taking it down rather than leaving it over the UI.");
            Hide();
            yield break;
        }

        measuredHeights[slot] = Mathf.CeilToInt(measured);
        SetHeight(measured);

        Debug.Log("[NativeAd] Measured " + ad.GetTemplateWidthInPixels() + " x " + measured
                  + " px (screen " + Screen.width + " x " + Screen.height + ").");
    }

    // Google's shipped medium template (gnt_medium_template_view.xml inside
    // googlemobileads-unity.aar) is a hard-coded 350dp NativeAdView whose body TextView is
    // wrap_content with no maxLines, while the CTA button below it carries a 35dp minimum height —
    // three lines of body text alone can exceed the ~90dp left for both, pushing the CTA past the
    // view's own bound ("Advertiser assets outside native ad view" in AdMob's validator). No field
    // here can fix that; NativeTemplateStyle has no maxLines/height control.
    //
    // The actual fix is /AndroidResOverride/layout/{gnt,nextgen}_{medium,small}_template_view.xml —
    // unityLibrary's own resources beat googlemobileads-unity.aar's for a same-named resource (see
    // mainTemplate.gradle), so those replace Google's layout with a wrap_content NativeAdView, a
    // body capped at maxLines="3" with ellipsize, and a fixed-height CTA that can never be squeezed.
    // Both name pairs exist because this project runs the Next-Gen Android SDK architecture
    // (GoogleMobileAdsSettings.asset: selectedGmaAndroidSdk = NextGen) — RenderTemplate() inflates
    // nextgen_*_template_view.xml at runtime, not the legacy gnt_* ones; the gnt_* overrides are
    // kept only in case that setting ever reverts to Standard.
    //
    // With the layout fixed, colours and sizes below are free to differ from Google's defaults —
    // that risk only existed against the old fixed-height template.
    static NativeTemplateStyle BuildStyle(NativeStyleMapping mapping)
    {
        NativeTemplateSize template = mapping != null ? mapping.template : NativeTemplateSize.Small;

        Debug.Log("[NativeAd] Style: template=" + template + ", theme colors applied.");

        Color mainBg = new Color32(12, 18, 42, 255);                 // #0C122A dark sci-fi background
        Color primaryTextColor = Color.white;                         // #FFFFFF crisp white headline
        Color secondaryTextColor = new Color32(159, 176, 199, 255);    // #9FB0C7 cool gray secondary
        Color bodyTextColor = new Color32(197, 209, 232, 255);         // #C5D1E8 readable bright gray-blue
        Color ctaBgColor = new Color32(0, 229, 255, 255);              // #00E5FF neon cyan
        Color ctaTextColor = new Color32(5, 11, 24, 255);              // #050B18 dark bold contrast

        NativeTemplateStyle style = new NativeTemplateStyle
        {
            TemplateId = template == NativeTemplateSize.Medium
                ? NativeTemplateId.Medium
                : NativeTemplateId.Small,
            MainBackgroundColor = mainBg,
            PrimaryText = new NativeTemplateTextStyle
            {
                TextColor = primaryTextColor,
                FontSize = template == NativeTemplateSize.Medium ? 15 : 14,
                BackgroundColor = Color.clear
            },
            SecondaryText = new NativeTemplateTextStyle
            {
                TextColor = secondaryTextColor,
                FontSize = 12,
                BackgroundColor = Color.clear
            },
            TertiaryText = new NativeTemplateTextStyle
            {
                TextColor = bodyTextColor,
                FontSize = 12,
                BackgroundColor = Color.clear
            },
            CallToActionText = new NativeTemplateTextStyle
            {
                TextColor = ctaTextColor,
                FontSize = template == NativeTemplateSize.Medium ? 14 : 13,
                BackgroundColor = ctaBgColor
            }
        };

        return style;
    }

    // What this slot measured last time on this device, or an initial estimate if it has not rendered
    // yet. Public because the UI reserves room from it before the ad exists.
    public int LastMeasuredHeightPx(AdSlotKind slot)
    {
        if (measuredHeights.TryGetValue(slot, out int height) && height > 0)
        {
            return height;
        }

        NativeStyleMapping mapping = Settings != null ? Settings.NativeStyle(slot) : null;
        if (mapping != null)
        {
            float density = NativeAdSpace.DisplayDensity;
            int defaultDp = mapping.template == NativeTemplateSize.Medium ? 300 : 90;
            return Mathf.RoundToInt(defaultDp * density);
        }

        return 0;
    }

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

    void SetHeight(float pixels)
    {
        if (Mathf.Approximately(HeightPixels, pixels))
        {
            return;
        }

        HeightPixels = pixels;
        OnHeightChanged?.Invoke(pixels);
    }

    // Raises OnHeightChanged too, even though no height moved. The reserved room is opened at the
    // template's own expected height, so the measured height that follows is usually identical —
    // without this the one event that matters, "the skeleton can come down now", would never reach
    // the UI.
    void SetShowingAd(bool value)
    {
        if (ShowingAd == value)
        {
            return;
        }

        ShowingAd = value;
        OnHeightChanged?.Invoke(HeightPixels);
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

            // The load callback never came back. Google's callbacks always fire, but a skeleton left
            // up forever because one of them did not is a visible bug, not a missing ad.
            if (loadInFlight && currentAd == null)
            {
                Debug.LogWarning("[NativeAd] No answer in " + LoadSettleTimeoutSeconds
                                 + "s — giving the reserved room back.");
                SetShowingAd(false);
                SetHeight(0f);
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
        lastMoveX = null;
        lastMoveY = null;
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
