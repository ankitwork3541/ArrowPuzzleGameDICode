using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Story 002 (level pipeline): the shipped binary format. The writer and the reader are two
// hand-rolled halves of one bit layout, so these tests pack with the writer's own chunk builder and
// read back with the game's reader, entirely in memory (no files are written).
public class LevelBinaryTests
{
    LevelData target;

    [SetUp]
    public void SetUp()
    {
        target = ScriptableObject.CreateInstance<LevelData>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(target);
    }

    // ---------- fixtures ----------

    // A long arrow that snakes right, up, left, up, ... so its direction bits cross byte boundaries.
    static List<Vector2Int> Snake(int rows, int runLength)
    {
        int[] steps = new int[rows * runLength + (rows - 1)];
        int at = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int i = 0; i < runLength; i++) steps[at++] = (r % 2 == 0) ? 2 : 3;   // right / left
            if (r < rows - 1) steps[at++] = 0;                                         // up
        }
        return TestLevels.Path(new Vector2Int(0, 0), steps);
    }

    static LevelBinaryWriter.LevelRecord SmallLevel()
    {
        return new LevelBinaryWriter.LevelRecord
        {
            width = 6,
            height = 5,
            difficulty = Difficulty.Medium,
            arrows = new List<List<Vector2Int>>
            {
                TestLevels.Path(new Vector2Int(0, 0), 2, 2, 0, 0),    // right, right, up, up
                TestLevels.Path(new Vector2Int(5, 4), 3, 3, 1, 3),    // left, left, down, left
                TestLevels.Path(new Vector2Int(0, 4), 2),
            },
            locks = new[] { 0, 2, 1 }
        };
    }

    static LevelBinaryWriter.LevelRecord SnakeLevel()
    {
        return new LevelBinaryWriter.LevelRecord
        {
            width = 12,
            height = 12,
            difficulty = Difficulty.Hardest,
            arrows = new List<List<Vector2Int>>
            {
                Snake(4, 10),                                          // 44 cells, 43 steps
                TestLevels.Path(new Vector2Int(0, 11), 2),
            },
            locks = new[] { 0, 1 }
        };
    }

    static LevelBinaryWriter.LevelRecord UnlockedLevel()
    {
        return new LevelBinaryWriter.LevelRecord
        {
            width = 4,
            height = 3,
            difficulty = Difficulty.Nightmare,
            arrows = new List<List<Vector2Int>> { TestLevels.Path(new Vector2Int(0, 0), 2, 2, 2) },
            locks = null
        };
    }

    // BuildChunk is private to the writer (WriteAll also deletes and rewrites the Resources
    // folder, which a test must never do), so call just the pure byte-packing step.
    static byte[] BuildChunk(List<LevelBinaryWriter.LevelRecord> levels, int first, int count, int total)
    {
        MethodInfo build = typeof(LevelBinaryWriter).GetMethod(
            "BuildChunk", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(build, "LevelBinaryWriter.BuildChunk was renamed; update this test");
        return (byte[])build.Invoke(null, new object[] { levels, first, count, total });
    }

    static List<LevelBinaryWriter.LevelRecord> ThreeLevels()
    {
        return new List<LevelBinaryWriter.LevelRecord> { SmallLevel(), SnakeLevel(), UnlockedLevel() };
    }

    void AssertMatches(LevelBinaryWriter.LevelRecord expected)
    {
        Assert.AreEqual(expected.width, target.gridWidth, "width");
        Assert.AreEqual(expected.height, target.gridHeight, "height");
        Assert.AreEqual(expected.difficulty, target.difficulty, "difficulty");
        Assert.AreEqual(expected.arrows.Count, target.arrows.Count, "arrow count");

        for (int a = 0; a < expected.arrows.Count; a++)
        {
            Assert.IsTrue(TestLevels.SamePath(expected.arrows[a], target.arrows[a].path), "arrow " + a + " cells");
            int wantLock = expected.locks != null ? expected.locks[a] : 0;
            Assert.AreEqual(wantLock, target.arrows[a].lockThreshold, "arrow " + a + " lock");
        }
    }

    // ---------- round trip ----------

    [Test]
    public void ReadLevelInto_SmallLevel_RestoresCellsAndLocks()
    {
        var levels = ThreeLevels();
        byte[] chunk = BuildChunk(levels, 0, levels.Count, levels.Count);

        LevelBinary.ReadLevelInto(chunk, 0, target);

        AssertMatches(levels[0]);
    }

    [Test]
    public void ReadLevelInto_LongSnakeArrow_RestoresEveryCell()
    {
        var levels = ThreeLevels();
        byte[] chunk = BuildChunk(levels, 0, levels.Count, levels.Count);

        LevelBinary.ReadLevelInto(chunk, 1, target);

        Assert.AreEqual(44, target.arrows[0].path.Count);
        AssertMatches(levels[1]);
    }

    [Test]
    public void ReadLevelInto_LevelWithoutLocks_ReadsEveryThresholdAsZero()
    {
        var levels = ThreeLevels();
        byte[] chunk = BuildChunk(levels, 0, levels.Count, levels.Count);

        LevelBinary.ReadLevelInto(chunk, 2, target);

        AssertMatches(levels[2]);
    }

    [Test]
    public void ReadLevelInto_AnyOrder_EachLevelIsReadIndependently()
    {
        var levels = ThreeLevels();
        byte[] chunk = BuildChunk(levels, 0, levels.Count, levels.Count);

        foreach (int index in new[] { 2, 0, 1, 1, 2, 0 })
        {
            LevelBinary.ReadLevelInto(chunk, index, target);
            AssertMatches(levels[index]);
        }
    }

    [Test]
    public void ReadLevelInto_ReusedTarget_DoesNotLeakTheLongerPreviousLevel()
    {
        var levels = ThreeLevels();
        byte[] chunk = BuildChunk(levels, 0, levels.Count, levels.Count);

        LevelBinary.ReadLevelInto(chunk, 0, target);    // 3 arrows
        LevelBinary.ReadLevelInto(chunk, 2, target);    // 1 arrow, shorter

        AssertMatches(levels[2]);
    }

    // ---------- header ----------

    [Test]
    public void BuildChunk_Header_CarriesCountsAndCampaignLength()
    {
        var levels = ThreeLevels();

        byte[] chunk = BuildChunk(levels, 0, 3, 1500);

        string error;
        Assert.IsTrue(LevelBinary.IsValidChunk(chunk, out error), error);
        Assert.AreEqual(3, LevelBinary.LevelCountIn(chunk));
        Assert.AreEqual(0, LevelBinary.FirstLevelIndexIn(chunk));
        Assert.AreEqual(1500, LevelBinary.TotalLevelsIn(chunk));
    }

    [Test]
    public void IsValidChunk_NullOrTruncated_IsRejected()
    {
        string error;
        Assert.IsFalse(LevelBinary.IsValidChunk(null, out error));
        Assert.IsFalse(LevelBinary.IsValidChunk(new byte[LevelBinary.HeaderSize - 1], out error));
        Assert.IsNotNull(error);
    }

    [Test]
    public void IsValidChunk_WrongMagic_IsRejected()
    {
        byte[] chunk = BuildChunk(ThreeLevels(), 0, 3, 3);
        chunk[0] ^= 0xFF;

        string error;
        Assert.IsFalse(LevelBinary.IsValidChunk(chunk, out error));
        StringAssert.Contains("magic", error);
    }

    [Test]
    public void IsValidChunk_OldFormatVersion_IsRefusedNotHalfRead()
    {
        byte[] chunk = BuildChunk(ThreeLevels(), 0, 3, 3);
        chunk[4] = 1;    // version 1: no lock byte per arrow
        chunk[5] = 0;

        string error;
        Assert.IsFalse(LevelBinary.IsValidChunk(chunk, out error));
        StringAssert.Contains("version", error);
    }

    // ---------- pure helpers ----------

    [Test]
    public void DirIndex_FourNeighbours_MapToTheirFormatIndex()
    {
        // The ORDER of Dirs is the on-disk format; reordering it would corrupt every chunk.
        Assert.AreEqual(0, LevelBinary.DirIndex(new Vector2Int(0, 1)));
        Assert.AreEqual(1, LevelBinary.DirIndex(new Vector2Int(0, -1)));
        Assert.AreEqual(2, LevelBinary.DirIndex(new Vector2Int(1, 0)));
        Assert.AreEqual(3, LevelBinary.DirIndex(new Vector2Int(-1, 0)));
    }

    [Test]
    public void DirIndex_NonAdjacentStep_ReturnsMinusOne()
    {
        Assert.AreEqual(-1, LevelBinary.DirIndex(new Vector2Int(2, 0)));
        Assert.AreEqual(-1, LevelBinary.DirIndex(new Vector2Int(1, 1)));
        Assert.AreEqual(-1, LevelBinary.DirIndex(Vector2Int.zero));
    }

    [Test]
    public void DirectionBytes_PacksTwoBitsPerStepRoundedUp()
    {
        Assert.AreEqual(1, LevelBinary.DirectionBytes(2));     // 1 step  -> 2 bits
        Assert.AreEqual(1, LevelBinary.DirectionBytes(5));     // 4 steps -> 8 bits
        Assert.AreEqual(2, LevelBinary.DirectionBytes(6));     // 5 steps -> 10 bits
        Assert.AreEqual(11, LevelBinary.DirectionBytes(44));   // 43 steps -> 86 bits
    }

    [Test]
    public void ChunkIndexing_SplitsLevelsEveryThousand()
    {
        Assert.AreEqual(0, LevelBinary.ChunkIndexOf(0));
        Assert.AreEqual(0, LevelBinary.ChunkIndexOf(999));
        Assert.AreEqual(1, LevelBinary.ChunkIndexOf(1000));
        Assert.AreEqual(499, LevelBinary.IndexWithinChunk(1499));
        Assert.AreEqual("Levels/Chunk_01", LevelBinary.ChunkResourcePath(1));
    }

    // ---------- writer validation (pure, runs before anything is written) ----------

    [Test]
    public void Validate_WellFormedLevels_IsAccepted()
    {
        string error;
        Assert.IsTrue(LevelBinaryWriter.Validate(ThreeLevels(), out error), error);
    }

    [Test]
    public void Validate_LockAboveArrowsMinusOne_IsRejected()
    {
        var level = SmallLevel();
        level.locks = new[] { 0, 3, 0 };    // only 2 other arrows exist

        string error;
        Assert.IsFalse(LevelBinaryWriter.Validate(new List<LevelBinaryWriter.LevelRecord> { level }, out error));
        StringAssert.Contains("lock", error);
    }

    [Test]
    public void Validate_NegativeLock_IsRejected()
    {
        var level = SmallLevel();
        level.locks = new[] { 0, -1, 0 };

        string error;
        Assert.IsFalse(LevelBinaryWriter.Validate(new List<LevelBinaryWriter.LevelRecord> { level }, out error));
    }

    [Test]
    public void Validate_LockCountDifferentFromArrowCount_IsRejected()
    {
        var level = SmallLevel();
        level.locks = new[] { 0, 1 };

        string error;
        Assert.IsFalse(LevelBinaryWriter.Validate(new List<LevelBinaryWriter.LevelRecord> { level }, out error));
        StringAssert.Contains("lock entries", error);
    }

    [Test]
    public void Validate_NonAdjacentStep_IsRejected()
    {
        var level = SmallLevel();
        level.arrows[2] = new List<Vector2Int> { new Vector2Int(0, 4), new Vector2Int(2, 4) };

        string error;
        Assert.IsFalse(LevelBinaryWriter.Validate(new List<LevelBinaryWriter.LevelRecord> { level }, out error));
        StringAssert.Contains("4-adjacent", error);
    }

    [Test]
    public void Validate_SingleCellArrow_IsRejected()
    {
        var level = SmallLevel();
        level.arrows[2] = new List<Vector2Int> { new Vector2Int(0, 4) };

        string error;
        Assert.IsFalse(LevelBinaryWriter.Validate(new List<LevelBinaryWriter.LevelRecord> { level }, out error));
    }

    [Test]
    public void Validate_GridWiderThanAByte_IsRejected()
    {
        var level = SmallLevel();
        level.width = 256;

        string error;
        Assert.IsFalse(LevelBinaryWriter.Validate(new List<LevelBinaryWriter.LevelRecord> { level }, out error));
    }
}
