using UnityEngine;

// Where PLAY points, and when a win counts as new progress. The "frontier" is the first level not
// yet beaten: PLAY and the Home card always show it, however many older levels the player replays
// from the map.
//
// Pure functions on purpose: StageLoader and FlowDirector are scene components, and this rule needs
// to be testable without one.
public static class FrontierRule
{
    // 1-based number of the first level not yet beaten. A negative count is treated as none beaten.
    public static int FrontierLevel(int highestCompleted)
    {
        return Mathf.Max(0, highestCompleted) + 1;
    }

    // 0-based index of the frontier level. Once the whole campaign is beaten it wraps back to the
    // first level, which is what the campaign did before the map existed. totalLevels <= 0 means
    // the campaign size is not known yet, so nothing is wrapped.
    public static int FrontierIndex(int highestCompleted, int totalLevels)
    {
        int index = Mathf.Max(0, highestCompleted);
        if (totalLevels > 0 && index >= totalLevels) index = 0;
        return index;
    }

    // Banked progress after beating `completedLevel`. Max() so replaying an old level can never
    // walk progress backwards.
    public static int HighestAfter(int highestCompleted, int completedLevel)
    {
        return Mathf.Max(highestCompleted, completedLevel);
    }

    // True only when `completedLevel` is the frontier, i.e. the win is new progress. Replays of
    // beaten levels are not, and neither is anything past the frontier (which the map refuses).
    // Decides whether the level-complete analytics ping is sent.
    public static bool IsFrontierCompletion(int highestBefore, int completedLevel)
    {
        return completedLevel == FrontierLevel(highestBefore);
    }
}
