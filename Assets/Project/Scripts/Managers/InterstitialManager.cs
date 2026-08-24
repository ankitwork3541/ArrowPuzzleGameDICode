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
    private bool adWasSeen;

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
            unit.OnOpened += OnAdOpened;
            unit.OnClosed += OnAdClosed;
        }
    }

    // The cycle restarts when the player gets the game back, not when the ad appears — otherwise
    // however long they spent watching would come out of the next cooldown.
    void OnAdOpened()
    {
        adWasSeen = true;
    }

    // Guarded on adWasSeen because OnClosed also fires for an ad that failed to display. Resetting
    // for one of those would cost an impression the player never got.
    void OnAdClosed()
    {
        if (!adWasSeen) return;

        adWasSeen = false;
        ResetTracking();
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
            unit.OnOpened -= OnAdOpened;
            unit.OnClosed -= OnAdClosed;
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
        if (ads == null || ads.Settings == null || !ads.Settings.InterstitialEnabled)
        {
            onProceed?.Invoke();
            return;
        }

        int requiredClicks = ads.Settings.AdClickCount;
        int requiredTime = ads.Settings.AdClickTimeSec;

        // Both triggers off means there is no click-based interstitial at all — not even a counter
        // worth keeping.
        if (requiredClicks <= 0 && requiredTime <= 0)
        {
            onProceed?.Invoke();
            return;
        }

        globalClickCount++;

        // EITHER gate is enough. A 0 from the backend switches that one trigger off rather than
        // satisfying it instantly — otherwise ad_click_time_sec of 0 would put an ad on every
        // single button in the game.
        bool timeMet = requiredTime > 0 && timer >= requiredTime;
        bool countMet = requiredClicks > 0 && globalClickCount >= requiredClicks;

        string state = "clicks=" + globalClickCount + "/" + requiredClicks
                       + ", timer=" + timer.ToString("F1") + "s/" + requiredTime + "s";

        // Time first, as the spec's flow chart has it: a finished timer shows an ad on its own and
        // never looks at the counter. The counter is what covers a player who is racing through
        // faster than the clock.
        if (timeMet || countMet)
        {
            Present(ads, onProceed, "Click ad by " + (timeMet ? "TIME" : "COUNT") + ". " + state);
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
