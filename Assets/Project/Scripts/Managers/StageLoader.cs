using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

public class StageLoader : MonoBehaviour
{
    public BoardBuilder gridBuilder;
    [FormerlySerializedAs("arrowPrefab")]
    public DragonController dragonPrefab;
    public BoardCamera cameraController;

    [Header("Level entrance animation")]
    public float entranceDuration = 0.4f;   // how long ONE arrow takes to draw itself
    public float entranceStagger = 0.5f;    // total window the whole wave is spread over

    // The HUD hides itself between these two — the board draws in on a clean screen.
    public event System.Action OnEntranceStart;
    public event System.Action OnEntranceComplete;

    private Tween entranceTween;
    private List<DragonController> entranceOrder = new List<DragonController>();

    private int currentLevelIndex = 0;
    private List<DragonController> spawnedDragons = new List<DragonController>();

    // Read-only view for systems that need to inspect the live arrows (LatticeGuides).
    public IReadOnlyList<DragonController> SpawnedDragons => spawnedDragons;

    // Levels come from packed binary chunks (see StagePack), loaded on demand. This replaced a
    // serialized `StageData[]`, which made every level resident and needed manual Inspector wiring.

    private byte[] chunkBytes;
    private int loadedChunkIndex = -1;

    // One reused StageData instance, refilled in place. Creating a ScriptableObject per level would
    // leak one per load, and every other system already speaks StageData.
    private StageData current;
    private int decodedLevelIndex = -1;

    private int totalLevels;

    // Picked up before anything asks what level we are on — Home reads the number as soon as it
    // draws. Not validated here on purpose: totalLevels is unknown until a chunk has been read,
    // so a save pointing past the end of the campaign is caught in EnsureCurrent() instead.
    void Awake()
    {
        currentLevelIndex = Mathf.Max(0, ProgressStore.Data.currentLevelIndex);
    }

    void SaveProgress()
    {
        ProgressStore.Data.currentLevelIndex = currentLevelIndex;
        ProgressStore.Save();
    }

    // The campaign in a build can be shorter than the one a save was written against — levels get
    // re-baked. Rather than error on every frame from then on, drop the player back to the start.
    StageData RestartCampaign(string reason)
    {
        Debug.LogWarning($"[StageLoader] {reason} Starting again from the first level.");
        currentLevelIndex = 0;
        SaveProgress();
        return EnsureCurrent();   // terminates: the index is 0 now, so this path cannot repeat
    }

    public int TotalLevels
    {
        get
        {
            EnsureCurrent();
            return totalLevels;
        }
    }

    // Cheap to call repeatedly — Home asks for the difficulty label before the board exists.
    private StageData EnsureCurrent()
    {
        if (current == null) current = ScriptableObject.CreateInstance<StageData>();
        if (decodedLevelIndex == currentLevelIndex && chunkBytes != null) return current;

        int wantChunk = StagePack.ChunkIndexOf(currentLevelIndex);

        if (loadedChunkIndex != wantChunk || chunkBytes == null)
        {
            string path = StagePack.ChunkResourcePath(wantChunk);
            TextAsset asset = Resources.Load<TextAsset>(path);

            if (asset == null)
            {
                if (currentLevelIndex != 0)
                    return RestartCampaign($"saved level {currentLevelIndex} needs chunk "
                                           + $"{wantChunk}, which this build does not have.");

                Debug.LogError($"[StageLoader] no level chunk at Resources/{path}. "
                               + "Run LanternDragons > Campaign > Bake to create it.");
                return current;
            }

            // `.bytes` is a fresh copy, so the TextAsset can go straight back out of memory.
            byte[] bytes = asset.bytes;
            Resources.UnloadAsset(asset);

            string error;
            if (!StagePack.IsValidChunk(bytes, out error))
            {
                Debug.LogError($"[StageLoader] chunk {wantChunk} is unusable: {error}");
                return current;
            }

            chunkBytes = bytes;
            loadedChunkIndex = wantChunk;
            totalLevels = StagePack.TotalLevelsIn(bytes);
        }

        int withinChunk = StagePack.IndexWithinChunk(currentLevelIndex);
        if (withinChunk >= StagePack.LevelCountIn(chunkBytes))
        {
            if (currentLevelIndex != 0)
                return RestartCampaign($"saved level {currentLevelIndex} is past the end of "
                                       + $"chunk {loadedChunkIndex}.");

            Debug.LogError($"[StageLoader] level {currentLevelIndex} is past the end of chunk {loadedChunkIndex}.");
            return current;
        }

        StagePack.ReadLevelInto(chunkBytes, withinChunk, current);
        decodedLevelIndex = currentLevelIndex;
        return current;
    }

