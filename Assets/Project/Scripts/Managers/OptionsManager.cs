using UnityEngine;

// Player-facing options, persisted in PlayerPrefs.
//
// Sound and vibration are read at the moment they matter. The frame rate is the exception, because
// it is pushed into Unity rather than pulled, so this class applies it itself. Reduce Motion is read
// by each animation when it starts (ReduceMotionOn); the change event exists only for things that
// run continuously and need to react at once.
public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance;

    // Never rename once shipped — a changed key silently resets that option for every player. These
    // are the current names; values under the two earlier generations are copied across once in
    // Awake (KeyMigration).
    private const string SoundKey = "lanterndragons.settings.sound";
    private const string VibrationKey = "lanterndragons.settings.vibration";
    private const string RefreshKey = "lanterndragons.settings.nativeRefreshRate";

    // New in this game, so there is nothing older to migrate from.
    private const string ReduceMotionKey = "lanterndragons.reduceMotion";

    public bool SoundEnabled { get; private set; }
    public bool VibrationEnabled { get; private set; }
    public bool NativeRefreshRateEnabled { get; private set; }

    // Until the player touches the switch it follows ui.reduce_motion_default from the game config
    // (off when there is no config). Once they have chosen, their choice always wins. Unity exposes no
    // cheap read of the OS "reduce motion" preference, so it is an in-game switch.
    private bool reduceMotionChosen;
    private bool reduceMotionValue;

    public bool ReduceMotion =>
        reduceMotionChosen ? reduceMotionValue : GameConfigService.Current.ui.reduce_motion_default;

    // True when reduced motion is on. Safe to call before the manager exists (then it is off), so
    // animation code can use it without a null check.
    public static bool ReduceMotionOn => Instance != null && Instance.ReduceMotion;

    public event System.Action<bool> ReduceMotionChanged;

    void Awake()
    {
        Instance = this;

        KeyMigration.CopyIntIfMissing(SoundKey, "arrowqube.settings.sound", "settings.sound");
        KeyMigration.CopyIntIfMissing(VibrationKey, "arrowqube.settings.vibration", "settings.vibration");
        KeyMigration.CopyIntIfMissing(RefreshKey, "arrowqube.settings.nativeRefreshRate", "settings.nativeRefreshRate");

        // PlayerPrefs has no bool overload; everything defaults to on.
        SoundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;
        VibrationEnabled = PlayerPrefs.GetInt(VibrationKey, 1) == 1;
        NativeRefreshRateEnabled = PlayerPrefs.GetInt(RefreshKey, 1) == 1;
        reduceMotionChosen = PlayerPrefs.HasKey(ReduceMotionKey);
        reduceMotionValue = PlayerPrefs.GetInt(ReduceMotionKey, 0) == 1;

        ApplyFrameRate();
    }

    public void SetSound(bool on)
    {
        SoundEnabled = on;
        Store(SoundKey, on);
    }

    public void SetVibration(bool on)
    {
        VibrationEnabled = on;
        Store(VibrationKey, on);
    }

    public void SetNativeRefreshRate(bool on)
    {
        NativeRefreshRateEnabled = on;
        Store(RefreshKey, on);
        ApplyFrameRate();
    }

    public void SetReduceMotion(bool on)
    {
        bool before = ReduceMotion;

        // Even when the value matches the config default this records the player's own choice, so a
        // later change of the default no longer moves it.
        reduceMotionChosen = true;
        reduceMotionValue = on;
        Store(ReduceMotionKey, on);

        if (before != on) ReduceMotionChanged?.Invoke(on);
    }

    private static void Store(string key, bool on)
    {
        PlayerPrefs.SetInt(key, on ? 1 : 0);

        // Toggled rarely, so writing immediately is free and survives a crash before pause.
        PlayerPrefs.Save();
    }

    // Mobile defaults to 30fps and IGNORES vSyncCount, which is what made panning judder on a
    // 120Hz screen. Ask for the panel's actual rate rather than a fixed 60 — Android rounds a
    // request DOWN to a divisor, so 60 would halve the smoothness available on a 90Hz panel.
    private void ApplyFrameRate()
    {
        int refresh = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        Application.targetFrameRate = NativeRefreshRateEnabled ? Mathf.Max(60, refresh) : 60;
    }
}
