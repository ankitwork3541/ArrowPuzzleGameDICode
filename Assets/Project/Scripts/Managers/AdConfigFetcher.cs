using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Fetches the ad config from the backend and keeps the last good payload on disk, so an offline
// launch still gets real settings instead of the shipped defaults. Never blocks the game: every
// failure path just hands back null.
public class AdConfigFetcher : MonoBehaviour
{
    const string Url = "https://newvids.xyz/arrwgmln/gapp/strtdata";
    const int TimeoutSeconds = 5;
    const string CacheFileName = "ad_config.json";

    static string CachePath => Path.Combine(Application.persistentDataPath, CacheFileName);

    public AdRemoteConfig LoadCached()
    {
        try
        {
            return File.Exists(CachePath) ? Parse(File.ReadAllText(CachePath)) : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Ads] Cache read failed: " + e.Message);
            return null;
        }
    }

    public IEnumerator Fetch(Action<AdRemoteConfig> onDone)
    {
        // ver_name is the whole payload. install_campaign used to go along with it and the backend
        // no longer takes it.
        WWWForm form = new WWWForm();
        form.AddField("ver_name", Application.version);

        using (UnityWebRequest request = UnityWebRequest.Post(Url, form))
        {
            request.timeout = TimeoutSeconds;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[Ads] Fetch failed: " + request.error);
                onDone(null);
                yield break;
            }

            string body = request.downloadHandler.text;
            AdRemoteConfig config = Parse(body);

            if (config == null)
            {
                Debug.LogWarning("[Ads] Rejected response: " + body);
                onDone(null);
                yield break;
            }

            WriteCache(body);
            onDone(config);
        }
    }

    // The API answers HTTP 200 even for errors, so the code field is the only real success signal.
    static AdRemoteConfig Parse(string json)
    {
        try
        {
            AdRemoteConfig config = JsonUtility.FromJson<AdRemoteConfig>(json);
            return config != null && config.code == 1 ? config : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Ads] Parse failed: " + e.Message);
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
            Debug.LogWarning("[Ads] Cache write failed: " + e.Message);
        }
    }
}
