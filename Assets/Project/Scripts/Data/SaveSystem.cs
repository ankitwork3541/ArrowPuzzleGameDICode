using System.IO;
using UnityEngine;

// Reads and writes SaveData as JSON under Application.persistentDataPath — the only directory that
// is writable and survives an app update on every platform Unity ships to.
//
// Static rather than a MonoBehaviour on purpose: progress has to be readable from Awake(), and a
// scene component cannot promise it exists by then.
public static class SaveSystem
{
    // Bump this when the shape of SaveData changes, and teach Migrate() how to get there.
    public const int Version = 2;

    // Intentionally keeps the old "arrowqube" prefix: renaming a shipped file name orphans every
    // existing player's progress. A new package id already gives a fresh persistentDataPath.
    private const string FileName = "arrowqube_progress.json";

    private static SaveData cached;

    public static SaveData Data
    {
        get
        {
            if (cached == null) cached = Load();
            return cached;
        }
    }

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    private static SaveData Load()
    {
        // A missing file is the normal first-run case, and a corrupt one is a phone that lost power
        // mid-write. Neither should stop the game booting, so both fall through to a fresh save.
        try
        {
            if (File.Exists(FilePath))
            {
                SaveData loaded = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
                if (loaded != null) return Migrate(loaded);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveSystem] could not read save at {FilePath} ({e.Message}). "
                             + "Starting a fresh one.");
        }

        return new SaveData { version = Version };
    }

    // Brings an older save up to the current shape rather than wiping progress that is already out
    // on players' phones.
    private static SaveData Migrate(SaveData loaded)
    {
        if (loaded.version > Version)
        {
            // Written by a NEWER build — the player downgraded, or is sharing a device backup.
            // Reading it with the old code would silently misread fields it does not know about.
            Debug.LogWarning($"[SaveSystem] save is version {loaded.version}, this build "
                             + $"understands {Version}. Leaving it alone and starting fresh.");
            return new SaveData { version = Version };
        }

        // v1 -> v2: highestLevelCompleted did not exist, so it reads back as 0 and every milestone
        // would look unearned. currentLevelIndex is the count of levels already beaten (index 10 =
        // playing level 11 = ten done), which is exactly the number to backfill.
        if (loaded.version < 2)
            loaded.highestLevelCompleted = Mathf.Max(0, loaded.currentLevelIndex);

        loaded.version = Version;
        return loaded;
    }

    public static void Save()
    {
        if (cached == null) return;
        cached.version = Version;

        // Written to a temp file first, then swapped in. A phone killed mid-write would otherwise
        // leave a half-written progress.json, and the player would lose the whole campaign rather
        // than the one level they were on.
        string temp = FilePath + ".tmp";
        try
        {
            File.WriteAllText(temp, JsonUtility.ToJson(cached));
            if (File.Exists(FilePath)) File.Delete(FilePath);
            File.Move(temp, FilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] failed to write save to {FilePath}: {e.Message}");
        }
    }

#if UNITY_EDITOR
    // Testing the first-run path otherwise means hunting down persistentDataPath by hand.
    [UnityEditor.MenuItem("ArrowPuzzle/Save/Delete Progress")]
    private static void DeleteProgress()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        cached = null;
        Debug.Log($"[SaveSystem] deleted {FilePath}");
    }

    [UnityEditor.MenuItem("ArrowPuzzle/Save/Reveal Save Folder")]
    private static void RevealSaveFolder()
    {
        UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
#endif
}
