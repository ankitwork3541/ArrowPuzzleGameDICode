using System.Collections.Generic;
using UnityEngine;

// Five bands, matching the reference apps (which show "Hard" and "Nightmare" on their HUD).
// The value is MEASURED by the generator, never hand-picked — see LevelDifficulty.
public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    Hardest,
    Nightmare
}

[CreateAssetMenu(fileName = "LevelData", menuName = "ArrowPuzzle/Level Data")]
public class LevelData : ScriptableObject
{
    public int gridWidth = 5;
    public int gridHeight = 5;

    public List<ArrowPathData> arrows = new List<ArrowPathData>();

    [Header("Measured at generation — do not hand-edit")]
    // Arrow count used to stand in for difficulty, which was wrong: 70 short straight arrows and
    // 70 long tangled ones scored identically. This is the composite of the metrics below.
    public Difficulty difficulty = Difficulty.Easy;
    public int difficultyScore;      // 0-100
    public int generatorSeed;        // regenerates this exact level

    [Header("Scored metrics behind the score (tuning aid)")]
    // Mean number of legal moves per wave, as a fraction of all arrows. The smaller this is, the
    // more of the board must be scanned to find a move — the strongest single signal.
    public float measuredClearableRatio;
    public float measuredAvgRayLength;   // cells of ray the player must trace to verify a move
    public int measuredWaves;            // rounds the greedy solve takes to clear the board
    public float measuredAvgBends;       // bends per arrow — how hard a body is to follow

    [Header("Diagnostics (measured, not scored)")]
    public int measuredMinClearable;     // saturates at 1-2 on every board size, so not scored
    public float measuredDecoyRatio;     // ~0 at 100% fill; will matter if levels get gaps
    // Worst point in the solve: the longest ray the easiest legal move still needs traced.
    // A comfort signal for playtesting, not a pass/fail threshold.
    public int measuredHardestVerify;
}
