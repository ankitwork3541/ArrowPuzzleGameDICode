using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

// The game config: schema parsing and validation, defaults, the ad rules, how PromoSettings reads the
// config, the hint pool rule and the on-disk cache. Every test starts from a clean GameConfigService.
public class GameConfigTests
{
    // A copy of design/config/game-config.sample.json.
    private const string SampleJson = @"{
  ""code"": 1,
  ""msg"": ""ok"",
  ""config_version"": 2,
  ""app"": {
    ""name"": ""Lantern Dragons"",
    ""min_app_version"": ""1.0.0"",
    ""force_update"": false,
    ""update_url"": ""https://play.google.com/store/apps/details?id=in.dicodeinfotech.arrowdragonpuzzle"",
    ""maintenance_mode"": false,
    ""maintenance_text"": """"
  },
  ""links"": {
    ""rate_us"": ""https://play.google.com/store/apps/details?id=in.dicodeinfotech.arrowdragonpuzzle"",
    ""privacy"": ""https://example.com/arrowdragon/privacy"",
    ""terms"": ""https://example.com/arrowdragon/terms"",
    ""support_email"": ""support@example.com"",
    ""more_games"": """",
    ""share_text"": ""Slide the dragons off the board! Play Lantern Dragons:"",
    ""share_url"": ""https://play.google.com/store/apps/details?id=in.dicodeinfotech.arrowdragonpuzzle""
  },
  ""ads"": {
    ""enabled"": true,
    ""test_mode"": false,
    ""units"": {
      ""banner"": ""ca-app-pub-3940256099942544/6300978111"",
      ""banner_large"": ""ca-app-pub-3940256099942544/6300978112"",
      ""native"": ""ca-app-pub-3940256099942544/2247696110"",
      ""native_preload"": ""ca-app-pub-3940256099942544/2247696111"",
      ""interstitial"": ""ca-app-pub-3940256099942544/1033173712"",
      ""rewarded"": ""ca-app-pub-3940256099942544/5224354917""
    },
    ""formats"": {
      ""banner"": { ""enabled"": true },
      ""native"": { ""enabled"": true, ""refresh_sec"": 30, ""max_age_min"": 55 },
      ""interstitial"": { ""enabled"": true },
      ""rewarded"": { ""enabled"": true }
    },
    ""placements"": {
      ""splash"": { ""type"": ""none"" },
      ""home"": { ""type"": ""native_small"", ""show_after_level"": 0 },
      ""level_map"": { ""type"": ""none"" },
      ""play"": { ""type"": ""banner"", ""position"": ""bottom"", ""show_after_level"": 3 },
      ""win_sheet"": { ""type"": ""native_big"", ""show_after_level"": 0 },
      ""lose_sheet"": { ""type"": ""native_small"", ""show_after_level"": 0 },
      ""options"": { ""type"": ""native_small"", ""show_after_level"": 0 }
    },
    ""interstitial_rules"": {
      ""min_click_count"": 3,
      ""min_seconds_between"": 45,
      ""every_n_levels_on_next"": 2,
      ""on_back_to_home"": true,
      ""skip_first_levels"": 3,
      ""max_per_session"": 12
    },
    ""rewarded_rules"": {
      ""hint"": { ""enabled"": true },
      ""extra_life"": { ""enabled"": false },
      ""skip_level"": { ""enabled"": false }
    },
    ""test_device_ids"": []
  },
  ""game"": {
    ""lives"": { ""max"": 3 },
    ""hints"": { ""free_count"": 3, ""reset_on_change"": true },
    ""locks"": { ""intro_level"": 6 },
    ""difficulty_offset"": 0
  },
  ""ui"": {
    ""options_rows"": {
      ""vibration"": true,
      ""sound"": true,
      ""native_refresh_rate"": true,
      ""reduce_motion"": true,
      ""rate_us"": true,
      ""privacy"": true,
      ""terms"": true
    },
    ""reduce_motion_default"": false,
    ""confetti"": true,
    ""level_map"": true
  },
  ""message"": {
    ""banner_text"": """",
    ""banner_url"": """"
  }
}";

    private const string Minimal = @"{ ""code"": 1, ""config_version"": 2 }";

    [SetUp]
    public void SetUp()
    {
        GameConfigService.ResetToDefaults();
    }

    [TearDown]
    public void TearDown()
    {
        GameConfigService.ResetToDefaults();
    }

    private static GameConfig Parse(string json)
    {
        Assert.IsTrue(GameConfigParser.TryParse(json, out GameConfig config, out string reason), reason);
        return config;
    }

    private static PromoSettings SettingsFor(string json, bool bakedTestIds = false)
    {
        PromoConfig baked = ScriptableObject.CreateInstance<PromoConfig>();
        baked.useTestIds = bakedTestIds;
        GameConfigService.Set(Parse(json));
        return new PromoSettings(baked);
    }

    // ---------- parsing ----------

    [Test]
    public void Parse_SampleJson_ReadsEveryField()
    {
        GameConfig c = Parse(SampleJson);

        Assert.AreEqual("Lantern Dragons", c.app.name);
        Assert.AreEqual("1.0.0", c.app.min_app_version);
        Assert.IsFalse(c.app.force_update);
        Assert.IsFalse(c.app.maintenance_mode);
        StringAssert.Contains("arrowdragonpuzzle", c.app.update_url);

        Assert.AreEqual("https://example.com/arrowdragon/privacy", c.links.privacy);
        Assert.AreEqual("https://example.com/arrowdragon/terms", c.links.terms);
        Assert.AreEqual("support@example.com", c.links.support_email);
        Assert.AreEqual("", c.links.more_games);

        Assert.IsTrue(c.ads.enabled);
        Assert.IsFalse(c.ads.test_mode);
        Assert.AreEqual("ca-app-pub-3940256099942544/6300978111", c.ads.units.banner);
        Assert.AreEqual("ca-app-pub-3940256099942544/6300978112", c.ads.units.banner_large);
        Assert.AreEqual("ca-app-pub-3940256099942544/2247696111", c.ads.units.native_preload);
        Assert.AreEqual("ca-app-pub-3940256099942544/5224354917", c.ads.units.rewarded);
        Assert.AreEqual(30, c.ads.formats.native.refresh_sec);
        Assert.AreEqual(55, c.ads.formats.native.max_age_min);
        Assert.AreEqual(3, c.ads.placements.play.show_after_level);
        Assert.AreEqual("bottom", c.ads.placements.play.position);

        Assert.AreEqual(3, c.ads.interstitial_rules.min_click_count);
        Assert.AreEqual(45, c.ads.interstitial_rules.min_seconds_between);
        Assert.AreEqual(2, c.ads.interstitial_rules.every_n_levels_on_next);
        Assert.IsTrue(c.ads.interstitial_rules.on_back_to_home);
        Assert.AreEqual(3, c.ads.interstitial_rules.skip_first_levels);
        Assert.AreEqual(12, c.ads.interstitial_rules.max_per_session);

        Assert.IsTrue(c.ads.rewarded_rules.hint.enabled);
        Assert.IsFalse(c.ads.rewarded_rules.extra_life.enabled);
        Assert.IsFalse(c.ads.rewarded_rules.skip_level.enabled);
        Assert.AreEqual(0, c.ads.test_device_ids.Count);

        Assert.AreEqual(3, c.game.lives.max);
        Assert.AreEqual(3, c.game.hints.free_count);
        Assert.IsTrue(c.game.hints.reset_on_change);
        Assert.AreEqual(6, c.game.locks.intro_level);
        Assert.AreEqual(0, c.game.difficulty_offset);

        Assert.IsTrue(c.ui.options_rows.vibration);
        Assert.IsTrue(c.ui.options_rows.terms);
        Assert.IsFalse(c.ui.reduce_motion_default);
        Assert.IsTrue(c.ui.confetti);
        Assert.IsTrue(c.ui.level_map);

        Assert.AreEqual("", c.message.banner_text);
    }

    [TestCase("none", false, PromoSlotKind.None)]
    [TestCase("banner", false, PromoSlotKind.BannerStandard)]
    [TestCase("banner_large", false, PromoSlotKind.BannerLarge)]
    [TestCase("native_small", false, PromoSlotKind.NativeSmall)]
    [TestCase("native_big", false, PromoSlotKind.NativeBig)]
    [TestCase("NATIVE_BIG", false, PromoSlotKind.NativeBig)]
    [TestCase("whatever", false, PromoSlotKind.None)]
    [TestCase("", false, PromoSlotKind.None)]
    [TestCase("banner", true, PromoSlotKind.None)]
    [TestCase("banner_large", true, PromoSlotKind.None)]
    [TestCase("native_big", true, PromoSlotKind.NativeBig)]
    public void SlotFor_PlacementType_MapsToSlotKind(string type, bool nativeOnly, PromoSlotKind expected)
    {
        Assert.AreEqual(expected, GameConfigParser.SlotFor(type, nativeOnly));
    }

    [Test]
    public void Parse_BannerOnAResultSheet_ResolvesToNone()
    {
        GameConfig c = Parse(@"{ ""code"": 1, ""config_version"": 2, ""ads"": { ""enabled"": true, ""placements"": {
            ""win_sheet"": { ""type"": ""banner"" }, ""lose_sheet"": { ""type"": ""banner_large"" },
            ""home"": { ""type"": ""banner"" } } } }");

        Assert.AreEqual("none", c.ads.placements.win_sheet.type);
        Assert.AreEqual("none", c.ads.placements.lose_sheet.type);
        Assert.AreEqual("banner", c.ads.placements.home.type);
    }

    [Test]
    public void Parse_UnknownPlacementType_BecomesNone()
    {
        GameConfig c = Parse(@"{ ""code"": 1, ""config_version"": 2, ""ads"": { ""placements"": {
            ""home"": { ""type"": ""video_wall"" } } } }");

        Assert.AreEqual("none", c.ads.placements.home.type);
    }

    [Test]
    public void Parse_MissingSections_KeepTheDefaults()
    {
        GameConfig c = Parse(Minimal);

        Assert.IsFalse(c.ads.enabled);
        Assert.IsTrue(c.ads.formats.banner.enabled);
        Assert.AreEqual(30, c.ads.formats.native.refresh_sec);
        Assert.AreEqual(3, c.game.lives.max);
        Assert.AreEqual(3, c.game.hints.free_count);
        Assert.IsTrue(c.game.hints.reset_on_change);
        Assert.IsTrue(c.ui.confetti);
        Assert.IsTrue(c.ui.level_map);
        Assert.IsTrue(c.ui.options_rows.privacy);
        Assert.IsTrue(c.ads.rewarded_rules.hint.enabled);
        Assert.AreEqual("none", c.ads.placements.home.type);
        Assert.AreEqual("", c.links.privacy);
    }

    [Test]
    public void Parse_PartialSection_KeepsTheOtherDefaults()
    {
        GameConfig c = Parse(@"{ ""code"": 1, ""config_version"": 2, ""game"": { ""hints"": { ""free_count"": 7 } },
            ""ui"": { ""confetti"": false } }");

        Assert.AreEqual(7, c.game.hints.free_count);
        Assert.IsTrue(c.game.hints.reset_on_change, "a key the payload left out keeps its default");
        Assert.AreEqual(3, c.game.lives.max);
        Assert.IsFalse(c.ui.confetti);
        Assert.IsTrue(c.ui.level_map, "a sibling key of a present section keeps its default");
        Assert.IsTrue(c.ui.options_rows.sound);
    }

    [Test]
    public void Parse_UnknownConfigVersion_IsIgnored()
    {
        Assert.IsFalse(GameConfigParser.TryParse(@"{ ""code"": 1, ""config_version"": 3 }", out GameConfig c, out string reason));
        Assert.IsNull(c);
        StringAssert.Contains("config_version", reason);
    }

    [Test]
    public void Parse_MissingConfigVersion_IsIgnored()
    {
        Assert.IsFalse(GameConfigParser.TryParse(@"{ ""code"": 1 }", out _, out _));
    }

    [Test]
    public void Parse_CodeNotOne_IsIgnored()
    {
        Assert.IsFalse(GameConfigParser.TryParse(@"{ ""code"": 0, ""config_version"": 2 }", out _, out string reason));
        StringAssert.Contains("code", reason);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("not json at all")]
    [TestCase(@"{ ""code"": ")]
    public void Parse_UnusablePayload_IsIgnored(string json)
    {
        Assert.IsFalse(GameConfigParser.TryParse(json, out GameConfig c, out string reason));
        Assert.IsNull(c);
        Assert.IsNotEmpty(reason);
    }

    [Test]
    public void Parse_InvalidUnitIds_AreRejected()
    {
        GameConfig c = Parse(@"{ ""code"": 1, ""config_version"": 2, ""ads"": { ""units"": {
            ""banner"": ""not-an-ad-unit"", ""interstitial"": ""ca-app-pub-1234"",
            ""rewarded"": ""ca-app-pub-3940256099942544/5224354917"" } } }");

        Assert.AreEqual("", c.ads.units.banner);
        Assert.AreEqual("", c.ads.units.interstitial, "no slash means it is not a unit id");
        Assert.AreEqual("ca-app-pub-3940256099942544/5224354917", c.ads.units.rewarded);
    }

    [Test]
    public void Parse_NegativeNumbers_AreClampedToZero()
    {
        GameConfig c = Parse(@"{ ""code"": 1, ""config_version"": 2, ""ads"": {
            ""formats"": { ""native"": { ""refresh_sec"": -5, ""max_age_min"": -1 } },
            ""placements"": { ""home"": { ""type"": ""native_small"", ""show_after_level"": -9 } },
            ""interstitial_rules"": { ""min_click_count"": -1, ""min_seconds_between"": -30,
                ""every_n_levels_on_next"": -2, ""skip_first_levels"": -4, ""max_per_session"": -1 } },
            ""game"": { ""hints"": { ""free_count"": -3 } } }");

        Assert.AreEqual(0, c.ads.formats.native.refresh_sec);
        Assert.AreEqual(0, c.ads.formats.native.max_age_min);
        Assert.AreEqual(0, c.ads.placements.home.show_after_level);
        Assert.AreEqual(0, c.ads.interstitial_rules.min_click_count);
        Assert.AreEqual(0, c.ads.interstitial_rules.min_seconds_between);
        Assert.AreEqual(0, c.ads.interstitial_rules.every_n_levels_on_next);
        Assert.AreEqual(0, c.ads.interstitial_rules.skip_first_levels);
        Assert.AreEqual(0, c.ads.interstitial_rules.max_per_session);
        Assert.AreEqual(0, c.game.hints.free_count);
    }

    [TestCase(0, 3)]
    [TestCase(-1, 3)]
    [TestCase(1, 1)]
    [TestCase(5, 5)]
    [TestCase(9, GameConfigParser.MaxLivesCap)]
    public void Parse_LivesMax_IsKeptInTheRangeTheHudFits(int sent, int expected)
    {
        GameConfig c = Parse(@"{ ""code"": 1, ""config_version"": 2, ""game"": { ""lives"": { ""max"": " + sent + " } } }");

        Assert.AreEqual(expected, c.game.lives.max);
    }

    [Test]
    public void Defaults_HaveAdsOffNoLinksAndTheShippedTuning()
    {
        GameConfig c = GameConfig.Defaults();

        Assert.IsFalse(c.ads.enabled);
        Assert.AreEqual("", c.links.privacy);
        Assert.AreEqual(3, c.game.lives.max);
        Assert.AreEqual(3, c.game.hints.free_count);
        Assert.IsTrue(c.ui.confetti);
        Assert.IsFalse(c.app.force_update);
        Assert.IsFalse(c.app.maintenance_mode);
    }

    // ---------- the ad rules ----------

    [Test]
    public void PlacementActive_BeforeShowAfterLevel_IsOff()
    {
        var placement = new AdPlacement { type = "banner", show_after_level = 3 };

        Assert.IsFalse(AdRules.PlacementActive(placement, 0));
        Assert.IsFalse(AdRules.PlacementActive(placement, 2));
        Assert.IsTrue(AdRules.PlacementActive(placement, 3));
        Assert.IsTrue(AdRules.PlacementActive(placement, 40));
        Assert.IsFalse(AdRules.PlacementActive(null, 40));
    }

    [Test]
    public void InterstitialsUnlocked_RespectsSkipFirstLevels()
    {
        var rules = new InterstitialRules { skip_first_levels = 3 };

        Assert.IsFalse(AdRules.InterstitialsUnlocked(rules, 0));
        Assert.IsFalse(AdRules.InterstitialsUnlocked(rules, 2));
        Assert.IsTrue(AdRules.InterstitialsUnlocked(rules, 3));
        Assert.IsTrue(AdRules.InterstitialsUnlocked(new InterstitialRules { skip_first_levels = 0 }, 0));
    }

    [Test]
    public void SessionCapReached_ZeroMeansNoCap()
    {
        var capped = new InterstitialRules { max_per_session = 2 };
        var uncapped = new InterstitialRules { max_per_session = 0 };

        Assert.IsFalse(AdRules.SessionCapReached(capped, 1));
        Assert.IsTrue(AdRules.SessionCapReached(capped, 2));
        Assert.IsTrue(AdRules.SessionCapReached(capped, 5));
        Assert.IsFalse(AdRules.SessionCapReached(uncapped, 999));
    }

    [Test]
    public void ClickGates_ZeroSwitchesThatGateOffInsteadOfSatisfyingIt()
    {
        var timeOnly = new InterstitialRules { min_click_count = 0, min_seconds_between = 30 };
        var countOnly = new InterstitialRules { min_click_count = 3, min_seconds_between = 0 };
        var neither = new InterstitialRules { min_click_count = 0, min_seconds_between = 0 };

        Assert.IsFalse(AdRules.CountGateMet(timeOnly, 99), "0 clicks required means the count trigger is off");
        Assert.IsTrue(AdRules.TimeGateMet(timeOnly, 30f));
        Assert.IsFalse(AdRules.TimeGateMet(timeOnly, 29.9f));

        Assert.IsFalse(AdRules.TimeGateMet(countOnly, 9999f), "0 seconds means the time trigger is off");
        Assert.IsFalse(AdRules.CountGateMet(countOnly, 2));
        Assert.IsTrue(AdRules.CountGateMet(countOnly, 3));

        Assert.IsTrue(AdRules.ClickAdsOff(neither));
        Assert.IsFalse(AdRules.ClickAdsOff(timeOnly));
        Assert.IsFalse(AdRules.ClickAdsOff(countOnly));
    }

    [TestCase(2, 1, false)]
    [TestCase(2, 2, true)]
    [TestCase(2, 3, false)]
    [TestCase(2, 4, true)]
    [TestCase(0, 4, false)]
    [TestCase(3, 0, false)]
    [TestCase(1, 1, true)]
    public void LevelAdDue_EveryNthLevel(int everyN, int completed, bool due)
    {
        var rules = new InterstitialRules { every_n_levels_on_next = everyN };

        Assert.AreEqual(due, AdRules.LevelAdDue(rules, completed));
    }

    // ---------- PromoSettings reads the config ----------

    [Test]
    public void Settings_DefaultsWithNoPayload_KeepEveryAdOff()
    {
        PromoConfig baked = ScriptableObject.CreateInstance<PromoConfig>();
        baked.useTestIds = false;
        var settings = new PromoSettings(baked);

        Assert.IsFalse(settings.AdsEnabled);
        Assert.IsFalse(settings.HasRemote);
        Assert.IsFalse(settings.InterstitialEnabled);
        Assert.IsFalse(settings.RewardedEnabled);
        Assert.IsNull(settings.InterstitialId);
        Assert.AreEqual(PromoSlotKind.None, settings.SlotFor(PromoScreen.Home));
    }

    [Test]
    public void Settings_TestMode_UsesGoogleTestUnitsAndTurnsAdsOn()
    {
        PromoSettings viaBaked = SettingsFor(Minimal, bakedTestIds: true);
        Assert.IsTrue(viaBaked.AdsEnabled);
        Assert.AreEqual(PromoTestIds.Banner, viaBaked.BannerId);
        Assert.AreEqual(PromoTestIds.Native, viaBaked.NativeId);

        PromoSettings viaConfig = SettingsFor(@"{ ""code"": 1, ""config_version"": 2, ""ads"": { ""test_mode"": true } }");
        Assert.IsTrue(viaConfig.UsingTestIds);
        Assert.IsTrue(viaConfig.AdsEnabled);
        Assert.AreEqual(PromoTestIds.Interstitial, viaConfig.InterstitialId);
    }

    [Test]
    public void Settings_FromSample_ReadsUnitsFlagsAndRules()
    {
        PromoSettings s = SettingsFor(SampleJson);

        Assert.IsTrue(s.AdsEnabled);
        Assert.IsFalse(s.UsingTestIds);
        Assert.AreEqual("ca-app-pub-3940256099942544/1033173712", s.InterstitialId);
        Assert.AreEqual("ca-app-pub-3940256099942544/6300978111", s.BannerIdFor(false));
        Assert.AreEqual("ca-app-pub-3940256099942544/6300978112", s.BannerIdFor(true));
        Assert.AreEqual("ca-app-pub-3940256099942544/2247696111", s.PreloadNativeId);
        Assert.IsTrue(s.InterstitialEnabled);
        Assert.IsTrue(s.RewardedEnabled);
        Assert.IsTrue(s.HintRewardedEnabled);
        Assert.IsTrue(s.BackAdEnabled);
        Assert.AreEqual(2, s.AdLevelCount);
        Assert.AreEqual(3, s.AdClickCount);
        Assert.AreEqual(45, s.AdClickTimeSec);
        Assert.AreEqual(30, s.NativeRefreshSeconds);
        Assert.AreEqual(55 * 60f, s.AdMaxAgeSeconds);
    }

    [Test]
    public void Settings_LargeBannerWithoutItsOwnUnit_ReusesTheStandardOne()
    {
        PromoSettings s = SettingsFor(@"{ ""code"": 1, ""config_version"": 2, ""ads"": { ""enabled"": true,
            ""units"": { ""banner"": ""ca-app-pub-1/2"" } } }");

        Assert.AreEqual("ca-app-pub-1/2", s.BannerIdFor(true));
    }

    [Test]
    public void Settings_PreloadNativeWithoutItsOwnUnit_FallsBackToTheNativeUnit()
    {
        PromoSettings s = SettingsFor(@"{ ""code"": 1, ""config_version"": 2, ""ads"": { ""enabled"": true,
            ""units"": { ""native"": ""ca-app-pub-1/9"" } } }");

        Assert.AreEqual("ca-app-pub-1/9", s.PreloadNativeId);
    }

    [Test]
    public void Settings_MasterSwitchOff_TurnsEveryFormatOff()
    {
        PromoSettings s = SettingsFor(SampleJson.Replace(@"""enabled"": true,
    ""test_mode""", @"""enabled"": false,
    ""test_mode"""));

        Assert.IsFalse(s.AdsEnabled);
        Assert.IsFalse(s.InterstitialEnabled);
        Assert.IsFalse(s.RewardedEnabled);
        Assert.IsFalse(s.CanShow(PromoSlotKind.BannerStandard));
        Assert.IsFalse(s.CanShow(PromoSlotKind.NativeSmall));
    }

    [Test]
    public void Settings_FormatSwitchedOff_OnlyThatFormatStops()
    {
        PromoSettings s = SettingsFor(@"{ ""code"": 1, ""config_version"": 2, ""ads"": { ""enabled"": true,
            ""units"": { ""banner"": ""ca-app-pub-1/1"", ""native"": ""ca-app-pub-1/2"",
                         ""interstitial"": ""ca-app-pub-1/3"", ""rewarded"": ""ca-app-pub-1/4"" },
            ""formats"": { ""banner"": { ""enabled"": false }, ""rewarded"": { ""enabled"": false } } } }");

        Assert.IsFalse(s.CanShow(PromoSlotKind.BannerStandard));
        Assert.IsTrue(s.CanShow(PromoSlotKind.NativeSmall));
        Assert.IsTrue(s.InterstitialEnabled);
        Assert.IsFalse(s.RewardedEnabled);
    }

    [Test]
    public void Settings_HintRewardedRule_GatesOnlyHints()
    {
        PromoSettings s = SettingsFor(@"{ ""code"": 1, ""config_version"": 2, ""ads"": { ""enabled"": true,
            ""units"": { ""rewarded"": ""ca-app-pub-1/4"" },
            ""rewarded_rules"": { ""hint"": { ""enabled"": false } } } }");

        Assert.IsTrue(s.RewardedEnabled, "the format itself is still on");
        Assert.IsFalse(s.HintRewardedEnabled);
    }

    [Test]
    public void Settings_ShowAfterLevel_KeepsAPlacementOffUntilTheLevelIsCleared()
    {
        PromoSettings s = SettingsFor(SampleJson);

        GameConfigService.HighestCompletedProvider = () => 0;
        Assert.AreEqual(PromoSlotKind.None, s.SlotFor(PromoScreen.Play), "play starts after level 3");
        Assert.AreEqual(PromoSlotKind.NativeSmall, s.SlotFor(PromoScreen.Home));

        GameConfigService.HighestCompletedProvider = () => 3;
        Assert.AreEqual(PromoSlotKind.BannerStandard, s.SlotFor(PromoScreen.Play));
    }

    [Test]
    public void Settings_EveryScreenHasItsOwnPlacement()
    {
        GameConfigService.HighestCompletedProvider = () => 10;
        PromoSettings s = SettingsFor(SampleJson);

        Assert.AreEqual(PromoSlotKind.None, s.SlotFor(PromoScreen.Splash));
        Assert.AreEqual(PromoSlotKind.NativeSmall, s.SlotFor(PromoScreen.Home));
        Assert.AreEqual(PromoSlotKind.None, s.SlotFor(PromoScreen.LevelMap));
        Assert.AreEqual(PromoSlotKind.BannerStandard, s.SlotFor(PromoScreen.Play));
        Assert.AreEqual(PromoSlotKind.NativeBig, s.SlotFor(PromoScreen.WinSheet));
        Assert.AreEqual(PromoSlotKind.NativeSmall, s.SlotFor(PromoScreen.LoseSheet));
        Assert.AreEqual(PromoSlotKind.NativeSmall, s.SlotFor(PromoScreen.Options));
    }

    [Test]
    public void Settings_LevelEndPreload_UsesTheWinSlotWhenNativeElseTheLoseSlot()
    {
        GameConfigService.HighestCompletedProvider = () => 10;

        Assert.AreEqual(PromoScreen.WinSheet, SettingsFor(SampleJson).LevelEndPreloadScreen);

        PromoSettings winOff = SettingsFor(SampleJson.Replace(@"""win_sheet"": { ""type"": ""native_big"",", @"""win_sheet"": { ""type"": ""none"","));
        Assert.AreEqual(PromoScreen.LoseSheet, winOff.LevelEndPreloadScreen);
    }

    [Test]
    public void Settings_TestDeviceIds_MergeBakedAndConfigWithoutDuplicates()
    {
        PromoConfig baked = ScriptableObject.CreateInstance<PromoConfig>();
        baked.testDeviceIds.Add("AAA");
        baked.testDeviceIds.Add("BBB");
        GameConfigService.Set(Parse(@"{ ""code"": 1, ""config_version"": 2, ""ads"": { ""test_device_ids"": [""BBB"", ""CCC""] } }"));

        CollectionAssert.AreEquivalent(new[] { "AAA", "BBB", "CCC" }, new PromoSettings(baked).TestDeviceIds);
    }

    // ---------- links, options rows ----------

    [Test]
    public void LinkRowVisible_NeedsTheSwitchOnAndALink()
    {
        GameConfig c = Parse(SampleJson);
        Assert.IsTrue(GameConfigParser.LinkRowVisible(c, "privacy"));
        Assert.IsTrue(GameConfigParser.LinkRowVisible(c, "terms"));
        Assert.IsTrue(GameConfigParser.LinkRowVisible(c, "rate_us"));

        c.links.privacy = "";
        Assert.IsFalse(GameConfigParser.LinkRowVisible(c, "privacy"), "an empty link hides the row");

        c.ui.options_rows.terms = false;
        Assert.IsFalse(GameConfigParser.LinkRowVisible(c, "terms"), "the row switch hides it too");

        Assert.IsFalse(GameConfigParser.LinkRowVisible(c, "unknown_row"));
    }

    [Test]
    public void Defaults_HideEveryLinkRowBecauseThereAreNoLinks()
    {
        GameConfig c = GameConfig.Defaults();

        Assert.IsFalse(GameConfigParser.LinkRowVisible(c, "rate_us"));
        Assert.IsFalse(GameConfigParser.LinkRowVisible(c, "privacy"));
        Assert.IsFalse(GameConfigParser.LinkRowVisible(c, "terms"));
        Assert.IsTrue(c.ui.options_rows.sound, "the non-link rows stay visible");
    }

    [Test]
    public void OptionsRows_CanBeSwitchedOffIndividually()
    {
        GameConfig c = Parse(@"{ ""code"": 1, ""config_version"": 2, ""ui"": { ""options_rows"": {
            ""vibration"": false, ""reduce_motion"": false } } }");

        Assert.IsFalse(c.ui.options_rows.vibration);
        Assert.IsFalse(c.ui.options_rows.reduce_motion);
        Assert.IsTrue(c.ui.options_rows.sound);
        Assert.IsTrue(c.ui.options_rows.native_refresh_rate);
    }

    // ---------- the hint pool (game.hints) ----------

    [Test]
    public void ResolvePool_FirstRun_KeepsTheLeftoverPoolAndPinsTheCount()
    {
        int pool = ClueManager.ResolvePool(hasLastKey: false, lastApplied: 0, hasStoredRemaining: true,
                                           storedRemaining: 1, freeCount: 3, resetOnChange: true, out int last);

        Assert.AreEqual(1, pool);
        Assert.AreEqual(3, last);
    }

    [Test]
    public void ResolvePool_FirstRunWithNothingStored_StartsAtTheFreeCount()
    {
        int pool = ClueManager.ResolvePool(false, 0, false, 0, 4, true, out int last);

        Assert.AreEqual(4, pool);
        Assert.AreEqual(4, last);
    }

    [Test]
    public void ResolvePool_UnchangedCount_KeepsTheLeftovers()
    {
        int pool = ClueManager.ResolvePool(true, 3, true, 1, 3, true, out int last);

        Assert.AreEqual(1, pool);
        Assert.AreEqual(3, last);
    }

    [Test]
    public void ResolvePool_ChangedCountWithResetOn_ResetsThePool()
    {
        int pool = ClueManager.ResolvePool(true, 3, true, 1, 5, resetOnChange: true, out int last);

        Assert.AreEqual(5, pool);
        Assert.AreEqual(5, last);
    }

    [Test]
    public void ResolvePool_ChangedCountWithResetOff_KeepsTheLeftoversButRemembersTheNewCount()
    {
        int pool = ClueManager.ResolvePool(true, 3, true, 1, 5, resetOnChange: false, out int last);

        Assert.AreEqual(1, pool, "reset_on_change false leaves the pool alone");
        Assert.AreEqual(5, last, "but the new count becomes the baseline");
    }

    // ---------- the service and its cache ----------

    [Test]
    public void Service_ApplyJson_ReplacesTheConfigAndRaisesChanged()
    {
        int raised = 0;
        void Handler() { raised++; }
        GameConfigService.Changed += Handler;

        try
        {
            Assert.IsFalse(GameConfigService.HasPayload);
            Assert.IsTrue(GameConfigService.ApplyJson(SampleJson));

            Assert.IsTrue(GameConfigService.HasPayload);
            Assert.IsTrue(GameConfigService.Current.ads.enabled);
            Assert.AreEqual(1, raised);
        }
        finally
        {
            GameConfigService.Changed -= Handler;
        }
    }

    [Test]
    public void Service_BadPayload_ChangesNothing()
    {
        GameConfigService.ApplyJson(SampleJson);
        GameConfig before = GameConfigService.Current;

        Assert.IsFalse(GameConfigService.ApplyJson(@"{ ""code"": 1, ""config_version"": 99 }"));
        Assert.AreSame(before, GameConfigService.Current);
        Assert.IsTrue(GameConfigService.Current.ads.enabled);
    }

    [Test]
    public void Service_Cache_RoundTripsThroughAnInjectedPath()
    {
        string path = Path.Combine(Path.GetTempPath(), "lanterndragons_config_test_" + Guid.NewGuid().ToString("N") + ".json");
        GameConfigService.CachePathOverride = path;

        try
        {
            Assert.IsFalse(GameConfigService.LoadCache(), "no cache file yet");

            GameConfigService.WriteCache(SampleJson);
            GameConfigService.ResetToDefaults();
            GameConfigService.CachePathOverride = path;

            Assert.IsTrue(GameConfigService.LoadCache());
            Assert.IsTrue(GameConfigService.HasPayload);
            Assert.AreEqual("Lantern Dragons", GameConfigService.Current.app.name);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Test]
    public void Service_CorruptCache_IsIgnoredAndLeavesTheDefaults()
    {
        string path = Path.Combine(Path.GetTempPath(), "lanterndragons_config_test_" + Guid.NewGuid().ToString("N") + ".json");
        GameConfigService.CachePathOverride = path;

        try
        {
            File.WriteAllText(path, "{ this is not json");

            Assert.IsFalse(GameConfigService.LoadCache());
            Assert.IsFalse(GameConfigService.HasPayload);
            Assert.IsFalse(GameConfigService.Current.ads.enabled);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
