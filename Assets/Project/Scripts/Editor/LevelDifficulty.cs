using System.Collections.Generic;
using UnityEngine;

// Measures how hard a finished level actually is to PLAY, by simulating the solve.
//
// Why not arrow count: in this game removing a clearable arrow only ever frees cells, so it can
// never block another arrow. There is no wrong ORDER and no way to brick a level — the only way to
// lose a life is to tap an arrow whose exit ray turns out to be blocked. That makes difficulty a
// VISUAL SEARCH problem: how much board must be scanned to find a legal move, and how much work
// each candidate takes to verify.
//
// The four scored features and their anchors were chosen by measuring this generator's real output
// across board sizes and arrow-length tiers, not picked a priori. Two features that looked
// promising were measured and DROPPED from the score:
//
//   - decoy ratio (blockers far down the ray): measures 0.00-0.03 everywhere. At 100% fill a
//     blocked arrow's blocker is almost always directly adjacent, so decoys need empty space
//     this game's levels do not have. Still reported below, because it will matter if levels
//     with deliberate gaps are ever added.
//   - minimum clearable count: sits at 1-2 on every board from 6x8 to 28x38, because some corner
//     arrow can always just barely leave. Saturated, so it discriminates nothing. The AVERAGE
//     clearable count as a fraction of arrows does vary (0.31 down to 0.065) and is what is used.
public static class LevelDifficulty
{
    // A blocker this far or further down the ray makes an arrow a "decoy" — diagnostic only.
    const int DecoyDistance = 5;

    // Normalisation anchors: the low value is where a feature stops contributing, the high value
    // is where more of it stops making a level meaningfully harder. Taken from the measured range
    // of this generator's output, so the five bands actually spread across shippable levels.
    const float ScarcityEasy = 0.35f;   // clearable/arrows at or above this = trivial to find a move
    const float ScarcityHard = 0.05f;
    const float RayEasy = 1f;
    const float RayHard = 9f;
    const float WavesEasy = 3f;
    const float WavesHard = 18f;
    const float BendsEasy = 2f;
    const float BendsHard = 7f;

    public struct Result
    {
        public bool solvable;

        // Scored
        public float avgClearableRatio;   // mean legal moves per wave, as a fraction of all arrows
        public float avgRayLength;        // cells of ray a player must trace to verify a move
        public int waves;                 // rounds the greedy solve takes to clear the board
        public float avgBends;            // bends per arrow — how hard a body is to follow

        // Diagnostic only
        public int minClearable;
        public float decoyRatio;
        // The longest ray the EASIEST available legal move needs, at its worst point in the solve.
        // High means there is a moment where every legal move takes a long trace to confirm — that
        // is where players guess and lose a life. Reported, deliberately NOT gated on: a wrong tap
        // costs only a life and never bricks the level, so this is a comfort signal for playtesting
        // rather than a correctness threshold, and inventing a cutoff would be false precision.
        public int hardestVerify;

        public int score;
        public Difficulty band;
    }

    public static Result Measure(List<List<Vector2Int>> arrows, int width, int height)
    {
        Result r = new Result();

        int totalBends = 0;
        foreach (List<Vector2Int> p in arrows) totalBends += CountBends(p);
        r.avgBends = arrows.Count > 0 ? totalBends / (float)arrows.Count : 0f;

        // Greedy fixed-point solve, the same model the carve itself is built on: clear every
        // currently-legal arrow, then look again. Each round is one "wave". Order within a wave
        // never matters, because clearing an arrow can only ever help the others.
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        foreach (List<Vector2Int> p in arrows)
            foreach (Vector2Int c in p) occupied.Add(c);

        List<List<Vector2Int>> remaining = new List<List<Vector2Int>>(arrows);
        List<List<Vector2Int>> clearable = new List<List<Vector2Int>>();

        r.minClearable = int.MaxValue;
        int clearableSum = 0;
        int decoys = 0, blockedSeen = 0;
        float raySum = 0f;
        int rayCount = 0;

        while (remaining.Count > 0)
        {
            clearable.Clear();
            int easiestVerify = int.MaxValue;

            foreach (List<Vector2Int> p in remaining)
            {
                Vector2Int dir = HeadDir(p);
                int blockAt = RayBlockDistance(Head(p), dir, occupied, width, height);

                if (blockAt < 0)
                {
                    clearable.Add(p);
                    int toEdge = RayLengthToEdge(Head(p), dir, width, height);
                    raySum += toEdge;
                    rayCount++;
                    if (toEdge < easiestVerify) easiestVerify = toEdge;
                }
                else
                {
                    blockedSeen++;
                    if (blockAt >= DecoyDistance) decoys++;
                }
            }

            if (clearable.Count == 0)
            {
                r.solvable = false;
                return r;   // impossible for a carved level; caught here rather than shipped
            }

            r.waves++;
            clearableSum += clearable.Count;
            if (clearable.Count < r.minClearable) r.minClearable = clearable.Count;
            if (easiestVerify > r.hardestVerify) r.hardestVerify = easiestVerify;

            foreach (List<Vector2Int> p in clearable)
            {
                foreach (Vector2Int c in p) occupied.Remove(c);
                remaining.Remove(p);
            }
        }

        r.solvable = true;
        if (r.minClearable == int.MaxValue) r.minClearable = 0;

        float avgClearable = r.waves > 0 ? clearableSum / (float)r.waves : 0f;
        r.avgClearableRatio = arrows.Count > 0 ? avgClearable / arrows.Count : 1f;
        r.avgRayLength = rayCount > 0 ? raySum / rayCount : 0f;
        r.decoyRatio = blockedSeen > 0 ? decoys / (float)blockedSeen : 0f;

        r.score = Score(r);
        r.band = Band(r.score);
        return r;
    }

