using NUnit.Framework;

// The analytics config section and the pure helpers PlayerAnalytics uses to name and value events.
// Firebase itself is not started here: these tests do not touch the network or the SDK.
public class PlayerAnalyticsTests
{
    [Test]
    public void Defaults_SendEvents_NoManualAdRevenue_WithMilestones()
    {
        AnalyticsSection a = GameConfig.Defaults().analytics;

        Assert.IsTrue(a.enabled);
        Assert.IsFalse(a.log_ad_revenue);
        CollectionAssert.AreEqual(new[] { 5, 10, 20, 50, 100 }, a.milestone_levels);
    }

    [Test]
    public void PayloadWithoutAnalytics_KeepsDefaults()
    {
        Assert.IsTrue(GameConfigParser.TryParse(@"{ ""code"": 1, ""config_version"": 2 }",
            out GameConfig config, out string reason), reason);

        Assert.IsTrue(config.analytics.enabled);
        Assert.IsFalse(config.analytics.log_ad_revenue);
        CollectionAssert.AreEqual(new[] { 5, 10, 20, 50, 100 }, config.analytics.milestone_levels);
    }

    [Test]
    public void PayloadAnalytics_OverridesSwitchesAndMilestones()
    {
        const string json = @"{ ""code"": 1, ""config_version"": 2,
            ""analytics"": { ""enabled"": false, ""log_ad_revenue"": true, ""milestone_levels"": [3, 30] } }";

        Assert.IsTrue(GameConfigParser.TryParse(json, out GameConfig config, out string reason), reason);

        Assert.IsFalse(config.analytics.enabled);
        Assert.IsTrue(config.analytics.log_ad_revenue);
        CollectionAssert.AreEqual(new[] { 3, 30 }, config.analytics.milestone_levels);
    }

    [Test]
    public void MilestoneEventName_IsLevelNumberComplete()
    {
        Assert.AreEqual("level_10_complete", PlayerAnalytics.MilestoneEventName(10));
    }

    [Test]
    public void MicrosToValue_DividesByOneMillion()
    {
        Assert.AreEqual(0.00125d, PlayerAnalytics.MicrosToValue(1250), 1e-12);
        Assert.AreEqual(0d, PlayerAnalytics.MicrosToValue(0));
    }
}
