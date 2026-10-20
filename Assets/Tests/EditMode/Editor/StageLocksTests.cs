using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Story 001 (locked arrows): which arrows get a padlock, and that no threshold can ever trap the
// player. The rule under test lives in StageLocks.Assign.
public class StageLocksTests
{
    const int BoardWidth = 6;

    static int CountLocks(int[] locks)
    {
        int n = 0;
        foreach (int l in locks) if (l > 0) n++;
        return n;
    }

    [Test]
    public void Assign_BeforeFirstLockLevel_ReturnsNoLocks()
    {
        var dragons = TestStages.RowDragons(12);

        for (int level = 0; level < StageLocks.FirstLockLevelIndex; level++)
        {
            int[] locks = StageLocks.Assign(level, dragons, BoardWidth, 12);
            Assert.AreEqual(0, CountLocks(locks), "level index " + level);
        }
    }

    [Test]
    public void Assign_FewerThanFourDragons_ReturnsNoLocks()
    {
        var dragons = TestStages.RowDragons(3);

        int[] locks = StageLocks.Assign(StageLocks.FirstLockLevelIndex + 10, dragons, BoardWidth, 3);

        Assert.AreEqual(3, locks.Length);
        Assert.AreEqual(0, CountLocks(locks));
    }

    [Test]
    public void Assign_FirstLockLevel_PutsAtLeastOneLockOnAnAdequateBoard()
    {
        var dragons = TestStages.RowDragons(10);

        int[] locks = StageLocks.Assign(StageLocks.FirstLockLevelIndex, dragons, BoardWidth, 10);

        Assert.GreaterOrEqual(CountLocks(locks), 1);
    }

    [Test]
    public void Assign_AnyLevel_NeverLocksTheFirstDragonInSolveOrder()
    {
        foreach (int n in new[] { 4, 10, 40 })
        {
            var dragons = TestStages.RowDragons(n);
            for (int level = StageLocks.FirstLockLevelIndex; level < 80; level++)
            {
                int[] locks = StageLocks.Assign(level, dragons, BoardWidth, n);
                Assert.AreEqual(0, locks[0], $"{n} arrows, level index {level}");
            }
        }
    }

    [Test]
    public void Assign_AnyLevel_ThresholdNeverExceedsTheDragonsSolvePosition()
    {
        // Arrow i has exactly i arrows ahead of it in solve order, so a threshold above i could
        // never be met: that is the softlock the whole rule exists to prevent.
        foreach (int n in new[] { 4, 10, 40, 100 })
        {
            var dragons = TestStages.RowDragons(n);
            for (int level = StageLocks.FirstLockLevelIndex; level < 900; level += 7)
            {
                int[] locks = StageLocks.Assign(level, dragons, BoardWidth, n);
                for (int i = 0; i < n; i++)
                    Assert.LessOrEqual(locks[i], i, $"{n} arrows, level index {level}, arrow {i}");
            }
        }
    }

    [Test]
    public void Assign_AnyLevel_ThresholdStaysWithinOneReadableDigit()
    {
        var dragons = TestStages.RowDragons(100);

        for (int level = StageLocks.FirstLockLevelIndex; level < 900; level += 11)
        {
            foreach (int l in StageLocks.Assign(level, dragons, BoardWidth, 100))
            {
                Assert.GreaterOrEqual(l, 0);
                Assert.LessOrEqual(l, StageLocks.MaxThreshold, "level index " + level);
            }
        }
    }

    [Test]
    public void Assign_LargeBoard_LocksNeverExceedHalfTheDragonsOrTheCap()
    {
        var dragons = TestStages.RowDragons(100);

        for (int level = StageLocks.FirstLockLevelIndex; level < 900; level += 13)
        {
            int count = CountLocks(StageLocks.Assign(level, dragons, BoardWidth, 100));
            Assert.LessOrEqual(count, 30, "level index " + level);
            Assert.LessOrEqual(count, 50, "level index " + level);
        }
    }

    [Test]
    public void Assign_LaterLevel_PadlocksMoreOfTheBoardThanTheFirstLockLevel()
    {
        var dragons = TestStages.RowDragons(40);

        int early = CountLocks(StageLocks.Assign(StageLocks.FirstLockLevelIndex, dragons, BoardWidth, 40));
        int late = CountLocks(StageLocks.Assign(StageLocks.FirstLockLevelIndex + 400, dragons, BoardWidth, 40));

        Assert.Greater(late, early);
    }

    [Test]
    public void Assign_SameInputs_ReturnsTheSameLocks()
    {
        var dragons = TestStages.RowDragons(30);

        int[] a = StageLocks.Assign(123, dragons, BoardWidth, 30);
        int[] b = StageLocks.Assign(123, dragons, BoardWidth, 30);

        CollectionAssert.AreEqual(a, b);
    }

    [Test]
    public void Assign_DifferentLevelIndexes_DoNotAllProduceTheSamePattern()
    {
        var dragons = TestStages.RowDragons(30);
        HashSet<string> patterns = new HashSet<string>();

        for (int level = StageLocks.FirstLockLevelIndex; level < StageLocks.FirstLockLevelIndex + 20; level++)
            patterns.Add(string.Join(",", StageLocks.Assign(level, dragons, BoardWidth, 30)));

        Assert.Greater(patterns.Count, 1);
    }

    [Test]
    public void Assign_PrefersDragonsWhoseRayIsAlreadyClear()
    {
        // Two rows are free to leave; the rest are boxed in by a wall of cells in front of them.
        // A lock on a boxed-in arrow changes nothing the player can see, so the free ones go first.
        List<List<Vector2Int>> dragons = new List<List<Vector2Int>>();
        for (int i = 0; i < 8; i++)
            dragons.Add(TestStages.Path(new Vector2Int(0, i), 2));

        // Arrows 5, 6 and 7 get a one-cell wall on their ray, owned by a separate sentinel arrow
        // placed after them in the list so it does not disturb the solve-order rule.
        for (int row = 5; row <= 7; row++)
            dragons.Add(TestStages.Path(new Vector2Int(4, row), 3));    // (4,row) -> (3,row)

        int[] locks = StageLocks.Assign(StageLocks.FirstLockLevelIndex, dragons, BoardWidth, 8);

        // want = max(1, round(11 * 0.06)) = 1 lock, so it must land on a free arrow (index 1..4).
        Assert.AreEqual(1, CountLocks(locks));
        int locked = System.Array.FindIndex(locks, l => l > 0);
        Assert.That(locked, Is.InRange(1, 4));
    }
}
