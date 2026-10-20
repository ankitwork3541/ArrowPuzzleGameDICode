using System.Collections.Generic;

// The screens that can carry an ad. The names match the keys under ads.placements in the config.
public enum PromoScreen
{
    Splash,
    Home,
    LevelMap,
    Play,
    WinSheet,
    LoseSheet,
    Options
}

// The single place the rest of the game reads ad settings from. Every value comes from the current
// GameConfig (GameConfigService), so no caller needs to know whether the config API answered, was
// cached, or is missing. The baked PromoConfig only supplies development switches (test ids, test
// devices) and the native template table.
public class PromoSettings
{
    readonly PromoConfig baked;

    public PromoSettings(PromoConfig baked)
    {
        this.baked = baked ?? throw new System.ArgumentNullException(nameof(baked));
    }

    static GameConfig Config => GameConfigService.Current;
    static AdsSection Ads => Config.ads;

    public bool HasRemote => GameConfigService.HasPayload;
    public bool UsingTestIds => baked.useTestIds || Ads.test_mode;

    // The config is the only authority on whether ads run at all. With no payload, and not even last
    // launch's cached one, every format stays off. Test mode is the one exception, so the Editor and
    // debug builds still serve.
    public bool AdsEnabled => UsingTestIds || Ads.enabled;

    // Master switch is ads.enabled. formats.rewarded turns only THIS format off while banners,
    // interstitial and native stay on.
    public bool RewardedEnabled => AdsEnabled && Ads.formats.rewarded.enabled;

    // The hint button's rewarded fallback: the format has to be on AND hints have to be allowed to use it.
    public bool HintRewardedEnabled => RewardedEnabled && Ads.rewarded_rules.hint.enabled;

    public bool BackAdEnabled => AdsEnabled && Ads.interstitial_rules.on_back_to_home;

    public bool BannerFormatEnabled => Ads.formats.banner.enabled;
    public bool NativeFormatEnabled => Ads.formats.native.enabled;

    public string BannerId => BannerIdFor(false);

    // The large banner has its own unit id; without one it reuses the standard banner's.
    public string BannerIdFor(bool large)
    {
        if (UsingTestIds)
        {
            return PromoTestIds.Banner;
        }

        string id = large ? Ads.units.banner_large : null;
        return GameConfigParser.IsAdUnit(id) ? id : Valid(Ads.units.banner);
    }

    public string InterstitialId => UsingTestIds ? PromoTestIds.Interstitial : Valid(Ads.units.interstitial);
    public string RewardedId => UsingTestIds ? PromoTestIds.Rewarded : Valid(Ads.units.rewarded);
    public string NativeId => UsingTestIds ? PromoTestIds.Native : Valid(Ads.units.native);

    public string PreloadNativeId
    {
        get
        {
            if (UsingTestIds)
            {
                return PromoTestIds.Native;
            }

            string id = Valid(Ads.units.native_preload);
            return id ?? Valid(Ads.units.native);
        }
    }

    public bool InterstitialEnabled =>
        AdsEnabled && Ads.formats.interstitial.enabled && !string.IsNullOrEmpty(InterstitialId);

    // The interstitial rules, for the policy manager.
    public InterstitialRules Interstitials => Ads.interstitial_rules;

    // Kept as plain numbers for the call sites that only need one.
    public int AdLevelCount => Ads.interstitial_rules.every_n_levels_on_next;
    public int AdClickTimeSec => Ads.interstitial_rules.min_seconds_between;
    public int AdClickCount => Ads.interstitial_rules.min_click_count;

    // Zero here means "no refresh", not "use a default": refresh_sec is taken as-is.
    public int NativeRefreshSeconds => Ads.formats.native.refresh_sec;

    // Not a publisher value: staleness is about the SDK's cache, not about how often the publisher
    // wants a new creative. Never below a minute.
    public float AdMaxAgeSeconds => System.Math.Max(60, Ads.formats.native.max_age_min * 60);

    // The baked list (development devices) plus the config's.
    public List<string> TestDeviceIds
    {
        get
        {
            var ids = new List<string>();
            AddAll(ids, baked.testDeviceIds);
            AddAll(ids, Ads.test_device_ids);
            return ids;
        }
    }

