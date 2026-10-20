using UnityEngine;

public class BoardCell
{
    public Vector2Int Position;
    public GameObject Occupant; // Have empty rakhishu, pachi DragonController thi replace karshu

    public BoardCell(Vector2Int position)
    {
        Position = position;
        Occupant = null;
    }
}
