using System.Collections.Generic;
using UnityEngine;

// Carves a filled region into arrows, in solve order.
//
// The whole shape starts "filled". An arrow may only be carved out once its exit ray passes
// exclusively through already-carved (empty) cells, so the board peels outside-in. Two properties
// fall out for free: the carve order IS a valid solve order, and because the shape is being
// PARTITIONED rather than randomly filled, coverage is 100% by construction.
//
// An arrow can never block itself either: every body cell is still in `remaining` while it grows,
// and the ray is only allowed through non-`remaining` cells, so no separate self-check is needed.
public static class LevelCarver
{
    static readonly Vector2Int[] Dirs =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 0), new Vector2Int(-1, 0)
    };

    public class Params
    {
        public int minLength = 2;
        public int maxLength = 7;

        // A minority of very long snaking arrows is what gives the reference boards their tangled
        // look. A single uniform length range reads as evenly diced and much flatter.
        public int longMinLength = 10;
        public int longMaxLength = 22;
        public float longChance = 0.25f;

        // How strongly growth prefers turning over continuing straight. This is the knob that
        // produces bends — with it at 0 the carve grows near-straight arrows, which is exactly
        // what the old generator did and why its levels never looked tangled.
        public float straightPenalty = 1.5f;

        public int attempts = 12;   // retries until the carve reaches 100% coverage
    }

    // ---------- carving ----------

    public static List<List<Vector2Int>> CarveFull(HashSet<Vector2Int> mask, int width, int height, Params prm, out int stranded)
    {
        List<List<Vector2Int>> best = null;
        int bestStranded = int.MaxValue;

        // Random is seeded by the caller, so this whole retry loop is deterministic per seed.
        for (int i = 0; i < prm.attempts; i++)
        {
            int s;
            List<List<Vector2Int>> arrows = Carve(mask, width, height, prm, out s);

            if (s < bestStranded)
            {
                bestStranded = s;
                best = arrows;
            }
            if (s == 0) break;
        }

        stranded = bestStranded;
        return best;
    }

    static List<List<Vector2Int>> Carve(HashSet<Vector2Int> mask, int width, int height, Params prm, out int stranded)
    {
        HashSet<Vector2Int> remaining = new HashSet<Vector2Int>(mask);
        List<List<Vector2Int>> arrows = new List<List<Vector2Int>>();

        List<Vector2Int> headCandidates = new List<Vector2Int>();
        List<Vector2Int> dirCandidates = new List<Vector2Int>();
        List<Vector2Int> options = new List<Vector2Int>();
        List<Vector2Int> best = new List<Vector2Int>();

        while (remaining.Count > 0)
        {
            // Which (head, direction) pairs could legally exit right now? A path needs at least
            // 2 cells, so the cell behind the head must still be filled too.
            headCandidates.Clear();
            dirCandidates.Clear();
            int minDegree = int.MaxValue;

            foreach (Vector2Int c in remaining)
            {
                foreach (Vector2Int d in Dirs)
                {
                    if (!remaining.Contains(c - d)) continue;
                    if (!RayIsClear(c, d, remaining, width, height)) continue;

                    int degree = 0;
                    foreach (Vector2Int nd in Dirs)
                        if (remaining.Contains(c + nd)) degree++;

                    // Fewest-neighbour heads first, so danglers get consumed before they become
                    // isolated — a lone single cell can never form an arrow, which needs 2.
                    if (degree < minDegree)
                    {
                        minDegree = degree;
                        headCandidates.Clear();
                        dirCandidates.Clear();
                    }
                    if (degree == minDegree)
                    {
                        headCandidates.Add(c);
                        dirCandidates.Add(d);
                    }
                }
            }

            if (headCandidates.Count == 0) break;   // wedged — the retry loop reseeds

            int pick = Random.Range(0, headCandidates.Count);
            Vector2Int head = headCandidates[pick];
            Vector2Int dir = dirCandidates[pick];

            // Grow the body backwards from the head, staying inside `remaining`.
            List<Vector2Int> path = new List<Vector2Int> { head, head - dir };
            int targetLength = PickLength(prm);

            while (true)
            {
                Vector2Int tail = path[path.Count - 1];
                Vector2Int lastDir = tail - path[path.Count - 2];

                options.Clear();
                foreach (Vector2Int d in Dirs)
                {
                    Vector2Int n = tail + d;
                    if (remaining.Contains(n) && !path.Contains(n)) options.Add(n);
                }
                if (options.Count == 0) break;

                // A neighbour that would be left with no remaining neighbours once this path is
                // removed must be absorbed now, or it strands permanently.
                Vector2Int mustCell = default;
                bool hasMust = false;
                foreach (Vector2Int n in options)
                {
                    if (DegreeAfter(n, remaining, path) == 0) { mustCell = n; hasMust = true; break; }
                }

                if (path.Count >= targetLength && !hasMust) break;

                if (hasMust)
                {
                    path.Add(mustCell);
                    continue;
                }

                // Lower score is better: still avoid creating near-isolated pockets, but pay a
                // penalty for continuing straight, so the body actually bends.
                float bestScore = float.MaxValue;
                foreach (Vector2Int n in options)
                {
                    float s = GrowthScore(n, tail, lastDir, remaining, path, prm);
                    if (s < bestScore) bestScore = s;
                }

                best.Clear();
                foreach (Vector2Int n in options)
                {
                    if (Mathf.Approximately(GrowthScore(n, tail, lastDir, remaining, path, prm), bestScore))
                        best.Add(n);
                }

                path.Add(best[Random.Range(0, best.Count)]);
            }

            path.Reverse();   // now [tail ... head]
            foreach (Vector2Int p in path) remaining.Remove(p);
            arrows.Add(path);
        }

        // Repair: attach any leftover cell to an adjacent arrow's TAIL. Safe because the head's
        // direction is untouched (so its ray is unchanged), and a never-carved cell is by
        // definition one that no ray ever crossed.
        bool progress = true;
        while (remaining.Count > 0 && progress)
        {
            progress = false;
            foreach (Vector2Int c in new List<Vector2Int>(remaining))
            {
                foreach (List<Vector2Int> path in arrows)
                {
                    Vector2Int tail = path[0];
                    if (Mathf.Abs(tail.x - c.x) + Mathf.Abs(tail.y - c.y) == 1)
                    {
                        path.Insert(0, c);
                        remaining.Remove(c);
                        progress = true;
                        break;
                    }
                }
            }
        }

        stranded = remaining.Count;
        return arrows;
    }

    static float GrowthScore(Vector2Int n, Vector2Int tail, Vector2Int lastDir,
                             HashSet<Vector2Int> remaining, List<Vector2Int> path, Params prm)
    {
        float s = DegreeAfter(n, remaining, path);
        if (n - tail == lastDir) s += prm.straightPenalty;
        return s;
    }

    static int PickLength(Params prm)
    {
        if (Random.value < prm.longChance)
            return Random.Range(prm.longMinLength, prm.longMaxLength + 1);
        return Random.Range(prm.minLength, prm.maxLength + 1);
    }

    static bool RayIsClear(Vector2Int head, Vector2Int dir, HashSet<Vector2Int> remaining, int width, int height)
    {
        Vector2Int cursor = head + dir;
        while (InBounds(cursor, width, height))
        {
            if (remaining.Contains(cursor)) return false;
            cursor += dir;
        }
        return true;
    }

    // How many neighbours of `n` are still filled once `path` is taken out.
    static int DegreeAfter(Vector2Int n, HashSet<Vector2Int> remaining, List<Vector2Int> path)
    {
        int count = 0;
        foreach (Vector2Int d in Dirs)
        {
            Vector2Int nb = n + d;
            if (remaining.Contains(nb) && !path.Contains(nb)) count++;
        }
        return count;
    }

    static bool InBounds(Vector2Int c, int w, int h)
    {
        return c.x >= 0 && c.x < w && c.y >= 0 && c.y < h;
    }

    // ---------- masks ----------

    public static HashSet<Vector2Int> Rectangle(int width, int height)
    {
        HashSet<Vector2Int> mask = new HashSet<Vector2Int>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                mask.Add(new Vector2Int(x, y));
        return mask;
    }

    // ASCII art, TOP row first so the literal in code reads the way the shape looks on screen.
    // '.' and space are empty; every other character is a filled cell.
    public static HashSet<Vector2Int> FromAscii(string[] rows)
    {
        HashSet<Vector2Int> mask = new HashSet<Vector2Int>();
        int h = rows.Length;

        for (int r = 0; r < h; r++)
        {
            string row = rows[r];
            for (int x = 0; x < row.Length; x++)
            {
                char ch = row[x];
                if (ch == '.' || ch == ' ') continue;
                mask.Add(new Vector2Int(x, h - 1 - r));   // flip, because grid y grows upward
            }
        }

        return mask;
    }

    // Nearest-neighbour scale-up: every source cell becomes a solid block, so connectivity is
    // preserved and a shape authored once can be reused at any difficulty size without redrawing.
    public static HashSet<Vector2Int> Upscale(HashSet<Vector2Int> mask, float factor, out int width, out int height)
    {
        int srcW = 0, srcH = 0;
        foreach (Vector2Int c in mask)
        {
            if (c.x + 1 > srcW) srcW = c.x + 1;
            if (c.y + 1 > srcH) srcH = c.y + 1;
        }

        width = Mathf.RoundToInt(srcW * factor);
        height = Mathf.RoundToInt(srcH * factor);

        HashSet<Vector2Int> scaled = new HashSet<Vector2Int>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sx = Mathf.Min(srcW - 1, (int)(x / factor));
                int sy = Mathf.Min(srcH - 1, (int)(y / factor));
                if (mask.Contains(new Vector2Int(sx, sy)))
                    scaled.Add(new Vector2Int(x, y));
            }
        }

        return scaled;
    }

    // Counts 4-connected regions. More than one is not automatically wrong — one reference level
    // is a shape plus a separate block under it — but a region of a single cell cannot hold an
    // arrow (they need 2 cells), so it would strand. Worth reporting before generating.
    public static int CountRegions(HashSet<Vector2Int> mask, out int smallestRegion)
    {
        HashSet<Vector2Int> unvisited = new HashSet<Vector2Int>(mask);
        List<Vector2Int> frontier = new List<Vector2Int>();
        int regions = 0;
        smallestRegion = int.MaxValue;

        while (unvisited.Count > 0)
        {
            Vector2Int start = default;
            foreach (Vector2Int c in unvisited) { start = c; break; }

            frontier.Clear();
            frontier.Add(start);
            unvisited.Remove(start);
            int size = 0;

            while (frontier.Count > 0)
            {
                Vector2Int c = frontier[frontier.Count - 1];
                frontier.RemoveAt(frontier.Count - 1);
                size++;

                foreach (Vector2Int d in Dirs)
                {
                    Vector2Int nb = c + d;
                    if (unvisited.Remove(nb)) frontier.Add(nb);
                }
            }

            regions++;
            if (size < smallestRegion) smallestRegion = size;
        }

        if (regions == 0) smallestRegion = 0;
        return regions;
    }
}
