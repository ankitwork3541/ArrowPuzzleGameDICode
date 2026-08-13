using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

// Note: System.Diagnostics is deliberately NOT imported — its `Debug` collides with
// UnityEngine.Debug, so Stopwatch is fully qualified below instead.

// Bakes the shipping campaign: N levels straight into packed binary chunks, never into .asset files.
//
// This is the tool the player never sees, and it is the whole point of the split — the reference
// game's own generator was never in its APK either, only the data.
//
// Levels are specified procedurally rather than listed, because 1,000 hand-written table rows is not
// a table any more. Two ideas drive the specification:
//
//  1. An ARROW-COUNT ramp, not a board-size ramp. Board area is derived from the wanted count, since
//     the count is what the player feels.
//  2. A SAWTOOTH, not a straight line. Measured in the reference game: 868 same-grid runs across its
//     1,806 campaign levels, and in 207 of the 216 runs longer than two levels the arrow count rises
//     across the run before resetting. A monotone ramp gives the player no breathing room.
public static class LevelCampaignGenerator
{
    // ---------- campaign shape ----------

    // The first stretch is the tutorial and is NOT part of the sawtooth: these counts are the
    // reference game's own early progression, and levels this small want exact counts.
    static readonly int[] OnboardingArrows = { 3, 6, 8, 10, 16, 20, 24, 28, 32, 36 };

    // Where the sawtooth's peaks travel, from the end of onboarding to the last level. 160 arrows on
    // roughly 40x50 matches the reference's hardest shipped bucket (37x54, ~160 arrows).
    const int PeakArrowsStart = 36;
    const int PeakArrowsEnd = 160;

    // Peaks approach the ceiling late rather than linearly: in the reference only ~4% of levels sit
    // in its hardest bucket, so most of the campaign should live in the middle.
    const float PeakCurvePower = 1.5f;

    // Each run drops back to this fraction of the current peak, then climbs to it. This IS the
    // sawtooth's tooth height — at 1.0 the ramp becomes monotone.
    const float RunFloorFraction = 0.55f;
    const int RunLengthMin = 5;
    const int RunLengthMax = 9;

    // ---------- generation effort ----------

    // Onboarding wants its exact counts, so it gets a real seed search. The bulk of the campaign
    // only needs to be near its target, and a carve on a 2,000-cell board is not cheap — attempts
    // here multiply into hours across a thousand levels.
    const int SeedAttemptsOnboarding = 60;
    const int SeedAttemptsBulk = 3;
    const int CarveAttemptsBulk = 3;
    const float ArrowTolerance = 0.12f;

    const float BoardAspect = 1.25f;    // portrait, height / width

    [MenuItem("ArrowPuzzle/Campaign/Bake 15 Levels (test)")]
    static void Bake15() { Bake(15); }

    [MenuItem("ArrowPuzzle/Campaign/Bake 1000 Levels")]
    static void Bake1000() { Bake(1000); }

