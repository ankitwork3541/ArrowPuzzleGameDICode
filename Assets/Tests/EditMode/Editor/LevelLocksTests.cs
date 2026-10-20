using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Story 001 (locked arrows): which arrows get a padlock, and that no threshold can ever trap the
// player. The rule under test lives in LevelLocks.Assign.
public class LevelLocksTests
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
        var arrows = TestLevels.RowArrows(12);

        for (int level = 0; level < LevelLocks.FirstLockLevelIndex; level++)
        {
            int[] locks = LevelLocks.Assign(level, arrows, BoardWidth, 12);
            Assert.AreEqual(0, CountLocks(locks), "level index " + level);
        }
    }

    [Test]
    public void Assign_FewerThanFourArrows_ReturnsNoLocks()
    {
        var arrows = TestLevels.RowArrows(3);

        int[] locks = LevelLocks.Assign(LevelLocks.FirstLockLevelIndex + 10, arrows, BoardWidth, 3);

        Assert.AreEqual(3, locks.Length);
        Assert.AreEqual(0, CountLocks(locks));
    }

    [Test]
    public void Assign_FirstLockLevel_PutsAtLeastOneLockOnAnAdequateBoard()
    {
        var arrows = TestLevels.RowArrows(10);

        int[] locks = LevelLocks.Assign(LevelLocks.FirstLockLevelIndex, arrows, BoardWidth, 10);

        Assert.GreaterOrEqual(CountLocks(locks), 1);
    }

    [Test]
    public void Assign_AnyLevel_NeverLocksTheFirstArrowInSolveOrder()
    {
        foreach (int n in new[] { 4, 10, 40 })
        {
            var arrows = TestLevels.RowArrows(n);
            for (int level = LevelLocks.FirstLockLevelIndex; level < 80; level++)
            {
                int[] locks = LevelLocks.Assign(level, arrows, BoardWidth, n);
                Assert.AreEqual(0, locks[0], $"{n} arrows, level index {level}");
            }
        }
    }

    [Test]
    public void Assign_AnyLevel_ThresholdNeverExceedsTheArrowsSolvePosition()
    {
        // Arrow i has exactly i arrows ahead of it in solve order, so a threshold above i could
        // never be met: that is the softlock the whole rule exists to prevent.
        foreach (int n in new[] { 4, 10, 40, 100 })
        {
            var arrows = TestLevels.RowArrows(n);
            for (int level = LevelLocks.FirstLockLevelIndex; level < 900; level += 7)
            {
                int[] locks = LevelLocks.Assign(level, arrows, BoardWidth, n);
                for (int i = 0; i < n; i++)
                    Assert.LessOrEqual(locks[i], i, $"{n} arrows, level index {level}, arrow {i}");
            }
        }
    }

    [Test]
    public void Assign_AnyLevel_ThresholdStaysWithinOneReadableDigit()
    {
        var arrows = TestLevels.RowArrows(100);

        for (int level = LevelLocks.FirstLockLevelIndex; level < 900; level += 11)
        {
            foreach (int l in LevelLocks.Assign(level, arrows, BoardWidth, 100))
            {
                Assert.GreaterOrEqual(l, 0);
                Assert.LessOrEqual(l, LevelLocks.MaxThreshold, "level index " + level);
            }
        }
    }

    [Test]
    public void Assign_LargeBoard_LocksNeverExceedHalfTheArrowsOrTheCap()
    {
        var arrows = TestLevels.RowArrows(100);

        for (int level = LevelLocks.FirstLockLevelIndex; level < 900; level += 13)
        {
            int count = CountLocks(LevelLocks.Assign(level, arrows, BoardWidth, 100));
            Assert.LessOrEqual(count, 30, "level index " + level);
            Assert.LessOrEqual(count, 50, "level index " + level);
        }
    }

    [Test]
    public void Assign_LaterLevel_PadlocksMoreOfTheBoardThanTheFirstLockLevel()
    {
        var arrows = TestLevels.RowArrows(40);

        int early = CountLocks(LevelLocks.Assign(LevelLocks.FirstLockLevelIndex, arrows, BoardWidth, 40));
        int late = CountLocks(LevelLocks.Assign(LevelLocks.FirstLockLevelIndex + 400, arrows, BoardWidth, 40));

        Assert.Greater(late, early);
    }

    [Test]
    public void Assign_SameInputs_ReturnsTheSameLocks()
    {
        var arrows = TestLevels.RowArrows(30);

        int[] a = LevelLocks.Assign(123, arrows, BoardWidth, 30);
        int[] b = LevelLocks.Assign(123, arrows, BoardWidth, 30);

        CollectionAssert.AreEqual(a, b);
    }

    [Test]
    public void Assign_DifferentLevelIndexes_DoNotAllProduceTheSamePattern()
    {
        var arrows = TestLevels.RowArrows(30);
        HashSet<string> patterns = new HashSet<string>();

        for (int level = LevelLocks.FirstLockLevelIndex; level < LevelLocks.FirstLockLevelIndex + 20; level++)
            patterns.Add(string.Join(",", LevelLocks.Assign(level, arrows, BoardWidth, 30)));

        Assert.Greater(patterns.Count, 1);
    }

    [Test]
    public void Assign_PrefersArrowsWhoseRayIsAlreadyClear()
    {
        // Two rows are free to leave; the rest are boxed in by a wall of cells in front of them.
        // A lock on a boxed-in arrow changes nothing the player can see, so the free ones go first.
        List<List<Vector2Int>> arrows = new List<List<Vector2Int>>();
        for (int i = 0; i < 8; i++)
            arrows.Add(TestLevels.Path(new Vector2Int(0, i), 2));

        // Arrows 5, 6 and 7 get a one-cell wall on their ray, owned by a separate sentinel arrow
        // placed after them in the list so it does not disturb the solve-order rule.
        for (int row = 5; row <= 7; row++)
            arrows.Add(TestLevels.Path(new Vector2Int(4, row), 3));    // (4,row) -> (3,row)

        int[] locks = LevelLocks.Assign(LevelLocks.FirstLockLevelIndex, arrows, BoardWidth, 8);

        // want = max(1, round(11 * 0.06)) = 1 lock, so it must land on a free arrow (index 1..4).
        Assert.AreEqual(1, CountLocks(locks));
        int locked = System.Array.FindIndex(locks, l => l > 0);
        Assert.That(locked, Is.InRange(1, 4));
    }
}
