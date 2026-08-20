using System.Collections.Generic;
using UnityEngine;

// Which of the plugin's two native templates a backend style digit maps to. The plugin ships only
// Small and Medium (NativeTemplateId), so several digits necessarily share a layout.
public enum NativeTemplateSize
{
    Small,
    Medium
}

[System.Serializable]
public class NativeStyleMapping
{
    [Tooltip("The digit the backend sends in main_screen / play_screen / success_screen.")]
    public int styleNumber = 1;

    public NativeTemplateSize template = NativeTemplateSize.Small;

    [Tooltip("Requested width in dp. 0 leaves it to the template.")]
    public int widthDp;

    [Tooltip("Requested height in dp. 0 leaves it to the template.")]
    public int heightDp;
}

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

    [Tooltip("Requested native refresh interval. Clamped up to 30s at runtime.")]
    public int nativeRefreshSeconds = 30;

    [Header("Native style fallbacks")]
    [Tooltip("Used until the backend answers.")]
    public int defaultMainScreenStyle = 2;
    public int defaultPlayScreenStyle = 1;
    public int defaultSuccessScreenStyle = 5;

    [Tooltip("Leave empty to use the built-in mapping: digits 1-2 render Small, 3 and above Medium.")]
    public List<NativeStyleMapping> nativeStyles = new List<NativeStyleMapping>();

    [Header("Player-friendly floor")]
    [Tooltip("No ads at all before this level number. 0 disables the grace period.")]
    public int minLevelForAds = 3;
}
