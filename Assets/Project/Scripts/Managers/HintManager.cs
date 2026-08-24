using System;
using UnityEngine;

// Free-hint pool plus on-demand rewarded ads. Remaining count follows the backend's
// free_hint_count: same number keeps the leftover pool; a changed number resets the pool.
public class HintManager : MonoBehaviour
{
    private const string PrefsRemainingHints = "HintManager_RemainingHints";
    private const string PrefsLastFreeHintCount = "HintManager_LastFreeHintCount";
    private const int UnsetCount = int.MinValue;

    private static HintManager instance;
    public static HintManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<HintManager>();
            }

            return instance;
        }
    }

    private bool requestInFlight;

    public int RemainingHints { get; private set; }
    public bool HasFreeHints => RemainingHints > 0;

    public bool IsRewardedAvailable =>
        AdsManager.Instance != null
        && AdsManager.Instance.Ready
        && AdsManager.Instance.Settings != null
        && AdsManager.Instance.Settings.RewardedEnabled;

    public bool CanUseHint => HasFreeHints || IsRewardedAvailable;

    public event Action<int, bool> OnHintStateChanged;

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

        SyncFromSettings();
    }

    void Start()
    {
        if (AdsManager.Instance == null)
        {
            return;
        }

        AdsManager.Instance.OnSettingsChanged += SyncFromSettings;
        AdsManager.Instance.OnReady += NotifyState;
        SyncFromSettings();
    }

    void OnDestroy()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.OnSettingsChanged -= SyncFromSettings;
            AdsManager.Instance.OnReady -= NotifyState;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    void SyncFromSettings()
    {
        int apiCount = AdsManager.Instance != null && AdsManager.Instance.Settings != null
            ? AdsManager.Instance.Settings.FreeHintCount
            : 3;

        int lastApplied = PlayerPrefs.GetInt(PrefsLastFreeHintCount, UnsetCount);
        bool hasLastKey = PlayerPrefs.HasKey(PrefsLastFreeHintCount);

        if (!hasLastKey)
        {
            // First run of this logic: keep any leftover pool, pin it to today's API count.
            RemainingHints = PlayerPrefs.HasKey(PrefsRemainingHints)
                ? PlayerPrefs.GetInt(PrefsRemainingHints, apiCount)
                : apiCount;
            PlayerPrefs.SetInt(PrefsLastFreeHintCount, apiCount);
            PlayerPrefs.SetInt(PrefsRemainingHints, RemainingHints);
            PlayerPrefs.DeleteKey("HintManager_Initialized");
            PlayerPrefs.Save();
            Debug.Log("[HintManager] Free hint pool " + RemainingHints
                      + " (baseline free_hint_count=" + apiCount + ").");
        }
        else if (lastApplied != apiCount)
        {
            RemainingHints = apiCount;
            PlayerPrefs.SetInt(PrefsLastFreeHintCount, apiCount);
            PlayerPrefs.SetInt(PrefsRemainingHints, RemainingHints);
            PlayerPrefs.Save();
            Debug.Log("[HintManager] free_hint_count changed " + lastApplied + " -> " + apiCount
                      + ". Pool reset to " + RemainingHints + ".");
        }
        else
        {
            RemainingHints = PlayerPrefs.GetInt(PrefsRemainingHints, apiCount);
        }

        NotifyState();
    }

    void SaveHints()
    {
        PlayerPrefs.SetInt(PrefsRemainingHints, RemainingHints);
        PlayerPrefs.Save();
        NotifyState();
    }

    public void NotifyState()
    {
        bool adRequired = !HasFreeHints && IsRewardedAvailable;
        OnHintStateChanged?.Invoke(RemainingHints, adRequired);
    }

    public void RequestHint(Action onHintUnlocked, Action onFailed, Action onAdLoadingStart = null, Action onAdOpening = null)
    {
        if (requestInFlight)
        {
            return;
        }

        if (HasFreeHints)
        {
            RemainingHints = Mathf.Max(0, RemainingHints - 1);
            SaveHints();
            Debug.Log("[HintManager] Used 1 free hint. Remaining: " + RemainingHints);
            onHintUnlocked?.Invoke();
            return;
        }

        if (!IsRewardedAvailable)
        {
            Debug.LogWarning("[HintManager] Cannot use hint: no free hints and rewarded ads off.");
            onFailed?.Invoke();
            return;
        }

        if (AdsManager.Instance == null || AdsManager.Instance.Rewarded == null)
        {
            Debug.LogWarning("[HintManager] AdsManager or RewardedAdUnit is missing.");
            onFailed?.Invoke();
            return;
        }

        requestInFlight = true;
        onAdLoadingStart?.Invoke();

        bool started = AdsManager.Instance.Rewarded.LoadAndShow(
            onCompletedWithReward: () =>
            {
                requestInFlight = false;
                Debug.Log("[HintManager] Rewarded ad completed. Unlocking hint.");
                onHintUnlocked?.Invoke();
            },
            onClosedOrFailed: () =>
            {
                requestInFlight = false;
                Debug.LogWarning("[HintManager] Ad failed, skipped, or not filled. Hint not unlocked.");
                onFailed?.Invoke();
            },
            onOpening: () =>
            {
                onAdOpening?.Invoke();
            }
        );

        if (!started)
        {
            requestInFlight = false;
            onFailed?.Invoke();
        }
    }
}
