using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


public class AdConfigFetcher : MonoBehaviour
{
    // Intentionally keeps the old "arrowqube" prefix (renaming drops the cached config for existing installs).
    const string CacheFileName = "arrowqube_ad_config.json";

    static string CachePath => Path.Combine(Application.persistentDataPath, CacheFileName);

    public AdRemoteConfig LoadCached()
    {
        try
        {
            return File.Exists(CachePath) ? Parse(File.ReadAllText(CachePath)) : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[unity_Ads] Cache read failed: " + e.Message);
            return null;
        }
    }

    public AdRemoteConfig Fetch()
    {
       
		string body = AndroidDataBridge.GetConfigFromAndroid();
		AdRemoteConfig config = Parse(body);
		
		if (config == null)
		{
			Debug.LogWarning("[unity_Ads] Rejected response: " + body);
			return null;
		}

		WriteCache(body);
		return config;
    }

    static AdRemoteConfig Parse(string json)
    {
        try
        {
            AdRemoteConfig config = JsonUtility.FromJson<AdRemoteConfig>(json);
            return config != null && config.code == 1 ? config : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[unity_Ads] Parse failed: " + e.Message);
            return null;
        }
    }

    static void WriteCache(string json)
    {
        try
        {
            File.WriteAllText(CachePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[unity_Ads] Cache write failed: " + e.Message);
        }
    }
}
