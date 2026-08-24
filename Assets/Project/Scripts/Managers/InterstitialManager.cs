using System;
using UnityEngine;

// Global interstitial policy. The ad object itself is preloaded by InterstitialAdUnit;
// this class only decides WHEN a ready ad may be shown.
//
// Tracking (click count + session timer) resets when the process starts (app kill / fresh
// launch) and when an interstitial actually displays. In-game Restart / Retry does not reset it.
public class InterstitialManager : MonoBehaviour
{
    private static InterstitialManager instance;
    public static InterstitialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<InterstitialManager>();
            }

            return instance;
        }
    }

    private int globalClickCount;
    private float timer;

    public int ClickCount => globalClickCount;
    public float TimeElapsed => timer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
            return;
        }

        ResetTracking();
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void ResetTracking()
    {
        globalClickCount = 0;
        timer = 0f;
        Debug.Log("[InterstitialManager] Tracking reset (clicks=0, timer=0s).");
    }

    public void TryShowClickAd(Action onActionProceed)
    {
        AdsManager ads = AdsManager.Instance;
        if (ads == null || ads.Settings == null || !ads.Settings.InterstitialEnabled
            || ads.Settings.AdClickCount <= 0)
        {
            onActionProceed?.Invoke();
            return;
        }

        globalClickCount++;

        int requiredClicks = ads.Settings.AdClickCount;
        float requiredTime = ads.Settings.AdClickTimeSec;
        bool countMet = globalClickCount >= requiredClicks;
        bool timeMet = timer >= requiredTime;

        if (countMet && timeMet)
        {
            if (TryPresent(ads, onActionProceed,
                    "Click ad. clicks=" + globalClickCount + "/" + requiredClicks
                    + " time=" + timer.ToString("F1") + "s/" + requiredTime + "s"))
            {
                return;
            }

            Debug.LogWarning("[InterstitialManager] Click + time met, but no preloaded interstitial. Skipping.");
            onActionProceed?.Invoke();
            return;
        }

        Debug.Log("[InterstitialManager] Click ad skipped (clicks=" + globalClickCount + "/"
                  + requiredClicks + ", timer=" + timer.ToString("F1") + "s/" + requiredTime + "s).");
        onActionProceed?.Invoke();
    }

    public void TryShowBackAd(Action onActionProceed)
    {
        AdsManager ads = AdsManager.Instance;
        if (ads != null && ads.Settings != null && ads.Settings.BackAdEnabled)
        {
            TryShowClickAd(onActionProceed);
            return;
        }

        onActionProceed?.Invoke();
    }

    // Win-panel NEXT only. No click/time gate. Shows a preloaded ad when the completed
    // level hits ad_level_count; never starts a load on the tap itself.
    public void TryShowLevelAd(int completedLevel, Action onActionProceed)
    {
        AdsManager ads = AdsManager.Instance;
        if (ads == null || ads.Settings == null || !ads.Settings.InterstitialEnabled)
        {
            onActionProceed?.Invoke();
            return;
        }

        int interval = ads.Settings.AdLevelCount;
        if (interval <= 0 || completedLevel <= 0 || (completedLevel % interval) != 0)
        {
            onActionProceed?.Invoke();
            return;
        }

        if (TryPresent(ads, onActionProceed,
                "Level ad. level=" + completedLevel + " interval=" + interval))
        {
            return;
        }

        Debug.LogWarning("[InterstitialManager] Level interval matched, but no preloaded interstitial. Skipping.");
        onActionProceed?.Invoke();
    }

    bool TryPresent(AdsManager ads, Action onActionProceed, string reason)
    {
        if (ads.Interstitial == null || !ads.Interstitial.IsReady)
        {
            return false;
        }

        Debug.Log("[InterstitialManager] Showing interstitial. " + reason);
        ResetTracking();
        ads.Interstitial.Show(onActionProceed);
        return true;
    }
}
