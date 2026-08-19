using UnityEngine;

// Player-facing options, persisted in PlayerPrefs.
//
// Nothing subscribes to changes — sound and vibration are read at the moment they matter. The
// frame rate is the exception, because it is pushed into Unity rather than pulled, so this class
// applies it itself.
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    // Never rename once shipped — a changed key silently resets that option for every player.
    private const string SoundKey = "settings.sound";
    private const string VibrationKey = "settings.vibration";
    private const string RefreshKey = "settings.nativeRefreshRate";

    public bool SoundEnabled { get; private set; }
    public bool VibrationEnabled { get; private set; }
    public bool NativeRefreshRateEnabled { get; private set; }

    void Awake()
    {
        Instance = this;

        // PlayerPrefs has no bool overload; everything defaults to on.
        SoundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;
        VibrationEnabled = PlayerPrefs.GetInt(VibrationKey, 1) == 1;
        NativeRefreshRateEnabled = PlayerPrefs.GetInt(RefreshKey, 1) == 1;

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