    static void Bake(int totalLevels)
    {
        System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

        List<LevelBinaryWriter.LevelRecord> records = new List<LevelBinaryWriter.LevelRecord>(totalLevels);
        StringBuilder ramp = new StringBuilder();
        ramp.AppendLine("level |   grid  | cells | arrows want/got | avgLen | bends/seg | score | band");

        int arrowMisses = 0;
        bool cancelled = false;

        try
        {
            for (int i = 0; i < totalLevels; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar(
                        "Baking campaign",
                        $"level {i + 1} / {totalLevels}   ({clock.Elapsed.TotalSeconds:F0}s elapsed)",
                        i / (float)totalLevels))
                {
                    cancelled = true;
                    break;
                }

                int wantArrows = TargetArrows(i, totalLevels);
                int width, height;
                BoardFor(wantArrows, out width, out height);

                LevelCarver.Params prm = ParamsFor(i, wantArrows);

                List<List<Vector2Int>> arrows;
                LevelDifficulty.Result result;
                CarveBest(i, wantArrows, width, height, prm, out arrows, out result);

                if (arrows == null)
                {
                    Debug.LogError($"[Campaign] level {i + 1}: no seed produced a fully-filled solvable "
                                   + $"{width}x{height} board. Skipped — the campaign will be short.");
                    continue;
                }

                records.Add(new LevelBinaryWriter.LevelRecord
                {
                    width = width,
                    height = height,
                    difficulty = result.band,
                    arrows = arrows
                });

                if (Mathf.Abs(arrows.Count - wantArrows) > Mathf.Max(1, wantArrows * ArrowTolerance))
                    arrowMisses++;

                // Only the head of the campaign is logged per level; a thousand rows is unreadable
                // and Unity truncates the console anyway.
                if (i < 20 || (i + 1) % 100 == 0)
                    ramp.AppendLine(RampRow(i, width, height, wantArrows, arrows, result));
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (cancelled)
        {
            Debug.LogWarning($"[Campaign] cancelled after {records.Count} levels — nothing written.");
            return;
        }

        string error;
        if (!LevelBinaryWriter.Validate(records, out error))
        {
            Debug.LogError("[Campaign] refusing to write, data cannot be encoded: " + error);
            return;
        }

        int bytes = LevelBinaryWriter.WriteAll(records);

        if (!LevelBinaryWriter.VerifyRoundTrip(records, out error))
        {
            Debug.LogError("[Campaign] WROTE, BUT READ BACK WRONG: " + error
                           + "\nThe writer and reader have drifted — do not ship this.");
            return;
        }

        int chunks = (records.Count + LevelBinary.LevelsPerChunk - 1) / LevelBinary.LevelsPerChunk;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine($"CAMPAIGN BAKED — {records.Count} levels, {chunks} chunk(s), "
                           + $"{bytes / 1024f:F0} KB total ({bytes / (float)records.Count:F0} bytes/level)");
        summary.AppendLine($"round-trip verified cell-for-cell   |   baked in {clock.Elapsed.TotalSeconds:F1}s");
        if (arrowMisses > 0)
            summary.AppendLine($"{arrowMisses} level(s) outside the {ArrowTolerance:P0} arrow-count tolerance "
                               + "(harmless for the ramp, worth a look if it is most of them)");
        summary.AppendLine();
        summary.Append(ramp);
        Debug.Log(summary.ToString());

        // The first few boards as text, so the shape can be judged without entering Play mode.
        for (int i = 0; i < Mathf.Min(3, records.Count); i++)
        {
            LevelBinaryWriter.LevelRecord r = records[i];
            Debug.Log($"Level {i + 1}  {r.width}x{r.height}  {r.arrows.Count} arrows\n"
                      + RenderAscii(r.arrows, r.width, r.height));
        }
    }

    // ---------- the ramp ----------

    static int TargetArrows(int levelIndex, int totalLevels)
    {
        if (levelIndex < OnboardingArrows.Length) return OnboardingArrows[levelIndex];

        int postOnboarding = levelIndex - OnboardingArrows.Length;
        int span = Mathf.Max(1, totalLevels - OnboardingArrows.Length - 1);
        float u = Mathf.Clamp01(postOnboarding / (float)span);

        float peak = Mathf.Lerp(PeakArrowsStart, PeakArrowsEnd, Mathf.Pow(u, PeakCurvePower));

        // Where in the current tooth this level sits. Run lengths vary so the sawtooth does not
        // read as a fixed mechanical pulse; derived from the index so it stays deterministic.
        int runStart, runLength;
        RunAt(postOnboarding, out runStart, out runLength);
        float withinRun = runLength <= 1 ? 1f : (postOnboarding - runStart) / (float)(runLength - 1);

        float arrows = Mathf.Lerp(peak * RunFloorFraction, peak, withinRun);
        return Mathf.Max(3, Mathf.RoundToInt(arrows));
    }

    // Walks the run pattern from the start rather than storing it. Deterministic, and cheap enough
    // at these counts that a lookup table would be premature.
    static void RunAt(int postOnboardingIndex, out int runStart, out int runLength)
    {
        int at = 0;
        int run = 0;
        while (true)
        {
            // A small deterministic hash, so run lengths vary without a Random instance whose state
            // would depend on how many times this has been called.
            uint h = (uint)run * 2654435761u;
            int length = RunLengthMin + (int)(h % (uint)(RunLengthMax - RunLengthMin + 1));

            if (postOnboardingIndex < at + length)
            {
                runStart = at;
                runLength = length;
                return;
            }

            at += length;
            run++;
        }
    }

    // Board area from wanted arrow count.
    //
    // The obvious arithmetic — area = arrows x average length — overshoots badly on small boards,
    // because an arrow there cannot reach its target length: the remaining region fragments around
    // it. Measured on this carver, cells-per-arrow climbs from ~5.3 on a tiny board to ~12 on a big
    // one, which is where the reference game's own average sits. These pairs are measurements.
    static readonly int[] FitArrows = { 3, 10, 20, 30, 40, 160 };
    static readonly float[] FitCellsPerArrow = { 5.3f, 6.4f, 8.0f, 10.0f, 11.5f, 12.5f };

    static float CellsPerArrow(int arrows)
    {
        if (arrows <= FitArrows[0]) return FitCellsPerArrow[0];

        for (int i = 1; i < FitArrows.Length; i++)
        {
            if (arrows <= FitArrows[i])
            {
                float t = (arrows - FitArrows[i - 1]) / (float)(FitArrows[i] - FitArrows[i - 1]);
                return Mathf.Lerp(FitCellsPerArrow[i - 1], FitCellsPerArrow[i], t);
            }
        }

        return FitCellsPerArrow[FitCellsPerArrow.Length - 1];
    }

    static void BoardFor(int wantArrows, out int width, out int height)
    {
        float area = wantArrows * CellsPerArrow(wantArrows);

        height = Mathf.Max(3, Mathf.RoundToInt(Mathf.Sqrt(area * BoardAspect)));
        width = Mathf.Max(3, Mathf.RoundToInt(area / height));

        // The format stores a grid side in one byte and a cell index in a ushort.
        width = Mathf.Min(width, 255);
        height = Mathf.Min(height, 255);
    }

    static LevelCarver.Params ParamsFor(int levelIndex, int wantArrows)
    {
        // Level 1 teaches "tap an arrow, it slides out" and nothing else, so its bodies run almost
        // perfectly straight — the reference's own first board measures 0.00 bends per segment.
        if (levelIndex == 0)
            return new LevelCarver.Params { lengthDecay = 0.80f, turnChance = 0.08f, attempts = 8 };

        // turnChance is deliberately CONSTANT across the whole campaign. The reference scales
        // difficulty by board size, not by tangle: its bend rate is flat (~0.30 bends per segment)
        // across all four of its difficulty buckets. Only arrow length grows.
        float decay = Mathf.Lerp(0.86f, 0.92f, Mathf.Clamp01(wantArrows / 60f));

        return new LevelCarver.Params
        {
            lengthDecay = decay,
            turnChance = 0.33f,
            attempts = levelIndex < OnboardingArrows.Length ? 12 : CarveAttemptsBulk
        };
    }

    static void CarveBest(int levelIndex, int wantArrows, int width, int height, LevelCarver.Params prm,
                          out List<List<Vector2Int>> bestArrows, out LevelDifficulty.Result bestResult)
    {
        bestArrows = null;
        bestResult = default;
        int bestMiss = int.MaxValue;

        int attempts = levelIndex < OnboardingArrows.Length ? SeedAttemptsOnboarding : SeedAttemptsBulk;
        int tolerance = Mathf.Max(1, Mathf.RoundToInt(wantArrows * ArrowTolerance));

        // Deterministic per level, so re-baking reproduces the same campaign byte for byte unless
        // the ramp or the carver changes.
        int baseSeed = 7919 + levelIndex * 104729;

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            Random.InitState(baseSeed + attempt);

            int stranded;
            List<List<Vector2Int>> arrows = LevelCarver.CarveFull(
                LevelCarver.Rectangle(width, height), width, height, prm, out stranded);

            if (arrows == null || stranded > 0) continue;   // never ship unreachable cells

            LevelDifficulty.Result result = LevelDifficulty.Measure(arrows, width, height);
            if (!result.solvable) continue;

            int miss = Mathf.Abs(arrows.Count - wantArrows);
            if (miss < bestMiss)
            {
                bestMiss = miss;
                bestArrows = arrows;
                bestResult = result;
            }

            if (miss <= tolerance) break;   // close enough; stop paying for carves
        }
    }

