using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(LineRenderer))]
public class ArrowController : MonoBehaviour
{
    public List<Vector2Int> path = new List<Vector2Int>();
    public GridBuilder gridBuilder; // Inspector ma GridBuilder object drag karo
    public Transform headVisual; // Inspector ma "Head" child object drag karo
    public Transform tailVisual; // Inspector ma "Tail" child object drag karo

    private bool isMoving = false;
    public float stepTime = 0.09f;

    [Header("Run acceleration")]
    // Deliberately steep: only the first step or two read as a wind-up, then the arrow is at
    // full speed for the rest of the run. A gentler ramp made the whole slide feel slow.
    public float stepSpeedUp = 0.55f;    // each step lasts this fraction of the previous one
    // Floor. Roughly 1.7 frames per cell at 60fps — going much below this stops reading as
    // faster motion and starts reading as the arrow skipping cells.
    public float minStepTime = 0.028f;
    // Safety floor for weak devices, in rendered frames. At 60fps this is 0.025s, so minStepTime
    // above still wins; at 30fps it becomes 0.05s and stops the arrow skipping whole cells.
    public float minFramesPerStep = 1.5f;

    private int runStepIndex;

    [Header("Blocked revert")]
    // A blocked run rewinds all the way to where it was tapped. Leaving the arrow parked
    // wherever it jammed would put the board in a position the generator never validated —
    // the level could become genuinely unsolvable. Now the only cost of a bad tap is a life.
    // No wind-up on the way back (the run out has one), so the rewind reads faster than the run.
    public float revertStepTime = 0.028f;

    // Every cell the head has stood on this run, tail-first: the starting path, then one entry
    // appended per step. After k steps the tail sits on runTrace[k] and the head on
    // runTrace[k + path.Count - 1]. That indexing is what lets the rewind be exact.
    private List<Vector2Int> runTrace = new List<Vector2Int>();
    private int stepsTaken;

    // Decided at tap time, and knowable there because an arrow's heading never changes during a
    // run — CanExit() walks the very cells the run is about to walk. A doomed run touches grid
    // occupancy not at all: this arrow's cells stay claimed the whole way out and back, so no
    // other arrow can take one mid-run and leave the rewind unable to reclaim it. It also stops
    // the dots under a doomed run flashing open for cells that were never really freed.
    private bool runDoomed;

    // Mid-move or mid-exit. The hint scan skips these — an arrow already on its way out is
    // not something the player needs pointed at, and it can't be pulsed safely either.
    public bool IsBusy => isMoving;

    // Worn while the arrow is actually travelling. BlockedBounce puts white back when a run
    // ends against another arrow; a run that ends off-grid destroys the arrow instead.
    public Color activeColor = new Color(0f, 0.898f, 1f);   // Neon Cyan #00E5FF

    [Header("Hint pulse")]
    public float hintPulseScale = 1.25f;   // how far the body swells at the peak
    public float hintPulseTime = 0.2f;     // one half-pulse; 10 yoyo cycles = 5 swells
    public Color hintColor = new Color(1f, 0.85f, 0.2f);

    private Tween hintTween;
    private Vector3[] hintBasePoints;      // non-null only while a pulse is running
    private Vector3 hintCentre;
    private Vector3 hintHeadScale, hintTailScale;

    private LineRenderer line;
    private SpriteRenderer headSprite;
    private SpriteRenderer tailSprite;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
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


    // LevelLoader resolves a tap to a grid cell and calls this on whichever arrow occupies it.
    //
    // Input used to be per-arrow: every ArrowController polled the tap itself and hit-tested its
    // own EdgeCollider2D. That was wrong twice over. Unity documents that Collider2D.OverlapPoint
    // "will always return false when used on an EdgeCollider2D", and even where edgeRadius made it
    // partly work the target was only ~0.1 of a cell either side of the line — so taps landed
    // sometimes and missed sometimes. It also meant 110 arrows each polling input every frame.
    // A cell is a 1x1 target and stays that size however thin the art gets, which is why the
    // reference game has thinner lines and no touch trouble at all.
    public void HandleTap()
    {
        // A fresh tap starts a new run, so the wind-up starts from slow again. Guarded by
        // isMoving because TryAdvance() also re-enters itself to continue the current run.
        if (!isMoving) BeginRun();
        TryAdvance();
    }

