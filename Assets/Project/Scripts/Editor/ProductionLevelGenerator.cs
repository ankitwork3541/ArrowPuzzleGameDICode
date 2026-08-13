using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

// Builds the shipping levels. Generation and validation are deliberately coupled: carve a filled
// region into arrows, measure how hard the result actually is to play, and only accept a seed whose
// MEASURED band is the one the slot asked for. A level is never labelled by assumption.
//
// The size/tier combinations in the table below were chosen by measuring this generator's output
// across every board size and length tier (see the LevelDifficulty header for what got dropped and
// why), so each slot's target band is one its configuration can actually reach.
public static class ProductionLevelGenerator
{
    // Arrow-length presets. The tiers deliberately differ only MILDLY, and they no longer differ
    // at all in how much the bodies bend.
    //
    // They used to: straightPenalty climbed 1.2 -> 1.5 -> 2.0, on the assumption that a more
    // tangled board is a harder board. Measuring the reference game's 47k shipped arrows showed
    // that is not how it scales difficulty — its bend rate is FLAT across all four of its tiers
    // (~0.30 bends per segment) and so is its mean arrow length (10.2 -> 12.5). What actually
    // ramps is board size and arrow count, which is the size table below, not these presets.
    // So turnChance stays constant here and only the length decay moves, matching that 10 -> 12.5.
    // Tutorial only. Short-ish arrows that run almost perfectly straight, so the very first board
    // teaches "tap an arrow, it slides out" with nothing else going on — the reference game's own
    // first level is literally three straight columns, measured at 0.00 bends per segment.
    static LevelCarver.Params Tier0()
    {
        return new LevelCarver.Params { lengthDecay = 0.80f, turnChance = 0.08f };
    }

    static LevelCarver.Params Tier1()
    {
        return new LevelCarver.Params { lengthDecay = 0.86f, turnChance = 0.33f };
    }

    static LevelCarver.Params Tier2()
    {
        return new LevelCarver.Params { lengthDecay = 0.90f, turnChance = 0.33f };
    }

    static LevelCarver.Params Tier3()
    {
        return new LevelCarver.Params { lengthDecay = 0.92f, turnChance = 0.33f };
    }

    class Spec
    {
        public string name;
        public string shapeName;        // for the log; "Rectangle" when ascii is null
        public string[] ascii;          // null = plain rectangle
        public float scale = 1f;        // upscale factor for ascii masks
        public int rectWidth, rectHeight;
        public System.Func<LevelCarver.Params> tier;
        public Difficulty target;

        // How many arrows this slot wants. 0 = don't care, pick the seed by measured band instead
        // (right for the shaped levels, where the silhouette already fixes the cell count).
        public int targetArrows;
    }

