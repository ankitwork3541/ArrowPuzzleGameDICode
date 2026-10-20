using System.Collections.Generic;
using UnityEngine;

// Decides which arrows of a freshly carved level get a padlock, and how many other arrows must
// leave before it opens.
//
// Why a lock can never trap the player: StageCarver returns arrows in SOLVE ORDER, and arrow i's
// exit ray only ever crosses cells of arrows with a LOWER index. So the lowest-indexed arrow still
// on the board is always free, and every arrow with a lower index has already left, which means
// at least i arrows have exited. A threshold of at most i is therefore already met when it is that
// arrow's turn, whatever order the player took. The solver (StageRating.Measure) re-checks
// every baked level anyway.
public static class StageLocks
{
    // The first level that carries a lock (0-based). Earlier levels teach the plain mechanic.
    public const int FirstLockLevelIndex = 5;

    // Share of a level's arrows that are padlocked: starts small, grows over ShareRampLevels.
    const float ShareStart = 0.06f;
    const float ShareEnd = 0.22f;
    const int ShareRampLevels = 400;

    // Caps. A threshold has to stay readable as one digit on the padlock, and a level where
    // half the board is locked stops being a puzzle about the ray.
    public const int MaxThreshold = 9;
    const int MaxLocksPerLevel = 30;

    // Lowest threshold, as a fraction of the arrow's solve position, so a lock actually waits
    // for something instead of opening after the first exit.
    const float MinThresholdFraction = 0.5f;

    // One lock threshold per arrow (0 = unlocked), same order as `arrows`.
    public static int[] Assign(int levelIndex, List<List<Vector2Int>> dragons, int width, int height)
    {
        int n = dragons.Count;
        int[] locks = new int[n];
        if (levelIndex < FirstLockLevelIndex || n < 4) return locks;

        float u = Mathf.Clamp01((levelIndex - FirstLockLevelIndex) / (float)ShareRampLevels);
        int want = Mathf.Max(1, Mathf.RoundToInt(n * Mathf.Lerp(ShareStart, ShareEnd, u)));
        want = Mathf.Min(want, Mathf.Min(MaxLocksPerLevel, n / 2));

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        foreach (List<Vector2Int> p in dragons)
            foreach (Vector2Int c in p) occupied.Add(c);

        // Arrow 0 cannot be locked (no arrow can have left before it). Among the rest, arrows that
        // are free to leave from the start come first: a lock on an arrow that was blocked anyway
        // changes nothing the player can see.
        List<KeyValuePair<float, int>> ranked = new List<KeyValuePair<float, int>>(n);
        for (int i = 1; i < n; i++)
        {
            float key = (IsFreeNow(dragons[i], occupied, width, height) ? 0f : 1f)
                        + StageCampaignGenerator.Hash01(levelIndex * 977 + i);
            ranked.Add(new KeyValuePair<float, int>(key, i));
        }
        ranked.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int k = 0; k < want && k < ranked.Count; k++)
        {
            int i = ranked[k].Value;

            int hi = Mathf.Min(MaxThreshold, i);
            int lo = Mathf.Min(hi, Mathf.Max(2, Mathf.CeilToInt(i * MinThresholdFraction)));
            if (i == 1) lo = 1;

            float pick = StageCampaignGenerator.Hash01(levelIndex * 1553 + i * 31 + 5);
            locks[i] = lo + Mathf.Min(hi - lo, Mathf.FloorToInt(pick * (hi - lo + 1)));
        }

        return locks;
    }

    // True when the arrow's whole exit ray is empty on the untouched board.
    static bool IsFreeNow(List<Vector2Int> path, HashSet<Vector2Int> occupied, int width, int height)
    {
        Vector2Int head = path[path.Count - 1];
        Vector2Int dir = head - path[path.Count - 2];

        Vector2Int cursor = head + dir;
        while (cursor.x >= 0 && cursor.x < width && cursor.y >= 0 && cursor.y < height)
        {
            if (occupied.Contains(cursor)) return false;
            cursor += dir;
        }
        return true;
    }
}