    public void LoadLevel()
    {
        ClearDragons();

        StageData levelData = EnsureCurrent();
        if (levelData.dragons == null || levelData.dragons.Count == 0)
        {
            Debug.LogError("[StageLoader] current level has no arrows — nothing to load.");
            return;
        }

        gridBuilder.BuildGrid(levelData.gridWidth, levelData.gridHeight);

        Vector2Int min, max;
        if (TryGetDragonBounds(levelData, out min, out max))
            gridBuilder.CenterOnContent(min, max);

        // Seeded on the level index, so a level always dresses its arrows the same way — a restart
        // never reshuffles the board — while each level still gets its own mix of the palette.
        DragonPalette.Shuffler colors = new DragonPalette.Shuffler(currentLevelIndex);

        foreach (DragonPathData dragonData in levelData.dragons)
        {
            if (dragonData.path == null || dragonData.path.Count < 2)
            {
                Debug.LogWarning("Skipping arrow: path has less than 2 points, invalid.");
                continue;
            }

            DragonController newDragon = Instantiate(dragonPrefab);
            newDragon.Init(new List<Vector2Int>(dragonData.path), gridBuilder);
            newDragon.SetBaseColor(colors.Next());
            newDragon.SetLock(dragonData.lockThreshold);
            spawnedDragons.Add(newDragon);
        }

        // The cells this level actually uses, so dots appear only inside the level's own shape
        // instead of filling the whole grid rectangle around it.
        HashSet<Vector2Int> footprint = new HashSet<Vector2Int>();
        foreach (DragonPathData dragonData in levelData.dragons)
        {
            if (dragonData.path == null) continue;
            foreach (Vector2Int pos in dragonData.path) footprint.Add(pos);
        }
        gridBuilder.SetFootprint(footprint);

        StageManager.Instance.SetDragonCount(spawnedDragons.Count);
        FitCameraToSpawnedDragons();
        PlayEntrance();
    }

    // One place resolves a tap for the whole board: screen -> world -> grid cell -> whoever
    // occupies it. Replaces every arrow polling input and hit-testing its own EdgeCollider2D,
    // which was both unreliable (see DragonController.HandleTap) and O(arrows) per tap.
    void Update()
    {
        if (BoardCamera.Instance == null) return;

        // Out of lives: the losing arrow is still rewinding, so taps must not start fresh runs
        // that would still be animating when ShowLose() freezes the board under the Lose card.
        if (EmberManager.Instance != null && EmberManager.Instance.lives <= 0) return;

        Vector2 screenPos;
        if (!BoardCamera.Instance.TryGetTapThisFrame(out screenPos)) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2Int cell = gridBuilder.WorldToGrid(cam.ScreenToWorldPoint(screenPos));

        GameObject occupant = gridBuilder.GetOccupant(cell);
        if (occupant == null) return;   // tapped a dot or empty space

        DragonController dragon = occupant.GetComponent<DragonController>();
        if (dragon != null) dragon.HandleTap();
    }

