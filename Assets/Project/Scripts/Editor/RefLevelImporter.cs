using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

// TEST / BENCHMARK TOOL ONLY — NOT FOR SHIPPING.
//
// Converts a reference game's level JSON into LevelData assets so its boards can be played and
// measured side by side with this generator's output. The point is to answer one question: does
// our carve now produce boards that FEEL like the reference's?
//
// The imported assets are third-party level content. They go into their own folder, they are never
// added to the shipping level list, and `ArrowPuzzle/Delete Reference Levels` removes them. Delete
// them once the comparison is done — do not ship them, and do not treat them as our content.
//
// Editor-only by folder, so none of this is ever compiled into a build.
public static class RefLevelImporter
{
    // Point this at whichever folder is being compared. The reference's onboarding folder is the
    // right one for early levels: its `levels/` folder is NOT the player's first experience — it is
    // a flat campaign pool of 25x38-and-up boards, while onboarding is the actual 5x6 -> 26x38 ramp.
    const string SourceFolder =
        "/Users/shiftxinfotech/Downloads/arrow-reference/ExportedProject/Assets/Resources/alllevels/onboardinglevels";
    const string SourcePrefix = "OnboardingLevels_";
    const int ImportCount = 10;

    const string DestFolder = "Assets/Project/Levels/RefTest";

    // Their JSON shape. Field names must match the file exactly for JsonUtility.
    [System.Serializable]
    class RefArrow
    {
        public int Dx, Dy, X, Y;
        public List<int> Indices;
        public int BendCount;   // present in their data, unused at their runtime and ~10% wrong
    }

    [System.Serializable]
    class RefLevel
    {
        public int XSize, YSize;
        public List<RefArrow> Arrows;
    }

