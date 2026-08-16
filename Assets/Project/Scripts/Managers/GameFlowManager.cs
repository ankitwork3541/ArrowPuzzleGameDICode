using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("UI Toolkit panel roots")]
    public GameObject homeRoot;
    public GameObject gameplayRoot;
    public GameObject settingsRoot;

    // The Starfield / ShootingStars particle backdrop used to be toggled from here, on for the
    // menus and off for gameplay. It has been replaced by BackdropArrows, which hangs off the
    // camera, costs nothing to leave running, and is quiet enough to stay up on every screen —
    // so there is no longer anything for the flow to switch.

    [Header("Systems")]
    public LevelLoader levelLoader;
    public LivesManager livesManager;

    // The target frame rate used to be set here. It now lives in SettingsManager, because the
    // player can turn it off — two Awake()s both writing Application.targetFrameRate would race,
    // and Unity does not define which one wins.

    public void OnPlayButtonClicked()
    {
        homeRoot.SetActive(false);
        gameplayRoot.SetActive(true);   // activate before loading, so the HUD is
        livesManager.ResetLives();      // subscribed when SetArrowCount fires
        levelLoader.LoadLevel();
    }

    public void OnBackToHomeClicked()
    {
        levelLoader.ClearLevel();   // abandon it, or an in-flight arrow exit still reports a win
        CameraController.Instance.FrameMenu();
        gameplayRoot.SetActive(false);
        homeRoot.SetActive(true);
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

    // Home <-> Settings. The panels themselves are transparent, so what shows through either way
    // is the camera's solid colour with the backdrop arrows drifting behind it.
    public void OnSettingsNavClicked()
    {
        homeRoot.SetActive(false);
        settingsRoot.SetActive(true);
    }

    public void OnHomeNavClicked()
    {
        settingsRoot.SetActive(false);
        homeRoot.SetActive(true);
    }

    // TODO: Win panel is not rebuilt in UI Toolkit yet — for now the flow advances
    // a level and returns to Home so the loop stays playable.
    public void ShowWin()
    {
        Debug.Log("LEVEL WON (Win panel not rebuilt in UI Toolkit yet)");
        levelLoader.ClearLevel();
        levelLoader.AdvanceLevel();   // advance only — the level loads when Play is pressed
        CameraController.Instance.FrameMenu();
        gameplayRoot.SetActive(false);
        homeRoot.SetActive(true);
    }

    // TODO: Lose panel is not rebuilt in UI Toolkit yet — same temporary behaviour.
    public void ShowLose()
    {
        Debug.Log("LEVEL LOST (Lose panel not rebuilt in UI Toolkit yet)");
        levelLoader.ClearLevel();
        CameraController.Instance.FrameMenu();
        gameplayRoot.SetActive(false);
        homeRoot.SetActive(true);
    }

    // Reads what the generator measured. It used to bucket by arrow count, which said nothing
    // about how hard a board is to read — the real difficulty is finding a legal move, so it is
    // measured by simulating the solve. See LevelDifficulty.
    public static string GetDifficultyLabel(LevelData level)
    {
        return level.difficulty.ToString().ToUpper();
    }
}
