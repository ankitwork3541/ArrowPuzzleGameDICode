// Mirrors the JSON returned by the ads config API. Field names match the JSON keys exactly and
// the 1/0 flags stay ints, because JsonUtility maps by name and will not coerce 1 into a bool.
[System.Serializable]
public class AdRemoteConfig
{
    public int code;
    public string msg;

    public string banner_adunit_id;
    public string native_adunit_id;
    public string preload_native_adunit_id;
    public string interstitial_adunit_id;
    public string rewarded_adunit_id;

    public int ad_enable;
    public int rewarded_ad_enable;
    public int back_ad_on;

    public int ad_click_time_sec;
    public int refresh_native_sec;
    public int ad_click_count;
    public int ad_level_count;

    // Meaning not confirmed with the backend owner yet — parsed and stored, deliberately unused.
    public int main_screen;
    public int play_screen;
    public int success_screen;
}
