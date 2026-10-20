using System;
using System.Collections.Generic;
using UnityEngine;

// The whole game configuration, as the config API returns it (see design/config/game-config.sample.json
// and design/config/README.md). Field names match the JSON keys exactly because JsonUtility maps by
// name. JsonUtility has no dictionary support, so every map in the JSON (placements, formats, rewarded
// rules) is modelled as one named field per known key; an unknown key is simply ignored.
//
// Every field that has a sensible default carries it in its initialiser, so a payload that leaves a
// section or a key out keeps the default instead of turning it off.
[Serializable]
public class GameConfig
{
    // The only schema version this build understands. A payload that says anything else is ignored.
    public const int SupportedVersion = 2;

    public int code;
    public string msg;
    public int config_version;

    public AppSection app = new AppSection();
    public LinksSection links = new LinksSection();
    public AdsSection ads = new AdsSection();
    public GameSection game = new GameSection();
    public UiSection ui = new UiSection();
    public MessageSection message = new MessageSection();

    // What the game runs on when there is no payload and no cached copy: ads off, no links, the
    // tuning the game shipped with.
    public static GameConfig Defaults()
    {
        return new GameConfig { code = 1, config_version = SupportedVersion };
    }
}

[Serializable]
public class AppSection
{
    public string name = "Arrow Dragon Puzzle";
    public string min_app_version = "";
    public bool force_update;
    public string update_url = "";
    public bool maintenance_mode;
    public string maintenance_text = "";
}

[Serializable]
public class LinksSection
{
    public string rate_us = "";
    public string privacy = "";
    public string terms = "";
    public string support_email = "";
    public string more_games = "";
    public string share_text = "";
    public string share_url = "";
}

[Serializable]
public class AdsSection
{
    public bool enabled;
    public bool test_mode;

    public AdUnits units = new AdUnits();
    public AdFormats formats = new AdFormats();
    public AdPlacements placements = new AdPlacements();
    public InterstitialRules interstitial_rules = new InterstitialRules();
    public RewardedRules rewarded_rules = new RewardedRules();

    public List<string> test_device_ids = new List<string>();
}

[Serializable]
public class AdUnits
{
    public string banner = "";
    public string banner_large = "";
    public string native = "";
    public string native_preload = "";
    public string interstitial = "";
    public string rewarded = "";
}

[Serializable]
public class FormatToggle
{
    public bool enabled = true;
}

[Serializable]
public class NativeFormat
{
    public bool enabled = true;
    public int refresh_sec = 30;
    public int max_age_min = 30;
}

[Serializable]
public class AdFormats
{
    public FormatToggle banner = new FormatToggle();
    public NativeFormat native = new NativeFormat();
    public FormatToggle interstitial = new FormatToggle();
    public FormatToggle rewarded = new FormatToggle();
}

// One ad slot per screen. `type` is one of: none, banner, banner_large, native_small, native_big.
[Serializable]
public class AdPlacement
{
    public string type = "none";
    public string position = "bottom";
    public int show_after_level;
}

[Serializable]
public class AdPlacements
{
    // splash and level_map are parsed but have no slot in the UI today, so nothing renders for them.
    public AdPlacement splash = new AdPlacement();
    public AdPlacement home = new AdPlacement();
    public AdPlacement level_map = new AdPlacement();
    public AdPlacement play = new AdPlacement();
    public AdPlacement win_sheet = new AdPlacement();
    public AdPlacement lose_sheet = new AdPlacement();
    public AdPlacement options = new AdPlacement();
}

// 0 on a count or a time means that gate is off. For max_per_session 0 means no cap.
[Serializable]
public class InterstitialRules
{
    public int min_click_count = 3;
    public int min_seconds_between = 30;
    public int every_n_levels_on_next = 2;
    public bool on_back_to_home;
    public int skip_first_levels;
    public int max_per_session;
}

[Serializable]
public class RewardedUse
{
    public bool enabled;
}

[Serializable]
public class RewardedRules
{
    public RewardedUse hint = new RewardedUse { enabled = true };
    public RewardedUse extra_life = new RewardedUse();
    public RewardedUse skip_level = new RewardedUse();
}

[Serializable]
public class LivesSection
{
    public int max = 3;
}

[Serializable]
public class HintsSection
{
    public int free_count = 3;
    public bool reset_on_change = true;
}

[Serializable]
public class LocksSection
{
    // Informational: the 1,500 levels are baked, so this does not move the first padlock.
    public int intro_level = 6;
}

[Serializable]
public class GameSection
{
    public LivesSection lives = new LivesSection();
    public HintsSection hints = new HintsSection();
    public LocksSection locks = new LocksSection();

