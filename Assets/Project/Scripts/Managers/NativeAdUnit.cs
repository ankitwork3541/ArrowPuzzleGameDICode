using System;
using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

// Where Android should draw the overlay: anchored to a screen edge, or at explicit screen pixels so
// it can sit inside a dialog card rather than at the bottom of the display.
public struct NativeAdPlacement
{
    public bool anchored;
    public AdPosition anchor;
    public int x;
    public int y;

    // Requested template width in dp. Zero lets it span the screen, which is right for an ad
    // anchored to a screen edge and wrong for one sitting inside a card.
    public int widthDp;

    public static NativeAdPlacement Bottom()
    {
        return new NativeAdPlacement { anchored = true, anchor = AdPosition.Bottom };
    }

    public static NativeAdPlacement At(int x, int y, int widthDp = 0)
    {
        return new NativeAdPlacement { anchored = false, x = x, y = y, widthDp = widthDp };
    }
}

// Owns the native overlay ad. Android draws it on top of the game, in screen pixels, while the
// game's own layout lives in panel units — so the only honest way to reserve room for it is to
// measure the rendered template and publish that height. Nothing is reserved until an ad exists.
public class NativeAdUnit : MonoBehaviour
{
    const float MeasureTimeoutSeconds = 2f;

    // Upper bounds for the container when a width is requested but no height is configured. Only
    // ceilings — the template still sizes itself by its own aspect ratio inside them.
    const int SmallHeightCapDp = 150;
    const int MediumHeightCapDp = 450;

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

        // Covers both "ads switched off" and "enabled but the unit ID came back blank".
        if (!IsAllowed(adUnitId))
        {
            Hide();
            return;
        }

        AdsManager manager = AdsManager.Instance;
        if (manager == null || !manager.Ready)
        {
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

    // Called when a level starts, so the win or lose card has an ad in hand instead of a wait.
    public void Preload(string adUnitId)
    {
        AdsManager manager = AdsManager.Instance;
        if (manager == null || !manager.Ready || !IsAllowed(adUnitId))
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

    // Consumes the preloaded ad and then starts the next one straight away. Without that second
    // preload only the first level end is instant, and every one after it waits on a live load.
    public void ShowPreloaded(string adUnitId, int styleNumber, NativeAdPlacement placement)
    {
        wantVisible = true;
        activeUnitId = adUnitId;
        activeStyle = styleNumber;
        activePlacement = placement;
        activeAutoRefresh = false;
        StopRefresh();

        if (!IsAllowed(adUnitId))
        {
            Hide();
            return;
        }

        if (preloadedAd == null)
        {
            Show(adUnitId, styleNumber, placement, autoRefresh: false);
            return;
        }

        DestroyActive();
        currentAd = preloadedAd;
        preloadedAd = null;

        Render(currentAd, styleNumber, placement);
        Preload(adUnitId);
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
                if (!wantVisible || !IsAllowed(adUnitId))
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
        NativeTemplateStyle style = BuildStyle(mapping);
        AdSize size = BuildSize(mapping, placement);

        if (size != null)
        {
            if (placement.anchored)
            {
                ad.RenderTemplate(style, size, placement.anchor);
            }
            else
            {
                ad.RenderTemplate(style, size, placement.x, placement.y);
            }
        }
        else if (placement.anchored)
        {
            ad.RenderTemplate(style, placement.anchor);
        }
        else
        {
            ad.RenderTemplate(style, placement.x, placement.y);
        }

        ad.Show();
        StartMeasure();

        Debug.Log("[NativeAd] Rendered style " + styleNumber + " as template '" + style.TemplateId + "'.");
    }

    // Colours come from the game's own palette so the ad reads as part of the screen rather than a
    // white slab dropped onto it. The arrangement is fixed by the template and cannot be changed.
    static NativeTemplateStyle BuildStyle(NativeStyleMapping mapping)
    {
        NativeTemplateSize template = mapping != null ? mapping.template : NativeTemplateSize.Small;
        Color ink = new Color(0.02f, 0.03f, 0.086f);
        Color muted = new Color(0.65f, 0.70f, 0.80f);

        return new NativeTemplateStyle
        {
            TemplateId = template == NativeTemplateSize.Medium
                ? NativeTemplateId.Medium
                : NativeTemplateId.Small,

            MainBackgroundColor = ink,

            PrimaryText = new NativeTemplateTextStyle
            {
                TextColor = Color.white,
                FontSize = 15,
                Style = NativeTemplateFontStyle.Bold
            },
            SecondaryText = new NativeTemplateTextStyle
            {
                TextColor = muted,
                FontSize = 12,
                Style = NativeTemplateFontStyle.Normal
            },
            TertiaryText = new NativeTemplateTextStyle
            {
                TextColor = muted,
                FontSize = 12,
                Style = NativeTemplateFontStyle.Normal
            },
            CallToActionText = new NativeTemplateTextStyle
            {
                BackgroundColor = new Color(0f, 0.898f, 1f),
                TextColor = ink,
                FontSize = 14,
                Style = NativeTemplateFontStyle.Bold
            }
        };
    }

    // Null means "let the template size itself" — correct for a screen-edge ad. A width only has to
    // be requested when the ad must fit inside something narrower, such as a dialog card.
    //
    // The height passed alongside it is a container bound, not a layout guess: the templates keep a
    // fixed aspect ratio and scale to the width they are given, and the height the UI reserves
    // still comes from measuring the result afterwards.
    static AdSize BuildSize(NativeStyleMapping mapping, NativeAdPlacement placement)
    {
        int width = placement.widthDp > 0
            ? placement.widthDp
            : (mapping != null && mapping.widthDp > 0 ? mapping.widthDp : 0);

        int height = mapping != null && mapping.heightDp > 0 ? mapping.heightDp : 0;

        if (width <= 0 && height <= 0)
        {
            return null;
        }

        if (width <= 0)
        {
            width = AdSize.FullWidth;
        }

        if (height <= 0)
        {
            bool medium = mapping != null && mapping.template == NativeTemplateSize.Medium;
            height = medium ? MediumHeightCapDp : SmallHeightCapDp;
        }

        return new AdSize(width, height);
    }

    static NativeAdOptions BuildOptions()
    {
        return new NativeAdOptions
        {
            MediaAspectRatio = MediaAspectRatio.Landscape,
            AdChoicesPlacement = AdChoicesPlacement.TopRightCorner
        };
    }

    static bool IsAllowed(string adUnitId)
    {
        AdSettings settings = Settings;
        return settings != null && settings.AdsEnabled && !string.IsNullOrEmpty(adUnitId);
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

        int seconds = Settings != null ? Settings.NativeRefreshSeconds : AdSettings.MinNativeRefreshSeconds;
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

            if (!wantVisible || !IsAllowed(activeUnitId))
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