    [MenuItem("ArrowPuzzle/Import Reference Levels (test only)")]
    static void Import()
    {
        if (!Directory.Exists(SourceFolder))
        {
            Debug.LogError($"[RefImport] Source folder not found:\n{SourceFolder}");
            return;
        }

        if (!AssetDatabase.IsValidFolder(DestFolder))
            AssetDatabase.CreateFolder("Assets/Project/Levels", "RefTest");

        StringBuilder log = new StringBuilder();
        log.AppendLine("REFERENCE LEVELS (imported for comparison — delete before shipping)");
        log.AppendLine("asset      |   grid  | cells | arrows | avgLen | bends/seg | longRun | bends | ray  | clr%  | waves | score | band");

        int imported = 0;

        for (int i = 1; i <= ImportCount; i++)
        {
            string file = Path.Combine(SourceFolder, SourcePrefix + i + ".json");
            if (!File.Exists(file))
            {
                Debug.LogWarning($"[RefImport] missing: {file}");
                continue;
            }

            RefLevel src = JsonUtility.FromJson<RefLevel>(File.ReadAllText(file));
            if (src == null || src.Arrows == null || src.Arrows.Count == 0)
            {
                Debug.LogWarning($"[RefImport] unparsable or empty: {file}");
                continue;
            }

            List<List<Vector2Int>> paths = Convert(src);
            LevelDifficulty.Result r = LevelDifficulty.Measure(paths, src.XSize, src.YSize);

            string assetName = "RefLevel" + i.ToString("00");
            Save(assetName, src.XSize, src.YSize, paths, r);
            imported++;

            log.AppendLine(Row(assetName, src.XSize, src.YSize, paths, r));

            // Same ASCII preview the production generator uses, so the two can be eyeballed
            // in the console without loading either one into the scene.
            Debug.Log($"{assetName}  {src.XSize}x{src.YSize}  {paths.Count} arrows\n"
                      + ProductionLevelGenerator.RenderAscii(paths, src.XSize, src.YSize));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        log.AppendLine();
        log.AppendLine("OUR CURRENT LEVELS (same measurements, for comparison)");
        log.AppendLine("asset      |   grid  | cells | arrows | avgLen | bends/seg | longRun | bends | ray  | clr%  | waves | score | band");
        AppendOurLevels(log);

        Debug.Log(log.ToString());
        Debug.Log($"[RefImport] {imported} reference levels written to {DestFolder}. "
                  + "To play them, point LevelLoader's Levels array at them — then run "
                  + "ArrowPuzzle/Delete Reference Levels when finished.");
    }

    [MenuItem("ArrowPuzzle/Delete Reference Levels")]
    static void DeleteImported()
    {
        if (!AssetDatabase.IsValidFolder(DestFolder))
        {
            Debug.Log("[RefImport] nothing to delete.");
            return;
        }

        AssetDatabase.DeleteAsset(DestFolder);
        AssetDatabase.Refresh();
        Debug.Log($"[RefImport] deleted {DestFolder}. Remember to clear any LevelLoader slots that pointed at it.");
    }

    // Their Indices are flattened cell ids (index = Y * XSize + X) ordered HEAD FIRST — verified
    // against 12,995 of their arrows. Ours are tail-first, so the list is reversed on the way in.
    static List<List<Vector2Int>> Convert(RefLevel src)
    {
        List<List<Vector2Int>> paths = new List<List<Vector2Int>>();

        foreach (RefArrow a in src.Arrows)
        {
            if (a.Indices == null || a.Indices.Count < 2) continue;

            List<Vector2Int> path = new List<Vector2Int>(a.Indices.Count);
            for (int i = a.Indices.Count - 1; i >= 0; i--)
            {
                int idx = a.Indices[i];
                path.Add(new Vector2Int(idx % src.XSize, idx / src.XSize));
            }
            paths.Add(path);
        }

        return paths;
    }

    static void Save(string assetName, int width, int height,
                     List<List<Vector2Int>> paths, LevelDifficulty.Result r)
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.gridWidth = width;
        level.gridHeight = height;
        level.arrows = new List<ArrowPathData>();

        foreach (List<Vector2Int> p in paths)
            level.arrows.Add(new ArrowPathData { path = p });

        level.difficulty = r.band;
        level.difficultyScore = r.score;
        level.generatorSeed = -1;          // not ours, never regenerable from a seed
        level.measuredClearableRatio = r.avgClearableRatio;
        level.measuredAvgRayLength = r.avgRayLength;
        level.measuredWaves = r.waves;
        level.measuredAvgBends = r.avgBends;
        level.measuredMinClearable = r.minClearable;
        level.measuredDecoyRatio = r.decoyRatio;
        level.measuredHardestVerify = r.hardestVerify;

        string filePath = DestFolder + "/" + assetName + ".asset";
        LevelData existing = AssetDatabase.LoadAssetAtPath<LevelData>(filePath);

        if (existing != null)
        {
            EditorUtility.CopySerialized(level, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(level);
        }
        else
        {
            AssetDatabase.CreateAsset(level, filePath);
        }
    }

    static void AppendOurLevels(StringBuilder log)
    {
        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/Project/Levels" });
        List<string> paths = new List<string>();

        foreach (string guid in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.StartsWith(DestFolder)) continue;     // skip the imported ones
            paths.Add(p);
        }
        paths.Sort();

        foreach (string p in paths)
        {
            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(p);
            if (level == null || level.arrows == null || level.arrows.Count == 0) continue;

            List<List<Vector2Int>> arrowPaths = new List<List<Vector2Int>>();
            foreach (ArrowPathData a in level.arrows) arrowPaths.Add(a.path);

            LevelDifficulty.Result r = LevelDifficulty.Measure(arrowPaths, level.gridWidth, level.gridHeight);
            log.AppendLine(Row(level.name, level.gridWidth, level.gridHeight, arrowPaths, r));
        }
    }

    // The three shape columns are the ones that matter for "does it look like theirs": how often a
    // body turns, and how long its straight runs get. Their shipped average is 0.30 and ~5.0.
    static string Row(string name, int width, int height,
                      List<List<Vector2Int>> paths, LevelDifficulty.Result r)
    {
        int cells = 0, segments = 0, bends = 0, runTotal = 0;

        foreach (List<Vector2Int> p in paths)
        {
            cells += p.Count;
            segments += Mathf.Max(1, p.Count - 1);

            int best = 1, run = 1;
            for (int i = 2; i < p.Count; i++)
            {
                if (p[i] - p[i - 1] == p[i - 1] - p[i - 2]) run++;
                else { bends++; run = 1; }
                if (run > best) best = run;
            }
            runTotal += best + 1;
        }

        float avgLen = cells / (float)paths.Count;
        float bendsPerSegment = bends / (float)segments;
        float avgRun = runTotal / (float)paths.Count;

        return string.Format(
            "{0,-10} | {1,3}x{2,-3} | {3,5} | {4,6} | {5,6:F1} | {6,9:F2} | {7,7:F1} | {8,5:F1} | {9,4:F1} | {10,5:P0} | {11,5} | {12,5} | {13}",
            name, width, height, cells, paths.Count, avgLen, bendsPerSegment, avgRun,
            r.avgBends, r.avgRayLength, r.avgClearableRatio, r.waves, r.score, r.band);
    }
}
