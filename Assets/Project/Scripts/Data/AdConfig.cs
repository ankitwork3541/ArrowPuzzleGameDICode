using UnityEngine;

// Baked fallback ad settings, shipped inside the build. The backend config overrides these at
// runtime, but an offline or first-launch player never gets one — so these have to be usable
// values, not placeholders.
[CreateAssetMenu(fileName = "AdConfig", menuName = "ArrowPuzzle/Ad Config")]
public class AdConfig : ScriptableObject
{
    [Header("Development")]
    [Tooltip("Forces Google's test ad units, ignoring both the IDs below and the backend.")]
    public bool useTestIds = true;

    [Header("Ad unit IDs (Android)")]
    public string banner;
    public string interstitial;
    public string rewarded;
    public string nativeAd;
    public string preloadNativeAd;

    [Header("Rules")]
    public bool adsEnabled = true;
    public bool rewardedEnabled = true;
    public bool backAdEnabled = false;

    [Tooltip("Show an interstitial after this many completed levels.")]
    public int levelsPerInterstitial = 2;

    [Tooltip("Minimum gap between two interstitials.")]
    public int interstitialCooldownSeconds = 30;

    public int nativeRefreshSeconds = 10;

    [Header("Player-friendly floor")]
    [Tooltip("No ads at all before this level number. 0 disables the grace period.")]
    public int minLevelForAds = 3;
}
