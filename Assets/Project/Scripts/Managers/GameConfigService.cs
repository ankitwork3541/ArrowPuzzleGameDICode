using System;
using System.IO;
using UnityEngine;

// The one owner of the current GameConfig. Everything configurable reads from here; nothing keeps its
// own copy. It starts on GameConfig.Defaults(), is replaced by the cached copy of the last good
// payload at start-up, and again by a fresh payload when the config API answers.
public static class GameConfigService
{
    // Only a cache of the last good payload: it is refetched on every start.
    public const string CacheFileName = "lanterndragons_config_cache.json";

    static GameConfig current = GameConfig.Defaults();

    // Tests point these elsewhere; the game never sets them.
    public static string CachePathOverride;
    public static Func<int> HighestCompletedProvider;

    public static GameConfig Current => current;

    // How a level builds itself in (ui.entrance_style). Always a usable style: unknown or missing = ink.
    public static EntranceStyle EntranceStyle =>
        EntranceMath.ParseStyle(current != null && current.ui != null ? current.ui.entrance_style : null);

    // True once a real payload (cached or fresh) has been applied this session.
    public static bool HasPayload { get; private set; }

    // Raised after every successful apply. Screens re-read what they show.
    public static event Action Changed;

    public static string CachePath =>
        CachePathOverride ?? Path.Combine(Application.persistentDataPath, CacheFileName);

    // Levels the player has cleared; gates show_after_level and skip_first_levels.
    public static int HighestLevelCompleted =>
        HighestCompletedProvider != null
            ? HighestCompletedProvider()
            : Mathf.Max(0, ProgressStore.Data.highestLevelCompleted);

    // Parses, validates and applies a payload. Returns false (and changes nothing) for one to ignore.
    public static bool ApplyJson(string json, bool fromCache = false)
    {
        if (!GameConfigParser.TryParse(json, out GameConfig config, out string reason))
        {
            Debug.LogWarning("[Config] Payload ignored: " + reason);
            return false;
        }

        current = config;
        HasPayload = true;
        Debug.Log("[Config] " + (fromCache ? "Cached" : "Fresh") + " config applied (ads.enabled="
                  + config.ads.enabled + ").");
        Changed?.Invoke();
        return true;
    }

    // The last good payload, if there is one. Returns false when there is no usable cache.
    public static bool LoadCache()
    {
        try
        {
            return File.Exists(CachePath) && ApplyJson(File.ReadAllText(CachePath), fromCache: true);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Config] Cache read failed: " + e.Message);
            return false;
        }
    }

    public static void WriteCache(string json)
    {
        try
        {
            File.WriteAllText(CachePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Config] Cache write failed: " + e.Message);
        }
    }

    // Tests and the dev screenshot driver use these to put a known config in place.
    public static void Set(GameConfig config)
    {
        current = config ?? GameConfig.Defaults();
        HasPayload = config != null;
        Changed?.Invoke();
    }

    public static void ResetToDefaults()
    {
        current = GameConfig.Defaults();
        HasPayload = false;
        CachePathOverride = null;
        HighestCompletedProvider = null;
        Changed?.Invoke();
    }
}
