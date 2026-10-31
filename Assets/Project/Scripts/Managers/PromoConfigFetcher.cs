using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Fetches the game config (ads, links, tuning, UI switches) from the config API and hands it to
// GameConfigService. The cache of the last good payload lives in the service too.
public class PromoConfigFetcher : MonoBehaviour
{
    // The config API. It returns { "status": "success", "data": <the JSON in
    // design/config/game-config.sample.json> }; GameConfigParser unwraps the envelope. If the URL is
    // ever emptied no request is made and the game runs on the cached copy, else GameConfig.Defaults().
    const string ConfigUrl = "https://api.dicodeinfotech.in/api/arrow-dragon-puzzle/config";
    // The bearer token lives in Assets/Project/Resources/ConfigApiToken.txt, which is git-ignored so the
    // key never lands in the repository. It still ships inside the build, so it must be a read-only key.
    const string TokenResource = "ConfigApiToken";
    const int TimeoutSeconds = 5;

    static string LoadToken()
    {
        TextAsset asset = Resources.Load<TextAsset>(TokenResource);
        return asset != null ? asset.text.Trim() : "";
    }

    // Last launch's payload, so an offline session still runs on real settings rather than defaults.
    public bool ApplyCached()
    {
        return GameConfigService.LoadCache();
    }

    // Applies the fresh payload and caches it. Returns on the same frame when there is no URL yet.
    // onDone receives true only when a fresh payload was accepted.
    public IEnumerator Fetch(System.Action<bool> onDone)
    {
        if (string.IsNullOrEmpty(ConfigUrl))
        {
            onDone(false);
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(ConfigUrl))
        {
            request.timeout = TimeoutSeconds;
            request.SetRequestHeader("accept", "application/json");
            string token = LoadToken();
            if (token.Length > 0)
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }
            else
            {
                Debug.LogWarning("[Config] Resources/" + TokenResource + ".txt is missing; calling without a token.");
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[Config] Request failed: " + request.error);
                onDone(false);
                yield break;
            }

            string body = request.downloadHandler.text;
            bool accepted = GameConfigService.ApplyJson(body);
            if (accepted)
            {
                GameConfigService.WriteCache(body);
            }

            onDone(accepted);
        }
    }
}
