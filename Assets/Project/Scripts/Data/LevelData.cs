using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ArrowPuzzle/Level Data")]
public class LevelData : ScriptableObject
{
    public int gridWidth = 5;
    public int gridHeight = 5;

    public List<ArrowPathData> arrows = new List<ArrowPathData>();
}
