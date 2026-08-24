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

    void Update()
    {
        if (BackPressedThisFrame()) HandleBackPressed();
    }

    // Android's hardware/gesture back button.
    //
    // This deliberately uses the LEGACY Input class, which is the only way to read it. The Input
    // System package has no Android keyboard layout at all — its Android plugin is gamepad, axis
    // and sensor devices only — and Android reports back as KEYCODE_BACK (4), which is a different
    // key from Escape (111). So `Keyboard.current.escapeKey` never fires for it, whatever the
    // forum answers say. Needs Project Settings > Player > Active Input Handling = Both.
    //
    // Also depends on androidPredictiveBackSupport staying OFF: with predictive back enabled
    // Android stops delivering KEYCODE_BACK entirely and expects an OnBackInvokedCallback instead.
    static bool BackPressedThisFrame()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    // One step back per press, checked innermost screen first. Home is the root, so back there
    // leaves the app — which is what Android expects of a root activity.
    void HandleBackPressed()
    {
        if (settingsRoot.activeSelf) { OnHomeNavClicked(); return; }
        if (winRoot.activeSelf) { OnBackFromWin(); return; }
        if (loseRoot.activeSelf) { OnBackFromLose(); return; }
        if (gameplayRoot.activeSelf) { OnBackToHomeClicked(); return; }
        if (homeRoot.activeSelf) { Application.Quit(); return; }

        // Splash is the only screen left, and it has nowhere to go back to — it advances by itself.
    }

    // A hardware back press is a BACK press wherever it lands, so it follows back_ad_on even on the
    // result cards, whose own Home buttons sit on the click counter instead.
    void OnBackFromWin()
    {
        AudioManager.Instance.StopJingle();
        InterstitialManager.ShowBackAd(() =>
        {
            winRoot.SetActive(false);
            GoHome();
        });
    }

    void OnBackFromLose()
    {
        AudioManager.Instance.StopJingle();
        InterstitialManager.ShowBackAd(() =>
        {
            loseRoot.SetActive(false);
            GoHome();
        });
    }

    public void OnPlayButtonClicked()
    {
        InterstitialManager.ShowClickAd(() =>
        {
            homeRoot.SetActive(false);
            StartLevel();
        });
    }

    public void OnBackToHomeClicked()
    {
        InterstitialManager.ShowBackAd(() =>
        {
            levelLoader.ClearLevel();   // abandon it, or an in-flight arrow exit still reports a win
            CameraController.Instance.FrameMenu();
            gameplayRoot.SetActive(false);
            homeRoot.SetActive(true);
        });
    }

    public void OnRestartLevelClicked()
    {
        InterstitialManager.ShowClickAd(() =>
        {
            livesManager.ResetLives();
            levelLoader.LoadLevel();

            // A second chance at the result-card ad if the one requested at level start never arrived.
            // Preload no-ops when an ad is already in hand, so this costs nothing in the normal case.
            if (AdsManager.Instance != null)
            {
                AdsManager.Instance.PreloadLevelEndNative();
            }
        });
    }

    public void OnHintButtonClicked()
    {
        if (GameplayHUDController.Instance != null)
        {
            GameplayHUDController.Instance.HandleHintRequested();
        }
        else
        {
            HintManager hintMgr = HintManager.Instance ?? FindAnyObjectByType<HintManager>();
            if (hintMgr != null)
            {
                hintMgr.RequestHint(
                    onHintUnlocked: () => levelLoader.PlayHint(),
                    onFailed: () => {}
                );
            }
            else
            {
                levelLoader.PlayHint();
            }
        }
    }

    // Home <-> Settings. The panels themselves are transparent, so what shows through either way
    // is the camera's solid colour with the backdrop arrows drifting behind it.
    // The home ad is not touched here: HomeScreenController owns it from its own OnEnable/OnDisable,
    // and showing it here as well requested a second ad every time home opened.
    public void OnSettingsNavClicked()
    {
        InterstitialManager.ShowClickAd(() =>
        {
            homeRoot.SetActive(false);
            settingsRoot.SetActive(true);
        });
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
    // The number the win card shows. Needed because beating a level advances the campaign right
    // here, so by the time the card is drawn CurrentLevelNumber() is already the NEXT level.
    public int LastCompletedLevelNumber { get; private set; }

    public void ShowWin()
    {
        // Banked immediately, not when the player leaves the card. Quitting the app while the win
        // card is still up used to throw the win away — AdvanceLevel() lived in the exit handlers,
        // so closing the game there reopened it on the level that had just been beaten.
        LastCompletedLevelNumber = levelLoader.CurrentLevelNumber();
        levelLoader.AdvanceLevel();

        // Background analytics ping. Deliberately not awaited — the win card shows regardless of
        // whether the network answers, or answers at all.
        LevelTracker.ReportLevelComplete(LastCompletedLevelNumber);

        AudioManager.Instance.PlayLevelComplete();

        CameraController.Instance.SuspendInput();
        gameplayRoot.SetActive(false);
        winRoot.SetActive(true);

        // Keep a preloaded interstitial warm while the player reads the card, so NEXT can
        // show immediately. Does nothing if one is already in hand.
        if (AdsManager.Instance != null && AdsManager.Instance.Interstitial != null)
        {
            AdsManager.Instance.Interstitial.Load();
        }
    }

    public void ShowLose()
    {
        AudioManager.Instance.PlayLevelFailed();

        CameraController.Instance.SuspendInput();
        gameplayRoot.SetActive(false);
        loseRoot.SetActive(true);

    }

    // Neither win exit advances anything — ShowWin() already did, the moment the level was beaten.
    // These only decide where the player goes next.
    public void OnNextLevelClicked()
    {
        AudioManager.Instance.StopJingle();
        int completedLevel = LastCompletedLevelNumber;
        InterstitialManager.ShowLevelAd(completedLevel, () =>
        {
            winRoot.SetActive(false);
            levelLoader.ClearLevel();
            StartLevel();
        });
    }

    public void OnWinHomeClicked()
    {
        AudioManager.Instance.StopJingle();
        InterstitialManager.ShowClickAd(() =>
        {
            winRoot.SetActive(false);
            GoHome();
        });
    }

    public void OnRetryLevelClicked()
    {
        AudioManager.Instance.StopJingle();
        InterstitialManager.ShowClickAd(() =>
        {
            loseRoot.SetActive(false);
            levelLoader.ClearLevel();
            StartLevel();
        });
    }

    public void OnLoseHomeClicked()
    {
        AudioManager.Instance.StopJingle();
        InterstitialManager.ShowClickAd(() =>
        {
            loseRoot.SetActive(false);
            GoHome();
        });
    }

    private void StartLevel()
    {
        gameplayRoot.SetActive(true);   // activate before loading, so the HUD is
        livesManager.ResetLives();      // subscribed when SetArrowCount fires
        levelLoader.LoadLevel();

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowGameplayAd();
            AdsManager.Instance.PreloadLevelEndNative();
        }
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
