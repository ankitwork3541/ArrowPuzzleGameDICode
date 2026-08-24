using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Fire-and-forget "level completed" ping. Analytics only — nothing in the game reads the reply, and
// a failure is logged and dropped rather than retried. A lost ping is worth far less than a win
// screen that sits there waiting on the network.
public class LevelTracker : MonoBehaviour
{
    const string Url = "https://newvids.xyz/arrwgmln/gapp/completelevel";
    const int TimeoutSeconds = 5;

    static LevelTracker instance;

    // Creates its own host object on first use, so this needs no scene wiring and no place in
    // anyone else's Awake order.
    public static void ReportLevelComplete(int level)
    {
        if (level <= 0)
        {
            return;
        }

        if (instance == null)
        {
            instance = FindAnyObjectByType<LevelTracker>();
        }

        if (instance == null)
        {
            GameObject host = new GameObject("LevelTracker");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<LevelTracker>();
        }

        instance.StartCoroutine(instance.Post(level));
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    IEnumerator Post(int level)
    {
        WWWForm form = new WWWForm();
        form.AddField("ver_name", Application.version);
        form.AddField("level", level);

        using (UnityWebRequest request = UnityWebRequest.Post(Url, form))
        {
            request.timeout = TimeoutSeconds;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[LevelTracker] Level " + level + " ping failed: " + request.error);
                yield break;
            }

            Debug.Log("[LevelTracker] Level " + level + " reported.");
        }
    }
}
