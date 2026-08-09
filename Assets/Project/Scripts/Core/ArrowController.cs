using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class ArrowController : MonoBehaviour
{
    public List<Vector2Int> path = new List<Vector2Int>();
    public GridBuilder gridBuilder; // Inspector ma GridBuilder object drag karo
    public Transform headVisual; // Inspector ma "Head" child object drag karo
    public Transform tailVisual; // Inspector ma "Tail" child object drag karo

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

    public void Init(List<Vector2Int> newPath, GridBuilder builder)
    {
        path = newPath;
        gridBuilder = builder;
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
        // Input CameraController ek j jagyaae vanche che ane khali SACHO tap (drag ke pinch
        // nahi) j ahi sudhi aave che — etle level pan karta vakhte bhoolthi arrow move na thay.
        if (CameraController.Instance == null) return;

        Vector2 screenPos;
        if (!CameraController.Instance.TryGetTapThisFrame(out screenPos)) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        if (edgeCollider.OverlapPoint(worldPos))
            TryAdvance();
    }

    void DrawPath()
    {
        line.positionCount = path.Count;
        Vector2[] colliderPoints = new Vector2[path.Count];

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = gridBuilder.GridToWorld(path[i]);
            line.SetPosition(i, worldPos);
            colliderPoints[i] = worldPos;
        }

        edgeCollider.points = colliderPoints;
        edgeCollider.edgeRadius = 0.125f; // line ni half-width — tap area line jetlu j rahe

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
            Vector3 exitStep = new Vector3(dir.x, dir.y, 0f) * gridBuilder.spacing;
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

        // Capture old tail/head BEFORE shifting path (needed for the grow/shrink animation)
        Vector3 oldTailWorld = gridBuilder.GridToWorld(path[0]);
        Vector3 oldHeadWorld = gridBuilder.GridToWorld(path[n - 1]);
        Vector3 newHeadWorld = gridBuilder.GridToWorld(nextPos);

        gridBuilder.ClearOccupant(path[0]);
        for (int i = 0; i < n - 1; i++) path[i] = path[i + 1];
        path[n - 1] = nextPos;
        gridBuilder.SetOccupant(nextPos, gameObject);

        // Tail shrinks toward what is now path[0] (the old 2nd point) — always a straight line, no diagonal
        Vector3 newTailTarget = gridBuilder.GridToWorld(path[0]);

        // Line has n+1 points during the animation: [shrinking tail, frozen middle..., growing head]
        line.positionCount = n + 1;
        line.SetPosition(0, oldTailWorld);
        line.SetPosition(n, oldHeadWorld);
        for (int i = 0; i < n - 1; i++)
        {
            line.SetPosition(i + 1, gridBuilder.GridToWorld(path[i])); // middle points set once, never touched again — they stay frozen
        }

        isMoving = true;
        float t = 0f;
        DOTween.To(() => t, v => t = v, 1f, stepTime)
            .SetEase(DG.Tweening.Ease.Linear)
            .OnUpdate(() =>
            {
                line.SetPosition(0, Vector3.Lerp(oldTailWorld, newTailTarget, t));
                line.SetPosition(n, Vector3.Lerp(oldHeadWorld, newHeadWorld, t));
                UpdateHeadPosition();
                UpdateTailPosition();
            })
            .OnComplete(() =>
            {
                // Finalize: drop the now-redundant tail point (merged with what was point 2)
                Vector3[] finalPositions = new Vector3[n];
                for (int i = 0; i < n; i++)
                    finalPositions[i] = gridBuilder.GridToWorld(path[i]);

                line.positionCount = n;
                line.SetPositions(finalPositions);

                Vector2[] colliderPoints = new Vector2[n];
                for (int i = 0; i < n; i++)
                    colliderPoints[i] = finalPositions[i];
                edgeCollider.points = colliderPoints;

                isMoving = false;
                TryAdvance();
            });
    }
    
    void BlockedBounce()
    {
        LivesManager.Instance.LoseLife();

        int n = path.Count;
        Vector3 originalHeadWorld = line.GetPosition(n - 1);

        Vector2Int head = path[n - 1];
        Vector2Int prev = path[n - 2];
        Vector2Int dir = head - prev;
        Vector3 bumpHeadWorld = originalHeadWorld + new Vector3(dir.x, dir.y, 0f) * gridBuilder.spacing * 0.3f;

        SetLineColor(Color.red);

        isMoving = true;
        float t = 0f;

        // Khali head no point (n - 1) bump kare che — baki badha points (bends + tail) tadan freeze rahe
        DOTween.To(() => t, v => t = v, 1f, stepTime * 0.5f)
            .SetEase(DG.Tweening.Ease.Linear)
            .OnUpdate(() =>
            {
                line.SetPosition(n - 1, Vector3.Lerp(originalHeadWorld, bumpHeadWorld, t));
                UpdateHeadPosition();
            })
            .OnComplete(() =>
            {
                float t2 = 0f;
                DOTween.To(() => t2, v => t2 = v, 1f, stepTime * 0.5f)
                    .SetEase(DG.Tweening.Ease.Linear)
                    .OnUpdate(() =>
                    {
                        line.SetPosition(n - 1, Vector3.Lerp(bumpHeadWorld, originalHeadWorld, t2));
                        UpdateHeadPosition();
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

        Vector3 oldTailWorld = oldWorld[0];
        Vector3 newTailTarget = oldWorld[1];
        Vector3 oldHeadWorld = oldWorld[n - 1];
        Vector3 newHeadWorld = oldWorld[n - 1] + exitStep;

        // Same grow/shrink technique as MoveStep: freeze the middle, only animate the two ends
        line.positionCount = n + 1;
        line.SetPosition(0, oldTailWorld);
        line.SetPosition(n, oldHeadWorld);
        for (int i = 1; i < n; i++)
            line.SetPosition(i, oldWorld[i]); // frozen middle, unchanged from old positions

        float t = 0f;
        DOTween.To(() => t, v => t = v, 1f, stepTime)
            .SetEase(DG.Tweening.Ease.Linear)
            .OnUpdate(() =>
            {
                line.SetPosition(0, Vector3.Lerp(oldTailWorld, newTailTarget, t));
                line.SetPosition(n, Vector3.Lerp(oldHeadWorld, newHeadWorld, t));
                UpdateHeadPosition();
                UpdateTailPosition();
            })
            .OnComplete(() =>
            {
                Vector3[] finalPositions = new Vector3[n];
                for (int i = 0; i < n - 1; i++)
                    finalPositions[i] = oldWorld[i + 1];
                finalPositions[n - 1] = newHeadWorld;

                line.positionCount = n;
                line.SetPositions(finalPositions);

                if (IsFullyOffScreen())
                {
                    LevelManager.Instance.ArrowExited();
                    Destroy(gameObject);
                }
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

    public Bounds GetBounds()
    {
        Bounds bounds = line.bounds;
        if (headSprite != null) bounds.Encapsulate(headSprite.bounds);
        if (tailSprite != null) bounds.Encapsulate(tailSprite.bounds);
        return bounds;
    }

    // Hint mate: aa arrow ne have tap karay to e exit sudhi (bina blocked thaya) pahochi shake ke nahi
    public bool CanExit()
    {
        int n = path.Count;
        if (n < 2) return false;

        Vector2Int head = path[n - 1];
        Vector2Int prev = path[n - 2];
        Vector2Int dir = head - prev;
        Vector2Int cursor = head + dir;

        while (gridBuilder.GetCell(cursor) != null)
        {
            if (gridBuilder.IsCellOccupied(cursor)) return false;
            cursor += dir;
        }

        return true; // grid ni bahar sudhi pahochi gayu, koi avrodh na malyo
    }

    public void PlayHintPulse()
    {
        Color normalColor = Color.white;
        Color highlightColor = Color.yellow;

        PrimeTween.Sequence sequence = PrimeTween.Sequence.Create();
        for (int i = 0; i < 5; i++)
        {
            sequence.Chain(PrimeTween.Tween.Custom(0f, 1f, duration: 0.2f, onValueChange: v => SetLineColor(Color.Lerp(normalColor, highlightColor, v))));
            sequence.Chain(PrimeTween.Tween.Custom(1f, 0f, duration: 0.2f, onValueChange: v => SetLineColor(Color.Lerp(normalColor, highlightColor, v))));
        }
    }

}
