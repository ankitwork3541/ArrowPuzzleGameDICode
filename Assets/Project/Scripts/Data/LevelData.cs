using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Tools/Level Data")]
public class LevelData : ScriptableObject
{
    public int gridWidth = 8;
    public int gridHeight = 8;
    public float gridSpacing = 1f;

    public List<ArrowPathData> arrows = new List<ArrowPathData>();
    
}
