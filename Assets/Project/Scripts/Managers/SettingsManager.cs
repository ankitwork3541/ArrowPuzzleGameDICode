using UnityEngine;

// Player-facing options, persisted across launches. Singleton to match
// LivesManager / LevelManager / AudioManager.
//
// Nothing subscribes to changes: whoever needs a setting reads it at the moment
// it matters (AudioManager before playing a clip, ArrowController before
// vibrating). The one setting that cannot work that way is the frame rate,
// because it is pushed to Unity rather than pulled — so this class applies it
// itself, both at startup and whenever it is toggled.
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    // Never rename these once the game has shipped — a changed key silently
    // resets that option back to its default for every existing player.
    private const string SoundKey = "settings.sound";
    private const string VibrationKey = "settings.vibration";
    private const string RefreshKey = "settings.nativeRefreshRate";

    public bool SoundEnabled { get; private set; }
    public bool VibrationEnabled { get; private set; }
    public bool NativeRefreshRateEnabled { get; private set; }

    void Awake()
    {
        Instance = this;

        // PlayerPrefs has no bool overload, so 1/0 is the standard idiom.
        // Everything defaults to on, matching the reference app.
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

        // Android/iOS also flush on pause, but a crash before that would lose the
        // change. Settings are toggled rarely, so writing immediately is free.
        PlayerPrefs.Save();
    }

    // Unity's documented default frame rate on mobile is 30fps, to save battery — and mobile
    // platforms IGNORE QualitySettings.vSyncCount, so the project's vSyncCount:1 does nothing
    // here. Everything was rendering at 30fps on a 120Hz screen, which is what made panning
    // read as juddery no matter how the input maths was tuned.
    //
    // Ask for whatever the screen actually runs at rather than hardcoding 60: Android rounds a
    // requested rate DOWN to a divisor of the refresh rate, so a fixed 60 would cost half the
    // available smoothness on a 90Hz panel. Turning the option off falls back to a flat 60,
    // which is the point of the option — a high-refresh panel costs real battery.
    private void ApplyFrameRate()
    {
        int refresh = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        Application.targetFrameRate = NativeRefreshRateEnabled ? Mathf.Max(60, refresh) : 60;
    }
}
