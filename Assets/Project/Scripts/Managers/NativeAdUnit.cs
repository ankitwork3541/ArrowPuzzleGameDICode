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
}

// Owns the native overlay ad. Android draws it on top of the game, in screen pixels, while the
// game's own layout lives in panel units — so the only honest way to reserve room for it is to
// measure the rendered template and publish that height. Nothing is reserved until an ad exists.
public class NativeAdUnit : MonoBehaviour
{
    const float MeasureTimeoutSeconds = 2f;

    // Read off the template layouts in googlemobileads-unity.aar, not chosen.
    const int MediumTemplateHeightDp = 350;
    const int SmallTemplateMarginDp = 10;

    NativeOverlayAd currentAd;
    NativeOverlayAd preloadedAd;
    bool isPreloading;

    bool wantVisible;
    string activeUnitId;
    int activeStyle;
    NativeAdPlacement activePlacement;
    bool activeAutoRefresh;

    Coroutine refreshRoutine;
    Coroutine measureRoutine;

    // Lets the result card decide whether to reserve room before it opens anything. Without this it
    // would open the box, discover on the next frame that no ad was preloaded, and collapse again —
    // a visible jump in the card's layout.
    public bool HasPreloadedAd => preloadedAd != null;

    // Screen pixels. Zero means nothing is on screen, which is the signal for the UI to collapse
    // its reserved space completely rather than leave a hole.
    public float HeightPixels { get; private set; }
    public event Action<float> OnHeightChanged;

    static AdSettings Settings => AdsManager.Instance != null ? AdsManager.Instance.Settings : null;

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

