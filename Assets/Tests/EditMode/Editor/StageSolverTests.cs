using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Story 001 / 002: the solver (StageRating.Measure) is the gate that stops a level with a lock
// that can never open from shipping. It also models the runtime rule "a padlocked arrow does not
// move until enough other arrows have left".
public class StageSolverTests
{
    const int Width = 6;

    [Test]
    public void Measure_NoLocks_ClearsEveryFreeDragonInOneWave()
    {
        var dragons = TestStages.RowDragons(2);

        StageRating.Result r = StageRating.Measure(dragons, Width, 2);

        Assert.IsTrue(r.solvable);
        Assert.AreEqual(1, r.waves);
    }

    [Test]
    public void Measure_LockedFreeDragon_WaitsForALaterWave()
    {
        // Arrow 1's ray is clear, but it needs one other arrow to leave first.
        var dragons = TestStages.RowDragons(2);

        StageRating.Result r = StageRating.Measure(dragons, Width, 2, new[] { 0, 1 });

        Assert.IsTrue(r.solvable);
        Assert.AreEqual(2, r.waves, "the locked arrow must not clear in wave 1");
    }

    [Test]
    public void Measure_ThresholdExactlyDragonsMinusOne_IsStillSolvable()
    {
        var dragons = TestStages.RowDragons(4);

        StageRating.Result r = StageRating.Measure(dragons, Width, 4, new[] { 0, 0, 0, 3 });

        Assert.IsTrue(r.solvable);
        Assert.AreEqual(2, r.waves);
    }

    [Test]
    public void Measure_ThresholdAboveTheNumberOfOtherDragons_IsReportedUnsolvable()
    {
        // Arrow 1 needs five arrows to leave, but only one other arrow exists.
        var dragons = TestStages.RowDragons(2);

        StageRating.Result r = StageRating.Measure(dragons, Width, 2, new[] { 0, 5 });

        Assert.IsFalse(r.solvable);
    }

    [Test]
    public void Measure_MutuallyWaitingLocks_AreReportedUnsolvable()
    {
        // Each arrow waits for the other, so neither can ever move.
        var dragons = TestStages.RowDragons(2);

        StageRating.Result r = StageRating.Measure(dragons, Width, 2, new[] { 1, 1 });

        Assert.IsFalse(r.solvable);
    }

    [Test]
    public void Measure_BlockedDragonBehindAnother_NeedsTheBlockerToLeaveFirst()
    {
        // Arrow A heads right along row 0 and runs straight into arrow B, which stands upright at
        // x = 3. B's own ray (3,2), (3,3) is clear, so B leaves in wave 1 and A in wave 2.
        var dragons = new List<List<Vector2Int>>
        {
            TestStages.Path(new Vector2Int(0, 0), 2),    // A: (0,0)-(1,0), head right, ray hits (3,0)
            TestStages.Path(new Vector2Int(3, 0), 0),    // B: (3,0)-(3,1), head up, clear ray
        };

        StageRating.Result r = StageRating.Measure(dragons, Width, 4);

        Assert.IsTrue(r.solvable);
        Assert.AreEqual(2, r.waves);
    }

    [Test]
    public void Measure_AssignedLocksOnARowBoard_AreAlwaysSolvable()
    {
        var dragons = TestStages.RowDragons(40);

        for (int level = StageLocks.FirstLockLevelIndex; level < 700; level += 9)
        {
            int[] locks = StageLocks.Assign(level, dragons, Width, 40);
            Assert.IsTrue(StageRating.Measure(dragons, Width, 40, locks).solvable, "level index " + level);
        }
    }

    [Test]
    public void Band_HigherScore_NeverMapsToAnEasierBand()
    {
        StageTier previous = StageRating.Band(0);

        for (int score = 1; score <= 100; score++)
        {
            StageTier band = StageRating.Band(score);
            Assert.GreaterOrEqual((int)band, (int)previous, "score " + score);
            previous = band;
        }
    }
}
