using NUnit.Framework;

// Story 005 (level map): where PLAY points after a replay, and when a win is reported to analytics.
// FrontierRule is pure, so none of this needs a scene.
public class FrontierRuleTests
{
    const int Total = 1500;

    [Test]
    public void FrontierLevel_NothingBeaten_IsLevelOne()
    {
        Assert.AreEqual(1, FrontierRule.FrontierLevel(0));
    }

    [Test]
    public void FrontierLevel_NegativeHighest_IsTreatedAsNothingBeaten()
    {
        Assert.AreEqual(1, FrontierRule.FrontierLevel(-5));
    }

    [Test]
    public void FrontierLevel_TenBeaten_IsLevelEleven()
    {
        Assert.AreEqual(11, FrontierRule.FrontierLevel(10));
    }

    [Test]
    public void HighestAfter_ReplayingAnOldLevel_NeverWalksProgressBackwards()
    {
        Assert.AreEqual(10, FrontierRule.HighestAfter(10, 3));
    }

    [Test]
    public void HighestAfter_BeatingTheFrontier_AdvancesProgress()
    {
        Assert.AreEqual(11, FrontierRule.HighestAfter(10, 11));
    }

    [Test]
    public void FrontierIndex_MidCampaign_IsTheNumberOfLevelsBeaten()
    {
        // Index 10 is level 11, the frontier when ten are beaten.
        Assert.AreEqual(10, FrontierRule.FrontierIndex(10, Total));
    }

    [Test]
    public void FrontierIndex_AfterReplayingAnOldLevel_StillPointsAtTheFrontier()
    {
        // Replaying level 3 banks nothing new, so PLAY must point at level 11, never level 4.
        int highest = FrontierRule.HighestAfter(10, 3);

        Assert.AreEqual(10, FrontierRule.FrontierIndex(highest, Total));
    }

    [Test]
    public void FrontierIndex_WholeCampaignBeaten_WrapsToTheFirstLevel()
    {
        Assert.AreEqual(0, FrontierRule.FrontierIndex(Total, Total));
    }

    [Test]
    public void FrontierIndex_CampaignSizeUnknown_DoesNotWrap()
    {
        Assert.AreEqual(7, FrontierRule.FrontierIndex(7, 0));
    }

    [Test]
    public void IsFrontierCompletion_FirstLevelOnAFreshSave_IsNewProgress()
    {
        Assert.IsTrue(FrontierRule.IsFrontierCompletion(0, 1));
    }

    [Test]
    public void IsFrontierCompletion_TheFrontierLevel_IsNewProgress()
    {
        Assert.IsTrue(FrontierRule.IsFrontierCompletion(10, 11));
    }

    [Test]
    public void IsFrontierCompletion_ReplayOfABeatenLevel_IsNotReported()
    {
        Assert.IsFalse(FrontierRule.IsFrontierCompletion(10, 3));
    }

    [Test]
    public void IsFrontierCompletion_TheLastBeatenLevel_IsNotReportedAgain()
    {
        Assert.IsFalse(FrontierRule.IsFrontierCompletion(10, 10));
    }

    [Test]
    public void IsFrontierCompletion_LevelPastTheFrontier_IsNotReported()
    {
        Assert.IsFalse(FrontierRule.IsFrontierCompletion(10, 12));
    }
}
