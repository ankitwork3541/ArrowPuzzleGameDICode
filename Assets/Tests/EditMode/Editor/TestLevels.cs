using System.Collections.Generic;
using UnityEngine;

// Hand-built boards shared by the EditMode tests. Test data lives here, never in the game code.
public static class TestLevels
{
    // A tail-first path: start at `tail`, then one step per entry, each an index into
    // LevelBinary.Dirs (0 = up, 1 = down, 2 = right, 3 = left).
    public static List<Vector2Int> Path(Vector2Int tail, params int[] steps)
    {
        List<Vector2Int> path = new List<Vector2Int> { tail };
        Vector2Int cell = tail;
        foreach (int s in steps)
        {
            cell += LevelBinary.Dirs[s];
            path.Add(cell);
        }
        return path;
    }

    // `count` two-cell arrows, one per row, all heading right with a clear ray to the edge. Arrow i
    // is in solve position i, and no arrow's ray crosses another's cells, so any lock threshold up
    // to its index is satisfiable. Use a board at least 3 wide and `count` high.
    public static List<List<Vector2Int>> RowArrows(int count)
    {
        List<List<Vector2Int>> arrows = new List<List<Vector2Int>>(count);
        for (int i = 0; i < count; i++)
            arrows.Add(Path(new Vector2Int(0, i), 2));
        return arrows;
    }

    public static bool SamePath(List<Vector2Int> a, List<Vector2Int> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    public static bool SameArrows(List<List<Vector2Int>> a, List<List<Vector2Int>> b)
    {
        if (a == null || b == null) return a == b;
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
            if (!SamePath(a[i], b[i])) return false;
        return true;
    }

    // Runs `body` with UnityEngine.Random seeded, then puts the global random state back so one
    // test cannot leak its seed into the next.
    public static T WithSeed<T>(int seed, System.Func<T> body)
    {
        Random.State saved = Random.state;
        try
        {
            Random.InitState(seed);
            return body();
        }
        finally
        {
            Random.state = saved;
        }
    }

    public static List<List<Vector2Int>> Carve(int seed, int width, int height, out int stranded)
    {
        int strandedCells = 0;
        List<List<Vector2Int>> arrows = WithSeed(seed, () =>
        {
            HashSet<Vector2Int> mask = LevelCarver.Rectangle(width, height);
            return LevelCarver.CarveFull(mask, width, height, new LevelCarver.Params(), out strandedCells);
        });
        stranded = strandedCells;
        return arrows;
    }
}
