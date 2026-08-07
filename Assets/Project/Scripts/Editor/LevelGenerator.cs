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

    // Aakhi shape ne 100% bharva mate: random-mukine-check-karvani jagyaae, SOLVE ORDER ma
    // "carve" karvu. Sharuaat ma aakhi shape bharel gano, pachi ek pachi ek arrow kaadho —
    // ek arrow tya j kaadhi shakay jyare eno exit-ray FAKT already-kaadhel (khali) cells
    // mathi pasar thay. Aa rite bahar-thi-andar peel thay che.
    //
    // Arrow potani j body thi block kem nathi thato: body na badha cells `remaining` ma j che,
    // ane ray ne khali NON-remaining cells mathi j pasar thavani chhut che — etle self-block
    // structurally shakya j nathi, alag check ni jarur nathi.
    //
    // Carve order = solve order, etle level construction thi j solvable che. Ane apde shape ne
    // PARTITION kari rahya chie (random rite bharta nathi), etle coverage 100% by definition.
    static bool RayIsClear(Vector2Int head, Vector2Int dir, HashSet<Vector2Int> remaining, int width, int height)
    {
        Vector2Int cursor = head + dir;
        while (IsInBounds(cursor, width, height))
        {
            if (remaining.Contains(cursor)) return false;
            cursor += dir;
        }
        return true;
    }

    // n na ketla padoshi `remaining` ma che, aa path nikadi lidha pachi
    static int DegreeAfter(Vector2Int n, HashSet<Vector2Int> remaining, List<Vector2Int> path)
    {
        int count = 0;
        foreach (Vector2Int d in directions)
        {
            Vector2Int nb = n + d;
            if (remaining.Contains(nb) && !path.Contains(nb)) count++;
        }
        return count;
    }

    static List<List<Vector2Int>> Carve(HashSet<Vector2Int> mask, int width, int height, int minLength, int maxLength, out int strandedCount)
    {
        HashSet<Vector2Int> remaining = new HashSet<Vector2Int>(mask);
        List<List<Vector2Int>> arrows = new List<List<Vector2Int>>();

        List<Vector2Int> headCandidates = new List<Vector2Int>();
        List<Vector2Int> dirCandidates = new List<Vector2Int>();
        List<Vector2Int> options = new List<Vector2Int>();

        while (remaining.Count > 0)
        {
            // Atyare kaya (head, dir) legally exit kari shake? Path ne >= 2 cells joiye,
            // etle head ni pachal ni cell (head - dir) pan `remaining` ma hovi joiye.
            headCandidates.Clear();
            dirCandidates.Clear();
            int minDegree = int.MaxValue;

            foreach (Vector2Int c in remaining)
            {
                foreach (Vector2Int d in directions)
                {
                    if (!remaining.Contains(c - d)) continue;
                    if (!RayIsClear(c, d, remaining, width, height)) continue;

                    int degree = 0;
                    foreach (Vector2Int nd in directions)
                        if (remaining.Contains(c + nd)) degree++;

                    // Ochha-padoshi wala heads pehla — jethi danglers isolated thay e pehla
                    // j vaparai jay (isolated single cell mathi >=2 wali arrow banti j nathi).
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

            if (headCandidates.Count == 0) break; // atki gayu — bija attempt ma retry thashe

            int pick = Random.Range(0, headCandidates.Count);
            Vector2Int head = headCandidates[pick];
            Vector2Int dir = dirCandidates[pick];

            // Body ne head thi pachal tarf ugado, `remaining` ni andar j rahi ne.
            List<Vector2Int> path = new List<Vector2Int> { head, head - dir };
            int targetLength = Random.Range(minLength, maxLength + 1);

            while (true)
            {
                Vector2Int tail = path[path.Count - 1];

                options.Clear();
                foreach (Vector2Int d in directions)
                {
                    Vector2Int n = tail + d;
                    if (remaining.Contains(n) && !path.Contains(n)) options.Add(n);
                }
                if (options.Count == 0) break;

                // "must" = evi padoshi cell je aa path nikdya pachi ISOLATED thai jashe.
                // Ene atyar j absorb kari levi, nahi to e kayame stranded rahi jashe.
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
                }
                else
                {
                    int lowest = int.MaxValue;
                    foreach (Vector2Int n in options)
                        lowest = Mathf.Min(lowest, DegreeAfter(n, remaining, path));

                    List<Vector2Int> best = new List<Vector2Int>();
                    foreach (Vector2Int n in options)
                        if (DegreeAfter(n, remaining, path) == lowest) best.Add(n);

                    path.Add(best[Random.Range(0, best.Count)]);
                }
            }

            path.Reverse(); // have [tail ... head]
            foreach (Vector2Int p in path) remaining.Remove(p);
            arrows.Add(path);
        }

        // Repair: koi single cell bachi hoy to ene padoshi arrow na TAIL ma jodi do.
        // Safe kem: head ni direction badlati nathi (etle eno ray e j rahe), ane aa cell kayre
        // carve j nathi thai — matlab hamesha `remaining` ma hati — etle koi pan arrow no ray
        // ene touch j nathi karto (ray khali non-remaining cells mathi pasar thay che).
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

        strandedCount = remaining.Count;
        return arrows;
    }

    // Carve ne badhi seeds sudhi try karo jya sudhi 100% coverage na male.
    static List<List<Vector2Int>> CarveFull(HashSet<Vector2Int> mask, int width, int height, int minLength, int maxLength, int attempts)
    {
        List<List<Vector2Int>> best = null;
        int bestStranded = int.MaxValue;

        for (int i = 0; i < attempts; i++)
        {
            int stranded;
            List<List<Vector2Int>> arrows = Carve(mask, width, height, minLength, maxLength, out stranded);

            if (stranded < bestStranded)
            {
                bestStranded = stranded;
                best = arrows;
            }
            if (stranded == 0) break;
        }

        if (bestStranded > 0)
            Debug.LogWarning("100% fill na thayu — " + bestStranded + " cells stranded rahi gayi.");

        return best;
    }

    [MenuItem("ArrowPuzzle/Generate Heart Level")]
    static void GenerateHeartLevel()
    {
        int gridWidth = 24;
        int gridHeight = 22;

        HashSet<Vector2Int> mask = BuildHeartMask(gridWidth, gridHeight);
        List<List<Vector2Int>> arrows = CarveFull(mask, gridWidth, gridHeight, 2, 7, 8);

        int cells = 0;
        foreach (List<Vector2Int> path in arrows) cells += path.Count;
        Debug.Log("Heart level: " + arrows.Count + " arrows, " + cells + "/" + mask.Count + " cells filled ("
            + Mathf.RoundToInt(100f * cells / mask.Count) + "%)");

        SaveLevel(gridWidth, gridHeight, arrows, "HeartLevel");
    }

    [MenuItem("ArrowPuzzle/Generate Huge Level")]
    static void GenerateHugeLevel()
    {
        // Aa level jaanine screen karta motu che — max zoom-out par pan pooru na dekhay,
        // etle player e 4 baju pan/scroll karine j solve kari shake.
        int gridWidth = 30;
        int gridHeight = 50;

        HashSet<Vector2Int> mask = new HashSet<Vector2Int>();
        for (int row = 0; row < gridHeight; row++)
            for (int col = 0; col < gridWidth; col++)
                mask.Add(new Vector2Int(col, row));

        List<List<Vector2Int>> arrows = CarveFull(mask, gridWidth, gridHeight, 2, 7, 5);

        int cells = 0;
        foreach (List<Vector2Int> path in arrows) cells += path.Count;
        Debug.Log("Huge level: " + arrows.Count + " arrows, " + cells + "/" + mask.Count + " cells filled ("
            + Mathf.RoundToInt(100f * cells / mask.Count) + "%)");

        SaveLevel(gridWidth, gridHeight, arrows, "HugeLevel");
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
