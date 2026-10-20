using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

// On-demand rewarded ads (Unity RewardedAd.Load / Show). Load starts when the player asks —
// there is no preload cache, so the HUD can show a loading state. The reward is delivered
// only after the ad closes AND Google reported that the user earned it.
public class PromoRewardedUnit : MonoBehaviour
{
    private const float LoadTimeoutSeconds = 15f;

    private RewardedAd rewardedAd;
    private bool isLoading;
    private Coroutine timeoutRoutine;

    public bool IsBusy => isLoading || rewardedAd != null;
    public bool IsLoading => isLoading;
    public bool IsReady => rewardedAd != null && rewardedAd.CanShowAd();

    public int RequestCount { get; private set; }
    public int FillCount { get; private set; }
    public int ImpressionCount { get; private set; }
    public int RewardEarnedCount { get; private set; }

    void OnDestroy()
    {
        StopTimeout();
        DestroyAd();
    }

    // True when this call owns the request (success and failure arrive via the callbacks).
    public bool LoadAndShow(Action onCompletedWithReward, Action onClosedOrFailed, Action onOpening = null)
    {
        if (IsBusy)
        {
            Debug.LogWarning("[RewardedAd] Request ignored — a load or presentation is already in progress.");
            return false;
        }

        PromoManager manager = PromoManager.Instance;
        if (manager == null || !manager.Ready)
        {
            Debug.LogWarning("[RewardedAd] Cannot show ad — PromoManager not ready.");
            onClosedOrFailed?.Invoke();
            return true;
        }

        if (!manager.Settings.RewardedEnabled)
        {
            Debug.LogWarning("[RewardedAd] Rewarded ads disabled by config.");
            onClosedOrFailed?.Invoke();
            return true;
        }

        string unitId = manager.Settings.RewardedId;
        if (string.IsNullOrEmpty(unitId))
        {
            Debug.LogWarning("[RewardedAd] No rewarded unit ID configured.");
            onClosedOrFailed?.Invoke();
            return true;
        }

        isLoading = true;
        RequestCount++;

        bool rewardEarned = false;
        bool hasFinished = false;

        void FinishFailure(string reason)
        {
            if (hasFinished)
            {
                return;
            }

            hasFinished = true;
            isLoading = false;
            StopTimeout();
            DestroyAd();
            Debug.LogWarning("[RewardedAd] Failed: " + reason);
            onClosedOrFailed?.Invoke();
        }

        void FinishSuccess()
        {
            if (hasFinished)
            {
                return;
            }

            hasFinished = true;
            isLoading = false;
            StopTimeout();
            DestroyAd();
            Debug.Log("[RewardedAd] Closed after reward earned.");
            onCompletedWithReward?.Invoke();
        }

        StopTimeout();
        timeoutRoutine = StartCoroutine(TimeoutRoutine(() =>
        {
            if (isLoading)
            {
                FinishFailure("Load timed out.");
            }
        }));

        Debug.Log("[RewardedAd] Requesting — unit " + unitId + ".");

        RewardedAd.Load(unitId, new AdRequest(), (RewardedAd loadedAd, LoadAdError error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (hasFinished)
                {
                    if (loadedAd != null)
                    {
                        loadedAd.Destroy();
                    }

                    return;
                }

                StopTimeout();
                isLoading = false;

                if (error != null || loadedAd == null)
                {
                    FinishFailure(error != null ? error.GetMessage() : "null ad returned");
                    return;
                }

                if (!loadedAd.CanShowAd())
                {
                    loadedAd.Destroy();
                    FinishFailure("Loaded ad cannot be shown.");
                    return;
                }

                FillCount++;
                rewardedAd = loadedAd;
                Debug.Log("[RewardedAd] Loaded. Presenting now.");

                rewardedAd.OnAdFullScreenContentOpened += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    ImpressionCount++;
                    Debug.Log("[RewardedAd] Full screen content opened.");
                });

                rewardedAd.OnAdFullScreenContentFailed += (AdError showErr) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    FinishFailure(showErr != null ? showErr.GetMessage() : "Show error");
                });

                rewardedAd.OnAdFullScreenContentClosed += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    if (rewardEarned)
                    {
                        FinishSuccess();
                    }
                    else
                    {
                        FinishFailure("Closed without earning reward.");
                    }
                });

                onOpening?.Invoke();

                rewardedAd.Show((Reward reward) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    rewardEarned = true;
                    RewardEarnedCount++;
                    Debug.Log("[RewardedAd] Reward earned: " + reward.Amount + " " + reward.Type);
                }));
            });

            loadedAd.OnAdPaid += (AdValue adValue) =>
            {
                HostBridge.SendDataToAndroid(adValue.Value,adValue.CurrencyCode,"Rewarded");
            };
        });

        return true;
    }

    private System.Collections.IEnumerator TimeoutRoutine(Action onTimeout)
    {
        yield return new WaitForSecondsRealtime(LoadTimeoutSeconds);
        timeoutRoutine = null;
        onTimeout?.Invoke();
    }

    private void StopTimeout()
    {
        if (timeoutRoutine != null)
        {
            StopCoroutine(timeoutRoutine);
            timeoutRoutine = null;
        }
    }

    public void DestroyAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }
}
