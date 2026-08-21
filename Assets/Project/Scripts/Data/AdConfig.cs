using System.Collections.Generic;
using UnityEngine;

// Which of the plugin's two native templates a backend style digit maps to. The plugin ships only
// Small and Medium (NativeTemplateId), so digit 1 and 2 are the only ones that can render — every
// other digit is treated as "no ad on this screen".
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

    [Tooltip("Overrides how much room the UI reserves for this style, in PIXELS. 0 uses the "
             + "template's own height, which is almost always right. The ad itself always renders "
             + "at its natural size — asking the SDK for a custom size is a known-broken path.")]
    public int heightPx;
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

    [Tooltip("AdMob test device IDs. Required before testing with LIVE ad unit IDs: requesting or "
             + "clicking a live ad on an unregistered device counts as invalid activity. Run once "
             + "with the real IDs and copy the hash out of the logcat line "
             + "\"Use RequestConfiguration.Builder.setTestDeviceIds(...)\".")]
    public List<string> testDeviceIds = new List<string>();

    [Header("Ad unit IDs (Android)")]
    public string banner;
    public string interstitial;
    public string rewarded;
    public string nativeAd;
    public string preloadNativeAd;

    [Header("Rules")]
    [Tooltip("Fallbacks for the sub-flags only. There is no baked equivalent of ad_enable: with no "
             + "backend config, ads stay off entirely.")]
    public bool rewardedEnabled = true;
    public bool backAdEnabled = false;

    [Tooltip("Show an interstitial after this many completed levels.")]
    public int levelsPerInterstitial = 2;

    [Tooltip("Minimum gap between two interstitials.")]
    public int interstitialCooldownSeconds = 30;

    [Tooltip("Native refresh interval used until the backend answers. The backend's "
             + "refresh_native_sec is taken as-is, unclamped.")]
    public int nativeRefreshSeconds = 30;

    [Header("Native style fallbacks")]
    [Tooltip("Only used in test mode — outside it, no backend config means no ads at all. Must be "
             + "1 or 2; any other number renders nothing.")]
    public int defaultMainScreenStyle = 2;
    public int defaultPlayScreenStyle = 1;
    public int defaultSuccessScreenStyle = 2;

    [Tooltip("Leave empty to use the built-in mapping: digit 1 renders Small, digit 2 Medium, and "
             + "every other digit renders no ad.")]
    public List<NativeStyleMapping> nativeStyles = new List<NativeStyleMapping>();

    [Header("Ad freshness")]
    [Tooltip("An ad older than this is never shown, whether it was preloaded for a result card or "
             + "kept while the player was on another screen. Google treats a cached ad as stale "
             + "after roughly an hour, so this stays well inside that.")]
    public int adMaxAgeMinutes = 30;
}
