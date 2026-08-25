using System;
using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

// Keeps one interstitial loaded and ready, and fetches one on demand when a show site is reached
// with nothing in hand. Deliberately says nothing about WHEN an interstitial should appear — that
// policy belongs to InterstitialManager.
public class InterstitialAdUnit : MonoBehaviour
{
    // A failed request is retried on a doubling ladder. Without it a single no-fill at launch
    // leaves the whole session ad-less: nothing else in the game reloads on its own.
    private const int MinBackoffSeconds = 2;
    private const int MaxBackoffSeconds = 64;

    // How long the player is made to watch the loading modal before the ad is given up on. Short
    // on purpose — past a few seconds a wait reads as a frozen game, not as a loading ad.
    private const float OnDemandTimeoutSeconds = 5f;

    // Guards against a load callback that never arrives, which would otherwise leave `loading`
    // stuck true and block every future request.
    private const float LoadWatchdogSeconds = 30f;

    private InterstitialAd ad;
    private bool loading;
    private int failureStreak;

    private Coroutine retryRoutine;
    private Coroutine timeoutRoutine;
    private Coroutine watchdogRoutine;

    // Set while a ShowNow() call waits for an in-flight load.
    private Action waitingFinished;
    private Action waitingLoadingEnd;

    // Set while an ad is actually on screen.
    private Action presentingFinished;

    public bool IsReady => ad != null && ad.CanShowAd();
    public bool IsLoading => loading;

    // Raised when the ad reaches the screen, and again once it goes away — whether it closed
    // normally or failed to display. Click-tracking reset and audio pause/resume hang off these.
    public event Action OnOpened;
    public event Action OnClosed;

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

