using System;
using UnityEngine;

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

    // All-or-nothing on purpose: a bad deploy returning empty strings must not be able to blank
    // out every ad unit one field at a time.
    public bool ApplyRemote(AdRemoteConfig config)
    {
        if (config == null || config.code != 1)
        {
            return false;
        }

        if (!IsAdUnit(config.interstitial_adunit_id))
        {
            Debug.LogWarning("[Ads] Remote config carries no usable interstitial unit — ignored.");
            return false;
        }

        remote = config;
        return true;
    }

    public bool AdsEnabled => remote != null ? remote.ad_enable == 1 : baked.adsEnabled;
    public bool RewardedEnabled => remote != null ? remote.rewarded_ad_enable == 1 : baked.rewardedEnabled;
    public bool BackAdEnabled => remote != null ? remote.back_ad_on == 1 : baked.backAdEnabled;

    public string BannerId => Id(remote?.banner_adunit_id, baked.banner, AdTestIds.Banner);
    public string InterstitialId => Id(remote?.interstitial_adunit_id, baked.interstitial, AdTestIds.Interstitial);
    public string RewardedId => Id(remote?.rewarded_adunit_id, baked.rewarded, AdTestIds.Rewarded);
    public string NativeId => Id(remote?.native_adunit_id, baked.nativeAd, AdTestIds.Native);
    public string PreloadNativeId => Id(remote?.preload_native_adunit_id, baked.preloadNativeAd, AdTestIds.Native);

    public int LevelsPerInterstitial => Positive(remote?.ad_level_count, baked.levelsPerInterstitial);
    public int InterstitialCooldownSeconds => Positive(remote?.ad_click_time_sec, baked.interstitialCooldownSeconds);
    public int NativeRefreshSeconds => Positive(remote?.refresh_native_sec, baked.nativeRefreshSeconds);
    public int MinLevelForAds => baked.minLevelForAds;

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
