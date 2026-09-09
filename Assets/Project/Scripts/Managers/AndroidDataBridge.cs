using GoogleMobileAds.Api;
using UnityEngine;

public class AndroidDataBridge
{

    public static void SendDataToAndroid(
        long valueMicros,
        string currency,
        string format)
    {
	
#if UNITY_ANDROID && !UNITY_EDITOR

		using (AndroidJavaClass bridge =
        new AndroidJavaClass("com.arrow.puzzle.playneonarrows.escape.out.UnityAdBridge"))
     {

        bridge.CallStatic(
            "onAdData",
            valueMicros,
            currency,
            format
        );
		
	 }
#endif

    }
	
	
	public static string GetConfigFromAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
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
#else
    return "";
#endif
    }
	
	
}
