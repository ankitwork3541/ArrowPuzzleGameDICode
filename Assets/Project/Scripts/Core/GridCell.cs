using UnityEngine;

public class GridCell
{
    public Vector2Int Position;
    public GameObject Occupant; // Have empty rakhishu, pachi ArrowController thi replace karshu

    public GridCell(Vector2Int position)
    {
        Position = position;
        Occupant = null;
    }
}