    // Levels 1-10 are a deliberate ARROW-COUNT ramp, not a board-size ramp picked by feel: 3, 6, 8,
    // 10, 16, 20, 24, 28, 32, 36. Those counts are the reference game's own early progression, read
    // off its screens.
    //
    // The board sizes were FITTED to those counts by simulating this carver, not calculated. The
    // obvious arithmetic — cells = arrows x average length — overshoots badly on small boards,
    // because an arrow there cannot reach its target length: the remaining region fragments around
    // it, so average length runs ~7 on an 8x8 against ~11 on a 20x20. Sizing Level02 by the big
    // board average gave 10-11 arrows where 6 were wanted.
    //
    // All ten are plain rectangles on purpose. Every early board in the reference is rectangular
    // and its first shaped board arrives around its sixth level, so shapes are held back to
    // Level 11+ rather than mixed into the tutorial ramp.
    static List<Spec> Table()
    {
        return new List<Spec>
        {
            //                                                                            target arrows / fitted
            new Spec { name = "Level01", shapeName = "Rectangle", rectWidth = 4,  rectHeight = 4,  tier = Tier0, target = Difficulty.Easy,     targetArrows = 3  },
            new Spec { name = "Level02", shapeName = "Rectangle", rectWidth = 5,  rectHeight = 7,  tier = Tier1, target = Difficulty.Easy,     targetArrows = 6  },
            new Spec { name = "Level03", shapeName = "Rectangle", rectWidth = 6,  rectHeight = 8,  tier = Tier1, target = Difficulty.Easy,     targetArrows = 8  },
            new Spec { name = "Level04", shapeName = "Rectangle", rectWidth = 8,  rectHeight = 8,  tier = Tier2, target = Difficulty.Medium,   targetArrows = 10 },
            new Spec { name = "Level05", shapeName = "Rectangle", rectWidth = 10, rectHeight = 12, tier = Tier2, target = Difficulty.Medium,   targetArrows = 16 },
            new Spec { name = "Level06", shapeName = "Rectangle", rectWidth = 12, rectHeight = 16, tier = Tier2, target = Difficulty.Medium,   targetArrows = 20 },
            new Spec { name = "Level07", shapeName = "Rectangle", rectWidth = 13, rectHeight = 16, tier = Tier2, target = Difficulty.Hard,     targetArrows = 24 },
            new Spec { name = "Level08", shapeName = "Rectangle", rectWidth = 14, rectHeight = 17, tier = Tier2, target = Difficulty.Hard,     targetArrows = 28 },
            new Spec { name = "Level09", shapeName = "Rectangle", rectWidth = 16, rectHeight = 23, tier = Tier3, target = Difficulty.Hard,     targetArrows = 32 },
            new Spec { name = "Level10", shapeName = "Rectangle", rectWidth = 20, rectHeight = 20, tier = Tier3, target = Difficulty.Hardest,  targetArrows = 36 },

            // Shapes start here, once the mechanic is fully taught.
            new Spec { name = "Level11", shapeName = "Heart",    ascii = LevelShapes.Heart,    scale = 1.0f, tier = Tier2, target = Difficulty.Hard },
            new Spec { name = "Level12", shapeName = "Bulb",     ascii = LevelShapes.Bulb,     scale = 1.0f, tier = Tier2, target = Difficulty.Hard },
            new Spec { name = "Level13", shapeName = "Teardrop", ascii = LevelShapes.Teardrop, scale = 1.6f, tier = Tier2, target = Difficulty.Hardest },
            new Spec { name = "Level14", shapeName = "Cat",      ascii = LevelShapes.Cat,      scale = 1.6f, tier = Tier3, target = Difficulty.Hardest },
            new Spec { name = "Level15", shapeName = "Gun",      ascii = LevelShapes.Gun,      scale = 2.8f, tier = Tier3, target = Difficulty.Nightmare }
        };
    }

    // How many seeds to try before settling for the closest miss. Raised from 40 once slots started
    // naming an exact arrow count: a band has 5 buckets and is easy to land, an exact count is not.
    const int SeedAttempts = 150;

