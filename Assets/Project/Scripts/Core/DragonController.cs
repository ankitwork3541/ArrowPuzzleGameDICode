using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(LineRenderer))]
public class DragonController : MonoBehaviour
{
    public List<Vector2Int> path = new List<Vector2Int>();
    public BoardBuilder gridBuilder; // Inspector ma BoardBuilder object drag karo
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

    // How many OTHER arrows must still leave the board before this one can be tapped.
    // 0 means unlocked. Set through SetLock() so the padlock label stays in step.
    public int lockRemaining = 0;

    public bool IsLocked => lockRemaining > 0;

    // The padlock: a gold lock sprite on the arrow's middle cell with the remaining count on its
    // body. One parent transform so a shake or a pop moves the sprite and the number together.
    // Assigned on the arrow prefab; without it the number is still drawn, just on bare water.
    public Sprite padlockSprite;

    // Badge size as a fraction of one grid cell, and the digit height relative to the badge.
    const float LockBadgeCells = 1.25f;
    const float LockDigitSize = 0.17f;
    // Ink, the same dark tone as the HUD text: the badge is gold, and a purple digit on gold was the
    // weakest text on the board.
    static readonly Color LockDigitColor = new Color(0.227f, 0.141f, 0.094f);

    private Transform lockBadge;
    private TextMesh lockText;
    private SpriteRenderer lockSprite;
    private Tween lockTween;

    public void SetLock(int remaining)
    {
        lockRemaining = Mathf.Max(0, remaining);

        if (lockRemaining == 0)
        {
            RemoveLockBadge();
            return;
        }

        if (lockBadge == null) CreateLockBadge();
        lockText.text = lockRemaining.ToString();
    }

    void CreateLockBadge()
    {
        GameObject go = new GameObject("LockBadge");
        go.transform.SetParent(transform, true);
        go.transform.position = gridBuilder.GridToWorld(path[path.Count / 2]);
        go.transform.localScale = Vector3.one * (gridBuilder.spacing * LockBadgeCells);
        lockBadge = go.transform;

        if (padlockSprite != null)
        {
            lockSprite = go.AddComponent<SpriteRenderer>();
            lockSprite.sprite = padlockSprite;
            lockSprite.sortingOrder = 51;
        }

        GameObject textGo = new GameObject("Count");
        textGo.transform.SetParent(lockBadge, false);
        // The padlock body sits in the lower part of the sprite, so the digit goes there.
        textGo.transform.localPosition = padlockSprite != null ? new Vector3(0f, -0.13f, 0f) : Vector3.zero;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lockText = textGo.AddComponent<TextMesh>();
        lockText.font = font;
        lockText.fontSize = 64;
        lockText.characterSize = LockDigitSize;
        lockText.anchor = TextAnchor.MiddleCenter;
        lockText.alignment = TextAlignment.Center;
        lockText.fontStyle = FontStyle.Bold;
        lockText.color = padlockSprite != null ? LockDigitColor : Color.white;

        MeshRenderer meshRenderer = textGo.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = font.material;
        meshRenderer.sortingOrder = 52;
    }

    void RemoveLockBadge()
    {
        if (lockBadge == null) return;
        Destroy(lockBadge.gameObject);
        lockBadge = null;
        lockText = null;
        lockSprite = null;
    }

    // Every arrow that leaves the board counts one off the lock. At zero the lock opens.
    void OnOtherDragonLeft()
    {
        if (!IsLocked) return;

        lockRemaining--;
        if (lockRemaining > 0)
        {
            lockText.text = lockRemaining.ToString();
            return;
        }

        // Unlock: the badge swells and fades out, then goes (Reduce Motion: it only fades). It stays a
        // child of the arrow, so it is destroyed with it. The fields are cleared first, so
        // RemoveLockBadge and the exit never touch it while the tween finishes the fade.
        SoundDirector.Instance.PlayUnlock();
        float swell = OptionsManager.ReduceMotionOn ? 0f : 0.6f;
        Transform badge = lockBadge;
        TextMesh text = lockText;
        SpriteRenderer sprite = lockSprite;
        lockBadge = null;
        lockText = null;
        lockSprite = null;
        if (badge == null) return;

        if (lockTween.isAlive) lockTween.Stop();
        Vector3 startScale = badge.localScale;
        Color textStart = text != null ? text.color : Color.white;
        Tween.Custom(badge.gameObject, 0f, 1f, duration: 0.35f, ease: Ease.OutQuad, onValueChange: (go, t) =>
        {
            go.transform.localScale = startScale * (1f + swell * t);
            float alpha = 1f - t;
            if (sprite != null) { Color c = sprite.color; c.a = alpha; sprite.color = c; }
            if (text != null) { Color c = textStart; c.a = alpha; text.color = c; }
        }).OnComplete(badge.gameObject, warnIfTargetDestroyed: false, onComplete: go => Destroy(go));
    }

