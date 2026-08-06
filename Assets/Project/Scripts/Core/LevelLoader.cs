using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public GridBuilder grid;
    public GameObject arrowControllerPrefab;
    public LevelData levelData;

    void Start()
    {
        LoadLevel(levelData);
    }

    public void LoadLevel(LevelData data)
    {
        grid.width = data.gridWidth;
        grid.height = data.gridHeight;
        grid.spacing = data.gridSpacing;
        grid.BuildGrid();

        foreach (ArrowPathData arrowData in data.arrows)
        {
            SpawnArrow(arrowData);
        }
    }

    void SpawnArrow(ArrowPathData arrowData)
    {
        GameObject arrowObj = Instantiate(arrowControllerPrefab);
        ArrowController controller = arrowObj.GetComponent<ArrowController>();
        controller.grid = grid;
        controller.path = new List<Vector2Int>(arrowData.path);
    }
}
