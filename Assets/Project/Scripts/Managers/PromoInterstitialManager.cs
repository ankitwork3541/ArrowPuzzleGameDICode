using System;
using UnityEngine;

// Global interstitial policy. The ad object itself is owned by PromoInterstitialUnit; this class only
// decides WHEN one may be shown.
//
// Tracking (click count + session timer) resets when the process starts (app kill / fresh launch)
// and when an interstitial actually reaches the screen. In-game Restart / Retry does not reset it.
public class PromoInterstitialManager : MonoBehaviour
{
    private static PromoInterstitialManager instance;
    public static PromoInterstitialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<PromoInterstitialManager>();
            }

            return instance;
        }
    }

    // Every show site goes through these rather than through Instance directly. PromoManager only
    // adds this component once its config loads, so a missing PromoConfig would otherwise take down
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
    private int shownThisSession;

    public int ShownThisSession => shownThisSession;
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
        PromoInterstitialUnit unit = Unit();
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

        // Counted for the whole process, not reset with the click tracking: max_per_session is a cap
        // on interstitials the player actually saw.
        shownThisSession++;
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
        PromoInterstitialUnit unit = Unit();
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
        Debug.Log("[PromoInterstitialManager] Tracking reset (clicks=0, timer=0s).");
    }

    public void TryShowClickAd(Action onProceed)
    {
        // Swallowed, not passed through: the loading modal leaves the screen underneath tappable
        // for a moment, and running onProceed here would navigate behind the ad that is about to
        // appear — then navigate a second time when it closes.
        if (showInProgress)
        {
            Debug.Log("[PromoInterstitialManager] Click ignored — an interstitial is already in progress.");
            return;
        }

        PromoManager ads = PromoManager.Instance;
        if (ads == null || ads.Settings == null || !ads.Settings.InterstitialEnabled)
        {
            onProceed?.Invoke();
            return;
        }

        InterstitialRules rules = ads.Settings.Interstitials;
        int requiredClicks = rules.min_click_count;
        int requiredTime = rules.min_seconds_between;

        // Too early in the campaign, or this session's cap is used up: no interstitial, and not even a
        // counter worth keeping.
        if (!AdRules.InterstitialsUnlocked(rules, GameConfigService.HighestLevelCompleted)
            || AdRules.SessionCapReached(rules, shownThisSession))
        {
            onProceed?.Invoke();
            return;
        }

        // Both triggers off means there is no click-based interstitial at all — not even a counter
        // worth keeping.
        if (AdRules.ClickAdsOff(rules))
        {
            onProceed?.Invoke();
            return;
        }

        globalClickCount++;

        // EITHER gate is enough. A 0 from the config switches that one trigger off rather than
        // satisfying it instantly — otherwise min_seconds_between of 0 would put an ad on every
        // single button in the game.
        bool timeMet = AdRules.TimeGateMet(rules, timer);
        bool countMet = AdRules.CountGateMet(rules, globalClickCount);

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

        Debug.Log("[PromoInterstitialManager] Click ad skipped (" + state + ").");
        onProceed?.Invoke();
    }

    public void TryShowBackAd(Action onProceed)
    {
        if (showInProgress)
        {
            Debug.Log("[PromoInterstitialManager] Back ignored — an interstitial is already in progress.");
            return;
        }

        PromoManager ads = PromoManager.Instance;

        // interstitial_rules.on_back_to_home off means the back button is not an interstitial site at
        // all — it does not even count towards the click threshold.
        if (ads != null && ads.Settings != null && ads.Settings.BackAdEnabled)
        {
            TryShowClickAd(onProceed);
            return;
        }

        onProceed?.Invoke();
    }

    // Win-panel NEXT only. Runs off every_n_levels_on_next alone, with no click or time gate (the
    // skip_first_levels and max_per_session rules still apply).
    public void TryShowLevelAd(int completedLevel, Action onProceed)
    {
        if (showInProgress)
        {
            Debug.Log("[PromoInterstitialManager] NEXT ignored — an interstitial is already in progress.");
            return;
        }

        PromoManager ads = PromoManager.Instance;
        if (ads == null || ads.Settings == null || !ads.Settings.InterstitialEnabled)
        {
            onProceed?.Invoke();
            return;
        }

        InterstitialRules rules = ads.Settings.Interstitials;
        int interval = rules.every_n_levels_on_next;
        if (!AdRules.LevelAdDue(rules, completedLevel)
            || !AdRules.InterstitialsUnlocked(rules, GameConfigService.HighestLevelCompleted)
            || AdRules.SessionCapReached(rules, shownThisSession))
        {
            onProceed?.Invoke();
            return;
        }

        Present(ads, onProceed, "Level ad. level=" + completedLevel + " interval=" + interval);
    }

    void Present(PromoManager ads, Action onProceed, string reason)
    {
        PromoInterstitialUnit unit = ads.Interstitial;
        if (unit == null)
        {
            onProceed?.Invoke();
            return;
        }

        Debug.Log("[PromoInterstitialManager] " + reason);
        showInProgress = true;

        unit.ShowNow(
            onFinished: () =>
            {
                showInProgress = false;
                onProceed?.Invoke();
            },
            onLoadingStart: WaitSheet.Show,
            onLoadingEnd: WaitSheet.Hide);
    }

    static PromoInterstitialUnit Unit()
    {
        return PromoManager.Instance != null ? PromoManager.Instance.Interstitial : null;
    }
}