    // Informational for the same reason as locks.intro_level.
    public int difficulty_offset;
}

[Serializable]
public class OptionsRows
{
    public bool vibration = true;
    public bool sound = true;
    public bool native_refresh_rate = true;
    public bool reduce_motion = true;
    public bool rate_us = true;
    public bool privacy = true;
    public bool terms = true;
}

[Serializable]
public class UiSection
{
    public OptionsRows options_rows = new OptionsRows();
    public bool reduce_motion_default;
    public bool confetti = true;
    public bool level_map = true;
    // The one-time coach overlay shown on the first padlock level.
    public bool lock_tutorial = true;
    // How a level builds itself in: "ink" (ink-brush draw), "fly" (slide in from outside the board) or
    // "classic" (the first look, kept for comparison). Anything else falls back to the default.
    public string entrance_style = "ink";
}

[Serializable]
public class MessageSection
{
    public string banner_text = "";
    public string banner_url = "";
}

// Parse + validate. Pure (no Unity objects, no disk), so every rule here is unit-tested.
public static class GameConfigParser
{
    public const string AdUnitPrefix = "ca-app-pub-";

    // The HUD's lives pill shows one heart per life and fits this many.
    public const int MaxLivesCap = 5;

    // True and a fully sanitised config, or false (config == null) for a payload to ignore: not JSON,
    // code != 1, or a config_version this build does not know.
    public static bool TryParse(string json, out GameConfig config, out string reason)
    {
        config = null;
        reason = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            reason = "empty payload";
            return false;
        }

        GameConfig parsed;
        try
        {
            parsed = JsonUtility.FromJson<GameConfig>(json);
        }
        catch (Exception e)
        {
            reason = "not valid JSON: " + e.Message;
            return false;
        }

        if (parsed == null)
        {
            reason = "not valid JSON";
            return false;
        }

        if (parsed.code != 1)
        {
            reason = "code is " + parsed.code + ", expected 1";
            return false;
        }

        if (parsed.config_version != GameConfig.SupportedVersion)
        {
            reason = "config_version " + parsed.config_version + " is not supported (expected "
                     + GameConfig.SupportedVersion + ")";
            return false;
        }

