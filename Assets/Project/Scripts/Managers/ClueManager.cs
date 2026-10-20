using System;
using UnityEngine;

// Free-hint pool plus on-demand rewarded ads. Remaining count follows the backend's
// free_hint_count: same number keeps the leftover pool; a changed number resets the pool.
public class ClueManager : MonoBehaviour
{
    // Current keys. Values stored under the two earlier generations of names are copied across once
    // in Awake (KeyMigration), so existing players keep their hint pool.
    private const string PrefsRemainingHints = "lanterndragons.hint.remaining";
    private const string PrefsLastFreeHintCount = "lanterndragons.hint.lastFreeCount";
    private const int UnsetCount = int.MinValue;

    private static ClueManager instance;
    public static ClueManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<ClueManager>();
            }

            return instance;
        }
    }

    private bool requestInFlight;

    public int RemainingHints { get; private set; }
    public bool HasFreeHints => RemainingHints > 0;

    public bool IsRewardedAvailable =>
        PromoManager.Instance != null
        && PromoManager.Instance.Ready
        && PromoManager.Instance.Settings != null
        && PromoManager.Instance.Settings.RewardedEnabled;

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

        KeyMigration.CopyIntIfMissing(PrefsRemainingHints, "arrowqube.hint.remaining", "HintManager_RemainingHints");
        KeyMigration.CopyIntIfMissing(PrefsLastFreeHintCount, "arrowqube.hint.lastFreeCount", "HintManager_LastFreeHintCount");
        SyncFromSettings();
    }

    void Start()
    {
        if (PromoManager.Instance == null)
        {
            return;
        }

        PromoManager.Instance.OnSettingsChanged += SyncFromSettings;
        PromoManager.Instance.OnReady += NotifyState;
        SyncFromSettings();
    }

    void OnDestroy()
    {
        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.OnSettingsChanged -= SyncFromSettings;
            PromoManager.Instance.OnReady -= NotifyState;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    void SyncFromSettings()
    {
        int apiCount = PromoManager.Instance != null && PromoManager.Instance.Settings != null
            ? PromoManager.Instance.Settings.FreeHintCount
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
            Debug.Log("[ClueManager] Free hint pool " + RemainingHints
                      + " (baseline free_hint_count=" + apiCount + ").");
        }
        else if (lastApplied != apiCount)
        {
            RemainingHints = apiCount;
            PlayerPrefs.SetInt(PrefsLastFreeHintCount, apiCount);
            PlayerPrefs.SetInt(PrefsRemainingHints, RemainingHints);
            PlayerPrefs.Save();
            Debug.Log("[ClueManager] free_hint_count changed " + lastApplied + " -> " + apiCount
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
            Debug.Log("[ClueManager] Used 1 free hint. Remaining: " + RemainingHints);
            onHintUnlocked?.Invoke();
            return;
        }

        if (!IsRewardedAvailable)
        {
            Debug.LogWarning("[ClueManager] Cannot use hint: no free hints and rewarded ads off.");
            onFailed?.Invoke();
            return;
        }

        if (PromoManager.Instance == null || PromoManager.Instance.Rewarded == null)
        {
            Debug.LogWarning("[ClueManager] PromoManager or PromoRewardedUnit is missing.");
            onFailed?.Invoke();
            return;
        }

        requestInFlight = true;
        onAdLoadingStart?.Invoke();

        bool started = PromoManager.Instance.Rewarded.LoadAndShow(
            onCompletedWithReward: () =>
            {
                requestInFlight = false;
                Debug.Log("[ClueManager] Rewarded ad completed. Unlocking hint.");
                onHintUnlocked?.Invoke();
            },
            onClosedOrFailed: () =>
            {
                requestInFlight = false;
                Debug.LogWarning("[ClueManager] Ad failed, skipped, or not filled. Hint not unlocked.");
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
