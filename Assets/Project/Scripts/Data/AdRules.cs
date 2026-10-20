// The ad timing rules from the config, as pure functions so every one of them is unit-tested. The
// interstitial manager owns the counters (clicks, timer, shown-this-session) and asks these whether
// an ad is due.
public static class AdRules
{
    // A placement is live once the player has cleared `show_after_level` levels.
    public static bool PlacementActive(AdPlacement placement, int highestLevelCompleted)
    {
        return placement != null && highestLevelCompleted >= placement.show_after_level;
    }

    // No interstitial at all until the player has cleared `skip_first_levels` levels.
    public static bool InterstitialsUnlocked(InterstitialRules rules, int highestLevelCompleted)
    {
        return highestLevelCompleted >= rules.skip_first_levels;
    }

    // 0 means no cap.
    public static bool SessionCapReached(InterstitialRules rules, int shownThisSession)
    {
        return rules.max_per_session > 0 && shownThisSession >= rules.max_per_session;
    }

    // Both click triggers off means there is no click-based interstitial at all, not even a counter
    // worth keeping.
    public static bool ClickAdsOff(InterstitialRules rules)
    {
        return rules.min_click_count <= 0 && rules.min_seconds_between <= 0;
    }

    // EITHER gate is enough. A 0 switches that one trigger off rather than satisfying it instantly,
    // otherwise min_seconds_between 0 would put an ad on every button in the game.
    public static bool TimeGateMet(InterstitialRules rules, float secondsSinceLastAd)
    {
        return rules.min_seconds_between > 0 && secondsSinceLastAd >= rules.min_seconds_between;
    }

    public static bool CountGateMet(InterstitialRules rules, int clicks)
    {
        return rules.min_click_count > 0 && clicks >= rules.min_click_count;
    }

    // Win NEXT only: every Nth cleared level, with no click or time gate. 0 turns it off.
    public static bool LevelAdDue(InterstitialRules rules, int completedLevel)
    {
        return rules.every_n_levels_on_next > 0
               && completedLevel > 0
               && completedLevel % rules.every_n_levels_on_next == 0;
    }
}
