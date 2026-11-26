using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using GoogleMobileAds.Api;
using UnityEngine;

// Google Analytics for Firebase. Its job is paid-install reporting: Google Ads imports these events
// as conversions, so a campaign can bid for players who keep playing (and earn ad revenue) instead of
// for the cheapest install. first_open, session_start and screen time are logged by the SDK itself.
//
// Consent: Firebase is started before Google's UMP consent flow (PromoManager.Start). UMP then hands
// the player's GDPR answer to Analytics as consent mode, so nothing here calls SetConsent.
//
// Every call is safe before Firebase is ready: events wait in a short queue and are sent once it is,
// or dropped if Firebase cannot start on this device.
public static class PlayerAnalytics
{
    enum State { NotStarted, Starting, Ready, Unavailable }

    // Enough for anything a player can do during the start-up second; a device that never gets
    // Firebase must not grow this forever.
    const int MaxPending = 32;

    static State state = State.NotStarted;
    static readonly List<Action> pending = new List<Action>();

    /// <summary>True once Firebase has started, or has failed to (so callers can stop waiting).</summary>
    public static bool IsSettled => state == State.Ready || state == State.Unavailable;

    static AnalyticsSection Config => GameConfigService.Current?.analytics ?? new AnalyticsSection();

    /// <summary>Starts Firebase. Call once, before the consent flow. Later calls do nothing.</summary>
    public static void Initialize()
    {
        if (state != State.NotStarted)
        {
            return;
        }

        state = State.Starting;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled || task.Result != DependencyStatus.Available)
            {
                string why = task.IsFaulted ? task.Exception?.GetBaseException().Message : task.Result.ToString();
                Debug.LogWarning("[Analytics] Firebase unavailable: " + why + ". Events are not sent.");
                state = State.Unavailable;
                pending.Clear();
                return;
            }

            bool collect = Config.enabled;
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(collect);
            state = State.Ready;
            Debug.Log("[Analytics] Firebase ready. collection=" + collect + ", queued events=" + pending.Count);

            foreach (Action send in pending)
            {
                send();
            }

            pending.Clear();
        });
    }

    /// <summary>A level was opened (PLAY, NEXT, RETRY, Restart or a map tap).</summary>
    public static void LevelStart(int level)
    {
        Send(FirebaseAnalytics.EventLevelStart,
            new Parameter(FirebaseAnalytics.ParameterLevel, level),
            new Parameter(FirebaseAnalytics.ParameterLevelName, LevelName(level)));
    }

    /// <summary>A level ended: success true for a win, false for running out of hearts.</summary>
    public static void LevelEnd(int level, bool success)
    {
        Send(FirebaseAnalytics.EventLevelEnd,
            new Parameter(FirebaseAnalytics.ParameterLevel, level),
            new Parameter(FirebaseAnalytics.ParameterLevelName, LevelName(level)),
            new Parameter(FirebaseAnalytics.ParameterSuccess, success ? 1L : 0L));
    }

    /// <summary>
    /// A level beaten for the first time. Logs level_up, plus one event per milestone level
    /// (level_5_complete, level_10_complete, ...). Google Ads imports an event by name only, so each
    /// milestone needs its own name to be usable as a conversion.
    /// </summary>
    public static void FirstCompletion(int level)
    {
        Send(FirebaseAnalytics.EventLevelUp, new Parameter(FirebaseAnalytics.ParameterLevel, level));

        if (Config.milestone_levels != null && Config.milestone_levels.Contains(level))
        {
            Send(MilestoneEventName(level), new Parameter(FirebaseAnalytics.ParameterLevel, level));
        }
    }

    /// <summary>A hint was used; rewarded is true when the player watched an ad for it.</summary>
    public static void HintUsed(bool rewarded)
    {
        Send("hint_used", new Parameter("source", rewarded ? "rewarded" : "free"));
    }

    /// <summary>
    /// One paid ad impression (the ads SDK's OnAdPaid), logged as ad_impression. Off unless
    /// analytics.log_ad_revenue is on: the GMA Next-Gen SDK already logs ad_impression to Firebase by
    /// itself, and AdMob attaches the revenue once it is linked, so this would count it twice.
    /// </summary>
    public static void AdRevenue(string format, string adUnitId, AdValue value)
    {
        if (value == null || !Config.log_ad_revenue)
        {
            return;
        }

        Send(FirebaseAnalytics.EventAdImpression,
            new Parameter(FirebaseAnalytics.ParameterAdPlatform, "AdMob"),
            new Parameter(FirebaseAnalytics.ParameterAdSource, "AdMob"),
            new Parameter(FirebaseAnalytics.ParameterAdFormat, format),
            new Parameter(FirebaseAnalytics.ParameterAdUnitName, adUnitId ?? string.Empty),
            new Parameter(FirebaseAnalytics.ParameterValue, MicrosToValue(value.Value)),
            new Parameter(FirebaseAnalytics.ParameterCurrency,
                string.IsNullOrEmpty(value.CurrencyCode) ? "USD" : value.CurrencyCode));
    }

    /// <summary>The event name for a milestone level, e.g. 10 becomes level_10_complete.</summary>
    public static string MilestoneEventName(int level)
    {
        return "level_" + level + "_complete";
    }

    /// <summary>The ads SDK reports value in micros of the currency (1,000,000 = 1 unit).</summary>
    public static double MicrosToValue(long micros)
    {
        return micros / 1000000d;
    }

    static string LevelName(int level)
    {
        return "level_" + level;
    }

    static void Send(string name, params Parameter[] parameters)
    {
        void LogNow()
        {
            if (!Config.enabled)
            {
                return;
            }

            FirebaseAnalytics.LogEvent(name, parameters);
        }

        switch (state)
        {
            case State.Ready:
                LogNow();
                break;
            case State.Unavailable:
                break;
            default:
                if (pending.Count < MaxPending)
                {
                    pending.Add(LogNow);
                }

                break;
        }
    }
}
