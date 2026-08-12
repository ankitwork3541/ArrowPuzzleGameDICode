using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("UI Toolkit panel roots")]
    public GameObject homeRoot;
    public GameObject gameplayRoot;

    [Header("Background FX")]
    public GameObject spaceFx;   // Starfield + ShootingStars parent — Splash/Home only

    [Header("Systems")]
    public LevelLoader levelLoader;
    public LivesManager livesManager;

    void Awake()
    {
        // Unity's documented default frame rate on mobile is 30fps, to save battery — and mobile
        // platforms IGNORE QualitySettings.vSyncCount, so the project's vSyncCount:1 does nothing
        // here. Everything was rendering at 30fps on a 120Hz screen, which is what made panning
        // read as juddery no matter how the input maths was tuned.
        //
        // Ask for whatever the screen actually runs at rather than hardcoding 60: Android rounds a
        // requested rate DOWN to a divisor of the refresh rate, so a fixed 60 would cost half the
        // available smoothness on a 90Hz panel.
        int refresh = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        Application.targetFrameRate = Mathf.Max(60, refresh);   // never fall back to the 30fps default
    }

    public void OnPlayButtonClicked()
    {
        spaceFx.SetActive(false);       // the board needs a clean, undistracted background
        homeRoot.SetActive(false);
        gameplayRoot.SetActive(true);   // activate before loading, so the HUD is
        livesManager.ResetLives();      // subscribed when SetArrowCount fires
        levelLoader.LoadLevel();
    }

    public void OnBackToHomeClicked()
    {
        levelLoader.ClearLevel();   // abandon it, or an in-flight arrow exit still reports a win
        gameplayRoot.SetActive(false);
        homeRoot.SetActive(true);
        spaceFx.SetActive(true);
    }

    public void OnRestartLevelClicked()
    {
        livesManager.ResetLives();
        levelLoader.LoadLevel();
    }

    public void OnHintButtonClicked()
    {
        levelLoader.PlayHint();
    }

    // TODO: Win panel is not rebuilt in UI Toolkit yet — for now the flow advances
    // a level and returns to Home so the loop stays playable.
    public void ShowWin()
    {
        Debug.Log("LEVEL WON (Win panel not rebuilt in UI Toolkit yet)");
        levelLoader.ClearLevel();
        levelLoader.AdvanceLevel();   // advance only — the level loads when Play is pressed
        gameplayRoot.SetActive(false);
        homeRoot.SetActive(true);
        spaceFx.SetActive(true);
    }

    // TODO: Lose panel is not rebuilt in UI Toolkit yet — same temporary behaviour.
    public void ShowLose()
    {
        Debug.Log("LEVEL LOST (Lose panel not rebuilt in UI Toolkit yet)");
        levelLoader.ClearLevel();
        gameplayRoot.SetActive(false);
        homeRoot.SetActive(true);
        spaceFx.SetActive(true);
    }

    // Reads what the generator measured. It used to bucket by arrow count, which said nothing
    // about how hard a board is to read — the real difficulty is finding a legal move, so it is
    // measured by simulating the solve. See LevelDifficulty.
    public static string GetDifficultyLabel(LevelData level)
    {
        return level.difficulty.ToString().ToUpper();
    }
}
