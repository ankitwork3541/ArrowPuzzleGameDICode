using UnityEngine;

// The shipped level format: packed binary chunks in Resources, read on demand.
//
// An arrow is a chain of 4-connected cells, so only the tail cell and 2 bits per step are stored,
// not a coordinate per cell — ~0.25 bytes per cell against 24 for a ScriptableObject.
//
// Rejected: one .asset per level (a serialized array makes every level resident, and needs manual
// Inspector wiring), Resources-of-JSON (Unity documents ~seconds of startup at 10k assets), and
// Addressables (~30 MB Android / ~72 MB iOS overhead, and no remote content is needed).
public static class StagePack
{
    // 'L' 'D' 'L' 'C' — Lantern Dragons Level Chunk (little-endian, so the bytes read LDLC).
    public const uint Magic = 0x434C444C;
    // 3: new magic for this game. 2: every dragon record ends with a lock-threshold byte. Any older
    // chunk is refused by IsValidChunk with a clear message, never half-read.
    public const ushort Version = 3;

    // 50 files at 50,000 levels — few enough that Resources' index cost stays irrelevant.
    public const int LevelsPerChunk = 1000;

    public const string ResourceFolder = "Stages/";
    public const string ChunkPrefix = "Pack_";

    // Step directions. The ORDER IS THE FORMAT: an index here is what gets written into the two
    // bits, so reordering this array silently corrupts every chunk already built.
    public static readonly Vector2Int[] Dirs =
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0)
    };

    public static int DirIndex(Vector2Int step)
    {
        for (int i = 0; i < Dirs.Length; i++)
            if (Dirs[i] == step) return i;
        return -1;      // caller's problem: a non-adjacent step cannot be encoded
    }

    public static string ChunkResourcePath(int chunkIndex)
    {
        return ResourceFolder + ChunkPrefix + chunkIndex.ToString("00");
    }

    public static int ChunkIndexOf(int levelIndex) { return levelIndex / LevelsPerChunk; }
    public static int IndexWithinChunk(int levelIndex) { return levelIndex % LevelsPerChunk; }

    // ---------- layout ----------
    //
    // chunk header
    //   0  magic            uint32
    //   4  version          uint16
    //   6  levelCount       uint16    (levels in THIS chunk)
    //   8  firstLevelIndex  int32
    //  12  totalLevels      int32     (whole campaign; repeated in every chunk so the loader never
    //                                  has to probe for files to learn the length)
    //  16  offsets          int32 x levelCount   (absolute byte offset of each level record)
    //
    // level record
    //   0  gridWidth        uint8
    //   1  gridHeight       uint8
    //   2  difficulty       uint8
    //   3  arrowCount       uint16
    //   5  arrows...
    //
    // arrow
    //   0  tailIndex        uint16    (y * gridWidth + x of the TAIL cell)
    //   2  length           uint8     (cells, always >= 2)
    //   3  direction bits   ceil((length - 1) * 2 / 8) bytes, 2 bits per step, low bits first
    //   .  lockThreshold    uint8     (0 = unlocked; follows the direction bits)

    public const int HeaderSize = 16;

    public static int ReadU8(byte[] b, int at) { return b[at]; }
    public static int ReadU16(byte[] b, int at) { return b[at] | (b[at + 1] << 8); }
    public static int ReadI32(byte[] b, int at)
    {
        return b[at] | (b[at + 1] << 8) | (b[at + 2] << 16) | (b[at + 3] << 24);
    }

    public static uint ReadU32(byte[] b, int at)
    {
        return (uint)(b[at] | (b[at + 1] << 8) | (b[at + 2] << 16) | (b[at + 3] << 24));
    }

    public static int DirectionBytes(int length)
    {
        int steps = length - 1;
        return (steps * 2 + 7) / 8;
    }

    // ---------- reading ----------

    public static bool IsValidChunk(byte[] bytes, out string error)
    {
        if (bytes == null || bytes.Length < HeaderSize) { error = "chunk is empty or truncated"; return false; }
        if (ReadU32(bytes, 0) != Magic) { error = "bad magic — not a level chunk"; return false; }

        int version = ReadU16(bytes, 4);
        if (version != Version)
        {
            error = "chunk is format version " + version + ", this build reads " + Version;
            return false;
        }

        error = null;
        return true;
    }

    public static int LevelCountIn(byte[] bytes) { return ReadU16(bytes, 6); }
    public static int FirstLevelIndexIn(byte[] bytes) { return ReadI32(bytes, 8); }
    public static int TotalLevelsIn(byte[] bytes) { return ReadI32(bytes, 12); }

    // Fills an existing StageData rather than returning a new one, so switching levels allocates
    // no ScriptableObject and reuses the arrow lists.
    public static void ReadLevelInto(byte[] bytes, int indexWithinChunk, StageData target)
    {
        int at = ReadI32(bytes, HeaderSize + indexWithinChunk * 4);

        int width = ReadU8(bytes, at);
        int height = ReadU8(bytes, at + 1);
        int difficulty = ReadU8(bytes, at + 2);
        int dragonCount = ReadU16(bytes, at + 3);
        at += 5;

        target.gridWidth = width;
        target.gridHeight = height;
        target.difficulty = (StageTier)difficulty;

        if (target.dragons == null)
            target.dragons = new System.Collections.Generic.List<DragonPathData>(dragonCount);

        while (target.dragons.Count < dragonCount) target.dragons.Add(new DragonPathData());
        if (target.dragons.Count > dragonCount) target.dragons.RemoveRange(dragonCount, target.dragons.Count - dragonCount);

        for (int a = 0; a < dragonCount; a++)
        {
            int tailIndex = ReadU16(bytes, at);
            int length = ReadU8(bytes, at + 2);
            int bitsAt = at + 3;

            DragonPathData dragon = target.dragons[a];
            if (dragon == null) { dragon = new DragonPathData(); target.dragons[a] = dragon; }
            if (dragon.path == null) dragon.path = new System.Collections.Generic.List<Vector2Int>(length);
            dragon.path.Clear();

            Vector2Int cell = new Vector2Int(tailIndex % width, tailIndex / width);
            dragon.path.Add(cell);

            for (int s = 0; s < length - 1; s++)
            {
                int bitPos = s * 2;
                int packed = bytes[bitsAt + (bitPos >> 3)];
                int dir = (packed >> (bitPos & 7)) & 0x3;

                cell += Dirs[dir];
                dragon.path.Add(cell);
            }

            at = bitsAt + DirectionBytes(length);

            dragon.lockThreshold = ReadU8(bytes, at);
            at += 1;
        }
    }
}
