using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArrowController : MonoBehaviour
{
    public GridBuilder grid;
    public GameObject segmentPrefab;
    public List<Vector2Int> path = new List<Vector2Int>();

    private List<GameObject> segmentVisuals = new List<GameObject>();

    void Start()
    {
        SpawnSegments();
        ClaimCells();
    }

    void SpawnSegments()
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = grid.GridToWorld(path[i]);
            GameObject segment = Instantiate(segmentPrefab, worldPos, Quaternion.identity, transform);

            ArrowSegmentClick clickScript = segment.GetComponent<ArrowSegmentClick>();
            clickScript.owner = this;

            segmentVisuals.Add(segment);
        }
    }

    void ClaimCells()
    {
        foreach (Vector2Int coord in path)
        {
            GridCell cell = grid.GetCell(coord);
            if (cell != null)
                cell.Occupant = this;
        }
    }

    public void TryAdvance()
    {
        if (path.Count < 2) return;

        Vector2Int head = path[path.Count - 1];
        Vector2Int prev = path[path.Count - 2];
        Vector2Int direction = head - prev;
        Vector2Int nextCoord = head + direction;

        GridCell nextCell = grid.GetCell(nextCoord);

        if (nextCell == null)
        {
            Exit(direction);
            return;
        }

        if (nextCell.Occupant != null && nextCell.Occupant != this)
        {
            FlashBlocked();
            return;
        }

        AdvanceStep(nextCoord);
    }

    void AdvanceStep(Vector2Int nextCoord)
    {
        GridCell tailCell = grid.GetCell(path[0]);
        if (tailCell != null && tailCell.Occupant == this)
            tailCell.Occupant = null;

        path.RemoveAt(0);
        path.Add(nextCoord);

        GridCell headCell = grid.GetCell(nextCoord);
        if (headCell != null)
            headCell.Occupant = this;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 targetPos = grid.GridToWorld(path[i]);
            segmentVisuals[i].transform.DOMove(targetPos, 0.2f);
        }
    }

    void FlashBlocked()
    {
        foreach (GameObject segment in segmentVisuals)
        {
            SpriteRenderer sr = segment.GetComponent<SpriteRenderer>();
            Color original = sr.color;
            sr.DOColor(Color.red, 0.1f).OnComplete(() => sr.DOColor(original, 0.1f));
        }
    }

    void Exit(Vector2Int direction)
    {
        foreach (Vector2Int coord in path)
        {
            GridCell cell = grid.GetCell(coord);
            if (cell != null && cell.Occupant == this)
                cell.Occupant = null;
        }

        Vector3 exitOffset = new Vector3(direction.x, direction.y, 0f) * grid.spacing * 5f;

        foreach (GameObject segment in segmentVisuals)
        {
            Vector3 target = segment.transform.position + exitOffset;
            segment.transform.DOMove(target, 0.5f);
        }

        Destroy(gameObject, 0.5f);
    }

}
