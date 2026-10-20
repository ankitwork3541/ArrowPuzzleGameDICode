using UnityEngine;

// Everything about a player's run through the campaign that has to outlive the app.
//
// Deliberately NOT PlayerPrefs. Settings live there and should stay there — they are exactly the
// loose key/value flags it is meant for — but progress is structured game data that will grow
// (coins, stars, unlocked themes), and PlayerPrefs has no room for that shape and no way to
// migrate it when it changes.
//
// Plain public fields with no properties, because JsonUtility only serialises fields.
[System.Serializable]
public class ProgressData
{
    // Stamped on every write. Nothing reads it yet, but a save file with no version in it is
    // unmigratable forever after — by the time the format changes, the old files are already out
    // on players' phones. See ProgressStore.Migrate.
    public int version;

    // Zero-based, matching StageLoader. The player sees index + 1.
    public int currentLevelIndex;

    // Highest level NUMBER (1-based) ever beaten; 0 = none. Kept separate from currentLevelIndex,
    // which wraps back to 0 at the end of the campaign. It drives the level map (done / current /
    // locked), the frontier rule and the ad rules that depend on progress. The bottom-dock level
    // milestones it once also fed were removed; the field itself stays because it is saved.
    public int highestLevelCompleted;

    // The one-time "locked dragon" coach overlay has been shown (or was skipped because the first padlock
    // level was already beaten). A field added later is safe for old saves: JsonUtility leaves it false.
    public bool lockTutorialSeen;
}
