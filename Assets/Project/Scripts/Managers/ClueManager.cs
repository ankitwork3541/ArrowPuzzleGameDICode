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

    // The rewarded format has to be on AND the config has to let hints use it (rewarded_rules.hint).
    public bool IsRewardedAvailable =>
        PromoManager.Instance != null
        && PromoManager.Instance.Ready
        && PromoManager.Instance.Settings != null
        && PromoManager.Instance.Settings.HintRewardedEnabled;

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
        // The config can change at any time (a fresh payload after start-up), so the pool follows it
        // whether or not the ad stack exists.
        GameConfigService.Changed += SyncFromSettings;

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.OnReady += NotifyState;
        }

        SyncFromSettings();
    }

    void OnDestroy()
    {
        GameConfigService.Changed -= SyncFromSettings;

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.OnReady -= NotifyState;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    // What the free pool should be after a config sync. Pure, so both directions of
    // game.hints.reset_on_change are unit-tested.
    //   hasLastKey false  -> first run: keep any leftover pool, pin it to today's free_count.
    //   count unchanged   -> keep the leftover pool.
    //   count changed     -> the pool resets to the new count, but only when reset_on_change is true.
    public static int ResolvePool(bool hasLastKey, int lastApplied, bool hasStoredRemaining,
                                  int storedRemaining, int freeCount, bool resetOnChange,
                                  out int newLastApplied)
    {
        newLastApplied = freeCount;

        if (!hasLastKey)
        {
            return hasStoredRemaining ? storedRemaining : freeCount;
        }

        if (lastApplied != freeCount && resetOnChange)
        {
            return freeCount;
        }

        return hasStoredRemaining ? storedRemaining : freeCount;
    }

    void SyncFromSettings()
    {
        HintsSection hints = GameConfigService.Current.game.hints;
        int apiCount = hints.free_count;

        int lastApplied = PlayerPrefs.GetInt(PrefsLastFreeHintCount, UnsetCount);
        bool hasLastKey = PlayerPrefs.HasKey(PrefsLastFreeHintCount);
        bool hasRemaining = PlayerPrefs.HasKey(PrefsRemainingHints);

        RemainingHints = ResolvePool(hasLastKey, lastApplied, hasRemaining,
                                     PlayerPrefs.GetInt(PrefsRemainingHints, apiCount),
                                     apiCount, hints.reset_on_change, out int newLast);

        if (!hasLastKey || lastApplied != apiCount || !hasRemaining)
        {
            PlayerPrefs.SetInt(PrefsLastFreeHintCount, newLast);
            PlayerPrefs.SetInt(PrefsRemainingHints, RemainingHints);
            PlayerPrefs.DeleteKey("HintManager_Initialized");
            PlayerPrefs.Save();
            Debug.Log("[ClueManager] Free hint pool " + RemainingHints + " (game.hints.free_count="
                      + apiCount + ", last " + (hasLastKey ? lastApplied.ToString() : "none")
                      + ", reset_on_change=" + hints.reset_on_change + ").");
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
