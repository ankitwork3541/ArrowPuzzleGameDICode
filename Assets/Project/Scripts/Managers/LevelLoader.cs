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
            ArrowController newArrow = Instantiate(arrowPrefab);
            newArrow.path = new List<Vector2Int>(arrowData.path);
            newArrow.gridBuilder = gridBuilder;
        }
    }
}
