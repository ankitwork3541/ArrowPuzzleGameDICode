using System;
using System.Collections.Generic;

// The single place the rest of the game reads ad settings from. Resolves remote over baked, so no
// caller ever has to know whether the backend answered.
public class AdSettings
{
    readonly AdConfig baked;
    AdRemoteConfig remote;

    public AdSettings(AdConfig baked)
    {
        this.baked = baked;
    }

    public bool HasRemote => remote != null;
    public bool UsingTestIds => baked.useTestIds;

    public bool ApplyRemote(AdRemoteConfig config)
    {
        if (config == null || config.code != 1)
        {
            return false;
        }

        remote = config;
        return true;
    }

    // The backend is the only authority on whether ads run at all. With no payload — not even last
    // launch's cached one — every format stays off rather than falling back to whatever shipped in
    // the build. Test mode is the one exception, so the Editor and debug builds still serve.
    public bool AdsEnabled
    {
        get
        {
            if (baked.useTestIds)
            {
                return true;
            }

            return remote != null && remote.ad_enable == 1;
        }
    }

    public bool RewardedEnabled =>
        AdsEnabled && (remote != null ? remote.rewarded_ad_enable == 1 : baked.rewardedEnabled);

    public bool BackAdEnabled =>
        AdsEnabled && (remote != null ? remote.back_ad_on == 1 : baked.backAdEnabled);

    public string BannerId => Id(remote?.banner_adunit_id, baked.banner, AdTestIds.Banner);
    public string InterstitialId => Id(remote?.interstitial_adunit_id, baked.interstitial, AdTestIds.Interstitial);
    public string RewardedId => Id(remote?.rewarded_adunit_id, baked.rewarded, AdTestIds.Rewarded);
    public string NativeId => Id(remote?.native_adunit_id, baked.nativeAd, AdTestIds.Native);
    public string PreloadNativeId => Id(remote?.preload_native_adunit_id, baked.preloadNativeAd, AdTestIds.Native);

    // Lets a screen decide whether to open any room at all. Checking AdsEnabled alone would leave a
    // gap whenever the backend sends an enabled flag but a blank unit ID, or a style digit that maps
    // to no layout.
    public bool CanShowPreloadNative =>
        AdsEnabled && !string.IsNullOrEmpty(PreloadNativeId) && IsSupportedNativeStyle(SuccessScreenStyle);

    public int LevelsPerInterstitial => Positive(remote?.ad_level_count, baked.levelsPerInterstitial);
    public int InterstitialCooldownSeconds => Positive(remote?.ad_click_time_sec, baked.interstitialCooldownSeconds);

    public int NativeRefreshSeconds => Positive(remote?.refresh_native_sec, baked.nativeRefreshSeconds);

    // Not a backend value: staleness is about the SDK's cache, not about how often the publisher
    // wants a new creative.
    public float AdMaxAgeSeconds => Math.Max(60, baked.adMaxAgeMinutes * 60);

    public List<string> TestDeviceIds => baked.testDeviceIds;

    public int MainScreenStyle => Positive(remote?.main_screen, baked.defaultMainScreenStyle);
    public int PlayScreenStyle => Positive(remote?.play_screen, baked.defaultPlayScreenStyle);
    public int SuccessScreenStyle => Positive(remote?.success_screen, baked.defaultSuccessScreenStyle);

    // Only the two layouts the plugin actually ships are supported: digit 1 is the thin strip,
    // digit 2 the tall one with a media panel. Any other digit returns null, which every caller
    // treats as "no ad here" — the backend sending an unmapped number must leave the screen exactly
    // as it looks with ads off, not fall back to a size nobody asked for.
    //
    // The AdConfig table is checked first so the mapping can be retuned without a code change.
    public NativeStyleMapping NativeStyle(int styleNumber)
    {
        if (baked.nativeStyles != null)
        {
            foreach (NativeStyleMapping entry in baked.nativeStyles)
            {
                if (entry != null && entry.styleNumber == styleNumber)
                {
                    return entry;
                }
            }
        }

        if (styleNumber == 1)
        {
            return new NativeStyleMapping { styleNumber = 1, template = NativeTemplateSize.Small };
        }

        if (styleNumber == 2)
        {
            return new NativeStyleMapping { styleNumber = 2, template = NativeTemplateSize.Medium };
        }

        return null;
    }

    public bool IsSupportedNativeStyle(int styleNumber)
    {
        return NativeStyle(styleNumber) != null;
    }

    // Returns null rather than falling back to a test unit: showing test ads to real players earns
    // nothing and reads as a broken game.
    string Id(string remoteId, string bakedId, string testId)
    {
        if (baked.useTestIds)
        {
            return testId;
        }

        if (IsAdUnit(remoteId))
        {
            return remoteId;
        }

        return IsAdUnit(bakedId) ? bakedId : null;
    }

    static int Positive(int? remoteValue, int bakedValue)
    {
        return remoteValue.HasValue && remoteValue.Value > 0 ? remoteValue.Value : bakedValue;
    }

    static bool IsAdUnit(string id)
    {
        return !string.IsNullOrEmpty(id)
               && id.StartsWith("ca-app-pub-", StringComparison.Ordinal)
               && id.Contains("/");
    }
}