    // Draw every arrow in, rippling outwards from the centre of the board.
    void PlayEntrance()
    {
        gridBuilder.ClearDots();   // the dot grid joins the HUD after the wave
        OnEntranceStart?.Invoke();

        if (spawnedDragons.Count == 0)
        {
            OnEntranceComplete?.Invoke();
            return;
        }

        // Nearest-to-centre first. Centre is world (0,0) — CenterOnContent() already put
        // the level's bounding box there.
        entranceOrder = new List<DragonController>(spawnedDragons);
        entranceOrder.Sort((a, b) => a.GetBounds().center.sqrMagnitude.CompareTo(b.GetBounds().center.sqrMagnitude));

        foreach (DragonController dragon in entranceOrder)
            dragon.BeginEntrance();

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
                    DragonController dragon = entranceOrder[i];
                    if (dragon == null) continue;

                    // Spread over a FIXED window, not a fixed per-arrow gap — a 0.03s gap would
                    // make the Huge level's 338 arrows take ten seconds instead of half a second.
                    float delay = count > 1 ? (i / (float)(count - 1)) * entranceStagger : 0f;
                    float t = Mathf.Clamp01((elapsed - delay) / entranceDuration);
                    dragon.SetEntranceProgress(1f - (1f - t) * (1f - t));   // OutQuad
                }
            })
            .OnComplete(this, warnIfTargetDestroyed: false, onComplete: loader =>
            {
                foreach (DragonController dragon in entranceOrder)
                    if (dragon != null) dragon.EndEntrance();

                gridBuilder.SpawnDots();
                OnEntranceComplete?.Invoke();
            });
    }

    void FitCameraToSpawnedDragons()
    {
        if (spawnedDragons.Count == 0) return;

        Bounds combined = spawnedDragons[0].GetBounds();
        for (int i = 1; i < spawnedDragons.Count; i++)
            combined.Encapsulate(spawnedDragons[i].GetBounds());

        cameraController.FrameLevel(combined, gridBuilder.spacing);

        // Boards too big to fit open fully visible and close in. A no-op on boards that already
        // fit, so small levels are unaffected.
        cameraController.PlayIntro();
    }

    bool TryGetDragonBounds(StageData levelData, out Vector2Int min, out Vector2Int max)
    {
        min = new Vector2Int(int.MaxValue, int.MaxValue);
        max = new Vector2Int(int.MinValue, int.MinValue);
        bool foundAny = false;

        foreach (DragonPathData dragonData in levelData.dragons)
        {
            if (dragonData.path == null) continue;

            foreach (Vector2Int pos in dragonData.path)
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

    void ClearDragons()
    {
        // Restart calls LoadLevel() directly, so without this the previous wave's tween would
        // still be running and its OnComplete would spawn dots / show the HUD over the new one.
        entranceTween.Stop();
        entranceOrder.Clear();

        foreach (DragonController dragon in spawnedDragons)
        {
            if (dragon != null)
                Destroy(dragon.gameObject);
        }
        spawnedDragons.Clear();
    }

    // Abandon the level in progress (leaving to Home, win, lose). Without this the
    // arrows keep living in the world and an in-flight exit can still report a win.
    public void ClearLevel()
    {
        ClearDragons();
        gridBuilder.ClearDots();
        gridBuilder.SetFootprint(null);   // nothing to draw until the next level defines its own
        StageManager.Instance.ClearLevel();
    }

    // Move to the next level WITHOUT loading it — the level is loaded when Play is pressed.
    public void AdvanceLevel()
    {
        EnsureCurrent();        // so totalLevels is known before it is compared against

        // Banked before the index moves — the level just beaten is the one we are still on. Max()
        // so replaying an old level cannot walk progress backwards.
        ProgressStore.Data.highestLevelCompleted =
            FrontierRule.HighestAfter(ProgressStore.Data.highestLevelCompleted, CurrentLevelNumber());

        // Always lands on the frontier, not on "this level + 1": a beaten level replayed from the
        // map must not drag PLAY back to the level after it. For a first-time win the two are the
        // same level. Past the last level it wraps to the first (abhi mate: badha levels puri thay
        // etle pachu 1st thi).
        currentLevelIndex = FrontierRule.FrontierIndex(ProgressStore.Data.highestLevelCompleted, totalLevels);

        // The only place the saved index ever moves, so the only place progress needs writing.
        // Written immediately rather than on quit: Android can kill the app without ever running
        // OnApplicationQuit, and a level's worth of progress is not worth risking to save one write.
        SaveProgress();
    }

    // Jump to a level picked on the level map, WITHOUT loading it — same contract as AdvanceLevel().
    // A level past the frontier (the first one not yet beaten) is refused here as well as on the
    // map, so a stray caller cannot skip ahead.
    //
    // Deliberately NOT saved: a replay is a visit, not progress. The saved index stays on the
    // frontier, so killing the app mid-replay still reopens on the frontier.
    public void SelectLevel(int levelNumber)
    {
        EnsureCurrent();        // so totalLevels is known before it is compared against

        int number = Mathf.Min(levelNumber, FrontierRule.FrontierLevel(ProgressStore.Data.highestLevelCompleted));

        currentLevelIndex = Mathf.Clamp(number - 1, 0, Mathf.Max(0, totalLevels - 1));
    }

    // Back to the frontier after a replay is abandoned (leaving to Home), so PLAY and the Home card
    // never keep pointing at a level the player only visited.
    public void PointAtFrontier()
    {
        EnsureCurrent();        // so totalLevels is known before it is compared against

        currentLevelIndex = FrontierRule.FrontierIndex(ProgressStore.Data.highestLevelCompleted, totalLevels);
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

    public StageData CurrentLevel()
    {
        return EnsureCurrent();
    }

    void OnDestroy()
    {
        if (current != null) Destroy(current);   // created with CreateInstance, so ours to clean up
    }

    // Hint mate: atyare koi j arrow ni exit ray clear che ke nahi, e scan kari ne pehli mali te return kare
    public DragonController FindHintableDragon()
    {
        foreach (DragonController dragon in spawnedDragons)
        {
            if (dragon != null && !dragon.IsBusy && dragon.CanExit())
                return dragon;
        }
        return null;
    }

    // Hint: point the camera at the arrow first, then pulse it. Lives here rather than in
    // FlowDirector because this is where the camera and grid references already are.
    public void PlayHint()
    {
        DragonController dragon = FindHintableDragon();
        if (dragon == null) return;

        cameraController.FocusOn(dragon.GetBounds(), gridBuilder.spacing);
        dragon.PlayHintPulse();
    }
}
