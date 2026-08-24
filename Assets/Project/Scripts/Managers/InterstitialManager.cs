using System;
using UnityEngine;

// Global interstitial policy. The ad object itself is owned by InterstitialAdUnit; this class only
// decides WHEN one may be shown.
//
// Tracking (click count + session timer) resets when the process starts (app kill / fresh launch)
// and when an interstitial actually reaches the screen. In-game Restart / Retry does not reset it.
public class InterstitialManager : MonoBehaviour
{
    private static InterstitialManager instance;
    public static InterstitialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<InterstitialManager>();
            }

            return instance;
        }
    }

    // Every show site goes through these rather than through Instance directly. AdsManager only
    // adds this component once its config loads, so a missing AdConfig would otherwise take down
    // the button that called it — Play included.
    public static void ShowClickAd(Action onProceed)
    {
        if (Instance != null) Instance.TryShowClickAd(onProceed);
        else onProceed?.Invoke();
    }

    public static void ShowBackAd(Action onProceed)
    {
        if (Instance != null) Instance.TryShowBackAd(onProceed);
        else onProceed?.Invoke();
    }

    public static void ShowLevelAd(int completedLevel, Action onProceed)
    {
        if (Instance != null) Instance.TryShowLevelAd(completedLevel, onProceed);
        else onProceed?.Invoke();
    }

    private int globalClickCount;
    private float timer;
    private bool showInProgress;

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

    void Start()
    {
        InterstitialAdUnit unit = Unit();
        if (unit != null)
        {
            // Reset on the ad REACHING the screen, not on the decision to show it: an ad that
            // fails to display was never seen, and wiping the counter for it costs an impression.
            unit.OnOpened += ResetTracking;
        }
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
    }

    void OnDestroy()
    {
        InterstitialAdUnit unit = Unit();
        if (unit != null)
        {
            unit.OnOpened -= ResetTracking;
        }

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

    public void TryShowClickAd(Action onProceed)
    {
        // Swallowed, not passed through: the loading modal leaves the screen underneath tappable
        // for a moment, and running onProceed here would navigate behind the ad that is about to
        // appear — then navigate a second time when it closes.
        if (showInProgress)
        {
            Debug.Log("[InterstitialManager] Click ignored — an interstitial is already in progress.");
            return;
        }

        AdsManager ads = AdsManager.Instance;
        if (ads == null || ads.Settings == null
            || !ads.Settings.InterstitialEnabled || ads.Settings.AdClickCount <= 0)
        {
            onProceed?.Invoke();
            return;
        }

        globalClickCount++;

        int requiredClicks = ads.Settings.AdClickCount;
        float requiredTime = ads.Settings.AdClickTimeSec;
        string state = "clicks=" + globalClickCount + "/" + requiredClicks
                       + ", timer=" + timer.ToString("F1") + "s/" + requiredTime + "s";

        // Both gates, every time. Once the click threshold is passed the counter keeps climbing,
        // so each further click re-checks the clock until one of them finally lands an ad.
        if (globalClickCount >= requiredClicks && timer >= requiredTime)
        {
            Present(ads, onProceed, "Click ad. " + state);
            return;
        }

        Debug.Log("[InterstitialManager] Click ad skipped (" + state + ").");
        onProceed?.Invoke();
    }

    public void TryShowBackAd(Action onProceed)
    {
        if (showInProgress)
        {
            Debug.Log("[InterstitialManager] Back ignored — an interstitial is already in progress.");
            return;
        }

        AdsManager ads = AdsManager.Instance;

        // back_ad_on off means the back button is not an interstitial site at all — it does not
        // even count towards the click threshold.
        if (ads != null && ads.Settings != null && ads.Settings.BackAdEnabled)
        {
            TryShowClickAd(onProceed);
            return;
        }

        onProceed?.Invoke();
    }

    // Win-panel NEXT only. Runs off ad_level_count alone, with no click or time gate.
    public void TryShowLevelAd(int completedLevel, Action onProceed)
    {
        if (showInProgress)
        {
            Debug.Log("[InterstitialManager] NEXT ignored — an interstitial is already in progress.");
            return;
        }

        AdsManager ads = AdsManager.Instance;
        if (ads == null || ads.Settings == null || !ads.Settings.InterstitialEnabled)
        {
            onProceed?.Invoke();
            return;
        }

        int interval = ads.Settings.AdLevelCount;
        if (interval <= 0 || completedLevel <= 0 || (completedLevel % interval) != 0)
        {
            onProceed?.Invoke();
            return;
        }

        Present(ads, onProceed, "Level ad. level=" + completedLevel + " interval=" + interval);
    }

    void Present(AdsManager ads, Action onProceed, string reason)
    {
        InterstitialAdUnit unit = ads.Interstitial;
        if (unit == null)
        {
            onProceed?.Invoke();
            return;
        }

        Debug.Log("[InterstitialManager] " + reason);
        showInProgress = true;

        unit.ShowNow(
            onFinished: () =>
            {
                showInProgress = false;
                onProceed?.Invoke();
            },
            onLoadingStart: AdLoadingOverlay.Show,
            onLoadingEnd: AdLoadingOverlay.Hide);
    }

    static InterstitialAdUnit Unit()
    {
        return AdsManager.Instance != null ? AdsManager.Instance.Interstitial : null;
    }
}
