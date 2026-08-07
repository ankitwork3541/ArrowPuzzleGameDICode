using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelGenerator
{
    static Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),  // up
        new Vector2Int(0, -1), // down
        new Vector2Int(1, 0),  // right
        new Vector2Int(-1, 0)  // left
    };

    static bool IsInBounds(Vector2Int pos, int width, int height)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    // allowedCells = null hoy to aakhu grid vaparay; shape-levels (jem heart) mate
    // khali e shape ni andar na cells j pass thay.
    static List<Vector2Int> GenerateWalk(Vector2Int start, HashSet<Vector2Int> occupied, int width, int height, int length, HashSet<Vector2Int> allowedCells = null)
    {
        List<Vector2Int> walk = new List<Vector2Int>();
        walk.Add(start);
        Vector2Int current = start;

        for (int i = 1; i < length; i++)
        {
            List<Vector2Int> options = new List<Vector2Int>();

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (!IsInBounds(next, width, height)) continue;
                if (allowedCells != null && !allowedCells.Contains(next)) continue;
                if (occupied.Contains(next) || walk.Contains(next)) continue;
                options.Add(next);
            }

            if (options.Count == 0) break; // atki gayu, jetlu banyu etlu j rakho

            Vector2Int chosen = options[Random.Range(0, options.Count)];
            walk.Add(chosen);
            current = chosen;
        }

        return walk;
    }

    static bool IsExitClear(Vector2Int head, Vector2Int dir, HashSet<Vector2Int> occupied, List<Vector2Int> ownBody, int width, int height)
    {
        Vector2Int cursor = head + dir;

        while (IsInBounds(cursor, width, height))
        {
            if (occupied.Contains(cursor) || ownBody.Contains(cursor))
                return false; // koi biji arrow, YA POTANI J BODY (hook/loop shape), vachche aavi gai

            cursor += dir;
        }

        return true; // grid ni bahar sudhi koi avrodh na malyo
    }

    [MenuItem("ArrowPuzzle/Generate Level")]
    static void GenerateLevel()
    {
        int gridWidth = 10;
        int gridHeight = 10;
        int arrowCount = 15;
        int minLength = 2;
        int maxLength = 8;

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        List<List<Vector2Int>> arrowPaths = new List<List<Vector2Int>>();

        for (int i = 0; i < arrowCount; i++)
        {
            List<Vector2Int> path = null;
            int attempts = 0;

            while (attempts < 100)
            {
                attempts++;

                Vector2Int start = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
                if (occupied.Contains(start)) continue;

                int length = Random.Range(minLength, maxLength + 1);
                List<Vector2Int> walk = GenerateWalk(start, occupied, gridWidth, gridHeight, length);

                if (walk.Count < 2) continue;

                Vector2Int head = walk[walk.Count - 1];
                Vector2Int prev = walk[walk.Count - 2];
                Vector2Int dir = head - prev;

                if (!IsExitClear(head, dir, occupied, walk, gridWidth, gridHeight)) continue;

                path = walk;
                break;
            }

            if (path == null)
            {
                Debug.LogWarning("Arrow " + i + " mate valid path na mali — skip kari.");
                continue;
            }

            foreach (Vector2Int pos in path)
                occupied.Add(pos);

            arrowPaths.Add(path);
        }

        SaveLevel(gridWidth, gridHeight, arrowPaths);
    }

    // ---------- SHAPE LEVELS (heart vagere) ----------

    // Classic heart curve: (x^2 + y^2 - 1)^3 - x^2*y^3 <= 0 ni andar na points = heart ni andar.
    static HashSet<Vector2Int> BuildHeartMask(int width, int height)
    {
        HashSet<Vector2Int> mask = new HashSet<Vector2Int>();

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                float x = ((col - (width - 1) / 2f) / ((width - 1) / 2f)) * 1.20f;
                float y = ((row - (height - 1) / 2f) / ((height - 1) / 2f)) * 1.175f + 0.175f;

                float t = x * x + y * y - 1f;
                if (t * t * t - x * x * y * y * y <= 0f)
                    mask.Add(new Vector2Int(col, row));
            }
        }

        return mask;
    }

    // Box-Muller thi gaussian random (Unity ma built-in nathi), absolute value.
    static float AbsGaussian(float sigma)
    {
        float u1 = 1f - Random.value;
        float u2 = 1f - Random.value;
        float g = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
        return Mathf.Abs(g * sigma);
    }

    static List<List<Vector2Int>> FillShape(HashSet<Vector2Int> mask, int width, int height, int minLength, int maxLength, int maxFail, float centerBias)
    {
        // Mask cells ne ek j vaar center-thi-distance pramane sort kari lo (dar iteration ma
        // sort karvani jarur na pade — khali occupied skip karvana).
        List<Vector2Int> cellsByDistance = new List<Vector2Int>(mask);
        float cx = 0f, cy = 0f;
        foreach (Vector2Int c in cellsByDistance) { cx += c.x; cy += c.y; }
        cx /= cellsByDistance.Count;
        cy /= cellsByDistance.Count;
        cellsByDistance.Sort((a, b) =>
        {
            float da = (a.x - cx) * (a.x - cx) + (a.y - cy) * (a.y - cy);
            float db = (b.x - cx) * (b.x - cx) + (b.y - cy) * (b.y - cy);
            return da.CompareTo(db);
        });

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        List<List<Vector2Int>> arrows = new List<List<Vector2Int>>();
        List<Vector2Int> free = new List<Vector2Int>();

        // Multi-phase: pehla lamba walks (interesting shapes), pachi dhire dhire nana —
        // jethi bachela nana gaps pan bharai jay.
        for (int phaseMax = maxLength; phaseMax >= minLength; phaseMax--)
        {
            int fails = 0;
            while (fails < maxFail)
            {
                free.Clear();
                foreach (Vector2Int c in cellsByDistance)
                    if (!occupied.Contains(c)) free.Add(c);
                if (free.Count == 0) break;

                // CENTER-THI-BAHAR: pehla mukel arrow CHHELLE solve thay (board bharelu hoy
                // tyare) — etle e center ma hovu joiye. Chhelle mukel arrow PEHLA solve thay,
                // etle e bahar hovu joiye jya thi eno exit-ray turat board ni bahar nikli jay.
                int index = Mathf.Min((int)(AbsGaussian(centerBias) * free.Count), free.Count - 1);
                Vector2Int start = free[index];

                int length = Random.Range(minLength, phaseMax + 1);
                List<Vector2Int> walk = GenerateWalk(start, occupied, width, height, length, mask);

                if (walk.Count < 2) { fails++; continue; }

                // Ek j walk ne be rite vaparai shakay — koi pan chheda ne head banavo.
                // Banne try karo, clear-exit malvani shakyata bamani thai jay.
                bool placed = false;
                for (int variant = 0; variant < 2 && !placed; variant++)
                {
                    if (variant == 1) walk.Reverse();

                    Vector2Int head = walk[walk.Count - 1];
                    Vector2Int prev = walk[walk.Count - 2];
                    Vector2Int dir = head - prev;

                    if (IsExitClear(head, dir, occupied, walk, width, height))
                    {
                        foreach (Vector2Int pos in walk) occupied.Add(pos);
                        arrows.Add(new List<Vector2Int>(walk));
                        placed = true;
                    }
                }

                fails = placed ? 0 : fails + 1;
            }
        }

        return arrows;
    }

    [MenuItem("ArrowPuzzle/Generate Heart Level")]
    static void GenerateHeartLevel()
    {
        int gridWidth = 24;
        int gridHeight = 22;
        int minLength = 2;
        int maxLength = 7;
        int maxFail = 500;
        float centerBias = 0.33f; // prototype ma sweep karine malelo best value
        int attempts = 10;        // best-of-N: sauthi dense result rakhvo

        HashSet<Vector2Int> mask = BuildHeartMask(gridWidth, gridHeight);

        List<List<Vector2Int>> best = null;
        int bestCells = -1;

        for (int i = 0; i < attempts; i++)
        {
            List<List<Vector2Int>> arrows = FillShape(mask, gridWidth, gridHeight, minLength, maxLength, maxFail, centerBias);

            int cells = 0;
            foreach (List<Vector2Int> path in arrows) cells += path.Count;

            if (cells > bestCells)
            {
                bestCells = cells;
                best = arrows;
            }
        }

        Debug.Log("Heart level: " + best.Count + " arrows, " + bestCells + "/" + mask.Count
            + " mask cells filled (" + Mathf.RoundToInt(100f * bestCells / mask.Count) + "%)");

        SaveLevel(gridWidth, gridHeight, best, "HeartLevel");
    }

    static void SaveLevel(int width, int height, List<List<Vector2Int>> arrowPaths, string assetName = "GeneratedLevel")
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

        string filePath = AssetDatabase.GenerateUniqueAssetPath("Assets/Project/Levels/" + assetName + ".asset");
        AssetDatabase.CreateAsset(level, filePath);
        AssetDatabase.SaveAssets();

        Debug.Log("Level generated: " + level.arrows.Count + " arrows, saved to " + filePath);
    }
}