        Sanitize(parsed);
        config = parsed;
        return true;
    }

    // A valid AdMob unit id looks like ca-app-pub-<publisher>/<unit>.
    public static bool IsAdUnit(string id)
    {
        return !string.IsNullOrEmpty(id)
               && id.StartsWith(AdUnitPrefix, StringComparison.Ordinal)
               && id.Contains("/");
    }

    // Maps a placement `type` string to the slot kind the ad code understands. Anything unknown is
    // "no ad". The result sheets never carry a banner (a product decision), so a banner type there
    // also resolves to none.
    public static PromoSlotKind SlotFor(string type, bool nativeOnly)
    {
        PromoSlotKind slot;

        switch ((type ?? "").Trim().ToLowerInvariant())
        {
            case "banner": slot = PromoSlotKind.BannerStandard; break;
            case "banner_large": slot = PromoSlotKind.BannerLarge; break;
            case "native_small": slot = PromoSlotKind.NativeSmall; break;
            case "native_big": slot = PromoSlotKind.NativeBig; break;
            default: return PromoSlotKind.None;
        }

        if (nativeOnly && (slot == PromoSlotKind.BannerStandard || slot == PromoSlotKind.BannerLarge))
        {
            return PromoSlotKind.None;
        }

        return slot;
    }

    // Brings a freshly parsed config into the ranges the game can use. Never throws; a missing
    // section is already covered by the field initialisers.
    public static void Sanitize(GameConfig c)
    {
        c.app = c.app ?? new AppSection();
        c.links = c.links ?? new LinksSection();
        c.ads = c.ads ?? new AdsSection();
        c.game = c.game ?? new GameSection();
        c.ui = c.ui ?? new UiSection();
        c.message = c.message ?? new MessageSection();

        AdsSection ads = c.ads;
        ads.units = ads.units ?? new AdUnits();
        ads.formats = ads.formats ?? new AdFormats();
        ads.placements = ads.placements ?? new AdPlacements();
        ads.interstitial_rules = ads.interstitial_rules ?? new InterstitialRules();
        ads.rewarded_rules = ads.rewarded_rules ?? new RewardedRules();
        ads.test_device_ids = ads.test_device_ids ?? new List<string>();

        ads.units.banner = KeepAdUnit(ads.units.banner);
        ads.units.banner_large = KeepAdUnit(ads.units.banner_large);
        ads.units.native = KeepAdUnit(ads.units.native);
        ads.units.native_preload = KeepAdUnit(ads.units.native_preload);
        ads.units.interstitial = KeepAdUnit(ads.units.interstitial);
        ads.units.rewarded = KeepAdUnit(ads.units.rewarded);

        ads.formats.banner = ads.formats.banner ?? new FormatToggle();
        ads.formats.native = ads.formats.native ?? new NativeFormat();
        ads.formats.interstitial = ads.formats.interstitial ?? new FormatToggle();
        ads.formats.rewarded = ads.formats.rewarded ?? new FormatToggle();
        ads.formats.native.refresh_sec = Mathf.Max(0, ads.formats.native.refresh_sec);
        ads.formats.native.max_age_min = Mathf.Max(0, ads.formats.native.max_age_min);

        AdPlacements p = ads.placements;
        p.splash = CleanPlacement(p.splash, false);
        p.home = CleanPlacement(p.home, false);
        p.level_map = CleanPlacement(p.level_map, false);
        p.play = CleanPlacement(p.play, false);
        p.options = CleanPlacement(p.options, false);
        p.win_sheet = CleanPlacement(p.win_sheet, true);
        p.lose_sheet = CleanPlacement(p.lose_sheet, true);

        InterstitialRules r = ads.interstitial_rules;
        r.min_click_count = Mathf.Max(0, r.min_click_count);
        r.min_seconds_between = Mathf.Max(0, r.min_seconds_between);
        r.every_n_levels_on_next = Mathf.Max(0, r.every_n_levels_on_next);
        r.skip_first_levels = Mathf.Max(0, r.skip_first_levels);
        r.max_per_session = Mathf.Max(0, r.max_per_session);

        ads.rewarded_rules.hint = ads.rewarded_rules.hint ?? new RewardedUse();
        ads.rewarded_rules.extra_life = ads.rewarded_rules.extra_life ?? new RewardedUse();
        ads.rewarded_rules.skip_level = ads.rewarded_rules.skip_level ?? new RewardedUse();

        GameSection g = c.game;
        g.lives = g.lives ?? new LivesSection();
        g.hints = g.hints ?? new HintsSection();
        g.locks = g.locks ?? new LocksSection();

        // A game with zero lives could never be played, so a non-positive value means "use the
        // default". More than MaxLivesCap would not fit in the HUD's lives pill, so it is capped.
        if (g.lives.max < 1) g.lives.max = new LivesSection().max;
        g.lives.max = Mathf.Min(g.lives.max, MaxLivesCap);
        g.hints.free_count = Mathf.Max(0, g.hints.free_count);
        g.locks.intro_level = Mathf.Max(0, g.locks.intro_level);

        UiSection ui = c.ui;
        ui.options_rows = ui.options_rows ?? new OptionsRows();
        ui.entrance_style = EntranceMath.StyleKey(EntranceMath.ParseStyle(ui.entrance_style));

        c.app.name = c.app.name ?? "";
        c.app.min_app_version = c.app.min_app_version ?? "";
        c.app.update_url = c.app.update_url ?? "";
        c.app.maintenance_text = c.app.maintenance_text ?? "";
        c.message.banner_text = c.message.banner_text ?? "";
        c.message.banner_url = c.message.banner_url ?? "";

        LinksSection l = c.links;
        l.rate_us = l.rate_us ?? "";
        l.privacy = l.privacy ?? "";
        l.terms = l.terms ?? "";
        l.support_email = l.support_email ?? "";
        l.more_games = l.more_games ?? "";
        l.share_text = l.share_text ?? "";
        l.share_url = l.share_url ?? "";
    }

    // A link row is shown only when its switch is on AND the link is non-empty.
    public static bool LinkRowVisible(GameConfig c, string row)
    {
        switch (row)
        {
            case "rate_us": return c.ui.options_rows.rate_us && !string.IsNullOrEmpty(c.links.rate_us);
            case "privacy": return c.ui.options_rows.privacy && !string.IsNullOrEmpty(c.links.privacy);
            case "terms": return c.ui.options_rows.terms && !string.IsNullOrEmpty(c.links.terms);
            default: return false;
        }
    }

    static string KeepAdUnit(string id)
    {
        return IsAdUnit(id) ? id : "";
    }

    static AdPlacement CleanPlacement(AdPlacement placement, bool nativeOnly)
    {
        placement = placement ?? new AdPlacement();
        placement.show_after_level = Mathf.Max(0, placement.show_after_level);
        placement.type = SlotFor(placement.type, nativeOnly) switch
        {
            PromoSlotKind.BannerStandard => "banner",
            PromoSlotKind.BannerLarge => "banner_large",
            PromoSlotKind.NativeSmall => "native_small",
            PromoSlotKind.NativeBig => "native_big",
            _ => "none"
        };
        return placement;
    }
}
