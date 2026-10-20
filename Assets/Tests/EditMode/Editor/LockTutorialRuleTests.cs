using System;
using NUnit.Framework;

public class LockTutorialRuleTests
{
    // ---- first padlock level ----

    [Test]
    public void FirstLockLevelIndex_ReturnsTheFirstLevelThatHasALock()
    {
        int first = LockTutorialRule.FirstLockLevelIndex(i => i >= 5, 100);

        Assert.AreEqual(5, first);
    }

    [Test]
    public void FirstLockLevelIndex_NoLevelHasALock_ReturnsMinusOne()
    {
        Assert.AreEqual(-1, LockTutorialRule.FirstLockLevelIndex(i => false, 100));
    }

    [Test]
    public void FirstLockLevelIndex_OnlyScansTheCampaignThatExists()
    {
        // The lock sits at index 50 but the campaign only has 10 levels.
        Assert.AreEqual(-1, LockTutorialRule.FirstLockLevelIndex(i => i == 50, 10));
    }

    [Test]
    public void FirstLockLevelIndex_StopsAtTheScanWindow()
    {
        int asked = 0;
        int first = LockTutorialRule.FirstLockLevelIndex(i => { asked++; return false; }, 5000);

        Assert.AreEqual(-1, first);
        Assert.AreEqual(LockTutorialRule.MaxScan, asked);
    }

    [Test]
    public void FirstLockLevelIndex_NullPredicate_ReturnsMinusOne()
    {
        Assert.AreEqual(-1, LockTutorialRule.FirstLockLevelIndex(null, 100));
    }

    [Test]
    public void FirstLockLevelIndex_ZeroLevels_ReturnsMinusOne()
    {
        Assert.AreEqual(-1, LockTutorialRule.FirstLockLevelIndex(i => true, 0));
    }

    // ---- when to show ----

    [Test]
    public void ShouldShow_FirstLockLevelNeverBeaten_IsTrue()
    {
        Assert.IsTrue(LockTutorialRule.ShouldShow(true, false, currentLevelIndex: 5, firstLockLevelIndex: 5,
                                                  highestCompletedLevelNumber: 5));
    }

    [Test]
    public void ShouldShow_ConfigOff_IsFalse()
    {
        Assert.IsFalse(LockTutorialRule.ShouldShow(false, false, 5, 5, 5));
    }

    [Test]
    public void ShouldShow_AlreadySeen_IsFalse()
    {
        Assert.IsFalse(LockTutorialRule.ShouldShow(true, true, 5, 5, 5));
    }

    [Test]
    public void ShouldShow_NotOnTheFirstLockLevel_IsFalse()
    {
        Assert.IsFalse(LockTutorialRule.ShouldShow(true, false, currentLevelIndex: 4, firstLockLevelIndex: 5,
                                                   highestCompletedLevelNumber: 4));
        Assert.IsFalse(LockTutorialRule.ShouldShow(true, false, currentLevelIndex: 6, firstLockLevelIndex: 5,
                                                   highestCompletedLevelNumber: 5));
    }

    [Test]
    public void ShouldShow_FirstLockLevelAlreadyBeaten_IsFalse()
    {
        // An old save: level number 6 (index 5) was beaten before the tutorial existed.
        Assert.IsFalse(LockTutorialRule.ShouldShow(true, false, currentLevelIndex: 5, firstLockLevelIndex: 5,
                                                   highestCompletedLevelNumber: 6));
        Assert.IsFalse(LockTutorialRule.ShouldShow(true, false, currentLevelIndex: 5, firstLockLevelIndex: 5,
                                                   highestCompletedLevelNumber: 30));
    }

    [Test]
    public void ShouldShow_CampaignHasNoLock_IsFalse()
    {
        Assert.IsFalse(LockTutorialRule.ShouldShow(true, false, 0, -1, 0));
    }

    // ---- words ----

    [Test]
    public void BodyText_UsesTheRealCountAndPluralises()
    {
        StringAssert.Contains("Free 4 other dragons", LockTutorialRule.BodyText(4));
        StringAssert.Contains("Free 1 other dragon and", LockTutorialRule.BodyText(1));
    }

    [Test]
    public void RemainingLabel_NeverShowsZeroOrNegative()
    {
        Assert.AreEqual("Free 3 more", LockTutorialRule.RemainingLabel(3));
        Assert.AreEqual("Free 1 more", LockTutorialRule.RemainingLabel(0));
    }

    // ---- saved flag ----

    [Test]
    public void ProgressData_LockTutorialSeen_DefaultsToFalseForAnOldSave()
    {
        // A save written before the field existed: JsonUtility leaves the new field at its default.
        ProgressData old = UnityEngine.JsonUtility.FromJson<ProgressData>(
            "{\"version\":1,\"currentLevelIndex\":3,\"highestLevelCompleted\":3}");

        Assert.IsFalse(old.lockTutorialSeen);
        Assert.AreEqual(3, old.highestLevelCompleted);
    }

    [Test]
    public void ProgressData_LockTutorialSeen_RoundTripsThroughJson()
    {
        ProgressData data = new ProgressData { lockTutorialSeen = true, highestLevelCompleted = 7 };

        ProgressData back = UnityEngine.JsonUtility.FromJson<ProgressData>(UnityEngine.JsonUtility.ToJson(data));

        Assert.IsTrue(back.lockTutorialSeen);
        Assert.AreEqual(7, back.highestLevelCompleted);
    }

    // ---- config default ----

    [Test]
    public void Config_LockTutorial_DefaultsOnAndCanBeSwitchedOff()
    {
        Assert.IsTrue(GameConfig.Defaults().ui.lock_tutorial);

        Assert.IsTrue(GameConfigParser.TryParse(
            "{ \"code\": 1, \"config_version\": 2, \"ui\": { \"lock_tutorial\": false } }",
            out GameConfig off, out string reason), reason);

        Assert.IsFalse(off.ui.lock_tutorial);
        Assert.IsTrue(off.ui.confetti, "a sibling key keeps its default");
    }
}
