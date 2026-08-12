using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

public class LevelLoader : MonoBehaviour
{
    public LevelData[] levels;
    public GridBuilder gridBuilder;
    public ArrowController arrowPrefab;
    public CameraController cameraController;

    [Header("Level entrance animation")]
    public float entranceDuration = 0.4f;   // how long ONE arrow takes to draw itself
    public float entranceStagger = 0.5f;    // total window the whole wave is spread over

    // The HUD hides itself between these two — the board draws in on a clean screen.
    public event System.Action OnEntranceStart;
    public event System.Action OnEntranceComplete;

    private Tween entranceTween;
    private List<ArrowController> entranceOrder = new List<ArrowController>();

    private int currentLevelIndex = 0;
    private List<ArrowController> spawnedArrows = new List<ArrowController>();

    // Read-only view for systems that need to inspect the live arrows (GuideLines).
    public IReadOnlyList<ArrowController> SpawnedArrows => spawnedArrows;

    public void LoadLevel()
    {
        ClearArrows();

        LevelData levelData = levels[currentLevelIndex];
        gridBuilder.BuildGrid(levelData.gridWidth, levelData.gridHeight);

        Vector2Int min, max;
        if (TryGetArrowBounds(levelData, out min, out max))
            gridBuilder.CenterOnContent(min, max);

        foreach (ArrowPathData arrowData in levelData.arrows)
        {
            if (arrowData.path == null || arrowData.path.Count < 2)
            {
                Debug.LogWarning("Skipping arrow: path has less than 2 points, invalid.");
                continue;
            }

            ArrowController newArrow = Instantiate(arrowPrefab);
            newArrow.Init(new List<Vector2Int>(arrowData.path), gridBuilder);
            spawnedArrows.Add(newArrow);
        }

        // The cells this level actually uses, so dots appear only inside the level's own shape
        // instead of filling the whole grid rectangle around it.
        HashSet<Vector2Int> footprint = new HashSet<Vector2Int>();
        foreach (ArrowPathData arrowData in levelData.arrows)
        {
            if (arrowData.path == null) continue;
            foreach (Vector2Int pos in arrowData.path) footprint.Add(pos);
        }
        gridBuilder.SetFootprint(footprint);

        LevelManager.Instance.SetArrowCount(spawnedArrows.Count);
        FitCameraToSpawnedArrows();
        PlayEntrance();
    }

    // One place resolves a tap for the whole board: screen -> world -> grid cell -> whoever
    // occupies it. Replaces every arrow polling input and hit-testing its own EdgeCollider2D,
    // which was both unreliable (see ArrowController.HandleTap) and O(arrows) per tap.
    void Update()
    {
        if (CameraController.Instance == null) return;

        // Out of lives: the losing arrow is still rewinding, so taps must not start fresh runs
        // that would still be animating when ShowLose() tears the level down.
        if (LivesManager.Instance != null && LivesManager.Instance.lives <= 0) return;

        Vector2 screenPos;
        if (!CameraController.Instance.TryGetTapThisFrame(out screenPos)) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2Int cell = gridBuilder.WorldToGrid(cam.ScreenToWorldPoint(screenPos));

        GameObject occupant = gridBuilder.GetOccupant(cell);
        if (occupant == null) return;   // tapped a dot or empty space

        ArrowController arrow = occupant.GetComponent<ArrowController>();
        if (arrow != null) arrow.HandleTap();
    }

