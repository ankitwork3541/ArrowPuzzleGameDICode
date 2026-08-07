using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public LevelData[] levels;
    public GridBuilder gridBuilder;
    public ArrowController arrowPrefab;

    private int currentLevelIndex = 0;
    private List<ArrowController> spawnedArrows = new List<ArrowController>();

    public void LoadLevel()
    {
        ClearArrows();

        LevelData levelData = levels[currentLevelIndex];
        gridBuilder.BuildGrid(levelData.gridWidth, levelData.gridHeight);

        int spawnedCount = 0;

        foreach (ArrowPathData arrowData in levelData.arrows)
        {
            if (arrowData.path == null || arrowData.path.Count < 2)
            {
                Debug.LogWarning("Skipping arrow: path has less than 2 points, invalid.");
                continue;
            }

            ArrowController newArrow = Instantiate(arrowPrefab);
            newArrow.path = new List<Vector2Int>(arrowData.path);
            newArrow.gridBuilder = gridBuilder;
            spawnedArrows.Add(newArrow);
            spawnedCount++;
        }

        LevelManager.Instance.SetArrowCount(spawnedCount);
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
