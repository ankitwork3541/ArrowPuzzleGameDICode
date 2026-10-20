using System.Collections.Generic;
using UnityEngine;

// The ad slot a screen asks for: the `type` of an ads.placements.<screen> entry in the game config
// (none, banner, banner_large, native_small, native_big). The numeric values are also the key of the
// native template table below.
public enum PromoSlotKind
{
    None = 0,

    // Google's two fixed banner sizes: BANNER (320x50 dp) and LARGE_BANNER (320x100 dp).
    BannerStandard = 1,
    BannerLarge = 2,

    NativeSmall = 3,
    NativeBig = 4
}

// Which of the plugin's two native templates a native slot renders as. The plugin ships only these
// two (NativeTemplateId), which is why there are exactly two native slot types.
public enum PromoTemplateSize
{
    Small,
    Medium
}

[System.Serializable]
public class PromoStyleMapping
{
    [Tooltip("The PromoSlotKind number: 3 = native_small, 4 = native_big.")]
    public int styleNumber = 3;

    public PromoTemplateSize template = PromoTemplateSize.Small;
}

// Development switches that ship inside the build. Everything that can change after release (ad unit
// ids, flags, intervals, placements) comes from the game config API instead (GameConfig). In
// particular this asset carries NO ad unit ids: with no config payload, ads stay off.
[CreateAssetMenu(fileName = "PromoConfig", menuName = "LanternDragons/Promo Config")]
public class PromoConfig : ScriptableObject
{
    [Header("Development")]
    [Tooltip("Forces Google's test ad units and turns ads on, ignoring both the config's unit ids and "
             + "its ads.enabled flag. Turn OFF before release.")]
    public bool useTestIds = true;

    [Tooltip("AdMob test device IDs. Required before testing with LIVE ad unit IDs: requesting or "
             + "clicking a live ad on an unregistered device counts as invalid activity. Run once "
             + "with the real IDs and copy the hash out of the logcat line "
             + "\"Use RequestConfiguration.Builder.setTestDeviceIds(...)\". The config's "
             + "ads.test_device_ids are added to this list.")]
    public List<string> testDeviceIds = new List<string>();

    [Header("Native templates")]
    [Tooltip("Leave empty to use the built-in mapping: native_small renders Small, native_big Medium.")]
    public List<PromoStyleMapping> nativeStyles = new List<PromoStyleMapping>();
}
