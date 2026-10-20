using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Fetches the game config (ads, links, tuning, UI switches) from the config API and hands it to
// GameConfigService. The cache of the last good payload lives in the service too.
public class PromoConfigFetcher : MonoBehaviour
{
    // TODO(developer): the config API URL has not been supplied yet. While this is empty no request is
    // made and the game runs on the cached copy of the last good config, else on GameConfig.Defaults()
    // (ads off, no links). The endpoint must return the JSON in design/config/game-config.sample.json
    // (code 1, config_version 2). Fill it in here, in this one place.
    const string ConfigUrl = "";
    const int TimeoutSeconds = 5;

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
