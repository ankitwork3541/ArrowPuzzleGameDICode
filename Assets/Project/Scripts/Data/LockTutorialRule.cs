using System;

// When the one-time "locked dragon" coach overlay shows, and the words it uses. Pure (no Unity objects,
// no disk), so every rule is unit-tested.
public static class LockTutorialRule
{
    // How many levels from the start of the campaign are scanned for the first padlock. The baked campaign
    // has its first lock early; a campaign with none in this window simply gets no tutorial.
    public const int MaxScan = 400;

    // Zero-based index of the first level that has at least one padlocked dragon, or -1 when none does.
    // `levelHasLock` is asked for levels 0 .. min(totalLevels, MaxScan) - 1 in order.
    public static int FirstLockLevelIndex(Func<int, bool> levelHasLock, int totalLevels)
    {
        if (levelHasLock == null) return -1;

        int limit = Math.Min(Math.Max(0, totalLevels), MaxScan);
        for (int i = 0; i < limit; i++)
            if (levelHasLock(i)) return i;

        return -1;
    }

    // Show only on the very first padlock level, once per install, when the config allows it and the player
    // has not already beaten that level (a save from before this feature existed counts as "seen").
    // `highestCompletedLevelNumber` is 1-based (0 = none); `currentLevelIndex` and `firstLockLevelIndex` are 0-based.
    public static bool ShouldShow(bool enabledByConfig, bool alreadySeen, int currentLevelIndex,
                                  int firstLockLevelIndex, int highestCompletedLevelNumber)
    {
        if (!enabledByConfig || alreadySeen) return false;
        if (firstLockLevelIndex < 0) return false;
        if (currentLevelIndex != firstLockLevelIndex) return false;

        // Level number = index + 1; beaten means highest completed has reached it.
        return highestCompletedLevelNumber < firstLockLevelIndex + 1;
    }

    public static string BodyText(int remaining)
    {
        int n = Math.Max(1, remaining);
        string dragons = n == 1 ? "1 other dragon" : n + " other dragons";
        return "The number shows how many other dragons must leave the board first. Free "
               + dragons + " and this lock opens by itself.";
    }

    // The short reminder shown near a padlock when the player taps a locked dragon.
    public static string RemainingLabel(int remaining)
    {
        return "Free " + Math.Max(1, remaining) + " more";
    }
}
