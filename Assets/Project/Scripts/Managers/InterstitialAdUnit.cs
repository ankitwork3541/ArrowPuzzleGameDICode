using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

// Keeps one interstitial loaded and ready to show. Deliberately says nothing about WHEN an
// interstitial should appear — that policy belongs to whatever calls Show().
public class InterstitialAdUnit : MonoBehaviour
{
    InterstitialAd ad;
    bool loading;

    public bool IsReady => ad != null && ad.CanShowAd();

    // Raised once after a shown ad goes away, whether it closed normally or failed to display.
    // A caller that paused the game on Show() can resume here without special-casing failure.
    public event System.Action OnClosed;

    void Start()
    {
        AdsManager manager = AdsManager.Instance;
        if (manager == null)
        {
            return;
        }

        if (manager.Ready)
        {
            Load();
        }
        else
        {
            manager.OnReady += Load;
        }
    }

    void OnDestroy()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.OnReady -= Load;
        }

        DestroyAd();
    }

    public void Load()
    {
        if (loading || IsReady)
        {
            return;
        }

        AdsManager manager = AdsManager.Instance;
        if (manager == null || !manager.Ready)
        {
            return;
        }

        if (!manager.Settings.AdsEnabled)
        {
            Debug.Log("[Ads] Interstitial skipped — ads disabled by config.");
            return;
        }

        string unitId = manager.Settings.InterstitialId;
        if (string.IsNullOrEmpty(unitId))
        {
            Debug.LogWarning("[Ads] No interstitial unit available — nothing to load.");
            return;
        }

        loading = true;

        InterstitialAd.Load(unitId, new AdRequest(), (InterstitialAd loaded, LoadAdError error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                loading = false;

                if (error != null || loaded == null)
                {
                    Debug.LogWarning("[Ads] Interstitial load failed: "
                                     + (error != null ? error.GetMessage() : "no ad returned"));
                    return;
                }

                ad = loaded;
                RegisterEvents(ad);
                Debug.Log("[Ads] Interstitial loaded.");
            });
        });
    }

    private System.Action pendingClosedCallback;

    // Shows the loaded interstitial. onClosed is called once the ad dismisses, fails to show,
    // or immediately if no ad was ready.
    public bool Show(System.Action onClosed = null)
    {
        if (!IsReady)
        {
            Load();
            onClosed?.Invoke();
            return false;
        }

        pendingClosedCallback = onClosed;
        ad.Show();
        return true;
    }

    void RegisterEvents(InterstitialAd target)
    {
        target.OnAdFullScreenContentClosed += () => Finish(null);
        target.OnAdFullScreenContentFailed += error => Finish(error);
    }

    void Finish(AdError error)
    {
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (error != null)
            {
                Debug.LogWarning("[Ads] Interstitial failed to show: " + error.GetMessage());
            }

            // An interstitial is a single-use object: once shown it can never be shown again, so
            // the replacement has to be fetched now rather than at the next show site.
            DestroyAd();
            System.Action callback = pendingClosedCallback;
            pendingClosedCallback = null;

            OnClosed?.Invoke();
            callback?.Invoke();
            Load();
        });
    }

    void DestroyAd()
    {
        if (ad == null)
        {
            return;
        }

        ad.Destroy();
        ad = null;
    }
}