        StopRoutine(ref retryRoutine);
        StopRoutine(ref timeoutRoutine);
        StopRoutine(ref watchdogRoutine);
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
            Debug.Log("[Interstitial] Skipped — ads disabled by config.");
            return;
        }

        string unitId = manager.Settings.InterstitialId;
        if (string.IsNullOrEmpty(unitId))
        {
            Debug.LogWarning("[Interstitial] No unit ID available — nothing to load.");
            return;
        }

        StopRoutine(ref retryRoutine);
        StopRoutine(ref watchdogRoutine);
        loading = true;
        watchdogRoutine = StartCoroutine(LoadWatchdog());

        Debug.Log("[Interstitial] Requesting — unit " + unitId + ".");

        InterstitialAd.Load(unitId, new AdRequest(), (InterstitialAd loaded, LoadAdError error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (!loading)
                {
                    // The watchdog already gave up on this request. Whatever arrived now is a
                    // stray, and keeping it would leave two ads alive at once.
                    if (loaded != null) loaded.Destroy();
                    return;
                }

                StopRoutine(ref watchdogRoutine);
                loading = false;

                if (error != null || loaded == null)
                {
                    Debug.LogWarning("[Interstitial] Load failed: "
                                     + (error != null ? error.GetMessage() : "no ad returned"));
                    ScheduleRetry();
                    ReleaseWaiter(false);
                    return;
                }

                failureStreak = 0;
                ad = loaded;
                RegisterEvents(ad);
                Debug.Log("[Interstitial] Loaded.");
                ReleaseWaiter(true);
            });
        });
    }

    // Shows an interstitial and calls onFinished exactly once — after the ad closes, or straight
    // away when there is nothing to show. A preloaded ad goes up immediately; with none in hand one
    // is fetched on demand, and onLoadingStart / onLoadingEnd bracket that wait for the modal.
    public void ShowNow(Action onFinished, Action onLoadingStart = null, Action onLoadingEnd = null)
    {
        if (presentingFinished != null || waitingFinished != null)
        {
            Debug.LogWarning("[Interstitial] Request ignored — one is already in progress.");
            onFinished?.Invoke();
            return;
        }

        if (IsReady)
        {
            Present(onFinished);
            return;
        }

        Load();

        // Load() bails out silently when ads are off or no unit ID arrived. Nothing is coming, so
        // there is no point putting a loading modal in front of the player.
        if (!loading)
        {
            Debug.LogWarning("[Interstitial] No interstitial in hand and no load could be started. Skipping.");
            onFinished?.Invoke();
            return;
        }

        waitingFinished = onFinished;
        waitingLoadingEnd = onLoadingEnd;
        onLoadingStart?.Invoke();

        StopRoutine(ref timeoutRoutine);
        timeoutRoutine = StartCoroutine(OnDemandTimeout());
    }

    private IEnumerator OnDemandTimeout()
    {
        yield return new WaitForSecondsRealtime(OnDemandTimeoutSeconds);
        timeoutRoutine = null;

        Debug.LogWarning("[Interstitial] On-demand load timed out after "
                         + OnDemandTimeoutSeconds + "s. Skipping.");

        // The request itself is left running: if it lands late the ad is simply kept for next time.
        ReleaseWaiter(false);
    }

    private IEnumerator LoadWatchdog()
    {
        yield return new WaitForSecondsRealtime(LoadWatchdogSeconds);
        watchdogRoutine = null;

        if (!loading)
        {
            yield break;
        }

        Debug.LogWarning("[Interstitial] Load never answered. Releasing the request.");
        loading = false;
        ScheduleRetry();
        ReleaseWaiter(false);
    }

    // Hands the waiting caller its answer: present the freshly loaded ad, or let it carry on.
    private void ReleaseWaiter(bool adAvailable)
    {
        if (waitingFinished == null)
        {
            return;
        }

        StopRoutine(ref timeoutRoutine);

        Action finished = waitingFinished;
        Action loadingEnd = waitingLoadingEnd;
        waitingFinished = null;
        waitingLoadingEnd = null;

        loadingEnd?.Invoke();

        if (adAvailable && IsReady)
        {
            Present(finished);
            return;
        }

        finished?.Invoke();
    }

    private void Present(Action onFinished)
    {
        presentingFinished = onFinished;
        ad.Show();
    }

    private void RegisterEvents(InterstitialAd target)
    {
        target.OnAdPaid += value => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            Debug.Log("[Interstitial] Paid " + value.Value + " micro-" + value.CurrencyCode + "."));

        target.OnAdImpressionRecorded += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            Debug.Log("[Interstitial] Impression recorded."));

        target.OnAdClicked += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            Debug.Log("[Interstitial] Clicked."));

        target.OnAdFullScreenContentOpened += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            Debug.Log("[Interstitial] Opened.");
            OnOpened?.Invoke();
        });

        target.OnAdFullScreenContentClosed += () => Finish(null);
        target.OnAdFullScreenContentFailed += error => Finish(error);
    }

    void Finish(AdError error)
    {
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (error != null)
            {
                Debug.LogWarning("[Interstitial] Failed to show: " + error.GetMessage());
            }
            else
            {
                Debug.Log("[Interstitial] Closed by the player. Loading the next one.");
            }

            // An interstitial is a single-use object: once shown it can never be shown again, so
            // the replacement has to be fetched now rather than at the next show site.
            DestroyAd();

            Action finished = presentingFinished;
            presentingFinished = null;

            OnClosed?.Invoke();
            finished?.Invoke();
            Load();
        });
    }

    private void ScheduleRetry()
    {
        if (retryRoutine != null)
        {
            return;
        }

        int delay = Mathf.Min(MinBackoffSeconds << failureStreak, MaxBackoffSeconds);
        failureStreak = Mathf.Min(failureStreak + 1, 8);

        Debug.Log("[Interstitial] Retry in " + delay + "s.");
        retryRoutine = StartCoroutine(RetryAfter(delay));
    }

    private IEnumerator RetryAfter(int delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        retryRoutine = null;
        Load();
    }

    private void StopRoutine(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
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
