using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public LevelData[] levels;
    public GridBuilder gridBuilder;
    public ArrowController arrowPrefab;
    public CameraController cameraController;

    private int currentLevelIndex = 0;
    private List<ArrowController> spawnedArrows = new List<ArrowController>();

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

        LevelManager.Instance.SetArrowCount(spawnedArrows.Count);
        FitCameraToSpawnedArrows();
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
        foreach (ArrowController arrow in spawnedArrows)
        {
            if (arrow != null)
                Destroy(arrow.gameObject);
        }
        spawnedArrows.Clear();
    }

    public void GoToNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= levels.Length)
            currentLevelIndex = 0; // abhi mate: badha levels puri thay etle pachu 1st thi

        LoadLevel();
    }

    public int CurrentLevelNumber()
    {
        return currentLevelIndex + 1; // player ne "Level 1" dekhavvu che, index 0 nahi
    }
}
