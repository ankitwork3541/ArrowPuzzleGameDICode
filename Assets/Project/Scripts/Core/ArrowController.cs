using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArrowController : MonoBehaviour
{
    public GridBuilder grid;
    public GameObject segmentPrefab;
    public List<Vector2Int> path = new List<Vector2Int>();
    public Sprite straightSprite;
    public Sprite headSprite;
    public Sprite capSprite;
    public Sprite cornerSprite;



    private List<GameObject> segmentVisuals = new List<GameObject>();

    void Start()
    {
        SpawnSegments();
        ClaimCells();
    }

    float AngleForDirection(Vector2Int dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
    }

    float AngleForCorner(Vector2Int toPrev, Vector2Int toNext)
    {
        if ((toPrev == new Vector2Int(0, -1) && toNext == new Vector2Int(1, 0)) ||
            (toPrev == new Vector2Int(1, 0) && toNext == new Vector2Int(0, -1)))
            return 0f;

        if ((toPrev == new Vector2Int(1, 0) && toNext == new Vector2Int(0, 1)) ||
            (toPrev == new Vector2Int(0, 1) && toNext == new Vector2Int(1, 0)))
            return 90f;

        if ((toPrev == new Vector2Int(0, 1) && toNext == new Vector2Int(-1, 0)) ||
            (toPrev == new Vector2Int(-1, 0) && toNext == new Vector2Int(0, 1)))
            return 180f;

        return 270f;
    }



    void SpawnSegments()
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = grid.GridToWorld(path[i]);
            GameObject segment = Instantiate(segmentPrefab, worldPos, Quaternion.identity, transform);
            SpriteRenderer sr = segment.GetComponent<SpriteRenderer>();
            sr.color = Color.white;

            ApplySegmentSprite(i, sr, segment.transform);

            if (i == path.Count - 1)
            {
                GameObject shaft = Instantiate(segmentPrefab);
                shaft.transform.SetParent(segment.transform);
                shaft.transform.localPosition = new Vector3(0f, -0.22f, 0f);
                shaft.transform.localRotation = Quaternion.identity;
                shaft.transform.localScale = new Vector3(1f, 0.56f, 1f);

                SpriteRenderer shaftSr = shaft.GetComponent<SpriteRenderer>();
                shaftSr.sprite = straightSprite;
                shaftSr.color = Color.white;
                shaftSr.sortingOrder = 1;

                Destroy(shaft.GetComponent<ArrowSegmentClick>());
            }

            ArrowSegmentClick clickScript = segment.GetComponent<ArrowSegmentClick>();
            clickScript.owner = this;

            segmentVisuals.Add(segment);
        }
    }

    void ApplySegmentSprite(int i, SpriteRenderer sr, Transform segmentTransform)
    {
        if (i == 0)
        {
            Vector2Int outDir = path[i + 1] - path[i];
            sr.sprite = capSprite;
            segmentTransform.rotation = Quaternion.Euler(0, 0, AngleForDirection(outDir));
        }
        else if (i == path.Count - 1)
        {
            Vector2Int inDir = path[i] - path[i - 1];
            sr.sprite = headSprite;
            sr.sortingOrder = 2;
            segmentTransform.rotation = Quaternion.Euler(0, 0, AngleForDirection(inDir));
        }
        else
        {
            Vector2Int inDir = path[i] - path[i - 1];
            Vector2Int outDir = path[i + 1] - path[i];

            if (inDir == outDir)
            {
                sr.sprite = straightSprite;
                segmentTransform.rotation = Quaternion.Euler(0, 0, AngleForDirection(inDir));
            }
            else
            {
                sr.sprite = cornerSprite;
                segmentTransform.rotation = Quaternion.Euler(0, 0, AngleForCorner(-inDir, outDir));
            }
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

            SpriteRenderer sr = segmentVisuals[i].GetComponent<SpriteRenderer>();
            ApplySegmentSprite(i, sr, segmentVisuals[i].transform);
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
