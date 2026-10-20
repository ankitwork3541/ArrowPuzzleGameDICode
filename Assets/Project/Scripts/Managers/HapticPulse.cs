using UnityEngine;

// Short vibration pulses. Handheld.Vibrate() is a fixed ~400 ms buzz on Android, far too heavy for a
// gentle game, so on Android this asks the system Vibrator for a 12-22 ms one-shot instead. iOS has no
// duration control, so only the strong cue (the bump) uses its system vibration there.
//
// Everything respects the Vibration option and swallows failures: a missing vibrator must never cost the
// player a tap.
public static class HapticPulse
{
    public const int BumpMs = 22;
    public const int LightMs = 12;

    // VibrationEffect amplitude is 1..255.
    private const int BumpAmplitude = 150;
    private const int LightAmplitude = 55;

    public static void Bump() { Pulse(BumpMs, BumpAmplitude, true); }

    // The padlock shake: barely there.
    public static void Light() { Pulse(LightMs, LightAmplitude, false); }

    private static void Pulse(int ms, int amplitude, bool strong)
    {
        if (OptionsManager.Instance == null || !OptionsManager.Instance.VibrationEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidPulse(ms, amplitude);
#elif UNITY_IOS && !UNITY_EDITOR
        if (strong) Handheld.Vibrate();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject vibrator;
    private static int sdk;
    private static bool unavailable;

    private static void AndroidPulse(int ms, int amplitude)
    {
        if (unavailable) return;

        try
        {
            if (vibrator == null)
            {
                using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }

                using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    sdk = version.GetStatic<int>("SDK_INT");
                }
            }

            if (vibrator == null || !vibrator.Call<bool>("hasVibrator"))
            {
                unavailable = true;
                return;
            }

            if (sdk >= 26)
            {
                using (AndroidJavaClass effect = new AndroidJavaClass("android.os.VibrationEffect"))
                using (AndroidJavaObject oneShot = effect.CallStatic<AndroidJavaObject>("createOneShot", (long)ms, amplitude))
                {
                    vibrator.Call("vibrate", oneShot);
                }
            }
            else
            {
                vibrator.Call("vibrate", (long)ms);
            }
        }
        catch (System.Exception e)
        {
            unavailable = true;
            Debug.LogWarning("[Haptics] Vibrator call failed, falling back to the system buzz: " + e.Message);

            // Also what keeps Unity adding android.permission.VIBRATE to the manifest: the permission is
            // detected from this API call, and the AndroidJavaObject route above is invisible to it.
            Handheld.Vibrate();
        }
    }
#endif
}
