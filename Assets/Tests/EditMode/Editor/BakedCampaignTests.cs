using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Stories 001-003 end to end, against the campaign that actually ships: every chunk under
// Resources/Levels is read back and re-checked. Read-only: nothing here writes or rebakes.
[Category("Baked")]
public class BakedCampaignTests
{
    static List<byte[]> LoadChunks()
    {
        List<byte[]> chunks = new List<byte[]>();
        for (int i = 0; ; i++)
        {
            TextAsset asset = Resources.Load<TextAsset>(LevelBinary.ChunkResourcePath(i).Replace(".bytes", ""));
            if (asset == null) break;
            chunks.Add(asset.bytes);
        }
        return chunks;
    }

    [Test]
    public void Chunks_AreValidAndAgreeOnTheCampaignLength()
    {
        List<byte[]> chunks = LoadChunks();
        if (chunks.Count == 0) Assert.Ignore("no baked chunks under Resources/Levels; run ArrowPuzzle/Campaign/Bake first");

        int total = LevelBinary.TotalLevelsIn(chunks[0]);
        int counted = 0;
        for (int i = 0; i < chunks.Count; i++)
        {
            string error;
            Assert.IsTrue(LevelBinary.IsValidChunk(chunks[i], out error), "chunk " + i + ": " + error);
            Assert.AreEqual(total, LevelBinary.TotalLevelsIn(chunks[i]), "chunk " + i + " total");
            Assert.AreEqual(counted, LevelBinary.FirstLevelIndexIn(chunks[i]), "chunk " + i + " first index");
            counted += LevelBinary.LevelCountIn(chunks[i]);
        }

        Assert.AreEqual(total, counted, "levels across all chunks");
    }

    [Test, Timeout(300000)]
    public void EveryBakedLevel_IsAPartitionedBoard_WithLocksThatCanOpenAndSolve()
    {
        List<byte[]> chunks = LoadChunks();
        if (chunks.Count == 0) Assert.Ignore("no baked chunks under Resources/Levels; run ArrowPuzzle/Campaign/Bake first");

        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        try
        {
            int levelIndex = 0;
            int firstLockedLevel = -1;
            foreach (byte[] chunk in chunks)
            {
                int count = LevelBinary.LevelCountIn(chunk);
                for (int i = 0; i < count; i++, levelIndex++)
                {
                    LevelBinary.ReadLevelInto(chunk, i, level);

                    List<List<Vector2Int>> arrows = new List<List<Vector2Int>>(level.arrows.Count);
                    int[] locks = new int[level.arrows.Count];
                    HashSet<Vector2Int> cells = new HashSet<Vector2Int>();

                    for (int a = 0; a < level.arrows.Count; a++)
                    {
                        List<Vector2Int> path = level.arrows[a].path;
                        arrows.Add(new List<Vector2Int>(path));
                        locks[a] = level.arrows[a].lockThreshold;

                        Assert.GreaterOrEqual(path.Count, 2, $"level index {levelIndex} arrow {a} length");
                        foreach (Vector2Int c in path)
                        {
                            Assert.IsTrue(c.x >= 0 && c.x < level.gridWidth && c.y >= 0 && c.y < level.gridHeight,
                                $"level index {levelIndex} arrow {a}: {c} is off the {level.gridWidth}x{level.gridHeight} grid");
                            Assert.IsTrue(cells.Add(c), $"level index {levelIndex}: cell {c} belongs to two arrows");
                        }

                        // Arrows are stored in solve order, so a threshold above the position could
                        // never be met.
                        Assert.LessOrEqual(locks[a], a, $"level index {levelIndex} arrow {a} lock {locks[a]}");
                        Assert.LessOrEqual(locks[a], LevelLocks.MaxThreshold, $"level index {levelIndex} arrow {a}");

                        if (locks[a] > 0 && firstLockedLevel < 0) firstLockedLevel = levelIndex;
                    }

                    Assert.IsTrue(
                        LevelDifficulty.Measure(arrows, level.gridWidth, level.gridHeight, locks).solvable,
                        $"level index {levelIndex} is not solvable with its locks");
                }
            }

            // Story 001 / 002: the first lock appears on level 6 (index 5), never before.
            if (levelIndex > LevelLocks.FirstLockLevelIndex)
                Assert.AreEqual(LevelLocks.FirstLockLevelIndex, firstLockedLevel, "first padlocked level index");
        }
        finally
        {
            Object.DestroyImmediate(level);
        }
    }
}
