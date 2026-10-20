#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

// Evidence screenshots for the Visual/Feel and UI stories. The Editor is not the active application
// when driven from the command line, so UI Toolkit never paints there; a built development player
// is. Run it as:  Game.exe -screen-width 720 -screen-height 1280 -screen-fullscreen 0 --evidence <dir>
// Add `--evidence-small` for the short run used on a 360x640 window. Without `--evidence` it does
// nothing, and it is compiled out of release builds entirely.
//
// The win/lose cards are opened by switching the panel roots, not through ShowWin(): ShowWin banks
// the level and pings the analytics endpoint, and a screenshot run must do neither.
//
// Behaviour checks print `[EvidenceCheck] <name>: PASS|FAIL ...` lines to the player log.
public class ShotCapture : MonoBehaviour
{
    // Level 6 is where the baked campaign introduces its first padlock, so it has locks to open.
    private const int DemoLevelNumber = 6;

    private string dir;
    private FlowDirector flow;
    private StageLoader loader;

    // What the save file held before the run, so the seeded demo progress below is never left behind:
    // the player shares persistentDataPath with the Editor, and the game writes the save on every win
    // and level pick. null means there was no file. Put back in OnApplicationQuit.
    private static string originalSaveJson;
    private static bool originalSaveKept;

    private static string SavePath => Path.Combine(Application.persistentDataPath, ProgressStore.FileName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void SeedSave()
    {
        if (!TryGetDir(out _)) return;

        // Read the file before ProgressStore.Data is touched: loading it can create the file by
        // migrating an older-named save.
        originalSaveJson = File.Exists(SavePath) ? File.ReadAllText(SavePath) : null;
        originalSaveKept = true;

        // A fresh save would show an all-locked map and a lock-free level 1. Level 6 is the first
        // level with padlocks, and 5 beaten levels gives the map done, current and locked nodes.
        ProgressStore.Data.highestLevelCompleted = DemoLevelNumber - 1;
        ProgressStore.Data.currentLevelIndex = DemoLevelNumber - 1;

        // The first-lock coach overlay would sit over every level-6 frame, so every run except the dedicated
        // lock run (--evidence-lock) starts with it already seen.
        ProgressStore.Data.lockTutorialSeen = !HasFlag("--evidence-lock");

        // A game config to run on, read from a file instead of the API, so the config-driven screens
        // (hidden options rows, notices, maintenance, force update, lives) can be shot on demand.
        string configPath = GetArg("--evidence-config");
        if (configPath != null)
        {
            bool applied = File.Exists(configPath) && GameConfigService.ApplyJson(File.ReadAllText(configPath));
            Report("evidence config loaded", applied, configPath);
        }
    }

    private static string GetArg(string flag)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == flag) return args[i + 1];
        return null;
    }

    private void OnApplicationQuit()
    {
        if (!originalSaveKept) return;
        originalSaveKept = false;

        try
        {
            if (originalSaveJson != null) File.WriteAllText(SavePath, originalSaveJson);
            else if (File.Exists(SavePath)) File.Delete(SavePath);   // the run created it

            bool restored = originalSaveJson != null
                ? File.Exists(SavePath) && File.ReadAllText(SavePath) == originalSaveJson
                : !File.Exists(SavePath);
            Report("save file restored after the run", restored,
                originalSaveJson != null ? "contents equal the pre-run file" : "no file existed before, none left");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ShotCapture] could not restore the save file: {e.Message}");
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (!TryGetDir(out string path)) return;

        GameObject host = new GameObject("ShotCapture");
        DontDestroyOnLoad(host);
        host.AddComponent<ShotCapture>().dir = path;
    }

    private static bool TryGetDir(out string path)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--evidence")
            {
                path = args[i + 1];
                return true;
            }
        }

        path = null;
        return false;
    }

    private static bool HasFlag(string flag)
    {
        foreach (string arg in System.Environment.GetCommandLineArgs())
            if (arg == flag) return true;
        return false;
    }

    private IEnumerator Start()
    {
        Directory.CreateDirectory(dir);
        flow = FindAnyObjectByType<FlowDirector>();
        loader = flow.levelLoader;

        if (HasFlag("--evidence-small"))
        {
            yield return SmallRun();
            Debug.Log("[ShotCapture] done");
            Application.Quit();
            yield break;
        }

        if (HasFlag("--evidence-lock"))
        {
            yield return LockRun(GetArg("--evidence-prefix") ?? "lock");
            Debug.Log("[ShotCapture] done");
            Application.Quit();
            yield break;
        }

        if (HasFlag("--evidence-config-run"))
        {
            yield return ConfigRun(GetArg("--evidence-prefix") ?? "cfg");
            Debug.Log("[ShotCapture] done");
            Application.Quit();
            yield break;
        }

        // Splash advances by itself after two seconds; catch it part way through the bar.
        yield return new WaitForSecondsRealtime(1.2f);
        yield return Shoot("01-splash");

        yield return new WaitUntil(() => flow.homeRoot.activeSelf);
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot("02-home");
        yield return ShootCrop("14-gear-home", new Rect(0.70f, 0.82f, 0.28f, 0.15f), 3);

        HallScreenController home = FindAnyObjectByType<HallScreenController>();
        MethodInfo open = typeof(HallScreenController).GetMethod("OpenLevelMap",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (home != null && open != null)
        {
            open.Invoke(home, null);
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Shoot("03-level-map");
            yield return ShootCrop("18-locked-node-opaque", new Rect(0.05f, 0.45f, 0.9f, 0.45f), 2);
            home.TryCloseMap();
            yield return new WaitForSecondsRealtime(0.5f);

            // A cold open far down the campaign: the map must land on the current level, not the top.
            ProgressStore.Data.highestLevelCompleted = 900;
            open.Invoke(home, null);
            yield return new WaitForSecondsRealtime(2.0f);
            yield return Shoot("17-map-cold-start");
            home.TryCloseMap();
            yield return new WaitForSecondsRealtime(0.5f);
            ProgressStore.Data.highestLevelCompleted = DemoLevelNumber - 1;
        }

        flow.OnSettingsNavClicked();
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot("04-settings");
        yield return CheckReduceMotion();
        yield return Shoot("16-options-reduce-motion");
        OptionsManager.Instance.SetReduceMotion(false);
        flow.OnHomeNavClicked();
        yield return new WaitForSecondsRealtime(0.5f);

        flow.OnPlayButtonClicked();
        yield return new WaitUntil(() => flow.gameplayRoot.activeSelf);
        yield return new WaitForSecondsRealtime(7f);
        yield return Shoot("05-hud-board");
        yield return ShootCrop("15-pearl-zoom", new Rect(0.18f, 0.30f, 0.62f, 0.40f), 2);

        // The ad-loading card is a global overlay on its own panel, so it can be shown over the board.
        WaitSheet.Show();
        yield return new WaitForSecondsRealtime(1.2f);
        if (WaitSheet.Showing)
            yield return Shoot("11-ad-loading-modal");
        else
            Debug.LogWarning("[ShotCapture] WaitSheet did not show");
        WaitSheet.Hide();
        yield return new WaitForSecondsRealtime(0.6f);

        yield return CheckLockedTapCostsNoLife();
        yield return CheckMapSelection();
        yield return CheckSavePersistence();

        int livesBefore = EmberManager.Instance.lives;
        yield return Bump();
        yield return new WaitForSecondsRealtime(2.5f);
        int lost = livesBefore - EmberManager.Instance.lives;
        Report("bump costs exactly one life", lost == 1, $"lives {livesBefore} -> {EmberManager.Instance.lives}");

        // Unlock before the hint: the hint refocuses the camera on one arrow, and that zoom pushes
        // the padlocks to the board's edge, where the swell would be hidden under the HUD.
        yield return Unlock();
        yield return new WaitForSecondsRealtime(1.5f);

        loader.PlayHint();
        yield return new WaitForSecondsRealtime(0.6f);
        yield return Shoot("07-hint");
        yield return new WaitForSecondsRealtime(1.5f);

        // The cards sit over the board, so just swap the panel roots (see the note at the top).
        typeof(FlowDirector).GetProperty("LastCompletedLevelNumber")
            .SetValue(flow, DemoLevelNumber);
        flow.gameplayRoot.SetActive(false);
        flow.winRoot.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot("09-win");
        flow.winRoot.SetActive(false);
        flow.loseRoot.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot("10-lose");
        flow.loseRoot.SetActive(false);

        yield return FourDigitTitle();
        yield return CheckFrontierRule();

        Debug.Log("[ShotCapture] done");
        Application.Quit();
    }

    // Replaying a beaten level must leave PLAY on the frontier; beating the frontier itself advances
    // by one.
    private IEnumerator CheckFrontierRule()
    {
        ProgressStore.Data.highestLevelCompleted = 10;
        loader.SelectLevel(3);
        flow.ShowWin();
        yield return null;
        Report("frontier: a replay win leaves PLAY on the frontier",
            loader.CurrentLevelNumber() == 11 && ProgressStore.Data.highestLevelCompleted == 10,
            $"current {loader.CurrentLevelNumber()}, highest {ProgressStore.Data.highestLevelCompleted} (want 11 and 10)");

        loader.SelectLevel(11);
        flow.ShowWin();
        yield return null;
        Report("frontier: beating the frontier advances to the next level",
            loader.CurrentLevelNumber() == 12 && ProgressStore.Data.highestLevelCompleted == 11,
            $"current {loader.CurrentLevelNumber()}, highest {ProgressStore.Data.highestLevelCompleted} (want 12 and 11)");

        flow.winRoot.SetActive(false);
        ProgressStore.Data.highestLevelCompleted = DemoLevelNumber - 1;
        loader.SelectLevel(DemoLevelNumber);
    }

    // Reduce Motion ON must freeze the backdrop drift; OFF must let it move. Measured on the first
    // backdrop dragon over 1.5 s each way. The switch is put back OFF at the end.
    private IEnumerator CheckReduceMotion()
    {
        OptionsManager options = OptionsManager.Instance;
        LanternBackdrop backdrop = FindAnyObjectByType<LanternBackdrop>();
        if (options == null || backdrop == null)
        {
            Report("reduce motion: backdrop", false, "missing OptionsManager or LanternBackdrop");
            yield break;
        }

        Transform probe = null;
        foreach (Transform child in backdrop.GetComponentsInChildren<Transform>())
        {
            if (child != backdrop.transform) { probe = child; break; }
        }

        if (probe == null)
        {
            Report("reduce motion: backdrop", false, "backdrop has no child sprites");
            yield break;
        }

        options.SetReduceMotion(true);
        yield return null;
        Vector3 onA = probe.localPosition;
        yield return new WaitForSecondsRealtime(1.5f);
        Vector3 onB = probe.localPosition;
        Report("reduce motion ON freezes the backdrop", (onA - onB).sqrMagnitude < 1e-10f,
            $"moved {(onA - onB).magnitude:E2}");

        options.SetReduceMotion(false);
        yield return null;
        Vector3 offA = probe.localPosition;
        yield return new WaitForSecondsRealtime(1.5f);
        Vector3 offB = probe.localPosition;
        Report("reduce motion OFF lets the backdrop drift", (offA - offB).sqrMagnitude > 1e-10f,
            $"moved {(offA - offB).magnitude:E2}");

        options.SetReduceMotion(true);
        yield return new WaitForSecondsRealtime(0.5f);
    }

    // The config run: Home and Settings (and the board, when the game is not blocked by a notice) on
    // whatever config --evidence-config loaded. One run per config file, frames named by the prefix.
    private IEnumerator ConfigRun(string prefix)
    {
        yield return new WaitForSecondsRealtime(1.2f);
        yield return new WaitUntil(() => flow.homeRoot.activeSelf);
        yield return new WaitForSecondsRealtime(2f);

        GameConfig config = GameConfigService.Current;
        Report(prefix + ": a config payload is applied", GameConfigService.HasPayload, "HasPayload");
        yield return Shoot(prefix + "-home");

        if (config.app.maintenance_mode || config.app.force_update)
            yield break;

        flow.OnSettingsNavClicked();
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot(prefix + "-options");
        flow.OnHomeNavClicked();
        yield return new WaitForSecondsRealtime(0.5f);

        flow.OnPlayButtonClicked();
        yield return new WaitUntil(() => flow.gameplayRoot.activeSelf);
        yield return new WaitForSecondsRealtime(6f);
        Report(prefix + ": game.lives.max reaches the lives manager",
            EmberManager.Instance.maxLives == config.game.lives.max,
            $"lives {EmberManager.Instance.lives}/{EmberManager.Instance.maxLives}, config {config.game.lives.max}");
        yield return Shoot(prefix + "-hud");
    }

    // The lock UX run (--evidence-lock): the one-time coach overlay on the first padlock level (shown by the
    // game itself, because this run does not mark it seen), the padlock digit at 1, 4 and 12, and the
    // "Free N more" reminder after a tap on a locked dragon.
    private IEnumerator LockRun(string prefix)
    {
        yield return new WaitForSecondsRealtime(1.2f);
        yield return new WaitUntil(() => flow.homeRoot.activeSelf);
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot(prefix + "-0-home");

        int first = loader.FirstLockLevelIndex();
        Report("lock tutorial: first lock level is derived from the baked data", first >= 0 && first == DemoLevelNumber - 1,
            $"first lock level index {first} (level {first + 1})");

        flow.OnPlayButtonClicked();
        yield return new WaitUntil(() => flow.gameplayRoot.activeSelf);
        yield return new WaitForSecondsRealtime(7f);

        PlayScreenController play = PlayScreenController.Instance;
        Vector2 middle = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Report("lock tutorial: shows by itself on the first lock level and keeps taps off the board",
            play != null && play.IsPointerOverUI(middle), "overlay up, middle of the screen counts as UI");
        yield return Shoot(prefix + "-1-coach");

        // Reduce Motion: same overlay, nothing cycles or pulses.
        OptionsManager options = OptionsManager.Instance;
        if (options != null && play != null)
        {
            options.SetReduceMotion(true);
            play.HideLockCoachForEvidence();
            play.ShowLockCoachForEvidence();
            yield return new WaitForSecondsRealtime(1.0f);
            yield return Shoot(prefix + "-2-coach-reduce-motion");
            options.SetReduceMotion(false);
        }

        // GOT IT saves the flag and the overlay must not come back on a re-entry of the same level.
        if (play != null) play.DismissLockCoachForEvidence();
        Report("lock tutorial: GOT IT saves the seen flag", ProgressStore.Data.lockTutorialSeen, "lockTutorialSeen is set");
        Report("lock tutorial: board taps work again after GOT IT", play != null && !play.IsPointerOverUI(middle),
            "middle of the screen is board again");

        loader.LoadLevel();
        yield return new WaitForSecondsRealtime(7f);
        Report("lock tutorial: does not show a second time", play != null && !play.IsPointerOverUI(middle),
            "re-entered the same level, no overlay");
        yield return Shoot(prefix + "-3-board-after");

        // The digit on the padlock at 1, 4 and 12, then the reminder after a tap.
        DragonController locked = loader.FirstLockedDragon();
        if (locked == null)
        {
            Report("lock digit close-ups", false, "no padlocked dragon on this level");
            yield break;
        }

        foreach (int count in new[] { 1, 4, 12 })
        {
            locked.SetLock(count);
            yield return new WaitForSecondsRealtime(0.6f);
            yield return ShootBadgeCrop(locked, prefix + "-digit-" + count);
        }

        locked.SetLock(3);
        locked.HandleTap();
        yield return new WaitForSecondsRealtime(0.25f);
        yield return ShootBadgeCrop(locked, prefix + "-hint-free-n-more");
    }

    // A close-up of the padlock badge: a square window around it in screen space, enlarged.
    private IEnumerator ShootBadgeCrop(DragonController dragon, string name)
    {
        Vector3 world;
        float size;
        Camera cam = Camera.main;
        if (cam == null || !dragon.TryGetLockBadge(out world, out size))
            yield break;

        Vector3 screen = cam.WorldToScreenPoint(world);
        float half = Mathf.Abs(cam.WorldToScreenPoint(world + cam.transform.right * size).x - screen.x) * 1.1f;
        float x = Mathf.Clamp01((screen.x - half) / Screen.width);
        float y = Mathf.Clamp01((screen.y - half * 1.5f) / Screen.height);
        float w = Mathf.Min(1f - x, 2f * half / Screen.width);
        float h = Mathf.Min(1f - y, 3f * half / Screen.height);
        yield return ShootCrop(name, new Rect(x, y, w, h), 3);
    }

    // The 360x640 run: the four screens most likely to overflow on a narrow phone.
    private IEnumerator SmallRun()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        yield return Shoot("13-small-1-splash");

        yield return new WaitUntil(() => flow.homeRoot.activeSelf);
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot("13-small-2-home");

        flow.OnPlayButtonClicked();
        yield return new WaitUntil(() => flow.gameplayRoot.activeSelf);
        yield return new WaitForSecondsRealtime(7f);
        yield return Shoot("13-small-3-hud");

        typeof(FlowDirector).GetProperty("LastCompletedLevelNumber")
            .SetValue(flow, DemoLevelNumber);
        flow.gameplayRoot.SetActive(false);
        flow.winRoot.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot("13-small-4-win");
    }

    // Level 1500 puts a four-digit number in the HUD title. The save is put back afterwards.
    private IEnumerator FourDigitTitle()
    {
        ProgressStore.Data.highestLevelCompleted = 1499;
        loader.SelectLevel(1500);
        flow.gameplayRoot.SetActive(true);
        loader.LoadLevel();
        yield return new WaitForSecondsRealtime(8f);
        yield return Shoot("12-hud-level-1500");

        ProgressStore.Data.highestLevelCompleted = DemoLevelNumber - 1;
        loader.SelectLevel(DemoLevelNumber);
    }

    // A tap on a padlocked arrow shakes the padlock and must cost no life.
    private IEnumerator CheckLockedTapCostsNoLife()
    {
        DragonController locked = null;
        foreach (DragonController dragon in loader.SpawnedDragons)
        {
            if (dragon != null && dragon.IsLocked)
            {
                locked = dragon;
                break;
            }
        }

        if (locked == null)
        {
            Report("locked tap costs no life", false, "no padlocked arrow on this level");
            yield break;
        }

        int before = EmberManager.Instance.lives;
        locked.HandleTap();
        yield return new WaitForSecondsRealtime(1.0f);
        Report("locked tap costs no life",
            EmberManager.Instance.lives == before && locked.IsLocked,
            $"lives {before} -> {EmberManager.Instance.lives}, still locked={locked.IsLocked}");
    }

    // The map path is StageLoader.SelectLevel: a beaten or current level is accepted, a level past
    // the frontier is refused (clamped to the frontier).
    private IEnumerator CheckMapSelection()
    {
        int frontier = ProgressStore.Data.highestLevelCompleted + 1;

        loader.SelectLevel(3);
        Report("map: completed level 3 selectable", loader.CurrentLevelNumber() == 3,
            $"selected {loader.CurrentLevelNumber()}");

        loader.SelectLevel(frontier);
        Report("map: current level selectable", loader.CurrentLevelNumber() == frontier,
            $"selected {loader.CurrentLevelNumber()} (frontier {frontier})");

        loader.SelectLevel(frontier + 40);
        Report("map: locked level refused", loader.CurrentLevelNumber() == frontier,
            $"asked {frontier + 40}, got {loader.CurrentLevelNumber()} (frontier {frontier})");

        loader.SelectLevel(DemoLevelNumber);
        yield break;
    }

    // Set progress, write it, read the file back from disk and compare.
    private IEnumerator CheckSavePersistence()
    {
        int originalHighest = ProgressStore.Data.highestLevelCompleted;
        ProgressStore.Data.highestLevelCompleted = 42;
        ProgressStore.Save();

        string path = Path.Combine(Application.persistentDataPath, ProgressStore.FileName);
        bool exists = File.Exists(path);
        ProgressData onDisk = exists ? JsonUtility.FromJson<ProgressData>(File.ReadAllText(path)) : null;
        Report("restart persistence: progress read back from disk",
            onDisk != null && onDisk.highestLevelCompleted == 42,
            exists ? $"disk highestLevelCompleted={onDisk?.highestLevelCompleted}" : "save file missing: " + path);

        ProgressStore.Data.highestLevelCompleted = originalHighest;
        ProgressStore.Save();
        yield break;
    }

    private static void Report(string name, bool pass, string detail)
    {
        Debug.Log($"[EvidenceCheck] {name}: {(pass ? "PASS" : "FAIL")} - {detail}");
    }

    // Tap an arrow that is free to be tapped but has something in its way, and catch the flash
    // with time slowed so a single frame lands inside it.
    private IEnumerator Bump()
    {
        foreach (DragonController dragon in loader.SpawnedDragons)
        {
            if (dragon == null || dragon.IsLocked || dragon.CanExit()) continue;

            Time.timeScale = 0.1f;
            dragon.HandleTap();
            yield return new WaitForSecondsRealtime(0.15f);
            yield return Shoot("06-bump");
            Time.timeScale = 1f;
            yield break;
        }

        Debug.LogWarning("[ShotCapture] no blocked arrow found for the bump shot");
    }

    // Exit free arrows one at a time until a padlock opens, and catch the swell. The last arrow is
    // never tapped, so the level is never won (a win would bank the level and ping analytics).
    private IEnumerator Unlock()
    {
        int lockedBefore = CountLocked();
        if (lockedBefore == 0)
        {
            Debug.LogWarning("[ShotCapture] this level has no padlock");
            yield break;
        }

        for (int guard = 0; guard < 60; guard++)
        {
            if (StageManager.Instance.RemainingDragons <= 1) yield break;

            DragonController next = null;
            foreach (DragonController dragon in loader.SpawnedDragons)
            {
                if (dragon != null && !dragon.IsLocked && dragon.CanExit())
                {
                    next = dragon;
                    break;
                }
            }

            if (next == null) yield break;
            next.HandleTap();

            // Wait for it to leave the board, watching for the padlock count to drop.
            for (float t = 0f; t < 3f; t += Time.unscaledDeltaTime)
            {
                if (CountLocked() < lockedBefore)
                {
                    Time.timeScale = 0.2f;
                    yield return new WaitForSecondsRealtime(0.3f);
                    yield return Shoot("08-unlock");
                    Time.timeScale = 1f;
                    yield break;
                }

                yield return null;
            }
        }
    }

    private int CountLocked()
    {
        int n = 0;
        foreach (DragonController dragon in loader.SpawnedDragons)
            if (dragon != null && dragon.IsLocked) n++;
        return n;
    }

    private IEnumerator Shoot(string name)
    {
        // The development-build error console would sit over the bottom of every frame (AdMob has no
        // Windows client and logs an exception at start-up), so keep it out of the picture.
        Debug.developerConsoleVisible = false;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(dir, name + ".png"));
        yield return new WaitForSecondsRealtime(0.4f);
    }

    // A cropped, enlarged frame. `area` is in normalised screen space, origin bottom-left.
    private IEnumerator ShootCrop(string name, Rect area, int scale)
    {
        Debug.developerConsoleVisible = false;
        yield return new WaitForEndOfFrame();

        Texture2D full = ScreenCapture.CaptureScreenshotAsTexture();
        int x = Mathf.RoundToInt(area.x * full.width);
        int y = Mathf.RoundToInt(area.y * full.height);
        int w = Mathf.Min(Mathf.RoundToInt(area.width * full.width), full.width - x);
        int h = Mathf.Min(Mathf.RoundToInt(area.height * full.height), full.height - y);

        Color[] src = full.GetPixels(x, y, w, h);
        Texture2D big = new Texture2D(w * scale, h * scale, TextureFormat.RGBA32, false);
        Color[] dst = new Color[w * scale * h * scale];
        for (int j = 0; j < h * scale; j++)
            for (int i = 0; i < w * scale; i++)
                dst[j * w * scale + i] = src[(j / scale) * w + (i / scale)];
        big.SetPixels(dst);
        big.Apply();

        File.WriteAllBytes(Path.Combine(dir, name + ".png"), big.EncodeToPNG());
        Destroy(full);
        Destroy(big);
        yield return new WaitForSecondsRealtime(0.2f);
    }
}
#endif
