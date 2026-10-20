using UnityEngine;

public class FlowDirector : MonoBehaviour
{
    [Header("UI Toolkit panel roots")]
    public GameObject homeRoot;
    public GameObject gameplayRoot;
    public GameObject settingsRoot;
    public GameObject winRoot;
    public GameObject loseRoot;

    // The Starfield / ShootingStars particle backdrop used to be toggled from here, on for the
    // menus and off for gameplay. It has been replaced by LanternBackdrop, which hangs off the
    // camera, costs nothing to leave running, and is quiet enough to stay up on every screen —
    // so there is no longer anything for the flow to switch.

    [Header("Systems")]
    public StageLoader levelLoader;
    public EmberManager livesManager;

    // The target frame rate used to be set here. It now lives in OptionsManager, because the
    // player can turn it off — two Awake()s both writing Application.targetFrameRate would race,
    // and Unity does not define which one wins.

    // Found lazily in HandleBackPressed; a destroyed reference reads as null and is looked up again.
    private HallScreenController hall;

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
        if (homeRoot.activeSelf)
        {
            // An open level map is a screen of its own: back closes it rather than leaving the app.
            // Looked up once, on the first Back press that needs it, then kept.
            if (hall == null) hall = FindAnyObjectByType<HallScreenController>();
            if (hall != null && hall.TryCloseMap()) return;

            Application.Quit();
            return;
        }

        // Splash is the only screen left, and it has nowhere to go back to — it advances by itself.
    }

    // A hardware back press is a BACK press wherever it lands, so it follows back_ad_on even on the
    // result cards, whose own Home buttons sit on the click counter instead.
    void OnBackFromWin()
    {
        SoundDirector.Instance.StopJingle();
        PromoInterstitialManager.ShowBackAd(() =>
        {
            winRoot.SetActive(false);
            GoHome();
        });
    }

    void OnBackFromLose()
    {
        SoundDirector.Instance.StopJingle();
        PromoInterstitialManager.ShowBackAd(() =>
        {
            loseRoot.SetActive(false);
            GoHome();
        });
    }

    public void OnPlayButtonClicked()
    {
        PromoInterstitialManager.ShowClickAd(() =>
        {
            homeRoot.SetActive(false);
            StartLevel();
        });
    }

    // A level tapped on the Home level map. Same path as PLAY, after pointing the campaign at it.
    public void OnLevelSelected(int levelNumber)
    {
        PromoInterstitialManager.ShowClickAd(() =>
        {
            levelLoader.SelectLevel(levelNumber);
            homeRoot.SetActive(false);
            StartLevel();
        });
    }

    public void OnBackToHomeClicked()
    {
        PromoInterstitialManager.ShowBackAd(() =>
        {
            levelLoader.ClearLevel();   // abandon it, or an in-flight arrow exit still reports a win
            levelLoader.PointAtFrontier();   // a replay is only a visit; Home shows the frontier again
            BoardCamera.Instance.FrameMenu();
            gameplayRoot.SetActive(false);
            homeRoot.SetActive(true);
        });
    }

    public void OnRestartLevelClicked()
    {
        PromoInterstitialManager.ShowClickAd(() =>
        {
            livesManager.ResetLives();
            levelLoader.LoadLevel();

            // A second chance at the result-card ad if the one requested at level start never arrived.
            // Preload no-ops when an ad is already in hand, so this costs nothing in the normal case.
            if (PromoManager.Instance != null)
            {
                PromoManager.Instance.PreloadLevelEndNative();
            }
        });
    }

    public void OnHintButtonClicked()
    {
        if (PlayScreenController.Instance != null)
        {
            PlayScreenController.Instance.HandleHintRequested();
        }
        else
        {
            ClueManager hintMgr = ClueManager.Instance ?? FindAnyObjectByType<ClueManager>();
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
    // The home ad is not touched here: HallScreenController owns it from its own OnEnable/OnDisable,
    // and showing it here as well requested a second ad every time home opened.
    public void OnSettingsNavClicked()
    {
        PromoInterstitialManager.ShowClickAd(() =>
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

        // Decided before AdvanceLevel() banks the win: afterwards every level up to this one reads
        // as beaten. A replay from the map is not a new completion and must not be reported as one.
        bool newProgress = FrontierRule.IsFrontierCompletion(
            ProgressStore.Data.highestLevelCompleted, LastCompletedLevelNumber);
        levelLoader.AdvanceLevel();

        // Background analytics ping. Deliberately not awaited — the win card shows regardless of
        // whether the network answers, or answers at all.
        if (newProgress) StageTracker.ReportLevelComplete(LastCompletedLevelNumber);

        SoundDirector.Instance.PlayLevelComplete();

        BoardCamera.Instance.SuspendInput();
        gameplayRoot.SetActive(false);
        winRoot.SetActive(true);

        // Keep a preloaded interstitial warm while the player reads the card, so NEXT can
        // show immediately. Does nothing if one is already in hand.
        if (PromoManager.Instance != null && PromoManager.Instance.Interstitial != null)
        {
            PromoManager.Instance.Interstitial.Load();
        }
    }

    public void ShowLose()
    {
        SoundDirector.Instance.PlayLevelFailed();

        BoardCamera.Instance.SuspendInput();
        gameplayRoot.SetActive(false);
        loseRoot.SetActive(true);

    }

    // Neither win exit advances anything — ShowWin() already did, the moment the level was beaten.
    // These only decide where the player goes next.
    public void OnNextLevelClicked()
    {
        SoundDirector.Instance.StopJingle();
        int completedLevel = LastCompletedLevelNumber;
        PromoInterstitialManager.ShowLevelAd(completedLevel, () =>
        {
            winRoot.SetActive(false);
            levelLoader.ClearLevel();
            StartLevel();
        });
    }

    public void OnWinHomeClicked()
    {
        SoundDirector.Instance.StopJingle();
        PromoInterstitialManager.ShowClickAd(() =>
        {
            winRoot.SetActive(false);
            GoHome();
        });
    }

    public void OnRetryLevelClicked()
    {
        SoundDirector.Instance.StopJingle();
        PromoInterstitialManager.ShowClickAd(() =>
        {
            loseRoot.SetActive(false);
            levelLoader.ClearLevel();
            StartLevel();
        });
    }

    public void OnLoseHomeClicked()
    {
        SoundDirector.Instance.StopJingle();
        PromoInterstitialManager.ShowClickAd(() =>
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

        if (PromoManager.Instance != null)
        {
            PromoManager.Instance.ShowGameplayAd();
            PromoManager.Instance.PreloadLevelEndNative();
        }
    }

    private void GoHome()
    {
        levelLoader.ClearLevel();
        levelLoader.PointAtFrontier();   // a replay is only a visit; Home shows the frontier again
        BoardCamera.Instance.FrameMenu();
        homeRoot.SetActive(true);
    }

    // Reads what the generator measured. It used to bucket by arrow count, which said nothing
    // about how hard a board is to read — the real difficulty is finding a legal move, so it is
    // measured by simulating the solve. See StageRating.
    public static string GetDifficultyLabel(StageData level)
    {
        return level.difficulty.ToString().ToUpper();
    }
}
