using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public float spacing = 1f;

    public Dictionary<Vector2Int, GridCell> cells = new Dictionary<Vector2Int, GridCell>();

    void Start()
    {
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                Vector2Int gridPos = new Vector2Int(col, row);
                cells[gridPos] = new GridCell(gridPos);
            }
        }

        Debug.Log("Grid banyo! Total cells: " + cells.Count);
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var kvp in cells)
        {
            Vector3 worldPos = new Vector3(kvp.Key.x * spacing, kvp.Key.y * spacing, 0f);
            Gizmos.DrawWireSphere(worldPos, 0.1f);
        }
    }

    public GridCell GetCell(Vector2Int pos)
    {
        if (cells.ContainsKey(pos))
            return cells[pos];
        return null;
    }

    public bool IsCellOccupied(Vector2Int pos)
    {
        GridCell cell = GetCell(pos);
        if (cell == null) return false; // grid ni bahar nu coordinate = occupied nahi gaanay
        return cell.Occupant != null;
    }

    public void SetOccupant(Vector2Int pos, GameObject occupant)
    {
        GridCell cell = GetCell(pos);
        if (cell != null)
            cell.Occupant = occupant;
    }

    public void ClearOccupant(Vector2Int pos)
    {
        GridCell cell = GetCell(pos);
        if (cell != null)
            cell.Occupant = null;
    }

}
