using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Story 002 (level pipeline): determinism, coverage and the lock-aware solve of freshly carved
// boards. Every test seeds UnityEngine.Random explicitly, the same way the baker does.
public class StageCarverTests
{
    const int Width = 6;
    const int Height = 8;

    [Test]
    public void CarveFull_SameSeed_ReturnsIdenticalDragons()
    {
        int sa, sb;
        var a = TestStages.Carve(1234, Width, Height, out sa);
        var b = TestStages.Carve(1234, Width, Height, out sb);

        Assert.AreEqual(sa, sb);
        Assert.IsTrue(TestStages.SameDragons(a, b), "same seed must reproduce the same level");
    }

    [Test]
    public void CarveFull_DifferentSeeds_DoNotAllProduceTheSameLevel()
    {
        int s;
        var reference = TestStages.Carve(1, Width, Height, out s);

        bool anyDifferent = false;
        for (int seed = 2; seed <= 8 && !anyDifferent; seed++)
            anyDifferent = !TestStages.SameDragons(reference, TestStages.Carve(seed, Width, Height, out s));

        Assert.IsTrue(anyDifferent);
    }

    [Test]
    public void CarveFull_SomeSeedCoversTheWholeBoard_WithNoCellUsedTwice()
    {
        // The baker discards any carve that strands cells, so at least one seed must reach 100%.
        List<List<Vector2Int>> dragons = null;
        for (int seed = 0; seed < 40 && dragons == null; seed++)
        {
            int stranded;
            var carved = TestStages.Carve(seed, Width, Height, out stranded);
            if (carved != null && stranded == 0) dragons = carved;
        }
        Assert.IsNotNull(dragons, "no seed in 0..39 produced a full carve");

        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
        foreach (List<Vector2Int> path in dragons)
        {
            Assert.GreaterOrEqual(path.Count, 2, "an arrow needs a head and a tail");
            foreach (Vector2Int c in path)
            {
                Assert.IsTrue(seen.Add(c), "cell " + c + " belongs to two arrows");
                Assert.IsTrue(c.x >= 0 && c.x < Width && c.y >= 0 && c.y < Height, "cell out of bounds: " + c);
            }
        }
        Assert.AreEqual(Width * Height, seen.Count, "every cell must be covered");
    }

    [Test]
    public void CarveFull_EveryStepIsFourConnected()
    {
        int stranded;
        var dragons = TestStages.Carve(7, Width, Height, out stranded);

        foreach (List<Vector2Int> path in dragons)
            for (int s = 0; s < path.Count - 1; s++)
                Assert.GreaterOrEqual(StagePack.DirIndex(path[s + 1] - path[s]), 0,
                    "non-adjacent step " + path[s] + " -> " + path[s + 1]);
    }

    [Test]
    public void CarveFull_ListOrder_IsAValidSolveOrder()
    {
        // StageLocks relies on this: clearing the arrows in list order must never meet a blocker,
        // because arrow i's exit ray only crosses cells of arrows with a LOWER index.
        for (int seed = 0; seed < 10; seed++)
        {
            int stranded;
            var dragons = TestStages.Carve(seed, Width, Height, out stranded);

            HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
            foreach (List<Vector2Int> path in dragons)
                foreach (Vector2Int c in path) occupied.Add(c);

            for (int i = 0; i < dragons.Count; i++)
            {
                List<Vector2Int> path = dragons[i];
                Vector2Int head = path[path.Count - 1];
                Vector2Int dir = head - path[path.Count - 2];

                for (Vector2Int c = head + dir; c.x >= 0 && c.x < Width && c.y >= 0 && c.y < Height; c += dir)
                    Assert.IsFalse(occupied.Contains(c), $"seed {seed}: arrow {i} is blocked at {c} in list order");

                foreach (Vector2Int c in path) occupied.Remove(c);
            }
        }
    }

    [Test]
    public void CarveFull_Result_IsSolvableWithoutLocks()
    {
        for (int seed = 0; seed < 10; seed++)
        {
            int stranded;
            var dragons = TestStages.Carve(seed, Width, Height, out stranded);

            Assert.IsTrue(StageRating.Measure(dragons, Width, Height).solvable, "seed " + seed);
        }
    }

    [Test]
    public void CarveThenAssign_LocksNeverExceedSolvePositionAndLevelStaysSolvable()
    {
        foreach (int level in new[] { 5, 50, 150, 400 })
        {
            for (int seed = 0; seed < 8; seed++)
            {
                int stranded;
                var dragons = TestStages.Carve(seed, Width, Height, out stranded);

                int[] locks = StageLocks.Assign(level, dragons, Width, Height);

                for (int i = 0; i < locks.Length; i++)
                    Assert.LessOrEqual(locks[i], i, $"level {level} seed {seed} arrow {i}");
                Assert.IsTrue(StageRating.Measure(dragons, Width, Height, locks).solvable,
                    $"level {level} seed {seed}");
            }
        }
    }

    [Test]
    public void Hash01_SameInput_ReturnsTheSameValue()
    {
        Assert.AreEqual(StageCampaignGenerator.Hash01(12345), StageCampaignGenerator.Hash01(12345));
    }

    [Test]
    public void Hash01_AnyInput_StaysInsideZeroToOne()
    {
        for (int n = -2000; n <= 2000; n++)
        {
            float h = StageCampaignGenerator.Hash01(n);
            Assert.GreaterOrEqual(h, 0f, "n = " + n);
            Assert.Less(h, 1f, "n = " + n);
        }
    }

    [Test]
    public void Hash01_NeighbouringInputs_MostlyDiffer()
    {
        HashSet<float> distinct = new HashSet<float>();
        for (int n = 0; n < 100; n++) distinct.Add(StageCampaignGenerator.Hash01(n));

        Assert.GreaterOrEqual(distinct.Count, 90);
    }
}