    void DrawPath()
    {
        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
            line.SetPosition(i, gridBuilder.GridToWorld(path[i]));

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

        // Still standing still here, so restoring the geometry is safe. Left running, the
        // pulse would keep rewriting the same line points the move is animating.
        StopHintPulse();

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
    // A run winds up: every step is shorter than the last, so the arrow starts at a readable
    // pace and leaves the board fast. Pair this with Ease.Linear per step — any ease-out
    // would brake the arrow to a full stop at every single cell boundary.
    float NextStepTime()
    {
        // The timing is already frame-independent — PrimeTween advances by Time.deltaTime, so a
        // step lasts the same real seconds everywhere. What is NOT device-independent is how many
        // frames get drawn inside it: on a slow device 0.028s is under one frame, and the arrow
        // would visibly jump whole cells instead of sliding. Floor every step at real frames.
        float floor = Mathf.Max(minStepTime, Time.smoothDeltaTime * minFramesPerStep);

        float duration = Mathf.Max(floor, stepTime * Mathf.Pow(stepSpeedUp, runStepIndex));
        runStepIndex++;
        return duration;
    }

    // Called once per tap, not per step — TryAdvance() re-enters itself to continue a run.
    void BeginRun()
    {
        runStepIndex = 0;
        stepsTaken = 0;
        runDoomed = !CanExit();
        runTrace.Clear();
        runTrace.AddRange(path);
    }

    float RevertStepTime()
    {
        // Same frame floor as the run out: under roughly one rendered frame per cell the
        // rewind stops reading as motion and starts reading as the arrow teleporting.
        return Mathf.Max(revertStepTime, Time.smoothDeltaTime * minFramesPerStep);
    }

    void MoveStep(Vector2Int nextPos)
    {
        SetLineColor(activeColor);
        int n = path.Count;

        // Capture old tail/head BEFORE shifting path (needed for the grow/shrink animation)
        Vector3 oldTailWorld = gridBuilder.GridToWorld(path[0]);
        Vector3 oldHeadWorld = gridBuilder.GridToWorld(path[n - 1]);
        Vector3 newHeadWorld = gridBuilder.GridToWorld(nextPos);

        if (!runDoomed) gridBuilder.ClearOccupant(path[0]);
        for (int i = 0; i < n - 1; i++) path[i] = path[i + 1];
        path[n - 1] = nextPos;
        if (!runDoomed) gridBuilder.SetOccupant(nextPos, gameObject);

        // Extend the trace, not just a counter: a run can travel further than the arrow is
        // long, and past that point the tail is standing on cells the starting path never held.
        runTrace.Add(nextPos);
        stepsTaken++;

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
        Tween.Custom(this, 0f, 1f, duration: NextStepTime(), ease: Ease.Linear, onValueChange: (arrow, t) =>
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

            isMoving = false;
            TryAdvance();
        });
    }
    
    void BlockedBounce()
    {
        AudioManager.Instance.PlayArrowClickBlocked();
        if (SettingsManager.Instance.VibrationEnabled) Handheld.Vibrate();
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
            .OnComplete(this, warnIfTargetDestroyed: false, onComplete: arrow => arrow.RevertStep());
    }

    // Rewind the run one cell at a time, undoing exactly what MoveStep() did — the mirror of it.
    // Stays on the same "grow one end, shrink the other, freeze the middle" technique: a single
    // tween lerping every point at once would cut diagonally across every bend on the way back,
    // which is the exact bug that technique exists to avoid.
    void RevertStep()
    {
        if (stepsTaken <= 0)
        {
            // Back where the tap found it. Occupancy is identical to the tap-time board too,
            // because every step's claim was undone one at a time on the way here.
            SetLineColor(Color.white);
            isMoving = false;

            // Only now is it safe to end the level: ShowLose() destroys this arrow.
            LivesManager.Instance.ResolveBlockedTap();
            return;
        }

        int n = path.Count;

        Vector3 oldTailWorld = gridBuilder.GridToWorld(path[0]);
        Vector3 oldHeadWorld = gridBuilder.GridToWorld(path[n - 1]);

        // The tail steps back onto the cell it vacated on the way out.
        Vector2Int newTailCell = runTrace[stepsTaken - 1];

        // Mirrors MoveStep(): a doomed run never released anything, so there is nothing to reclaim.
        if (!runDoomed) gridBuilder.ClearOccupant(path[n - 1]);
        for (int i = n - 1; i >= 1; i--) path[i] = path[i - 1];
        path[0] = newTailCell;
        if (!runDoomed) gridBuilder.SetOccupant(newTailCell, gameObject);

        stepsTaken--;

        Vector3 newTailTarget = gridBuilder.GridToWorld(path[0]);
        Vector3 newHeadTarget = gridBuilder.GridToWorld(path[n - 1]);

        // n+1 points while animating: [growing tail, frozen middle..., shrinking head]
        line.positionCount = n + 1;
        line.SetPosition(0, oldTailWorld);
        for (int i = 1; i <= n - 1; i++)
            line.SetPosition(i, gridBuilder.GridToWorld(path[i]));
        line.SetPosition(n, oldHeadWorld);

        Tween.Custom(this, 0f, 1f, duration: RevertStepTime(), ease: Ease.Linear, onValueChange: (arrow, t) =>
        {
            line.SetPosition(0, Vector3.Lerp(oldTailWorld, newTailTarget, t));
            line.SetPosition(n, Vector3.Lerp(oldHeadWorld, newHeadTarget, t));
            UpdateHeadPosition();
            UpdateTailPosition();
        }).OnComplete(this, warnIfTargetDestroyed: false, onComplete: arrow =>
        {
            // Drop the now-redundant duplicate point at the head end.
            Vector3[] finalPositions = new Vector3[n];
            for (int i = 0; i < n; i++)
                finalPositions[i] = gridBuilder.GridToWorld(path[i]);

            line.positionCount = n;
            line.SetPositions(finalPositions);

            arrow.RevertStep();
        });
    }

