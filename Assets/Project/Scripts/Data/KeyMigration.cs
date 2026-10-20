using UnityEngine;

// One-time copy of a PlayerPrefs int from an older key name to the current one, so a player whose
// hints or settings sit under an earlier name keeps them. Copy, never move: the old value stays, so a
// build that still reads the old name keeps working too.
public static class KeyMigration
{
    // Does nothing when the current key already exists. Otherwise the first legacy key that exists
    // wins (list the newest generation first). Returns the legacy key that was copied, or null.
    public static string CopyIntIfMissing(string currentKey, params string[] legacyKeys)
    {
        if (PlayerPrefs.HasKey(currentKey)) return null;

        foreach (string legacy in legacyKeys)
        {
            if (!PlayerPrefs.HasKey(legacy)) continue;

            PlayerPrefs.SetInt(currentKey, PlayerPrefs.GetInt(legacy));
            return legacy;
        }

        return null;
    }
}
