using UnityEngine;

// The shipped level format: packed binary chunks in Resources, read on demand.
//
// An arrow is a chain of 4-connected cells, so only the tail cell and 2 bits per step are stored,
// not a coordinate per cell — ~0.25 bytes per cell against 24 for a ScriptableObject.
//
// Rejected: one .asset per level (a serialized array makes every level resident, and needs manual
// Inspector wiring), Resources-of-JSON (Unity documents ~seconds of startup at 10k assets), and
// Addressables (~30 MB Android / ~72 MB iOS overhead, and no remote content is needed).
public static class LevelBinary
{
    // 'A' 'P' 'L' 'C' — Arrow Puzzle Level Chunk.
    public const uint Magic = 0x434C5041;
    // 2: every arrow record ends with a lock-threshold byte. A version-1 chunk is refused by
    // IsValidChunk with a clear message, never half-read.
    public const ushort Version = 2;

    // 50 files at 50,000 levels — few enough that Resources' index cost stays irrelevant.
    public const int LevelsPerChunk = 1000;

    public const string ResourceFolder = "Levels/";
    public const string ChunkPrefix = "Chunk_";

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

    // Fills an existing LevelData rather than returning a new one, so switching levels allocates
    // no ScriptableObject and reuses the arrow lists.
    public static void ReadLevelInto(byte[] bytes, int indexWithinChunk, LevelData target)
    {
        int at = ReadI32(bytes, HeaderSize + indexWithinChunk * 4);

        int width = ReadU8(bytes, at);
        int height = ReadU8(bytes, at + 1);
        int difficulty = ReadU8(bytes, at + 2);
        int arrowCount = ReadU16(bytes, at + 3);
        at += 5;

        target.gridWidth = width;
        target.gridHeight = height;
        target.difficulty = (Difficulty)difficulty;

        if (target.arrows == null)
            target.arrows = new System.Collections.Generic.List<ArrowPathData>(arrowCount);

        while (target.arrows.Count < arrowCount) target.arrows.Add(new ArrowPathData());
        if (target.arrows.Count > arrowCount) target.arrows.RemoveRange(arrowCount, target.arrows.Count - arrowCount);

        for (int a = 0; a < arrowCount; a++)
        {
            int tailIndex = ReadU16(bytes, at);
            int length = ReadU8(bytes, at + 2);
            int bitsAt = at + 3;

            ArrowPathData arrow = target.arrows[a];
            if (arrow == null) { arrow = new ArrowPathData(); target.arrows[a] = arrow; }
            if (arrow.path == null) arrow.path = new System.Collections.Generic.List<Vector2Int>(length);
            arrow.path.Clear();

            Vector2Int cell = new Vector2Int(tailIndex % width, tailIndex / width);
            arrow.path.Add(cell);

            for (int s = 0; s < length - 1; s++)
            {
                int bitPos = s * 2;
                int packed = bytes[bitsAt + (bitPos >> 3)];
                int dir = (packed >> (bitPos & 7)) & 0x3;

                cell += Dirs[dir];
                arrow.path.Add(cell);
            }

            at = bitsAt + DirectionBytes(length);

            arrow.lockThreshold = ReadU8(bytes, at);
            at += 1;
        }
    }
}
