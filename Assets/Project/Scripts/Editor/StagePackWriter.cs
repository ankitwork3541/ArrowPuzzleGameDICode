using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

// Writes the packed level chunks the game ships. Editor-only by folder, so the writer is never
// compiled into a build — the same split the reference game has, where the tool that made its
// 5,000 levels was never in the APK at all.
//
// The layout is documented once, in StagePack. Everything here must stay the mirror of
// StagePack.ReadLevelInto, and Validate() below is what stops the two drifting silently.
public static class StagePackWriter
{
    public class StageRecord
    {
        public int width, height;
        public StageTier difficulty;
        public List<List<Vector2Int>> dragons;    // each path is tail-first, same as StageData
        public int[] locks;                      // lock threshold per arrow, same order; null = no locks
    }

    public const string OutputFolder = "Assets/Project/Resources/Stages";

    // Writes every chunk needed for `levels`, replacing whatever was there. Returns total bytes.
    public static int WriteAll(List<StageRecord> levels)
    {
        Directory.CreateDirectory(OutputFolder);

        // Stale chunks from a previous, longer run would still be loadable and would serve levels
        // the campaign no longer contains.
        foreach (string old in Directory.GetFiles(OutputFolder, StagePack.ChunkPrefix + "*.bytes"))
            AssetDatabase.DeleteAsset(old.Replace('\\', '/'));

        int chunkCount = (levels.Count + StagePack.LevelsPerChunk - 1) / StagePack.LevelsPerChunk;
        int totalBytes = 0;

        for (int chunk = 0; chunk < chunkCount; chunk++)
        {
            int first = chunk * StagePack.LevelsPerChunk;
            int count = Mathf.Min(StagePack.LevelsPerChunk, levels.Count - first);

            byte[] bytes = BuildChunk(levels, first, count, levels.Count);
            totalBytes += bytes.Length;

            string path = OutputFolder + "/" + StagePack.ChunkPrefix + chunk.ToString("00") + ".bytes";
            File.WriteAllBytes(path, bytes);
        }

        AssetDatabase.Refresh();
        return totalBytes;
    }

    static byte[] BuildChunk(List<StageRecord> levels, int firstLevelIndex, int count, int totalLevels)
    {
        // Every level record first, so the offset table can be filled in with real positions
        // instead of being patched afterwards.
        List<byte> data = new List<byte>(count * 400);
        int[] relativeOffsets = new int[count];

        for (int i = 0; i < count; i++)
        {
            relativeOffsets[i] = data.Count;
            AppendLevel(data, levels[firstLevelIndex + i]);
        }

        int headerAndTable = StagePack.HeaderSize + count * 4;
        byte[] bytes = new byte[headerAndTable + data.Count];

        WriteU32(bytes, 0, StagePack.Magic);
        WriteU16(bytes, 4, StagePack.Version);
        WriteU16(bytes, 6, count);
        WriteI32(bytes, 8, firstLevelIndex);
        WriteI32(bytes, 12, totalLevels);

        for (int i = 0; i < count; i++)
            WriteI32(bytes, StagePack.HeaderSize + i * 4, headerAndTable + relativeOffsets[i]);

        data.CopyTo(bytes, headerAndTable);
        return bytes;
    }

    static void AppendLevel(List<byte> data, StageRecord level)
    {
        data.Add((byte)level.width);
        data.Add((byte)level.height);
        data.Add((byte)(int)level.difficulty);
        data.Add((byte)(level.dragons.Count & 0xFF));
        data.Add((byte)((level.dragons.Count >> 8) & 0xFF));

        for (int dragonIndex = 0; dragonIndex < level.dragons.Count; dragonIndex++)
        {
            List<Vector2Int> path = level.dragons[dragonIndex];
            int tailIndex = path[0].y * level.width + path[0].x;
            data.Add((byte)(tailIndex & 0xFF));
            data.Add((byte)((tailIndex >> 8) & 0xFF));
            data.Add((byte)path.Count);

            // 2 bits per step, low bits first, zero-padded to a whole byte.
            int bitCount = (path.Count - 1) * 2;
            int byteCount = (bitCount + 7) / 8;
            int start = data.Count;
            for (int b = 0; b < byteCount; b++) data.Add(0);

            for (int s = 0; s < path.Count - 1; s++)
            {
                int dir = StagePack.DirIndex(path[s + 1] - path[s]);
                int bitPos = s * 2;
                data[start + (bitPos >> 3)] |= (byte)(dir << (bitPos & 7));
            }

            data.Add((byte)(level.locks != null ? level.locks[dragonIndex] : 0));
        }
    }

    // ---------- validation ----------

    // Checked BEFORE writing, because every one of these limits is silent corruption rather than an
    // exception: a width over 255 truncates, a non-adjacent step encodes as a wrong direction, and
    // the level simply loads as a different board.
    public static bool Validate(List<StageRecord> levels, out string error)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            StageRecord level = levels[i];

            if (level.width > 255 || level.height > 255)
            {
                error = $"level {i}: {level.width}x{level.height} — grid side must fit a byte (255)";
                return false;
            }
            if (level.width * level.height > 65535)
            {
                error = $"level {i}: {level.width * level.height} cells — cell index must fit a ushort";
                return false;
            }
            if (level.dragons.Count > 65535)
            {
                error = $"level {i}: {level.dragons.Count} arrows — count must fit a ushort";
                return false;
            }

