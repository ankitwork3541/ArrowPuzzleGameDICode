using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public LevelData levelData;
    public GridBuilder gridBuilder;
    public ArrowController arrowPrefab;

    void Start()
    {
        gridBuilder.BuildGrid(levelData.gridWidth, levelData.gridHeight);

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
        }
    }
}
