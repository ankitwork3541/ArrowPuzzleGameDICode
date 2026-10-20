using System.IO;
using UnityEngine;

// Reads and writes ProgressData as JSON under Application.persistentDataPath — the only directory that
// is writable and survives an app update on every platform Unity ships to.
//
// Static rather than a MonoBehaviour on purpose: progress has to be readable from Awake(), and a
// scene component cannot promise it exists by then.
public static class ProgressStore
{
    // Bump this when the shape of ProgressData changes, and teach Migrate() how to get there.
    public const int Version = 2;

    // The save file for this game. Two earlier names are migrated into it once (see
    // CopyLegacyFileIfMissing), so a player whose progress sits under an older name keeps it.
    public const string FileName = "lanterndragons_progress.json";

    // Oldest-first is wrong here: the first name that exists wins, newest generation first.
    public static readonly string[] LegacyFileNames = { "arrowqube_progress.json", "progress.json" };

    private static ProgressData cached;

    public static ProgressData Data
    {
        get
        {
            if (cached == null) cached = Load();
            return cached;
        }
    }

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    // One-time migration: if there is no current save yet but an older-named one exists, copy it over.
    // Copy, never move, so a build that still reads the old name keeps working. Returns the legacy file
    // name that was copied, or null when nothing was done. The directory is a parameter so a test can
    // point it at a temp folder.
    public static string CopyLegacyFileIfMissing(string directory)
    {
        string current = Path.Combine(directory, FileName);
        if (File.Exists(current)) return null;

        foreach (string legacy in LegacyFileNames)
        {
            string old = Path.Combine(directory, legacy);
            if (!File.Exists(old)) continue;

            File.Copy(old, current);
            return legacy;
        }

        return null;
    }

    private static ProgressData Load()
    {
        // A missing file is the normal first-run case, and a corrupt one is a phone that lost power
        // mid-write. Neither should stop the game booting, so both fall through to a fresh save.
        try
        {
            string copied = CopyLegacyFileIfMissing(Application.persistentDataPath);
            if (copied != null) Debug.Log($"[ProgressStore] migrated progress from {copied}.");

            if (File.Exists(FilePath))
            {
                ProgressData loaded = JsonUtility.FromJson<ProgressData>(File.ReadAllText(FilePath));
                if (loaded != null) return Migrate(loaded);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ProgressStore] could not read save at {FilePath} ({e.Message}). "
                             + "Starting a fresh one.");
        }

        return new ProgressData { version = Version };
    }

    // Brings an older save up to the current shape rather than wiping progress that is already out
    // on players' phones.
    private static ProgressData Migrate(ProgressData loaded)
    {
        if (loaded.version > Version)
        {
            // Written by a NEWER build — the player downgraded, or is sharing a device backup.
            // Reading it with the old code would silently misread fields it does not know about.
            Debug.LogWarning($"[ProgressStore] save is version {loaded.version}, this build "
                             + $"understands {Version}. Leaving it alone and starting fresh.");
            return new ProgressData { version = Version };
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
            Debug.LogError($"[ProgressStore] failed to write save to {FilePath}: {e.Message}");
        }
    }

#if UNITY_EDITOR
    // Testing the first-run path otherwise means hunting down persistentDataPath by hand.
    [UnityEditor.MenuItem("LanternDragons/Save/Delete Progress")]
    private static void DeleteProgress()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        cached = null;
        Debug.Log($"[ProgressStore] deleted {FilePath}");
    }

    [UnityEditor.MenuItem("LanternDragons/Save/Reveal Save Folder")]
    private static void RevealSaveFolder()
    {
        UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
#endif
}
