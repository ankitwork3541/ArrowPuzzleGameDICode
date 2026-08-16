using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("UI Toolkit panel roots")]
    public GameObject homeRoot;
    public GameObject gameplayRoot;
    public GameObject settingsRoot;
    public GameObject winRoot;
    public GameObject loseRoot;

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
        StartLevel();
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

    // The board is deliberately NOT cleared here, and the camera is deliberately NOT reframed.
    // Both result cards are scrims over the level the player just played, and clearing it would
    // leave them dimming an empty screen. Teardown happens on the way out instead — see the four
    // exit methods below, which are the only routes off these screens.
    public void ShowWin()
    {
        CameraController.Instance.SuspendInput();
        gameplayRoot.SetActive(false);
        winRoot.SetActive(true);
    }

    public void ShowLose()
    {
        CameraController.Instance.SuspendInput();
        gameplayRoot.SetActive(false);
        loseRoot.SetActive(true);
    }

    // Beating a level advances the campaign whichever way the player leaves the win card, so both
    // win exits call AdvanceLevel() and neither of the lose exits does. Advancing is left until
    // here rather than done in ShowWin() so the card can still show the number of the level that
    // was just completed.
    public void OnNextLevelClicked()
    {
        winRoot.SetActive(false);
        levelLoader.ClearLevel();
        levelLoader.AdvanceLevel();
        StartLevel();
    }

    public void OnWinHomeClicked()
    {
        winRoot.SetActive(false);
        levelLoader.AdvanceLevel();
        GoHome();
    }

    public void OnRetryLevelClicked()
    {
        loseRoot.SetActive(false);
        levelLoader.ClearLevel();
        StartLevel();
    }

    public void OnLoseHomeClicked()
    {
        loseRoot.SetActive(false);
        GoHome();
    }

    private void StartLevel()
    {
        gameplayRoot.SetActive(true);   // activate before loading, so the HUD is
        livesManager.ResetLives();      // subscribed when SetArrowCount fires
        levelLoader.LoadLevel();
    }

    private void GoHome()
    {
        levelLoader.ClearLevel();
        CameraController.Instance.FrameMenu();
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