        StopRefresh();
        StopMeasure();
        DestroyActive();
        DestroyPreloaded();
    }

    // A request that arrived before the SDK finished initialising was parked, not dropped.
    void OnAdsReady()
    {
        if (wantVisible && !string.IsNullOrEmpty(activeUnitId))
        {
            Show(activeUnitId, activeStyle, activePlacement, activeAutoRefresh);
        }
    }

    public void Show(string adUnitId, int styleNumber, NativeAdPlacement placement, bool autoRefresh = true)
    {
        wantVisible = true;
        activeUnitId = adUnitId;
        activeStyle = styleNumber;
        activePlacement = placement;
        activeAutoRefresh = autoRefresh;

        if (!IsAllowed(adUnitId, styleNumber))
        {
            Debug.Log("[NativeAd] Show refused for style " + styleNumber
                      + " — ads disabled, unit ID blank, or style unsupported.");
            Hide();
            return;
        }

        AdsManager manager = AdsManager.Instance;
        if (manager == null || !manager.Ready)
        {
            Debug.Log("[NativeAd] Show for style " + styleNumber + " parked until the SDK is ready.");
            return;
        }

        LoadAndRender(adUnitId, styleNumber, placement);

        if (autoRefresh)
        {
            StartRefresh();
        }
        else
        {
            StopRefresh();
        }
    }

    // Called when a level starts, so the win or lose card has an ad in hand instead of a wait. This
    // is the only place a result-card ad is ever requested — see ShowPreloaded.
    public void Preload(string adUnitId, int styleNumber)
    {
        AdsManager manager = AdsManager.Instance;
        if (manager == null || !manager.Ready || !IsAllowed(adUnitId, styleNumber))
        {
            return;
        }

        if (isPreloading || preloadedAd != null)
        {
            return;
        }

        isPreloading = true;

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
                preloadedAd = ad;
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
    public void ShowPreloaded(string adUnitId, int styleNumber, NativeAdPlacement placement)
    {
        wantVisible = true;
        activeUnitId = adUnitId;
        activeStyle = styleNumber;
        activePlacement = placement;
        activeAutoRefresh = false;
        StopRefresh();

        if (!IsAllowed(adUnitId, styleNumber))
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

        DestroyActive();
        currentAd = preloadedAd;
        preloadedAd = null;

        Debug.Log("[NativeAd] Using the preloaded ad for style " + styleNumber + ".");
        Render(currentAd, styleNumber, placement);
    }

    public void Hide()
    {
        wantVisible = false;
        StopRefresh();
        StopMeasure();
        DestroyActive();
        SetHeight(0f);
    }

    void LoadAndRender(string adUnitId, int styleNumber, NativeAdPlacement placement)
    {
        NativeOverlayAd.Load(adUnitId, new AdRequest(), BuildOptions(), (ad, error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("[NativeAd] Load failed: "
                                     + (error != null ? error.GetMessage() : "no ad returned"));
                    SetHeight(0f);
                    return;
                }

                // The player may have left the screen while the request was in flight.
                if (!wantVisible || !IsAllowed(adUnitId, styleNumber))
                {
                    ad.Destroy();
                    SetHeight(0f);
                    return;
                }

                DestroyActive();
                currentAd = ad;
                Render(currentAd, styleNumber, placement);
            });
        });
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

    void Render(NativeOverlayAd ad, int styleNumber, NativeAdPlacement placement)
    {
        NativeStyleMapping mapping = Settings != null ? Settings.NativeStyle(styleNumber) : null;

        // A fresh config can land between the load and this render — refresh_native_sec is 10s, so
        // that window is hit often enough to matter. Rendering an unmapped style would put an ad on
        // screen at a size the backend just withdrew.
        if (mapping == null)
        {
            Debug.Log("[NativeAd] Style " + styleNumber + " has no layout — nothing rendered.");
            Hide();
            return;
        }

        NativeTemplateStyle style = BuildStyle(mapping);

        if (placement.anchored)
        {
            ad.RenderTemplate(style, placement.anchor);
        }
        else
        {
            ad.RenderTemplate(style, placement.x, placement.y);
        }

        ad.Show();
        StartMeasure();

        Debug.Log("[NativeAd] Rendered style " + styleNumber + " as '" + style.TemplateId + "'"
                  + (placement.anchored
                      ? " anchored " + placement.anchor
                      : " at " + placement.x + "," + placement.y + "dp")
                  + " default size.");
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
    // Returns 0 for an unsupported style, which is the caller's signal to reserve nothing at all.
    public static int ExpectedHeightPx(int styleNumber)
    {
        AdSettings settings = Settings;
        NativeStyleMapping mapping = settings != null ? settings.NativeStyle(styleNumber) : null;

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
    // it does with ads disabled: the master flag, a blank unit ID, and a style digit the plugin has
    // no layout for.
    static bool IsAllowed(string adUnitId, int styleNumber)
    {
        AdSettings settings = Settings;

        return settings != null
               && settings.AdsEnabled
               && !string.IsNullOrEmpty(adUnitId)
               && settings.IsSupportedNativeStyle(styleNumber);
    }

    void StartMeasure()
    {
        StopMeasure();
        measureRoutine = StartCoroutine(MeasureRoutine());
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
    IEnumerator MeasureRoutine()
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
                          + " x " + height + " px (screen is " + Screen.width + " x " + Screen.height + ").");
                SetHeight(height);
                measureRoutine = null;
                yield break;
            }

            yield return null;
        }

        // Left collapsed on purpose: an unmeasured ad must not turn into a reserved empty strip.
        Debug.LogWarning("[NativeAd] Template never reported a height; space stays collapsed.");
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

    void StartRefresh()
    {
        StopRefresh();

        // Whatever refresh_native_sec asks for, unclamped. Zero or negative means never refresh.
        int seconds = Settings != null ? Settings.NativeRefreshSeconds : 0;
        if (seconds > 0)
        {
            refreshRoutine = StartCoroutine(RefreshRoutine(seconds));
        }
    }

    void StopRefresh()
    {
        if (refreshRoutine != null)
        {
            StopCoroutine(refreshRoutine);
            refreshRoutine = null;
        }
    }

    IEnumerator RefreshRoutine(int seconds)
    {
        while (true)
        {
            // Realtime, so a dialog that froze Time.timeScale does not stall the cycle.
            yield return new WaitForSecondsRealtime(seconds);

            if (!wantVisible || !IsAllowed(activeUnitId, activeStyle))
            {
                refreshRoutine = null;
                yield break;
            }

            LoadAndRender(activeUnitId, activeStyle, activePlacement);
        }
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
    }
}
