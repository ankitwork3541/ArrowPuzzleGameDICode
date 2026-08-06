using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    public GridCell cellPrefab;
    public int width = 8;
    public int height = 8;
    public float spacing = 1f;
    public Vector2 origin = Vector2.zero;

    private Dictionary<Vector2Int, GridCell> cells = new Dictionary<Vector2Int, GridCell>();

    void Start()
    {
        BuildGrid();
    }

    void BuildGrid()
    {
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                Vector2Int coord = new Vector2Int(col, row);
                Vector3 worldPos = GridToWorld(coord);

                GridCell cell = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
                cell.Coordinate = coord;
                cell.name = "Cell_" + col + "_" + row;

                cells[coord] = cell;
            }
        }
    }

    public Vector3 GridToWorld(Vector2Int coord)
    {
        float worldX = origin.x + coord.x * spacing;
        float worldY = origin.y + coord.y * spacing;
        return new Vector3(worldX, worldY, 0f);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int gridX = Mathf.RoundToInt((worldPos.x - origin.x) / spacing);
        int gridY = Mathf.RoundToInt((worldPos.y - origin.y) / spacing);
        return new Vector2Int(gridX, gridY);
    }

    public GridCell GetCell(Vector2Int coord)
    {
        if (cells.TryGetValue(coord, out GridCell cell))
            return cell;
        return null;
    }
}