            if (level.locks != null && level.locks.Length != level.dragons.Count)
            {
                error = $"level {i}: {level.locks.Length} lock entries for {level.dragons.Count} arrows";
                return false;
            }

            if (level.locks != null)
            {
                for (int a = 0; a < level.locks.Length; a++)
                {
                    // A lock needs `threshold` OTHER arrows to leave, so it can never exceed arrows - 1.
                    if (level.locks[a] < 0 || level.locks[a] > 255 || level.locks[a] > level.dragons.Count - 1)
                    {
                        error = $"level {i} arrow {a}: lock {level.locks[a]} is outside 0..{level.dragons.Count - 1}";
                        return false;
                    }
                }
            }

            foreach (List<Vector2Int> path in level.dragons)
            {
                if (path.Count < 2)
                {
                    error = $"level {i}: an arrow has {path.Count} cell(s); 2 is the minimum";
                    return false;
                }
                if (path.Count > 255)
                {
                    error = $"level {i}: an arrow is {path.Count} cells — length must fit a byte";
                    return false;
                }

                for (int s = 0; s < path.Count - 1; s++)
                {
                    if (StagePack.DirIndex(path[s + 1] - path[s]) < 0)
                    {
                        error = $"level {i}: arrow step {path[s]} -> {path[s + 1]} is not 4-adjacent, "
                              + "so it cannot be direction-encoded";
                        return false;
                    }
                }
            }
        }

        error = null;
        return true;
    }

    // Reads every chunk back and compares it cell-for-cell against what was meant to be written.
    // Worth the seconds it costs: the writer and the reader are two hand-rolled halves of one bit
    // layout, and a mismatch would ship as levels that are subtly the wrong shape.
    public static bool VerifyRoundTrip(List<StageRecord> levels, out string error)
    {
        StageData scratch = ScriptableObject.CreateInstance<StageData>();

        try
        {
            for (int chunk = 0; chunk * StagePack.LevelsPerChunk < levels.Count; chunk++)
            {
                string path = OutputFolder + "/" + StagePack.ChunkPrefix + chunk.ToString("00") + ".bytes";
                byte[] bytes = File.ReadAllBytes(path);

                string chunkError;
                if (!StagePack.IsValidChunk(bytes, out chunkError))
                {
                    error = $"chunk {chunk}: {chunkError}";
                    return false;
                }

                int count = StagePack.LevelCountIn(bytes);
                for (int i = 0; i < count; i++)
                {
                    StageRecord expected = levels[chunk * StagePack.LevelsPerChunk + i];
                    StagePack.ReadLevelInto(bytes, i, scratch);

                    if (scratch.gridWidth != expected.width || scratch.gridHeight != expected.height)
                    {
                        error = $"chunk {chunk} level {i}: grid {scratch.gridWidth}x{scratch.gridHeight} "
                              + $"!= {expected.width}x{expected.height}";
                        return false;
                    }
                    if (scratch.dragons.Count != expected.dragons.Count)
                    {
                        error = $"chunk {chunk} level {i}: {scratch.dragons.Count} arrows != {expected.dragons.Count}";
                        return false;
                    }

                    for (int a = 0; a < expected.dragons.Count; a++)
                    {
                        List<Vector2Int> want = expected.dragons[a];
                        List<Vector2Int> got = scratch.dragons[a].path;

                        int wantLock = expected.locks != null ? expected.locks[a] : 0;
                        if (scratch.dragons[a].lockThreshold != wantLock)
                        {
                            error = $"chunk {chunk} level {i} arrow {a}: lock {scratch.dragons[a].lockThreshold} != {wantLock}";
                            return false;
                        }

                        if (got.Count != want.Count)
                        {
                            error = $"chunk {chunk} level {i} arrow {a}: {got.Count} cells != {want.Count}";
                            return false;
                        }
                        for (int c = 0; c < want.Count; c++)
                        {
                            if (got[c] != want[c])
                            {
                                error = $"chunk {chunk} level {i} arrow {a} cell {c}: {got[c]} != {want[c]}";
                                return false;
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            Object.DestroyImmediate(scratch);
        }

        error = null;
        return true;
    }

    // ---------- little-endian primitives ----------

    static void WriteU16(byte[] b, int at, int v)
    {
        b[at] = (byte)(v & 0xFF);
        b[at + 1] = (byte)((v >> 8) & 0xFF);
    }

    static void WriteI32(byte[] b, int at, int v)
    {
        b[at] = (byte)(v & 0xFF);
        b[at + 1] = (byte)((v >> 8) & 0xFF);
        b[at + 2] = (byte)((v >> 16) & 0xFF);
        b[at + 3] = (byte)((v >> 24) & 0xFF);
    }

    static void WriteU32(byte[] b, int at, uint v)
    {
        b[at] = (byte)(v & 0xFF);
        b[at + 1] = (byte)((v >> 8) & 0xFF);
        b[at + 2] = (byte)((v >> 16) & 0xFF);
        b[at + 3] = (byte)((v >> 24) & 0xFF);
    }
}