    // ---------- logging ----------

    // Lets the tangle be judged without opening the game: '-' and '|' are straight body cells,
    // '+' is a bend, 'o' a tail, and < > ^ v the heads.
    public static string RenderAscii(List<List<Vector2Int>> arrows, int width, int height)
    {
        char[,] grid = new char[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                grid[x, y] = '.';

        foreach (List<Vector2Int> p in arrows)
        {
            for (int i = 0; i < p.Count; i++)
            {
                Vector2Int c = p[i];
                if (c.x < 0 || c.x >= width || c.y < 0 || c.y >= height) continue;

                char ch;
                if (i == p.Count - 1)
                {
                    Vector2Int d = p[i] - p[i - 1];
                    ch = d.x > 0 ? '>' : d.x < 0 ? '<' : d.y > 0 ? '^' : 'v';
                }
                else if (i == 0)
                {
                    ch = 'o';
                }
                else
                {
                    Vector2Int inDir = p[i] - p[i - 1];
                    Vector2Int outDir = p[i + 1] - p[i];
                    ch = inDir != outDir ? '+' : (inDir.x != 0 ? '-' : '|');
                }

                grid[c.x, c.y] = ch;
            }
        }

        StringBuilder sb = new StringBuilder();
        for (int y = height - 1; y >= 0; y--)   // top row first, so it reads like the screen
        {
            for (int x = 0; x < width; x++) sb.Append(grid[x, y]);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    static string RampRow(int levelIndex, int width, int height, int wantArrows,
                          List<List<Vector2Int>> arrows, LevelDifficulty.Result result)
    {
        int cells = 0, segments = 0, bends = 0;
        foreach (List<Vector2Int> p in arrows)
        {
            cells += p.Count;
            segments += Mathf.Max(1, p.Count - 1);
            for (int i = 2; i < p.Count; i++)
                if (p[i] - p[i - 1] != p[i - 1] - p[i - 2]) bends++;
        }

        return string.Format("{0,5} | {1,3}x{2,-3} | {3,5} | {4,7}/{5,-7} | {6,6:F1} | {7,9:F2} | {8,5} | {9}",
            levelIndex + 1, width, height, cells, wantArrows, arrows.Count,
            cells / (float)arrows.Count, bends / (float)segments,
            result.score, result.band.ToString().ToUpper());
    }
}
