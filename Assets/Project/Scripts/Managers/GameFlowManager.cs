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

    public static string GetDifficultyLabel(LevelData level)
    {
        int arrowCount = level.arrows.Count;
        if (arrowCount < 15) return "EASY";
        if (arrowCount < 50) return "MEDIUM";
        if (arrowCount < 150) return "HARD";
        return "HARDEST";
    }
}