    void ShakeLock()
    {
        if (lockBadge == null) return;
        if (lockTween.isAlive) lockTween.Complete();

        // Reduce Motion: no shake; the badge just dips in opacity and back so the tap still reads.
        if (OptionsManager.ReduceMotionOn)
        {
            SpriteRenderer sprite = lockSprite;
            TextMesh text = lockText;
            Color textBase = text != null ? text.color : Color.white;
            lockTween = Tween.Custom(lockBadge, 0f, 1f, duration: 0.25f, onValueChange: (badge, t) =>
            {
                float alpha = 1f - 0.5f * Mathf.Sin(t * Mathf.PI);
                if (sprite != null) { Color c = sprite.color; c.a = alpha; sprite.color = c; }
                if (text != null) { Color c = textBase; c.a = alpha; text.color = c; }
            });
            return;
        }

        lockTween = Tween.ShakeLocalPosition(lockBadge,
            strength: new Vector3(gridBuilder.spacing * 0.15f, 0f, 0f), duration: 0.25f, frequency: 20f);
    }

    void OnDestroy()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnDragonLeft -= OnOtherDragonLeft;
    }

    // This arrow's own colour, handed out by StageLoader at spawn. White is only the
    // fallback for an arrow dropped into the scene by hand, so the Editor still shows something.
    public Color baseColor = Color.white;

    // The board is light, so "ink" is the one dark tone the whole look hangs on: it is what a
    // travelling arrow deepens towards, what the bump flashes, and what outlines are mixed from.
    static readonly Color Ink = new Color(0.10f, 0.08f, 0.14f);

    // A travelling arrow deepens towards ink instead of switching to one shared "active" colour —
    // a shared colour would wipe out the arrow's identity the moment it moved.
    // Set to 0 in the Inspector to make a run keep the base colour exactly.
    [Range(0f, 1f)] public float activeBoost = 0.35f;

    Color ActiveColor() => Color.Lerp(baseColor, Ink, activeBoost);

    // Outline and shadow: a light board has no Bloom to lift a dragon off the paper, so each body is
    // drawn three times — a soft shadow, a darker slightly wider outline, then the coloured body.
    [Header("Outline and shadow")]
    public float outlineWidthScale = 1.4f;                 // outline width relative to the body
    public float outlineDarken = 0.55f;                    // how far the outline mixes towards ink
    public Vector2 shadowOffset = new Vector2(0.03f, -0.05f);
    [Range(0f, 1f)] public float shadowAlpha = 0.22f;
    public float spriteOutlineScale = 1.3f;               // head / pearl outline relative to the sprite
    // The pearl marks where the tail is, so it has to read at a glance on a small board: bigger than
    // the prefab's sprite, a fixed gold fill whatever the dragon's own colour, and a full-ink outline.
    public float pearlScale = 1.25f;
    public Color pearlFill = new Color(1f, 0.78f, 0.24f);

    const int OutlineSortingOffset = -2;
    const int ShadowSortingOffset = -3;

    private LineRenderer outlineLine;
    private LineRenderer shadowLine;
    private SpriteRenderer headOutline;
    private SpriteRenderer tailOutline;
    private bool outlineDirty;       // geometry was rewritten outside a tween (DrawPath etc.)
    private bool outlineSettle;      // one more mirror after the last animated frame

    // Safe before or after Init() — Awake() has already cached the renderers by the time
    // Instantiate() returns, so the colour lands whichever order StageLoader calls in.
    public void SetBaseColor(Color c)
    {
        baseColor = c;
        SetLineColor(c);
    }

    [Header("Hint pulse")]
    public float hintPulseScale = 1.25f;   // how far the body swells at the peak
    public float hintPulseTime = 0.2f;     // one half-pulse; 10 yoyo cycles = 5 swells
    // Flash when an arrow runs into another. Near-black ink rather than red or white: red is a
    // resting dragon colour, and white vanishes on cream paper.
    public Color bumpColor = new Color(0.07f, 0.05f, 0.10f);
    public Color hintColor = new Color(0f, 0.467f, 0.714f);   // strong blue #0077B6: no dragon is blue

    private Tween hintTween;
    private Vector3[] hintBasePoints;      // non-null only while a pulse is running
    private bool hintColourOnly;           // Reduce Motion: the pulse changes colour but does not swell
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
        {
            tailSprite = tailVisual.GetComponent<SpriteRenderer>();
            tailVisual.localScale *= pearlScale;
        }

        CreateOutlineLine(ShadowSortingOffset, out shadowLine, "Shadow");
        CreateOutlineLine(OutlineSortingOffset, out outlineLine, "Outline");
        headOutline = CreateSpriteOutline(headSprite);
        tailOutline = CreateSpriteOutline(tailSprite);
        SetLineColor(baseColor);
    }

    // A second LineRenderer that copies the body's look and shares its material, so it batches the
    // same way. World-space like the body; LateUpdate() keeps its points in step.
    void CreateOutlineLine(int sortingOffset, out LineRenderer result, string objectName)
    {
        GameObject go = new GameObject(objectName);
        go.layer = gameObject.layer;
        go.transform.SetParent(transform, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.sharedMaterial = line.sharedMaterial;
        lr.widthCurve = line.widthCurve;
        lr.widthMultiplier = line.widthMultiplier * outlineWidthScale;
        lr.numCornerVertices = line.numCornerVertices;
        lr.numCapVertices = line.numCapVertices;
        lr.alignment = line.alignment;
        lr.textureMode = line.textureMode;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.sortingLayerID = line.sortingLayerID;
        lr.sortingOrder = line.sortingOrder + sortingOffset;
        lr.positionCount = 0;
        result = lr;
    }

    // A darker, slightly larger copy of a head / pearl sprite parented to it, so it inherits the
    // sprite's position, rotation and the hint pulse's scaling for free.
    SpriteRenderer CreateSpriteOutline(SpriteRenderer source)
    {
        if (source == null) return null;

        GameObject go = new GameObject("Outline");
        go.layer = source.gameObject.layer;
        go.transform.SetParent(source.transform, false);
        go.transform.localScale = Vector3.one * spriteOutlineScale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = source.sprite;
        sr.sharedMaterial = source.sharedMaterial;
        sr.sortingLayerID = source.sortingLayerID;
        sr.sortingOrder = source.sortingOrder + OutlineSortingOffset;
        return sr;
    }

    // Mirrors the body's points into the outline and the (offset) shadow. Runs only while something
    // is animating, plus one settling frame after it stops — the last tween frame writes the final
    // points and then clears isMoving in the same call, so without the settle frame it would be
    // skipped. No steady-state allocations: the points move through two reused buffers with one
    // GetPositions and two SetPositions calls (a LineRenderer's positionCount is capped by the
    // buffers' lengths on read, so a buffer longer than the line is harmless).
    private Vector3[] mirrorBuffer = new Vector3[16];
    private Vector3[] shadowBuffer = new Vector3[16];

    void LateUpdate()
    {
        bool active = isMoving || hintBasePoints != null || outlineDirty;
        if (!active && !outlineSettle) return;

        outlineSettle = active;
        outlineDirty = false;

        int count = line.positionCount;
        outlineLine.positionCount = count;
        shadowLine.positionCount = count;

        // Grows only when a dragon gets longer than any it has been before.
        if (mirrorBuffer.Length < count)
        {
            mirrorBuffer = new Vector3[count + 16];
            shadowBuffer = new Vector3[count + 16];
        }

        line.GetPositions(mirrorBuffer);
        outlineLine.SetPositions(mirrorBuffer);

        Vector3 offset = new Vector3(shadowOffset.x, shadowOffset.y, 0f);
        for (int i = 0; i < count; i++)
            shadowBuffer[i] = mirrorBuffer[i] + offset;
        shadowLine.SetPositions(shadowBuffer);

        // The buffers are usually longer than the line. Writing the count again afterwards makes
        // the result the same whether SetPositions ignores the extra entries or grows the line.
        outlineLine.positionCount = count;
        shadowLine.positionCount = count;
    }

    public void Init(List<Vector2Int> newPath, BoardBuilder builder)
    {
        path = newPath;
        gridBuilder = builder;
        DrawPath();
        ClaimOccupancy();

        if (StageManager.Instance != null)
            StageManager.Instance.OnDragonLeft += OnOtherDragonLeft;
    }

    void ClaimOccupancy()
    {
        foreach (Vector2Int pos in path)
        {
            gridBuilder.SetOccupant(pos, gameObject);
        }
    }


    // StageLoader resolves a tap to a grid cell and calls this on whichever arrow occupies it.
    //
    // Input used to be per-arrow: every DragonController polled the tap itself and hit-tested its
    // own EdgeCollider2D. That was wrong twice over. Unity documents that Collider2D.OverlapPoint
    // "will always return false when used on an EdgeCollider2D", and even where edgeRadius made it
    // partly work the target was only ~0.1 of a cell either side of the line — so taps landed
    // sometimes and missed sometimes. It also meant 110 arrows each polling input every frame.
    // A cell is a 1x1 target and stays that size however thin the art gets, which is why the
    // reference game has thinner lines and no touch trouble at all.
    public void HandleTap()
    {
        // A locked arrow never starts a run: no move, no bump, no life lost. Only the padlock reacts.
        if (IsLocked)
        {
            ShakeLock();
            SoundDirector.Instance.PlayLockShake();
            HapticPulse.Light();
            return;
        }

        // A fresh tap starts a new run, so the wind-up starts from slow again. Guarded by
        // isMoving because TryAdvance() also re-enters itself to continue the current run.
        if (!isMoving) BeginRun();
        TryAdvance();
    }

    void DrawPath()
    {
        outlineDirty = true;
        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
            line.SetPosition(i, gridBuilder.GridToWorld(path[i]));

        UpdateHeadPosition();
        UpdateTailPosition();
    }

    // Level-entrance: the body draws itself from the tail towards the head, like a pen
    // stroke. Deliberately split into begin / per-frame / end rather than owning a tween —
    // StageLoader drives every arrow from ONE tween, because a tween per arrow would mean
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
        outlineDirty = true;
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

        BoardCell nextCell = gridBuilder.GetCell(nextPos);

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
        SetLineColor(ActiveColor());
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
        Tween.Custom(this, 0f, 1f, duration: NextStepTime(), ease: Ease.Linear, onValueChange: (dragon, t) =>
        {
            line.SetPosition(0, Vector3.Lerp(oldTailWorld, newTailTarget, t));
            line.SetPosition(n, Vector3.Lerp(oldHeadWorld, newHeadWorld, t));
            UpdateHeadPosition();
            UpdateTailPosition();
        }).OnComplete(this, warnIfTargetDestroyed: false, onComplete: dragon =>
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
        SoundDirector.Instance.PlayDragonClickBlocked();
        HapticPulse.Bump();
        EmberManager.Instance.LoseLife();

        int n = path.Count;
        Vector3 originalHeadWorld = line.GetPosition(n - 1);

        Vector2Int head = path[n - 1];
        Vector2Int prev = path[n - 2];
        Vector2Int dir = head - prev;
        Vector3 bumpHeadWorld = originalHeadWorld + new Vector3(dir.x, dir.y, 0f) * gridBuilder.spacing * 0.3f;

        SetLineColor(bumpColor);

        isMoving = true;

        // Reduce Motion: no overshoot of the head, just the colour change held for the same time
        // before the run rewinds, so the block is still readable.
        if (OptionsManager.ReduceMotionOn)
        {
            Tween.Custom(this, 0f, 1f, duration: stepTime, onValueChange: (dragon, t) => { })
                .OnComplete(this, warnIfTargetDestroyed: false, onComplete: dragon => dragon.RevertStep());
            return;
        }

        // Khali head no point (n - 1) bump kare che — baki badha points (bends + tail) tadan freeze rahe
        Sequence.Create()
            .Chain(Tween.Custom(this, 0f, 1f, duration: stepTime * 0.5f, onValueChange: (dragon, t) =>
            {
                line.SetPosition(n - 1, Vector3.Lerp(originalHeadWorld, bumpHeadWorld, t));
                UpdateHeadPosition();
            }))
            .Chain(Tween.Custom(this, 0f, 1f, duration: stepTime * 0.5f, onValueChange: (dragon, t2) =>
            {
                line.SetPosition(n - 1, Vector3.Lerp(bumpHeadWorld, originalHeadWorld, t2));
                UpdateHeadPosition();
            }))
            .OnComplete(this, warnIfTargetDestroyed: false, onComplete: dragon => dragon.RevertStep());
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
            SetLineColor(baseColor);
            isMoving = false;

            // Only now is it safe to end the level: ShowLose() destroys this arrow.
            EmberManager.Instance.ResolveBlockedTap();
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

        Tween.Custom(this, 0f, 1f, duration: RevertStepTime(), ease: Ease.Linear, onValueChange: (dragon, t) =>
        {
            line.SetPosition(0, Vector3.Lerp(oldTailWorld, newTailTarget, t));
            line.SetPosition(n, Vector3.Lerp(oldHeadWorld, newHeadTarget, t));
            UpdateHeadPosition();
            UpdateTailPosition();
        }).OnComplete(this, warnIfTargetDestroyed: false, onComplete: dragon =>
        {
            // Drop the now-redundant duplicate point at the head end.
            Vector3[] finalPositions = new Vector3[n];
            for (int i = 0; i < n; i++)
                finalPositions[i] = gridBuilder.GridToWorld(path[i]);

            line.positionCount = n;
            line.SetPositions(finalPositions);

            dragon.RevertStep();
        });
    }

    void StartExit(Vector3 exitStep)
    {
        SoundDirector.Instance.PlayDragonClickSuccess();
        SetLineColor(ActiveColor());   // an arrow one cell from the edge exits without ever calling MoveStep

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

        Tween.Custom(this, 0f, 1f, duration: NextStepTime(), ease: Ease.Linear, onValueChange: (dragon, t) =>
        {
            line.SetPosition(0, Vector3.Lerp(oldTailWorld, newTailTarget, t));
            line.SetPosition(n, Vector3.Lerp(oldHeadWorld, newHeadWorld, t));
            UpdateHeadPosition();
            UpdateTailPosition();
        }).OnComplete(this, warnIfTargetDestroyed: false, onComplete: dragon =>
        {
            Vector3[] finalPositions = new Vector3[n];
            for (int i = 0; i < n - 1; i++)
                finalPositions[i] = oldWorld[i + 1];
            finalPositions[n - 1] = newHeadWorld;

            line.positionCount = n;
            line.SetPositions(finalPositions);

            if (IsFullyOffScreen())
            {
                SoundDirector.Instance.PlayDragonExit();
                StageManager.Instance.DragonExited();
                Destroy(gameObject);
            }
            else
                ExitStepOnce(exitStep);
        });
    }

    // One bounds test, not a Camera.main lookup per point — the renderer already tracks the line's
    // world-space box.
    bool IsFullyOffScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return true;   // no camera to be on-screen of

        Bounds b = line.bounds;

        // Orthographic keeps axis alignment, so the two extreme corners bound the whole box.
        Vector3 min = cam.WorldToViewportPoint(b.min);
        Vector3 max = cam.WorldToViewportPoint(b.max);

        float loX = Mathf.Min(min.x, max.x), hiX = Mathf.Max(min.x, max.x);
        float loY = Mathf.Min(min.y, max.y), hiY = Mathf.Max(min.y, max.y);

        return hiX < 0f || loX > 1f || hiY < 0f || loY > 1f;
    }

    // Vertex colours only, never `line.material` — reading `.material` instantiates a private copy
    // per renderer, so 160 arrows meant 160 unique materials and no batching at all.
    void SetLineColor(Color c)
    {
        line.startColor = c;
        line.endColor = c;

        Color edge = Color.Lerp(c, Ink, outlineDarken);
        if (outlineLine != null)
        {
            outlineLine.startColor = edge;
            outlineLine.endColor = edge;
        }
        if (shadowLine != null)
        {
            Color shade = new Color(Ink.r, Ink.g, Ink.b, shadowAlpha);
            shadowLine.startColor = shade;
            shadowLine.endColor = shade;
        }

        if (headSprite != null)
            headSprite.color = c;
        if (tailSprite != null)
            tailSprite.color = pearlFill;
        if (headOutline != null)
            headOutline.color = edge;
        if (tailOutline != null)
            tailOutline.color = Ink;
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
        // A locked arrow cannot be tapped, so hint and guide lines must not treat it as a way out.
        if (IsLocked) return false;

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
        hintColourOnly = OptionsManager.ReduceMotionOn;

        // One tween, 10 yoyo cycles = 5 swells. A Sequence of 10 chained tweens would cost
        // 10 slots in PrimeTween's pre-allocated pool for exactly the same result.
        hintTween = Tween.Custom(this, 0f, 1f, duration: hintPulseTime, cycles: 10,
            cycleMode: CycleMode.Yoyo, ease: Ease.InOutSine,
            onValueChange: (dragon, v) => dragon.ApplyHintPulse(v))
            .OnComplete(this, warnIfTargetDestroyed: false, onComplete: dragon => dragon.StopHintPulse());
    }

    void ApplyHintPulse(float v)
    {
        SetLineColor(Color.Lerp(baseColor, hintColor, v));
        if (hintColourOnly) return;

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

        SetLineColor(baseColor);
        if (headVisual != null) headVisual.localScale = hintHeadScale;
        if (tailVisual != null) tailVisual.localScale = hintTailScale;

        // Belt and braces: a move/exit tween owns the line's point count while it runs, so
        // never rewrite the geometry underneath it — that animation restores it anyway.
        if (!isMoving) DrawPath();
    }

}