    // Draw every arrow in, rippling outwards from the centre of the board.
    void PlayEntrance()
    {
        gridBuilder.ClearDots();   // the dot grid joins the HUD after the wave
        OnEntranceStart?.Invoke();

        if (spawnedArrows.Count == 0)
        {
            OnEntranceComplete?.Invoke();
            return;
        }

        // Nearest-to-centre first. Centre is world (0,0) — CenterOnContent() already put
        // the level's bounding box there.
        entranceOrder = new List<ArrowController>(spawnedArrows);
        entranceOrder.Sort((a, b) => a.GetBounds().center.sqrMagnitude.CompareTo(b.GetBounds().center.sqrMagnitude));

        foreach (ArrowController arrow in entranceOrder)
            arrow.BeginEntrance();

        float total = entranceStagger + entranceDuration;

        // ONE tween drives the whole wave. Giving each arrow its own tween is what blew past
        // PrimeTween's 200-tween pool on a 150+ arrow level. Ease.Linear because the tween
        // carries elapsed TIME here — the easing is applied per arrow, below.
        entranceTween = Tween.Custom(this, 0f, total, duration: total, ease: Ease.Linear,
            onValueChange: (loader, elapsed) =>
            {
                int count = entranceOrder.Count;   // read live — ClearArrows() can empty this
                for (int i = 0; i < count; i++)
                {
                    ArrowController arrow = entranceOrder[i];
                    if (arrow == null) continue;

                    // Spread over a FIXED window, not a fixed per-arrow gap — a 0.03s gap would
                    // make the Huge level's 338 arrows take ten seconds instead of half a second.
                    float delay = count > 1 ? (i / (float)(count - 1)) * entranceStagger : 0f;
                    float t = Mathf.Clamp01((elapsed - delay) / entranceDuration);
                    arrow.SetEntranceProgress(1f - (1f - t) * (1f - t));   // OutQuad
                }
            })
            .OnComplete(this, warnIfTargetDestroyed: false, onComplete: loader =>
            {
                foreach (ArrowController arrow in entranceOrder)
                    if (arrow != null) arrow.EndEntrance();

                gridBuilder.SpawnDots();
                OnEntranceComplete?.Invoke();
            });
    }

    void FitCameraToSpawnedArrows()
    {
        if (spawnedArrows.Count == 0) return;

        Bounds combined = spawnedArrows[0].GetBounds();
        for (int i = 1; i < spawnedArrows.Count; i++)
            combined.Encapsulate(spawnedArrows[i].GetBounds());

        cameraController.FrameLevel(combined, gridBuilder.spacing);
    }

    bool TryGetArrowBounds(LevelData levelData, out Vector2Int min, out Vector2Int max)
    {
        min = new Vector2Int(int.MaxValue, int.MaxValue);
        max = new Vector2Int(int.MinValue, int.MinValue);
        bool foundAny = false;

        foreach (ArrowPathData arrowData in levelData.arrows)
        {
            if (arrowData.path == null) continue;

            foreach (Vector2Int pos in arrowData.path)
            {
                foundAny = true;
                if (pos.x < min.x) min.x = pos.x;
                if (pos.y < min.y) min.y = pos.y;
                if (pos.x > max.x) max.x = pos.x;
                if (pos.y > max.y) max.y = pos.y;
            }
        }

        return foundAny;
    }

    void ClearArrows()
    {
        // Restart calls LoadLevel() directly, so without this the previous wave's tween would
        // still be running and its OnComplete would spawn dots / show the HUD over the new one.
        entranceTween.Stop();
        entranceOrder.Clear();

        foreach (ArrowController arrow in spawnedArrows)
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }
        spawnedArrows.Clear();
    }

    // Abandon the level in progress (leaving to Home, win, lose). Without this the
    // arrows keep living in the world and an in-flight exit can still report a win.
    public void ClearLevel()
    {
        ClearArrows();
        gridBuilder.ClearDots();
        gridBuilder.SetFootprint(null);   // nothing to draw until the next level defines its own
        LevelManager.Instance.ClearLevel();
    }

    // Move to the next level WITHOUT loading it — the level is loaded when Play is pressed.
    public void AdvanceLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= levels.Length)
            currentLevelIndex = 0; // abhi mate: badha levels puri thay etle pachu 1st thi
    }

    public void GoToNextLevel()
    {
        AdvanceLevel();
        LoadLevel();
    }

    public int CurrentLevelNumber()
    {
        return currentLevelIndex + 1; // player ne "Level 1" dekhavvu che, index 0 nahi
    }

    public LevelData CurrentLevel()
    {
        return levels[currentLevelIndex];
    }

    // Hint mate: atyare koi j arrow ni exit ray clear che ke nahi, e scan kari ne pehli mali te return kare
    public ArrowController FindHintableArrow()
    {
        foreach (ArrowController arrow in spawnedArrows)
        {
            if (arrow != null && !arrow.IsBusy && arrow.CanExit())
                return arrow;
        }
        return null;
    }

    // Hint: point the camera at the arrow first, then pulse it. Lives here rather than in
    // GameFlowManager because this is where the camera and grid references already are.
    public void PlayHint()
    {
        ArrowController arrow = FindHintableArrow();
        if (arrow == null) return;

        cameraController.FocusOn(arrow.GetBounds(), gridBuilder.spacing);
        arrow.PlayHintPulse();
    }
}