    void StartExit(Vector3 exitStep)
    {
        AudioManager.Instance.PlayArrowClickSuccess();
        SetLineColor(activeColor);   // an arrow one cell from the edge exits without ever calling MoveStep

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

        Tween.Custom(this, 0f, 1f, duration: NextStepTime(), ease: Ease.Linear, onValueChange: (arrow, t) =>
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

    // Hint highlight: the arrow swells and shrinks 5 times while flashing its hint colour,
    // so the player can see which one to tap. The body is a world-space LineRenderer, so
    // "scaling" means pushing its points out from their own centre — transform.localScale
    // would do nothing at all here.
    public void PlayHintPulse()
    {
        // Never pulse an arrow that is mid-move or mid-exit. Those animations grow the line
        // to n+1 points, and StopHintPulse()'s DrawPath() would shrink it back to n under
        // them — the running tween then writes index n and LineRenderer throws.
        if (isMoving) return;

        StopHintPulse();
        if (path.Count < 2) return;

        int n = line.positionCount;
        hintBasePoints = new Vector3[n];
        hintCentre = Vector3.zero;
        for (int i = 0; i < n; i++)
        {
            hintBasePoints[i] = line.GetPosition(i);
            hintCentre += hintBasePoints[i];
        }
        hintCentre /= n;

        if (headVisual != null) hintHeadScale = headVisual.localScale;
        if (tailVisual != null) hintTailScale = tailVisual.localScale;

        // One tween, 10 yoyo cycles = 5 swells. A Sequence of 10 chained tweens would cost
        // 10 slots in PrimeTween's pre-allocated pool for exactly the same result.
        hintTween = Tween.Custom(this, 0f, 1f, duration: hintPulseTime, cycles: 10,
            cycleMode: CycleMode.Yoyo, ease: Ease.InOutSine,
            onValueChange: (arrow, v) => arrow.ApplyHintPulse(v))
            .OnComplete(this, warnIfTargetDestroyed: false, onComplete: arrow => arrow.StopHintPulse());
    }

    void ApplyHintPulse(float v)
    {
        SetLineColor(Color.Lerp(Color.white, hintColor, v));

        float scale = Mathf.Lerp(1f, hintPulseScale, v);
        for (int i = 0; i < hintBasePoints.Length; i++)
            line.SetPosition(i, hintCentre + (hintBasePoints[i] - hintCentre) * scale);

        UpdateHeadPosition();
        UpdateTailPosition();
        if (headVisual != null) headVisual.localScale = hintHeadScale * scale;
        if (tailVisual != null) tailVisual.localScale = hintTailScale * scale;
    }

    // Safe to call at any time — TryAdvance() uses it so a pulse can never fight a move.
    public void StopHintPulse()
    {
        if (hintBasePoints == null) return;

        hintTween.Stop();
        hintBasePoints = null;

        SetLineColor(Color.white);
        if (headVisual != null) headVisual.localScale = hintHeadScale;
        if (tailVisual != null) tailVisual.localScale = hintTailScale;

        // Belt and braces: a move/exit tween owns the line's point count while it runs, so
        // never rewrite the geometry underneath it — that animation restores it anyway.
        if (!isMoving) DrawPath();
    }

}
