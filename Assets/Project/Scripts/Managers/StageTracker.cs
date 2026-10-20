using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Fire-and-forget "level completed" ping. Analytics only — nothing in the game reads the reply, and
// a failure is logged and dropped rather than retried. A lost ping is worth far less than a win
// screen that sits there waiting on the network.
public class StageTracker : MonoBehaviour
{
    // NOTE: this endpoint belongs to the PREVIOUS game and the form carries no game id, so wins from
    // this game would be counted as that game's. The backend owner has to confirm a new endpoint (or
    // that an extra field is safe) before a release build. Not changed here on purpose: nothing in
    // this repo can show how the old backend treats unknown fields.
    const string Url = "https://newvids.xyz/arrwgmln/gapp/completelevel";
    const int TimeoutSeconds = 5;

    static StageTracker instance;

    // Test hooks for the evidence run: how many pings were requested, and a switch that stops them
    // reaching the network (a check must never post a fake win to the real backend).
    public static int ReportCount;
    public static bool SuppressNetworkForTests;

    // Creates its own host object on first use, so this needs no scene wiring and no place in
    // anyone else's Awake order.
    public static void ReportLevelComplete(int level)
    {
        if (level <= 0)
        {
            return;
        }

        ReportCount++;
        if (SuppressNetworkForTests)
        {
            return;
        }

        if (instance == null)
        {
            instance = FindAnyObjectByType<StageTracker>();
        }

        if (instance == null)
        {
            GameObject host = new GameObject("StageTracker");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<StageTracker>();
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
                Debug.LogWarning("[StageTracker] Level " + level + " ping failed: " + request.error);
                yield break;
            }

            Debug.Log("[StageTracker] Level " + level + " reported.");
        }
    }
}
