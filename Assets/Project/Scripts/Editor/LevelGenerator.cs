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

    static List<Vector2Int> GenerateWalk(Vector2Int start, HashSet<Vector2Int> occupied, int width, int height, int length)
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
                if (IsInBounds(next, width, height) && !occupied.Contains(next) && !walk.Contains(next))
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

    static void SaveLevel(int width, int height, List<List<Vector2Int>> arrowPaths)
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

        string filePath = AssetDatabase.GenerateUniqueAssetPath("Assets/Project/Levels/GeneratedLevel.asset");
        AssetDatabase.CreateAsset(level, filePath);
        AssetDatabase.SaveAssets();

        Debug.Log("Level generated: " + level.arrows.Count + " arrows, saved to " + filePath);
    }
}
