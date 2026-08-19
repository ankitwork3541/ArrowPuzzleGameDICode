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
public class SaveData
{
    // Stamped on every write. Nothing reads it yet, but a save file with no version in it is
    // unmigratable forever after — by the time the format changes, the old files are already out
    // on players' phones. See SaveSystem.Migrate.
    public int version;

    // Zero-based, matching LevelLoader. The player sees index + 1.
    public int currentLevelIndex;

    // Highest level NUMBER (1-based) ever beaten; 0 = none. Kept separate from currentLevelIndex,
    // which wraps back to 0 at the end of the campaign — an unlocked milestone must never re-lock.
    public int highestLevelCompleted;
}
