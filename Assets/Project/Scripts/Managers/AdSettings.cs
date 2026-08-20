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

    public bool ApplyRemote(AdRemoteConfig config)
    {
        if (config == null || config.code != 1)
        {
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

    // Lets a screen decide whether to open any room at all. Checking AdsEnabled alone would leave a
    // gap whenever the backend sends an enabled flag but a blank unit ID.
    public bool CanShowPreloadNative => AdsEnabled && !string.IsNullOrEmpty(PreloadNativeId);

    public int LevelsPerInterstitial => Positive(remote?.ad_level_count, baked.levelsPerInterstitial);
    public int InterstitialCooldownSeconds => Positive(remote?.ad_click_time_sec, baked.interstitialCooldownSeconds);
    public int MinLevelForAds => baked.minLevelForAds;

    // Clamped to the floor AdMob enforces on banner auto-refresh. The backend currently asks for
    // 10s, which buys many requests and few viewable impressions.
    public int NativeRefreshSeconds
    {
        get
        {
            int requested = Positive(remote?.refresh_native_sec, baked.nativeRefreshSeconds);
            return Mathf.Max(requested, MinNativeRefreshSeconds);
        }
    }

    public const int MinNativeRefreshSeconds = 30;

    public int MainScreenStyle => Positive(remote?.main_screen, baked.defaultMainScreenStyle);
    public int PlayScreenStyle => Positive(remote?.play_screen, baked.defaultPlayScreenStyle);
    public int SuccessScreenStyle => Positive(remote?.success_screen, baked.defaultSuccessScreenStyle);

    // The AdConfig table wins so the mapping can be retuned without a code change; the built-in
    // fallback keeps a freshly created asset working.
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

        return new NativeStyleMapping
        {
            styleNumber = styleNumber,
            template = styleNumber >= 3 ? NativeTemplateSize.Medium : NativeTemplateSize.Small
        };
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
