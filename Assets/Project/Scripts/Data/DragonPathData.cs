using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DragonPathData
{
    public List<Vector2Int> path = new List<Vector2Int>();

    // How many OTHER arrows must have left the board before this one can be tapped. 0 = no lock.
    // The generator guarantees it never exceeds the arrow's position in the solve order, so a lock
    // can always be opened (see StageLocks).
    public int lockThreshold;
}
