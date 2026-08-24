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

    // Master switch is ad_enable (AdsEnabled). rewarded_ad_enable only turns THIS format off
    // while banners / interstitial / native can stay on. Any digit other than 1 is off.
    public bool RewardedEnabled =>
        AdsEnabled && (remote != null ? remote.rewarded_ad_enable == 1 : baked.rewardedEnabled);

    public bool BackAdEnabled =>
        AdsEnabled && (remote != null ? remote.back_ad_on == 1 : baked.backAdEnabled);

    public string BannerId => Id(remote?.banner_adunit_id, baked.banner, AdTestIds.Banner);
    public string InterstitialId => Id(remote?.interstitial_adunit_id, baked.interstitial, AdTestIds.Interstitial);
    public string RewardedId => Id(remote?.rewarded_adunit_id, baked.rewarded, AdTestIds.Rewarded);
    public string NativeId => Id(remote?.native_adunit_id, baked.nativeAd, AdTestIds.Native);
    public string PreloadNativeId => Id(remote?.preload_native_adunit_id, baked.preloadNativeAd, AdTestIds.Native);

    public bool InterstitialEnabled => AdsEnabled && !string.IsNullOrEmpty(InterstitialId);

    // Lets a screen decide whether to open any room at all. Checking AdsEnabled alone would leave a
    // gap whenever the backend sends an enabled flag but a blank unit ID, or a digit that maps to
    // nothing this screen can show.
    public bool CanShowPreloadNative =>
        AdsEnabled && !string.IsNullOrEmpty(PreloadNativeId) && IsNative(SuccessScreenSlot);

    // 0 is a real answer (that trigger is off). Positive() would treat it as missing.
    public int AdLevelCount =>
        remote != null ? Math.Max(0, remote.ad_level_count) : Math.Max(0, baked.levelsPerInterstitial);

    public int LevelsPerInterstitial => AdLevelCount;

    public int AdClickTimeSec =>
        remote != null ? Math.Max(0, remote.ad_click_time_sec) : Math.Max(0, baked.interstitialCooldownSeconds);

    public int InterstitialCooldownSeconds => AdClickTimeSec;

    public int AdClickCount =>
        remote != null ? Math.Max(0, remote.ad_click_count) : Math.Max(0, baked.adClickCount);

    // 0 is a real answer ("no free hints"). Positive() would treat it as missing and fall back.
    public int FreeHintCount =>
        remote != null ? Math.Max(0, remote.free_hint_count) : Math.Max(0, baked.defaultFreeHintCount);

    public int NativeRefreshSeconds => Positive(remote?.refresh_native_sec, baked.nativeRefreshSeconds);

    // Not a backend value: staleness is about the SDK's cache, not about how often the publisher
    // wants a new creative.
    public float AdMaxAgeSeconds => Math.Max(60, baked.adMaxAgeMinutes * 60);

    public List<string> TestDeviceIds => baked.testDeviceIds;

    // Digit 0 is a real answer ("no ad here"), so these cannot use Positive() — that treats 0 as
    // "unset" and falls back to the baked value, which would turn an ad ON that the backend just
    // switched off.
    public int MainScreenStyle => Digit(remote?.main_screen, baked.defaultMainScreenStyle);
    public int PlayScreenStyle => Digit(remote?.play_screen, baked.defaultPlayScreenStyle);
    public int SuccessScreenStyle => Digit(remote?.success_screen, baked.defaultSuccessScreenStyle);

    public AdSlotKind MainScreenSlot => SlotFor(MainScreenStyle);
    public AdSlotKind PlayScreenSlot => SlotFor(PlayScreenStyle);

    // The result cards never carry a banner — that is a product decision, not a technical limit. A
    // banner digit here resolves to nothing rather than quietly showing one.
    public AdSlotKind SuccessScreenSlot
    {
        get
        {
            AdSlotKind slot = SlotFor(SuccessScreenStyle);
            return IsNative(slot) ? slot : AdSlotKind.None;
        }
    }

    public static AdSlotKind SlotFor(int digit)
    {
        switch (digit)
        {
            case 1: return AdSlotKind.BannerStandard;
            case 2: return AdSlotKind.BannerLarge;
            case 3: return AdSlotKind.NativeSmall;
            case 4: return AdSlotKind.NativeBig;
            default: return AdSlotKind.None;
        }
    }

    public static bool IsBanner(AdSlotKind slot)
    {
        return slot == AdSlotKind.BannerStandard || slot == AdSlotKind.BannerLarge;
    }

    public static bool IsNative(AdSlotKind slot)
    {
        return slot == AdSlotKind.NativeSmall || slot == AdSlotKind.NativeBig;
    }

    // Every way a screen's slot can turn out to be nothing: ads off, an unmapped digit, or the digit
    // asking for a format whose unit ID never arrived.
    public bool CanShow(AdSlotKind slot)
    {
        if (!AdsEnabled)
        {
            return false;
        }

        if (IsBanner(slot))
        {
            return !string.IsNullOrEmpty(BannerId);
        }

        return IsNative(slot) && !string.IsNullOrEmpty(NativeId);
    }

    // The template a native slot renders as, or null when the slot is not a native one at all.
    //
    // SlotFor() is the authority and is checked FIRST. The AdConfig table only refines a slot the
    // digit scheme already agrees is native — otherwise an Inspector row saying "digit 1 is native"
    // would contradict SlotFor(1), which says banner, and the two dispatch paths would disagree
    // about what the same digit means.
    public NativeStyleMapping NativeStyle(AdSlotKind slot)
    {
        if (!IsNative(slot))
        {
            return null;
        }

        // AdSlotKind's values are the backend's digits, so the table can stay keyed by digit.
        int digit = (int)slot;

        if (baked.nativeStyles != null)
        {
            foreach (NativeStyleMapping entry in baked.nativeStyles)
            {
                if (entry != null && entry.styleNumber == digit)
                {
                    return entry;
                }
            }
        }

        return new NativeStyleMapping
        {
            styleNumber = digit,
            template = slot == AdSlotKind.NativeBig
                ? NativeTemplateSize.Medium
                : NativeTemplateSize.Small
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

    // Keeps a backend 0, which means "no ad on this screen", instead of reading it as missing.
    static int Digit(int? remoteValue, int bakedValue)
    {
        return remoteValue.HasValue ? remoteValue.Value : bakedValue;
    }

    static bool IsAdUnit(string id)
    {
        return !string.IsNullOrEmpty(id)
               && id.StartsWith("ca-app-pub-", StringComparison.Ordinal)
               && id.Contains("/");
    }
}
