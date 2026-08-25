using System.Collections.Generic;
using UnityEngine;

// What the backend's main_screen / play_screen / success_screen digit asks a screen to show.
// Anything the backend sends that is not on this list means "no ad on that screen".
public enum AdSlotKind
{
    None = 0,

    // Google's two fixed banner sizes: BANNER (320x50 dp) and LARGE_BANNER (320x100 dp).
    BannerStandard = 1,
    BannerLarge = 2,

    NativeSmall = 3,
    NativeBig = 4
}

// Which of the plugin's two native templates a native slot renders as. The plugin ships only these
// two (NativeTemplateId), which is why there are exactly two native digits.
public enum NativeTemplateSize
{
    Small,
    Medium
}

[System.Serializable]
public class NativeStyleMapping
{
    [Tooltip("The digit the backend sends in main_screen / play_screen / success_screen.")]
    public int styleNumber = 3;

    public NativeTemplateSize template = NativeTemplateSize.Small;
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

    [Tooltip("Number of free hints provided before requiring rewarded ads.")]
    public int defaultFreeHintCount = 3;

    [Tooltip("Show an interstitial after this many completed levels.")]
    public int levelsPerInterstitial = 2;

    [Tooltip("Minimum gap between two interstitials.")]
    public int interstitialCooldownSeconds = 30;

    [Tooltip("Global button click threshold before eligible for click-based interstitial.")]
    public int adClickCount = 3;

    [Tooltip("Native refresh interval used until the backend answers. The backend's "
             + "refresh_native_sec is taken as-is, unclamped. Banners are NOT refreshed from code — "
             + "that is left to the ad unit's own auto-refresh in the AdMob dashboard.")]
    public int nativeRefreshSeconds = 30;

    [Header("Native templates")]
    [Tooltip("Leave empty to use the built-in mapping: digit 3 renders Small, digit 4 Medium.")]
    public List<NativeStyleMapping> nativeStyles = new List<NativeStyleMapping>();

    [Header("Ad freshness")]
    [Tooltip("An ad older than this is never shown, whether it was preloaded for a result card or "
             + "kept while the player was on another screen. Google treats a cached ad as stale "
             + "after roughly an hour, so this stays well inside that.")]
    public int adMaxAgeMinutes = 30;
}