    // What a screen shows. A placement that has not reached its show_after_level yet resolves to
    // None, exactly as if the config had asked for no ad there.
    public PromoSlotKind SlotFor(PromoScreen screen)
    {
        AdPlacement placement = PlacementFor(screen);
        if (!AdRules.PlacementActive(placement, GameConfigService.HighestLevelCompleted))
        {
            return PromoSlotKind.None;
        }

        // The result sheets never carry a banner: that is a product decision, not a technical limit.
        bool nativeOnly = screen == PromoScreen.WinSheet || screen == PromoScreen.LoseSheet;
        return GameConfigParser.SlotFor(placement.type, nativeOnly);
    }

    public AdPlacement PlacementFor(PromoScreen screen)
    {
        AdPlacements p = Ads.placements;

        switch (screen)
        {
            case PromoScreen.Splash: return p.splash;
            case PromoScreen.Home: return p.home;
            case PromoScreen.LevelMap: return p.level_map;
            case PromoScreen.Play: return p.play;
            case PromoScreen.WinSheet: return p.win_sheet;
            case PromoScreen.LoseSheet: return p.lose_sheet;
            default: return p.options;
        }
    }

    // The slot the level-end native is preloaded for: the win sheet's if that is a native slot, else
    // the lose sheet's. The preload happens at level start, before anyone knows which one is coming.
    public PromoScreen LevelEndPreloadScreen =>
        IsNative(SlotFor(PromoScreen.WinSheet)) ? PromoScreen.WinSheet : PromoScreen.LoseSheet;

    // Lets a screen decide whether to open any room at all. Checking AdsEnabled alone would leave a
    // gap whenever the config enables ads but sends a blank unit id, or a placement this screen
    // cannot show.
    public bool CanShowPreloadNative(PromoScreen sheet)
    {
        return AdsEnabled
               && NativeFormatEnabled
               && !string.IsNullOrEmpty(PreloadNativeId)
               && IsNative(SlotFor(sheet));
    }

    public static bool IsBanner(PromoSlotKind slot)
    {
        return slot == PromoSlotKind.BannerStandard || slot == PromoSlotKind.BannerLarge;
    }

    public static bool IsNative(PromoSlotKind slot)
    {
        return slot == PromoSlotKind.NativeSmall || slot == PromoSlotKind.NativeBig;
    }

    // Every way a screen's slot can turn out to be nothing: ads off, a placement of none, a format
    // switched off, or a format whose unit id never arrived.
    public bool CanShow(PromoSlotKind slot)
    {
        if (!AdsEnabled)
        {
            return false;
        }

        if (IsBanner(slot))
        {
            return BannerFormatEnabled && !string.IsNullOrEmpty(BannerIdFor(slot == PromoSlotKind.BannerLarge));
        }

        return IsNative(slot) && NativeFormatEnabled && !string.IsNullOrEmpty(NativeId);
    }

    // The template a native slot renders as, or null when the slot is not a native one at all. The
    // baked table only refines a native slot; it can never turn a banner into one.
    public PromoStyleMapping NativeStyle(PromoSlotKind slot)
    {
        if (!IsNative(slot))
        {
            return null;
        }

        // PromoSlotKind's values double as the table's key, so the table stays keyed by number.
        int digit = (int)slot;

        if (baked.nativeStyles != null)
        {
            foreach (PromoStyleMapping entry in baked.nativeStyles)
            {
                if (entry != null && entry.styleNumber == digit)
                {
                    return entry;
                }
            }
        }

        return new PromoStyleMapping
        {
            styleNumber = digit,
            template = slot == PromoSlotKind.NativeBig
                ? PromoTemplateSize.Medium
                : PromoTemplateSize.Small
        };
    }

    // Null rather than a test unit when there is no valid id: showing test ads to real players earns
    // nothing and reads as a broken game.
    static string Valid(string id)
    {
        return GameConfigParser.IsAdUnit(id) ? id : null;
    }

    static void AddAll(List<string> into, List<string> from)
    {
        if (from == null)
        {
            return;
        }

        foreach (string id in from)
        {
            if (!string.IsNullOrEmpty(id) && !into.Contains(id))
            {
                into.Add(id);
            }
        }
    }
}
