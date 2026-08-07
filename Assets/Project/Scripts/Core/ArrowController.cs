using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class ArrowController : MonoBehaviour
{
    public List<Vector2Int> path = new List<Vector2Int>();
    public GridBuilder gridBuilder; // Inspector ma GridBuilder object drag karo
    public Transform headVisual; // Inspector ma "Head" child object drag karo
    public Transform tailVisual; // Inspector ma "Tail" child object drag karo

    public float spacing = 1f;
    private bool isMoving = false;
    public float stepTime = 0.18f;

    private LineRenderer line;
    private EdgeCollider2D edgeCollider;
    private SpriteRenderer headSprite;
    private SpriteRenderer tailSprite;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();
        if (headVisual != null)
            headSprite = headVisual.GetComponent<SpriteRenderer>();
        if (tailVisual != null)
            tailSprite = tailVisual.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        DrawPath();
        ClaimOccupancy();
    }

    void ClaimOccupancy()
    {
        foreach (Vector2Int pos in path)
        {
            gridBuilder.SetOccupant(pos, gameObject);
        }
    }


    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (edgeCollider.OverlapPoint(worldPos))
            {
                if (edgeCollider.OverlapPoint(worldPos))
                {
                    TryAdvance();
                }

            }
        }
    }

    void DrawPath()
    {
        line.positionCount = path.Count;
        Vector2[] colliderPoints = new Vector2[path.Count];

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = new Vector3(path[i].x * spacing, path[i].y * spacing, 0f);
            line.SetPosition(i, worldPos);
            colliderPoints[i] = worldPos;
        }

        edgeCollider.points = colliderPoints;
        edgeCollider.edgeRadius = 0.09f; // line jetli jaadi che, e pramane

        UpdateHeadPosition();
        UpdateTailPosition();
    }

    void UpdateHeadPosition()
    {
        if (headVisual == null) return;

        Vector3 headPos = line.GetPosition(line.positionCount - 1);
        headVisual.position = headPos;
        UpdateHeadRotation();
    }

    void UpdateTailPosition()
    {
        if (tailVisual == null) return;
        tailVisual.position = line.GetPosition(0);
    }

    void UpdateHeadRotation()
    {
        if (headVisual == null || path.Count < 2) return;

        int n = path.Count;
        Vector2Int dir = path[n - 1] - path[n - 2];
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        headVisual.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    void TryAdvance()
    {
        if (isMoving) return; // already moving, extra click ignore

        int n = path.Count;
        if (n < 2) return;

        Vector2Int head = path[n - 1];
        Vector2Int prev = path[n - 2];
        Vector2Int dir = head - prev;
        Vector2Int nextPos = head + dir;

        GridCell nextCell = gridBuilder.GetCell(nextPos);

        if (nextCell == null)
        {
            Vector3 exitStep = new Vector3(dir.x, dir.y, 0f) * spacing;
            StartExit(exitStep);
        }
        else if (gridBuilder.IsCellOccupied(nextPos))
        {
            BlockedBounce();
        }
        else
        {
            MoveStep(nextPos);
        }
    }
    void MoveStep(Vector2Int nextPos)
    {
        int n = path.Count;

        Vector3[] oldWorld = new Vector3[n];
        line.GetPositions(oldWorld);

        gridBuilder.ClearOccupant(path[0]);
        for (int i = 0; i < n - 1; i++) path[i] = path[i + 1];
        path[n - 1] = nextPos;
        gridBuilder.SetOccupant(nextPos, gameObject);

        Vector3[] newWorld = new Vector3[n];
        for (int i = 0; i < n; i++)
            newWorld[i] = new Vector3(path[i].x * spacing, path[i].y * spacing, 0f);

        isMoving = true;
        float t = 0f;
        DOTween.To(() => t, v => t = v, 1f, stepTime)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                for (int i = 0; i < n; i++)
                    line.SetPosition(i, Vector3.Lerp(oldWorld[i], newWorld[i], t));
                UpdateHeadPosition();
                UpdateTailPosition();
            })
            .OnComplete(() =>
            {
                Vector2[] colliderPoints = new Vector2[n];
                for (int i = 0; i < n; i++)
                    colliderPoints[i] = newWorld[i];
                edgeCollider.points = colliderPoints;

                isMoving = false;
                TryAdvance();
            });
    }
    void BlockedBounce()
    {
        LivesManager.Instance.LoseLife();

        int n = path.Count;
        Vector3[] originalWorld = new Vector3[n];
        line.GetPositions(originalWorld);

        Vector2Int head = path[n - 1];
        Vector2Int prev = path[n - 2];
        Vector2Int dir = head - prev;
        Vector3 bumpOffset = new Vector3(dir.x, dir.y, 0f) * spacing * 0.3f;

        Vector3[] bumpWorld = new Vector3[n];
        for (int i = 0; i < n; i++)
            bumpWorld[i] = originalWorld[i] + bumpOffset;

        SetLineColor(Color.red);

        isMoving = true;
        float t = 0f;

        DOTween.To(() => t, v => t = v, 1f, stepTime * 0.5f)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                for (int i = 0; i < n; i++)
                    line.SetPosition(i, Vector3.Lerp(originalWorld[i], bumpWorld[i], t));
                UpdateHeadPosition();
                UpdateTailPosition();
            })
            .OnComplete(() =>
            {
                float t2 = 0f;
                DOTween.To(() => t2, v => t2 = v, 1f, stepTime * 0.5f)
                    .SetEase(Ease.Linear)
                    .OnUpdate(() =>
                    {
                        for (int i = 0; i < n; i++)
                            line.SetPosition(i, Vector3.Lerp(bumpWorld[i], originalWorld[i], t2));
                        UpdateHeadPosition();
                        UpdateTailPosition();
                    })
                    .OnComplete(() =>
                    {
                        SetLineColor(Color.white);
                        isMoving = false;
                    });
            });
    }

    void StartExit(Vector3 exitStep)
    {
        foreach (Vector2Int pos in path)
            gridBuilder.ClearOccupant(pos);

        isMoving = true;
        ExitStepOnce(exitStep);
    }

    void ExitStepOnce(Vector3 exitStep)
    {
        int n = line.positionCount;
        Vector3[] oldWorld = new Vector3[n];
        line.GetPositions(oldWorld);

        Vector3[] newWorld = new Vector3[n];
        for (int i = 0; i < n - 1; i++) newWorld[i] = oldWorld[i + 1];
        newWorld[n - 1] = oldWorld[n - 1] + exitStep;

        float t = 0f;
        DOTween.To(() => t, v => t = v, 1f, stepTime)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                for (int i = 0; i < n; i++)
                    line.SetPosition(i, Vector3.Lerp(oldWorld[i], newWorld[i], t));
                UpdateHeadPosition();
                UpdateTailPosition();
            })
            .OnComplete(() =>
            {
                if (IsFullyOffScreen())
                    Destroy(gameObject);
                else
                    ExitStepOnce(exitStep);
            });
    }

    bool IsFullyOffScreen()
    {
        int n = line.positionCount;
        for (int i = 0; i < n; i++)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(line.GetPosition(i));
            if (viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f)
                return false;
        }
        return true;
    }

    void SetLineColor(Color c)
    {
        if (line.material.HasProperty("_Color"))
            line.material.color = c;
        else
        {
            line.startColor = c;
            line.endColor = c;
        }

        if (headSprite != null)
            headSprite.color = c;
        if (tailSprite != null)
            tailSprite.color = c;
    }

}
