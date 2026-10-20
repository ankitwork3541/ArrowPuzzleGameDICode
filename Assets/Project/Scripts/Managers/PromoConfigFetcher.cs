using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


public class PromoConfigFetcher : MonoBehaviour
{
    // Only a cache of the last good config: it is refetched from the host on every start, so the
    // earlier-named cache files are simply left behind rather than migrated.
    const string CacheFileName = "lanterndragons_ad_config.json";

    static string CachePath => Path.Combine(Application.persistentDataPath, CacheFileName);

    public PromoRemoteConfig LoadCached()
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

    public PromoRemoteConfig Fetch()
    {
       
		string body = HostBridge.GetConfigFromAndroid();
		PromoRemoteConfig config = Parse(body);
		
		if (config == null)
		{
			Debug.LogWarning("[unity_Ads] Rejected response: " + body);
			return null;
		}

		WriteCache(body);
		return config;
    }

    static PromoRemoteConfig Parse(string json)
    {
        try
        {
            PromoRemoteConfig config = JsonUtility.FromJson<PromoRemoteConfig>(json);
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
