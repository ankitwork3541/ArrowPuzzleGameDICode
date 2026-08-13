using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("UI Toolkit panel roots")]
    public GameObject homeRoot;
    public GameObject gameplayRoot;
    public GameObject settingsRoot;

    [Header("Background FX")]
    public GameObject spaceFx;   // Starfield + ShootingStars parent — Splash/Home only

    [Header("Systems")]
    public LevelLoader levelLoader;
    public LivesManager livesManager;

    // The target frame rate used to be set here. It now lives in SettingsManager, because the
    // player can turn it off — two Awake()s both writing Application.targetFrameRate would race,
    // and Unity does not define which one wins.

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

    // Home <-> Settings. Settings kills the starfield the same way gameplay does: it is a dense
    // reading screen, and the drifting particles behind the text are a distraction. Turning the FX
    // off rather than covering it also stops the particles simulating while they cannot be seen —
    // the panel itself stays transparent, so what shows through is the camera's solid colour.
    public void OnSettingsNavClicked()
    {
        spaceFx.SetActive(false);
        homeRoot.SetActive(false);
        settingsRoot.SetActive(true);
    }

    public void OnHomeNavClicked()
    {
        settingsRoot.SetActive(false);
        homeRoot.SetActive(true);
        spaceFx.SetActive(true);
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
