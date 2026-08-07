using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelPainter : MonoBehaviour
{
    public GridBuilder gridBuilder;
    public LineRenderer previewLine;

    public int gridWidth = 10;
    public int gridHeight = 10;

    public LineRenderer committedLinePrefab; // template for a finished arrow's visual — drag a simple LineRenderer prefab

    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private bool isDrawing = false;
    private List<List<Vector2Int>> committedArrows = new List<List<Vector2Int>>();

    void Start()
    {
        gridBuilder.BuildGrid(gridWidth, gridHeight);
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2Int cell = WorldToGridCell();
            if (gridBuilder.GetCell(cell) == null) return; // grid ni bahar click, ignore
            if (gridBuilder.IsCellOccupied(cell)) return; // biji arrow no j cell che

            currentPath.Clear();
            currentPath.Add(cell);
            isDrawing = true;
            UpdatePreviewLine();
        }
        else if (Mouse.current.leftButton.isPressed && isDrawing)
        {
            Vector2Int cell = WorldToGridCell();
            TryExtendPath(cell);
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame && isDrawing)
        {
            isDrawing = false;
            CommitPath();
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            SaveLevel();
        }
    }

    Vector2Int WorldToGridCell()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return gridBuilder.WorldToGrid(worldPos);
    }

    void TryExtendPath(Vector2Int cell)
    {
        if (currentPath.Count == 0) return;
        Vector2Int last = currentPath[currentPath.Count - 1];

        if (cell == last) return; // same cell, ignore

        // Backtrack: dragged back onto the previous cell — undo the last step
        if (currentPath.Count >= 2 && cell == currentPath[currentPath.Count - 2])
        {
            currentPath.RemoveAt(currentPath.Count - 1);
            UpdatePreviewLine();
            return;
        }

        // Must be inside the grid, not already occupied by a committed arrow, exactly one grid step away, and not already used in this path
        if (gridBuilder.GetCell(cell) == null) return;
        if (gridBuilder.IsCellOccupied(cell)) return;

        Vector2Int diff = cell - last;
        bool isAdjacent = (Mathf.Abs(diff.x) + Mathf.Abs(diff.y)) == 1;
        if (!isAdjacent) return;
        if (currentPath.Contains(cell)) return;

        currentPath.Add(cell);
        UpdatePreviewLine();
    }

    void UpdatePreviewLine()
    {
        previewLine.positionCount = currentPath.Count;
        for (int i = 0; i < currentPath.Count; i++)
        {
            Vector3 worldPos = new Vector3(currentPath[i].x * gridBuilder.spacing, currentPath[i].y * gridBuilder.spacing, 0f);
            previewLine.SetPosition(i, worldPos);
        }
    }

    void CommitPath()
    {
        if (currentPath.Count < 2)
        {
            currentPath.Clear();
            UpdatePreviewLine();
            return;
        }

        List<Vector2Int> finishedPath = new List<Vector2Int>(currentPath);

        foreach (Vector2Int pos in finishedPath)
            gridBuilder.SetOccupant(pos, gameObject);

        committedArrows.Add(finishedPath);
        DrawCommittedLine(finishedPath);

        Debug.Log("Arrow committed with " + finishedPath.Count + " cells. Total arrows: " + committedArrows.Count);

        currentPath.Clear();
        UpdatePreviewLine();
    }

    void DrawCommittedLine(List<Vector2Int> path)
    {
        LineRenderer line = Instantiate(committedLinePrefab);
        line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = new Vector3(path[i].x * gridBuilder.spacing, path[i].y * gridBuilder.spacing, 0f);
            line.SetPosition(i, worldPos);
        }
    }

    bool IsLevelSolvable()
    {
        List<List<Vector2Int>> remaining = new List<List<Vector2Int>>(committedArrows);
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        foreach (List<Vector2Int> path in remaining)
            foreach (Vector2Int pos in path)
                occupied.Add(pos);

        bool removedAny = true;
        while (removedAny && remaining.Count > 0)
        {
            removedAny = false;

            for (int i = remaining.Count - 1; i >= 0; i--)
            {
                List<Vector2Int> path = remaining[i];
                Vector2Int head = path[path.Count - 1];
                Vector2Int prev = path[path.Count - 2];
                Vector2Int dir = head - prev;

                if (IsExitClearForCheck(head, dir, occupied))
                {
                    foreach (Vector2Int pos in path)
                        occupied.Remove(pos);
                    remaining.RemoveAt(i);
                    removedAny = true;
                }
            }
        }

        return remaining.Count == 0;
    }

    bool IsExitClearForCheck(Vector2Int head, Vector2Int dir, HashSet<Vector2Int> occupied)
    {
        Vector2Int cursor = head + dir;
        while (gridBuilder.GetCell(cursor) != null)
        {
            if (occupied.Contains(cursor)) return false;
            cursor += dir;
        }
        return true;
    }

    void SaveLevel()
    {
        if (!IsLevelSolvable())
        {
            Debug.LogWarning("Level SOLVABLE NATHI! Save nathi thayu — arrows/order badlo ane firi try karo.");
            return;
        }

#if UNITY_EDITOR
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.gridWidth = gridWidth;
        level.gridHeight = gridHeight;
        level.arrows = new List<ArrowPathData>();

        foreach (List<Vector2Int> path in committedArrows)
        {
            ArrowPathData arrowData = new ArrowPathData();
            arrowData.path = path;
            level.arrows.Add(arrowData);
        }

        string filePath = AssetDatabase.GenerateUniqueAssetPath("Assets/Project/Levels/HandmadeLevel.asset");
        AssetDatabase.CreateAsset(level, filePath);
        AssetDatabase.SaveAssets();

        Debug.Log("Level saved: " + level.arrows.Count + " arrows, saved to " + filePath);
#else
        Debug.LogWarning("Level saving only works inside the Unity Editor.");
#endif
    }
}