    static int Score(Result r)
    {
        // Scarcity is relative: 4 legal moves out of 200 arrows is a far harder search than 4 out
        // of 10, so what counts is the fraction of the board that is currently tappable.
        float scarcity = Norm(r.avgClearableRatio, ScarcityEasy, ScarcityHard);

        float composite = 0.30f * scarcity
                        + 0.25f * Norm(r.avgRayLength, RayEasy, RayHard)
                        + 0.20f * Norm(r.waves, WavesEasy, WavesHard)
                        + 0.25f * Norm(r.avgBends, BendsEasy, BendsHard);

        return Mathf.RoundToInt(composite * 100f);
    }

    // 0 at the easy anchor, 1 at the hard one. Works in either direction, so a feature where
    // LOWER is harder (scarcity) needs no special case.
    static float Norm(float value, float easy, float hard)
    {
        if (Mathf.Approximately(easy, hard)) return 0f;
        return Mathf.Clamp01((value - easy) / (hard - easy));
    }

    public static Difficulty Band(int score)
    {
        if (score <= 20) return Difficulty.Easy;
        if (score <= 40) return Difficulty.Medium;
        if (score <= 60) return Difficulty.Hard;
        if (score <= 80) return Difficulty.Hardest;
        return Difficulty.Nightmare;
    }

    public static string Describe(Result r, int arrowCount, int cellCount)
    {
        return string.Format(
            "{0} (score {1})  {2} arrows / {3} cells\n" +
            "    clearable {4:P1} of board per wave   ray {5:F1}   waves {6}   bends {7:F1}\n" +
            "    diagnostics: minClearable {8}   decoy {9:P0}   hardestVerify {10}",
            r.band.ToString().ToUpper(), r.score, arrowCount, cellCount,
            r.avgClearableRatio, r.avgRayLength, r.waves, r.avgBends,
            r.minClearable, r.decoyRatio, r.hardestVerify);
    }

    // ---------- geometry helpers ----------

    static Vector2Int Head(List<Vector2Int> p) { return p[p.Count - 1]; }
    static Vector2Int HeadDir(List<Vector2Int> p) { return p[p.Count - 1] - p[p.Count - 2]; }

    static bool InBounds(Vector2Int c, int w, int h)
    {
        return c.x >= 0 && c.x < w && c.y >= 0 && c.y < h;
    }

    // -1 when the ray reaches the grid edge unobstructed, otherwise how many cells along it the
    // first blocker sits (1 = directly in front of the head). The arrow's own body counts as a
    // blocker, which is correct: an arrow whose ray crosses itself could never leave.
    static int RayBlockDistance(Vector2Int head, Vector2Int dir, HashSet<Vector2Int> occupied, int w, int h)
    {
        Vector2Int cursor = head + dir;
        int d = 1;
        while (InBounds(cursor, w, h))
        {
            if (occupied.Contains(cursor)) return d;
            cursor += dir;
            d++;
        }
        return -1;
    }

    static int RayLengthToEdge(Vector2Int head, Vector2Int dir, int w, int h)
    {
        Vector2Int cursor = head + dir;
        int d = 0;
        while (InBounds(cursor, w, h)) { cursor += dir; d++; }
        return d;
    }

    static int CountBends(List<Vector2Int> p)
    {
        int bends = 0;
        for (int i = 2; i < p.Count; i++)
            if (p[i] - p[i - 1] != p[i - 1] - p[i - 2]) bends++;
        return bends;
    }
}