    [MenuItem("ArrowPuzzle/Generate Production Levels (15)")]
    static void GenerateAll()
    {
        List<Spec> table = Table();
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("PRODUCTION LEVELS");
        summary.AppendLine("level   | shape     |   grid  | cells | arrows | bends | ray  | clr%  | waves | vfy | score | band       | target");

        int misses = 0;

        try
        {
            for (int i = 0; i < table.Count; i++)
            {
                Spec spec = table[i];
                EditorUtility.DisplayProgressBar("Generating levels", spec.name + " (" + spec.shapeName + ")", i / (float)table.Count);

                string line;
                if (!Generate(spec, i, out line)) misses++;
                summary.AppendLine(line);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        summary.AppendLine();
        summary.AppendLine(misses == 0
            ? "Every level hit what its slot asked for (arrow count where named, band otherwise)."
            : misses + " level(s) missed — see the rows above. The `arrows` column reads got/want; "
              + "adjust that slot's grid size if the miss is large, since board area is what sets the count.");
        summary.AppendLine("Next: drag Level01..Level15 into LevelLoader's `levels` array in the Inspector, in order.");

        Debug.Log(summary.ToString());
    }

    static bool Generate(Spec spec, int index, out string summaryLine)
    {
        HashSet<Vector2Int> mask;
        int width, height;

        if (spec.ascii != null)
        {
            HashSet<Vector2Int> baseMask = LevelCarver.FromAscii(spec.ascii);

            int smallest;
            int regions = LevelCarver.CountRegions(baseMask, out smallest);
            if (smallest < 2)
            {
                Debug.LogError(spec.name + " (" + spec.shapeName + "): silhouette has a region of "
                    + smallest + " cell(s). An arrow needs 2, so that region would strand. Fix the ASCII art.");
            }
            else if (regions > 1)
            {
                Debug.Log(spec.name + " (" + spec.shapeName + "): silhouette has " + regions
                    + " separate regions, smallest " + smallest + " cells. Each is solved independently.");
            }

            mask = LevelCarver.Upscale(baseMask, spec.scale, out width, out height);
        }
        else
        {
            width = spec.rectWidth;
            height = spec.rectHeight;
            mask = LevelCarver.Rectangle(width, height);
        }

        // Deterministic per slot, so regenerating gives the same 11 levels unless a spec changes.
        int baseSeed = 1000 + index * 977;

        List<List<Vector2Int>> bestArrows = null;
        LevelDifficulty.Result bestResult = default;
        int bestSeed = 0, bestDistance = int.MaxValue;
        bool hit = false;

        for (int attempt = 0; attempt < SeedAttempts; attempt++)
        {
            int seed = baseSeed + attempt;
            Random.InitState(seed);

            int stranded;
            List<List<Vector2Int>> arrows = LevelCarver.CarveFull(mask, width, height, spec.tier(), out stranded);
            if (arrows == null || stranded > 0) continue;   // never ship a level with unreachable cells

            LevelDifficulty.Result r = LevelDifficulty.Measure(arrows, width, height);
            if (!r.solvable) continue;

            // Which of the two goals leads matters, and getting it wrong is subtle. Selecting on the
            // measured BAND alone quietly fought the size table: an Easy target prefers a board with
            // FEW, long arrows, so Level02 kept saving at 3 arrows however carefully its 5x7 was
            // fitted for 6. Where a slot names an arrow count, that count leads and the band is only
            // a tie-break — difficulty here is emergent from scale, exactly as it is in the
            // reference game, so it is measured and reported rather than requested.
            int arrowMiss = spec.targetArrows > 0 ? Mathf.Abs(arrows.Count - spec.targetArrows) : 0;
            int bandMiss = Mathf.Abs((int)r.band - (int)spec.target);
            int distance = spec.targetArrows > 0 ? arrowMiss * 10 + bandMiss : bandMiss;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestArrows = arrows;
                bestResult = r;
                bestSeed = seed;
            }

            // "Hit" means the slot got what it actually asked for: the arrow count where one is
            // named, the band otherwise.
            bool bullseye = spec.targetArrows > 0 ? arrowMiss == 0 : bandMiss == 0;
            if (bullseye) { hit = true; break; }
        }

        if (bestArrows == null)
        {
            summaryLine = string.Format("{0} | {1,-9} | FAILED — no seed produced a fully-filled solvable level", spec.name, spec.shapeName);
            Debug.LogError(summaryLine);
            return false;
        }

        int cells = 0;
        foreach (List<Vector2Int> p in bestArrows) cells += p.Count;

        SaveLevel(spec.name, width, height, bestArrows, bestResult, bestSeed);

        // "got/want" where the slot named a count, so a miss is readable at a glance.
        string arrowsColumn = spec.targetArrows > 0
            ? bestArrows.Count + "/" + spec.targetArrows
            : bestArrows.Count.ToString();

        summaryLine = string.Format(
            "{0} | {1,-9} | {2,3}x{3,-3} | {4,5} | {5,6} | {6,5:F1} | {7,4:F1} | {8,5:P1} | {9,5} | {10,3} | {11,5} | {12,-10} | {13}{14}",
            spec.name, spec.shapeName, width, height, cells, arrowsColumn,
            bestResult.avgBends, bestResult.avgRayLength, bestResult.avgClearableRatio,
            bestResult.waves, bestResult.hardestVerify, bestResult.score,
            bestResult.band.ToString().ToUpper(), spec.target.ToString().ToUpper(),
            hit ? "" : "  <-- MISS");

        return hit;
    }

    // ---------- single-level tools, for tuning one slot without regenerating everything ----------

    // Select a LevelData asset in the Project window and run this to see its board as text, plus
    // its measured numbers re-derived from the saved paths rather than read back from the fields.
    [MenuItem("ArrowPuzzle/Tools/Print Selected Level")]
    static void PrintSelectedLevel()
    {
        LevelData level = Selection.activeObject as LevelData;
        if (level == null)
        {
            Debug.LogWarning("Select a LevelData asset in the Project window first.");
            return;
        }

        List<List<Vector2Int>> arrows = new List<List<Vector2Int>>();
        foreach (ArrowPathData a in level.arrows)
            if (a.path != null && a.path.Count >= 2) arrows.Add(new List<Vector2Int>(a.path));

        if (arrows.Count == 0)
        {
            Debug.LogWarning(level.name + " has no valid arrows.");
            return;
        }

        int cells = 0;
        foreach (List<Vector2Int> p in arrows) cells += p.Count;

        LevelDifficulty.Result r = LevelDifficulty.Measure(arrows, level.gridWidth, level.gridHeight);

        Debug.Log(string.Format("{0}  seed {1}  grid {2}x{3}\n{4}\n{5}",
            level.name, level.generatorSeed, level.gridWidth, level.gridHeight,
            LevelDifficulty.Describe(r, arrows.Count, cells),
            RenderAscii(arrows, level.gridWidth, level.gridHeight)));
    }

    [MenuItem("ArrowPuzzle/Tools/Print Shape Silhouettes")]
    static void PrintSilhouettes()
    {
        StringBuilder sb = new StringBuilder("SHAPE SILHOUETTES (region check)\n");

        AppendShape(sb, "Heart", LevelShapes.Heart);
        AppendShape(sb, "Bulb", LevelShapes.Bulb);
        AppendShape(sb, "Teardrop", LevelShapes.Teardrop);
        AppendShape(sb, "Cat", LevelShapes.Cat);
        AppendShape(sb, "Gun", LevelShapes.Gun);

        Debug.Log(sb.ToString());
    }

    static void AppendShape(StringBuilder sb, string name, string[] ascii)
    {
        HashSet<Vector2Int> mask = LevelCarver.FromAscii(ascii);
        int smallest;
        int regions = LevelCarver.CountRegions(mask, out smallest);

        sb.AppendFormat("\n{0}: {1} cells, {2} region(s), smallest {3}{4}\n",
            name, mask.Count, regions, smallest, smallest < 2 ? "   <-- WOULD STRAND" : "");
        foreach (string row in ascii) sb.AppendLine("  " + row);
    }

    // Shows what one slot measures across all three length tiers, without writing assets — the
    // quickest way to retarget a slot that missed its band.
    [MenuItem("ArrowPuzzle/Tools/Sweep Tiers For Each Level")]
    static void SweepTiers()
    {
        List<Spec> table = Table();
        StringBuilder sb = new StringBuilder("TIER SWEEP (no assets written)\nlevel   | shape     | tier | arrows | bends | ray  | clr%  | waves | score | band\n");

        System.Func<LevelCarver.Params>[] tiers = { Tier0, Tier1, Tier2, Tier3 };
        string[] tierNames = { "T0", "T1", "T2", "T3" };

        for (int i = 0; i < table.Count; i++)
        {
            Spec spec = table[i];

            HashSet<Vector2Int> mask;
            int width, height;
            if (spec.ascii != null)
                mask = LevelCarver.Upscale(LevelCarver.FromAscii(spec.ascii), spec.scale, out width, out height);
            else
            {
                width = spec.rectWidth;
                height = spec.rectHeight;
                mask = LevelCarver.Rectangle(width, height);
            }

            for (int t = 0; t < tiers.Length; t++)
            {
                Random.InitState(1000 + i * 977);

                int stranded;
                List<List<Vector2Int>> arrows = LevelCarver.CarveFull(mask, width, height, tiers[t](), out stranded);
                if (arrows == null) continue;

                LevelDifficulty.Result r = LevelDifficulty.Measure(arrows, width, height);

                sb.AppendFormat("{0} | {1,-9} | {2}   | {3,6} | {4,5:F1} | {5,4:F1} | {6,5:P1} | {7,5} | {8,5} | {9}{10}\n",
                    spec.name, spec.shapeName, tierNames[t], arrows.Count, r.avgBends, r.avgRayLength,
                    r.avgClearableRatio, r.waves, r.score, r.band.ToString().ToUpper(),
                    r.band == spec.target ? "  <-- target" : "");
            }
        }

        Debug.Log(sb.ToString());
    }

    // ---------- saving ----------

    static void SaveLevel(string assetName, int width, int height,
                          List<List<Vector2Int>> arrowPaths, LevelDifficulty.Result r, int seed)
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.gridWidth = width;
        level.gridHeight = height;
        level.arrows = new List<ArrowPathData>();

        foreach (List<Vector2Int> path in arrowPaths)
        {
            ArrowPathData arrowData = new ArrowPathData();
            arrowData.path = path;
            level.arrows.Add(arrowData);
        }

        level.difficulty = r.band;
        level.difficultyScore = r.score;
        level.generatorSeed = seed;
        level.measuredClearableRatio = r.avgClearableRatio;
        level.measuredAvgRayLength = r.avgRayLength;
        level.measuredWaves = r.waves;
        level.measuredAvgBends = r.avgBends;
        level.measuredMinClearable = r.minClearable;
        level.measuredDecoyRatio = r.decoyRatio;
        level.measuredHardestVerify = r.hardestVerify;

        string filePath = "Assets/Project/Levels/" + assetName + ".asset";
        LevelData existing = AssetDatabase.LoadAssetAtPath<LevelData>(filePath);

        if (existing != null)
        {
            // Overwrite in place, so LevelLoader's array keeps pointing at a valid asset — creating
            // a new one would mean re-dragging all 11 references after every regeneration.
            EditorUtility.CopySerialized(level, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(level);
        }
        else
        {
            AssetDatabase.CreateAsset(level, filePath);
        }
    }

    // ---------- console preview ----------

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
}
