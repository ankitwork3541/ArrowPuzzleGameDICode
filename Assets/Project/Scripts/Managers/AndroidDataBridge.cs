using GoogleMobileAds.Api;
using UnityEngine;

public class AndroidDataBridge
{
    // The class lives in the native host app, not in this project, and it is named after the host's
    // package id. When the game gets its new package id this is the ONE line to change (see
    // production/qa/story-007-identity-switch-plan.md).
    const string BridgeClass = "in.dicodeinfotech.arrowqubepuzzle.UnityAdBridge";

    // Every paid-event handler (banner, interstitial, rewarded, native) goes through here. A host that
    // does not expose the class, or a renamed method, must cost a warning and nothing else: an ad
    // callback is never worth crashing the game over.
    public static void SendDataToAndroid(
        long valueMicros,
        string currency,
        string format)
    {

#if UNITY_ANDROID && !UNITY_EDITOR

        try
        {
            using (AndroidJavaClass bridge = new AndroidJavaClass(BridgeClass))
            {
                bridge.CallStatic(
                    "onAdData",
                    valueMicros,
                    currency,
                    format
                );
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[AndroidDataBridge] onAdData failed (host class '" + BridgeClass + "' missing or changed?): " + e.Message);
        }
#endif

    }


    // Empty when the host did not pass a config (or there is no host, as in the Editor and on a
    // standalone build); the caller treats that as "no remote config, use the cache or defaults".
    public static string GetConfigFromAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using AndroidJavaClass unityPlayer =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            using AndroidJavaObject activity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            using AndroidJavaObject intent =
                activity.Call<AndroidJavaObject>("getIntent");

            return intent.Call<string>(
                "getStringExtra",
                "game_config_json"
            );
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[AndroidDataBridge] could not read game_config_json: " + e.Message);
            return "";
        }
#else
        return "";
#endif
    }


}
