using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

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
        edgeCollider.edgeRadius = line.widthMultiplier * line.widthCurve.Evaluate(0f) / 2f;

        UpdateHeadPosition();
        UpdateTailPosition();
    }

    // Level-entrance: the body draws itself from the tail towards the head, like a pen
    // stroke. Deliberately split into begin / per-frame / end rather than owning a tween —
    // LevelLoader drives every arrow from ONE tween, because a tween per arrow would mean
    // 300+ simultaneous tweens on a big level and blow past PrimeTween's pre-allocated pool.
    private Vector3[] entrancePoints;
    private float entranceLength;

    public void BeginEntrance()
    {
        if (path.Count < 2) return;

        // Taps stay blocked until the draw finishes — TryAdvance() already bails on isMoving.
        isMoving = true;

        entrancePoints = new Vector3[path.Count];
        entranceLength = 0f;
        for (int i = 0; i < path.Count; i++)
        {
            entrancePoints[i] = gridBuilder.GridToWorld(path[i]);
            if (i > 0) entranceLength += Vector3.Distance(entrancePoints[i - 1], entrancePoints[i]);
        }

        // Collapse to a dot on the tail immediately — the full arrow Init() drew must not
        // stay on screen while this arrow waits its turn in the wave.
        DrawPartial(entrancePoints, entranceLength, 0f);
    }

    public void SetEntranceProgress(float fraction)
    {
        if (entrancePoints == null) return;
        DrawPartial(entrancePoints, entranceLength, fraction);
    }

    public void EndEntrance()
    {
        if (entrancePoints == null) return;

        entrancePoints = null;
        DrawPath();          // snap to the exact final state, collider included
        isMoving = false;
    }

    // Draw only the first `fraction` of the path, measured as length from the tail.
    void DrawPartial(Vector3[] full, float totalLength, float fraction)
    {
        float target = totalLength * fraction;
        float walked = 0f;
        int lastWhole = 0;              // last point the stroke has fully reached
        Vector3 tip = full[0];

        for (int i = 1; i < full.Length; i++)
        {
            float seg = Vector3.Distance(full[i - 1], full[i]);
            if (walked + seg >= target)
            {
                float k = seg > 0f ? (target - walked) / seg : 0f;
                tip = Vector3.Lerp(full[i - 1], full[i], k);
                lastWhole = i - 1;
                break;
            }
            walked += seg;
            lastWhole = i;
            tip = full[i];
        }

        line.positionCount = lastWhole + 2;
        for (int i = 0; i <= lastWhole; i++)
            line.SetPosition(i, full[i]);
        line.SetPosition(lastWhole + 1, tip);

        if (tailVisual != null)
            tailVisual.position = full[0];

        if (headVisual != null)
        {
            headVisual.position = tip;
            // At fraction 0 the tip sits on the tail, so aim along the first segment instead.
            Vector3 dir = tip - full[lastWhole];
            if (dir.sqrMagnitude < 0.000001f)
                dir = full[Mathf.Min(lastWhole + 1, full.Length - 1)] - full[lastWhole];
            if (dir.sqrMagnitude > 0.000001f)
                headVisual.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
        }
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
        // `this` is passed as the tween target so PrimeTween stops the tween by itself
        // if the arrow is destroyed mid-animation (level restart / next level).
        Tween.Custom(this, 0f, 1f, duration: stepTime, onValueChange: (arrow, t) =>
        {
            line.SetPosition(0, Vector3.Lerp(oldTailWorld, newTailTarget, t));
            line.SetPosition(n, Vector3.Lerp(oldHeadWorld, newHeadWorld, t));
            UpdateHeadPosition();
            UpdateTailPosition();
        }).OnComplete(this, warnIfTargetDestroyed: false, onComplete: arrow =>
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

        // Khali head no point (n - 1) bump kare che — baki badha points (bends + tail) tadan freeze rahe
        Sequence.Create()
            .Chain(Tween.Custom(this, 0f, 1f, duration: stepTime * 0.5f, onValueChange: (arrow, t) =>
            {
                line.SetPosition(n - 1, Vector3.Lerp(originalHeadWorld, bumpHeadWorld, t));
                UpdateHeadPosition();
            }))
            .Chain(Tween.Custom(this, 0f, 1f, duration: stepTime * 0.5f, onValueChange: (arrow, t2) =>
            {
                line.SetPosition(n - 1, Vector3.Lerp(bumpHeadWorld, originalHeadWorld, t2));
                UpdateHeadPosition();
            }))
            .OnComplete(this, warnIfTargetDestroyed: false, onComplete: arrow =>
            {
                SetLineColor(Color.white);
                isMoving = false;
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

        Tween.Custom(this, 0f, 1f, duration: stepTime, onValueChange: (arrow, t) =>
        {
            line.SetPosition(0, Vector3.Lerp(oldTailWorld, newTailTarget, t));
            line.SetPosition(n, Vector3.Lerp(oldHeadWorld, newHeadWorld, t));
            UpdateHeadPosition();
            UpdateTailPosition();
        }).OnComplete(this, warnIfTargetDestroyed: false, onComplete: arrow =>
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

        Sequence sequence = Sequence.Create();
        for (int i = 0; i < 5; i++)
        {
            sequence.Chain(Tween.Custom(this, 0f, 1f, duration: 0.2f, onValueChange: (arrow, v) => SetLineColor(Color.Lerp(normalColor, highlightColor, v))));
            sequence.Chain(Tween.Custom(this, 1f, 0f, duration: 0.2f, onValueChange: (arrow, v) => SetLineColor(Color.Lerp(normalColor, highlightColor, v))));
        }
    }

}
