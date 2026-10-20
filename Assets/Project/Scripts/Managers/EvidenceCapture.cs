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
public class EvidenceCapture : MonoBehaviour
{
    // Level 6 is where the baked campaign introduces its first padlock, so it has locks to open.
    private const int DemoLevelNumber = 6;

    private string dir;
    private GameFlowManager flow;
    private LevelLoader loader;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void SeedSave()
    {
        if (!TryGetDir(out _)) return;

        // A fresh save would show an all-locked map and a lock-free level 1. Level 6 is the first
        // level with padlocks, and 5 beaten levels gives the map done, current and locked nodes.
        SaveSystem.Data.highestLevelCompleted = DemoLevelNumber - 1;
        SaveSystem.Data.currentLevelIndex = DemoLevelNumber - 1;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (!TryGetDir(out string path)) return;

        GameObject host = new GameObject("EvidenceCapture");
        DontDestroyOnLoad(host);
        host.AddComponent<EvidenceCapture>().dir = path;
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
        flow = FindAnyObjectByType<GameFlowManager>();
        loader = flow.levelLoader;

        if (HasFlag("--evidence-small"))
        {
            yield return SmallRun();
            Debug.Log("[EvidenceCapture] done");
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

        HomeScreenController home = FindAnyObjectByType<HomeScreenController>();
        MethodInfo open = typeof(HomeScreenController).GetMethod("OpenLevelMap",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (home != null && open != null)
        {
            open.Invoke(home, null);
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Shoot("03-level-map");
            home.TryCloseMap();
            yield return new WaitForSecondsRealtime(0.5f);
        }

        flow.OnSettingsNavClicked();
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot("04-settings");
        flow.OnHomeNavClicked();
        yield return new WaitForSecondsRealtime(0.5f);

        flow.OnPlayButtonClicked();
        yield return new WaitUntil(() => flow.gameplayRoot.activeSelf);
        yield return new WaitForSecondsRealtime(7f);
        yield return Shoot("05-hud-board");
        yield return ShootCrop("15-pearl-zoom", new Rect(0.18f, 0.30f, 0.62f, 0.40f), 2);

        // The ad-loading card is a global overlay on its own panel, so it can be shown over the board.
        AdLoadingOverlay.Show();
        yield return new WaitForSecondsRealtime(1.2f);
        if (AdLoadingOverlay.Showing)
            yield return Shoot("11-ad-loading-modal");
        else
            Debug.LogWarning("[EvidenceCapture] AdLoadingOverlay did not show");
        AdLoadingOverlay.Hide();
        yield return new WaitForSecondsRealtime(0.6f);

        yield return CheckLockedTapCostsNoLife();
        yield return CheckMapSelection();
        yield return CheckSavePersistence();

        int livesBefore = LivesManager.Instance.lives;
        yield return Bump();
        yield return new WaitForSecondsRealtime(2.5f);
        int lost = livesBefore - LivesManager.Instance.lives;
        Report("bump costs exactly one life", lost == 1, $"lives {livesBefore} -> {LivesManager.Instance.lives}");

        // Unlock before the hint: the hint refocuses the camera on one arrow, and that zoom pushes
        // the padlocks to the board's edge, where the swell would be hidden under the HUD.
        yield return Unlock();
        yield return new WaitForSecondsRealtime(1.5f);

        loader.PlayHint();
        yield return new WaitForSecondsRealtime(0.6f);
        yield return Shoot("07-hint");
        yield return new WaitForSecondsRealtime(1.5f);

        // The cards sit over the board, so just swap the panel roots (see the note at the top).
        typeof(GameFlowManager).GetProperty("LastCompletedLevelNumber")
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

        Debug.Log("[EvidenceCapture] done");
        Application.Quit();
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

        typeof(GameFlowManager).GetProperty("LastCompletedLevelNumber")
            .SetValue(flow, DemoLevelNumber);
        flow.gameplayRoot.SetActive(false);
        flow.winRoot.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shoot("13-small-4-win");
    }

    // Level 1500 puts a four-digit number in the HUD title. The save is put back afterwards.
    private IEnumerator FourDigitTitle()
    {
        SaveSystem.Data.highestLevelCompleted = 1499;
        loader.SelectLevel(1500);
        flow.gameplayRoot.SetActive(true);
        loader.LoadLevel();
        yield return new WaitForSecondsRealtime(8f);
        yield return Shoot("12-hud-level-1500");

        SaveSystem.Data.highestLevelCompleted = DemoLevelNumber - 1;
        loader.SelectLevel(DemoLevelNumber);
    }

    // A tap on a padlocked arrow shakes the padlock and must cost no life.
    private IEnumerator CheckLockedTapCostsNoLife()
    {
        ArrowController locked = null;
        foreach (ArrowController arrow in loader.SpawnedArrows)
        {
            if (arrow != null && arrow.IsLocked)
            {
                locked = arrow;
                break;
            }
        }

        if (locked == null)
        {
            Report("locked tap costs no life", false, "no padlocked arrow on this level");
            yield break;
        }

        int before = LivesManager.Instance.lives;
        locked.HandleTap();
        yield return new WaitForSecondsRealtime(1.0f);
        Report("locked tap costs no life",
            LivesManager.Instance.lives == before && locked.IsLocked,
            $"lives {before} -> {LivesManager.Instance.lives}, still locked={locked.IsLocked}");
    }

    // The map path is LevelLoader.SelectLevel: a beaten or current level is accepted, a level past
    // the frontier is refused (clamped to the frontier).
    private IEnumerator CheckMapSelection()
    {
        int frontier = SaveSystem.Data.highestLevelCompleted + 1;

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
        int originalHighest = SaveSystem.Data.highestLevelCompleted;
        SaveSystem.Data.highestLevelCompleted = 42;
        SaveSystem.Save();

        string path = Path.Combine(Application.persistentDataPath, "arrowqube_progress.json");
        bool exists = File.Exists(path);
        SaveData onDisk = exists ? JsonUtility.FromJson<SaveData>(File.ReadAllText(path)) : null;
        Report("restart persistence: progress read back from disk",
            onDisk != null && onDisk.highestLevelCompleted == 42,
            exists ? $"disk highestLevelCompleted={onDisk?.highestLevelCompleted}" : "save file missing: " + path);

        SaveSystem.Data.highestLevelCompleted = originalHighest;
        SaveSystem.Save();
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
        foreach (ArrowController arrow in loader.SpawnedArrows)
        {
            if (arrow == null || arrow.IsLocked || arrow.CanExit()) continue;

            Time.timeScale = 0.1f;
            arrow.HandleTap();
            yield return new WaitForSecondsRealtime(0.15f);
            yield return Shoot("06-bump");
            Time.timeScale = 1f;
            yield break;
        }

        Debug.LogWarning("[EvidenceCapture] no blocked arrow found for the bump shot");
    }

    // Exit free arrows one at a time until a padlock opens, and catch the swell. The last arrow is
    // never tapped, so the level is never won (a win would bank the level and ping analytics).
    private IEnumerator Unlock()
    {
        int lockedBefore = CountLocked();
        if (lockedBefore == 0)
        {
            Debug.LogWarning("[EvidenceCapture] this level has no padlock");
            yield break;
        }

        for (int guard = 0; guard < 60; guard++)
        {
            if (LevelManager.Instance.RemainingArrows <= 1) yield break;

            ArrowController next = null;
            foreach (ArrowController arrow in loader.SpawnedArrows)
            {
                if (arrow != null && !arrow.IsLocked && arrow.CanExit())
                {
                    next = arrow;
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
        foreach (ArrowController arrow in loader.SpawnedArrows)
            if (arrow != null && arrow.IsLocked) n++;
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
