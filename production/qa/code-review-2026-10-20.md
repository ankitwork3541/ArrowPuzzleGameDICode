# Code review - uncommitted changes vs ad4a449 (2026-10-20)

Scope: 39 modified C#/UXML/USS files plus new `LevelMapController.cs`, `EvidenceCapture.cs`, `LevelMapScreen.uxml/.uss`.
Method: static read of diffs and new files, plus greps for class names, sprite paths, #if guards and key usage. Nothing was compiled or run in the Editor, so "looks solid" means "no defect found by reading".

Verdict: REQUEST CHANGES (no Critical; four Required).

## Ranked findings

### Required

1. **Persistence keys renamed while the comments say they were kept** -
   `Data/SaveSystem.cs:14-16`, `Managers/HintManager.cs:8-11`, `Managers/SettingsManager.cs:12-16`, `Managers/AdConfigFetcher.cs:10-11`.
   Old values: `progress.json`, `ad_config.json`, `HintManager_RemainingHints`, `HintManager_LastFreeHintCount`, `settings.sound/vibration/nativeRefreshRate`. New values all carry an `arrowqube` prefix. The comments say "intentionally keeps the old prefix", but the old names had no prefix, so every name changed. Nothing migrates the old values. If any build has shipped under `in.dicodeinfotech.arrowqubepuzzle` (the id in `ProjectSettings.asset` is not in this diff, so it predates it), those players lose progress, hint pool and settings. If the id has never shipped, the rename is harmless and only the comments are wrong.
   Fix: decide which case applies. If it shipped, revert to the original names, or read the old key once and copy it to the new one (`SaveSystem` file move, `PlayerPrefs.HasKey(old)` then copy and delete). If it did not ship, fix the three comments, because the next maintainer will "protect" names that were never shipped.

2. **Three LineRenderers per arrow, and extra sprite outlines, with no draw-call measurement** -
   `Core/ArrowController.cs:259-262` (Awake), `:263-292`.
   Every arrow now has body, outline and shadow `LineRenderer`s, plus two outline `SpriteRenderer`s. CLAUDE.md already records that `LineRenderer` is not SRP-Batcher compatible and that batch count on the biggest level has not been measured. This change triples that open risk, and also the overdraw (the outline is 1.4x wide with alpha blending).
   Fix: before merge, measure SetPass/batches on the largest baked level on a low-end Android. If it is bad, bake outline and shadow into one mesh, or drop the shadow on small devices (a quality flag, like the planned Effects toggle).

3. **LevelMap opens at the wrong scroll position on a slow first layout** - `UI/LevelMapController.cs:76` and `:109-124`.
   `ScrollToCurrent` runs once, 60 ms after `Open()`, and returns silently if `contentRect.height` is not yet valid. The map's slot goes from `display:none` to `flex` at that moment, so the first open on a slow device can miss the window. The player then sees the top of a 1,500-row list (the newest level, not their own). The scheduled item is also never cancelled if `Close()` comes first.
   Fix: keep the `IVisualElementScheduledItem`, and re-arm until the height is valid: `item = list.schedule.Execute(() => { if (!TryScroll()) return; item.Pause(); }).Every(30).Until(() => scrolled || !IsOpen);`. Alternatively scroll from a one-shot `GeometryChangedEvent` on the list.

4. **Backend analytics endpoint still belongs to the previous game** - `Managers/LevelTracker.cs:10-13` (note only, endpoint unchanged).
   This is already flagged in the code. It is also a release blocker, because every win is posted to the old game's counters, with no game id. The new level map makes it worse: replaying a beaten level (see 5) is reported as a completion too.
   Fix: do not ship a release build until the backend owner confirms the endpoint. Skip `ReportLevelComplete` when the finished level is not the frontier (`levelNumber <= highestLevelCompleted` before banking).

### Optional

5. **Replaying from the map leaves the campaign pointer on the replayed level** - `Managers/LevelLoader.cs:341-352` with `AdvanceLevel` at `:322-333`.
   `SelectLevel` sets `currentLevelIndex` to the old level and saves it. After a win, `AdvanceLevel` goes to old+1, so the PLAY button and the Home card then show an already-beaten level, not the frontier. `highestLevelCompleted` uses `Max`, so progress itself is safe.
   Fix: in `AdvanceLevel`, after banking, set the index to `Mathf.Min(highestLevelCompleted, totalLevels - 1)` when the replayed level was below the frontier. This is a design choice, so ask first.

6. **Unlock comment is wrong; the badge is not detached** - `Core/ArrowController.cs:~150` ("detaches from the arrow's lifetime").
   `lockBadge` stays a child of the arrow, so if the arrow is destroyed within 0.35 s of unlocking (a fast tap on the freshly opened arrow plus a short exit), the fade is cut off. Fix: `badge.SetParent(null, true)` before the tween, or correct the comment.

7. **LateUpdate mirror does 3 native calls per point per frame** - `Core/ArrowController.cs:~318-330`.
   Allocation free, as intended, but `GetPosition`/`SetPosition` per point is many P/Invokes while arrows move. Fix: one reusable `Vector3[]` buffer, `line.GetPositions(buf)` then `SetPositions` (the shadow needs a second buffer or an in-place offset pass). Do this only if profiling shows it.

8. **A missing host bridge class is now only a warning** - `Managers/AndroidDataBridge.cs:9-33`.
   The try/catch is right (an ad callback must never crash the game). But a wrong `BridgeClass` string drops every revenue report silently, once per ad, in the logs only. `BridgeClass` was changed from the old package; nothing in this repo contains `UnityAdBridge`, so I cannot confirm the host exposes it under the new id. Fix: confirm with the host project; log the warning once (a static bool) so it does not flood.

9. **Touch scroll starting on a node Button** - `UI/LevelMapController.cs:156-176`.
   Each node is a `Button` (Clickable captures the pointer) inside the ListView's ScrollView. Dragging that starts on a node may not scroll on some devices. I could not verify this without running it. Test a drag that starts on a node on a real touch device; if it fails, use a `Clickable` with a movement threshold or a `PointerUpEvent` check instead of Button.

### Nits

- `UI/LevelMapController.cs`/`LevelMapScreen.uss:151-156`: the locked node background is alpha 0.08, so the path line and the round-cap overlap dot show through. Use an opaque paper-tinted colour.
- `Managers/GameFlowManager.cs:~53`: `FindAnyObjectByType<HomeScreenController>()` on every Back press. Cache a serialized reference.
- `Managers/EvidenceCapture.cs:30-31`: seeds `SaveSystem.Data` in memory; the next `SaveProgress()` writes that fake progress to the real save. Dev/`--evidence` only, but use a separate save folder when `--evidence` is given.
- `UI/ResultScreenController.cs:~348-353`: the sheet slide ignores any reduced-motion setting (project UI rule: animations skippable). Cheap to add when a setting exists.
- `UI/ConfettiView.cs:18`: stray blank line before `{`.

## Looks solid

- Reload-callback symmetry: all six controllers store the versioned delegate in a field and unregister that same instance in `OnDisable`; no leaked or duplicate handlers.
- `LevelMapController` virtualisation: fixed row height, recycled `MapRow`s, `Bind` is idempotent (every class and the inline `left`/`display` are reset each time). Seam maths (`Mid`) is consistent between neighbouring rows. Spacer row (level 0) and the all-levels-done case (`highest + 1 > total`) are both handled. Locked levels are refused in both `OnNodeClicked` and `SelectLevel`.
- `ArrowController` outline: the dirty/settle logic covers the "last tween frame clears `isMoving`" case, hint pulse restore goes through `DrawPath`, and no per-frame allocations.
- `Handheld.Vibrate` is now behind `#if UNITY_ANDROID || UNITY_IOS`; `AndroidJavaClass` use is under `UNITY_ANDROID && !UNITY_EDITOR` (the existing `NativeAdSpace` one was not touched).
- `EvidenceCapture` is wrapped in `UNITY_EDITOR || DEVELOPMENT_BUILD`, does nothing without `--evidence`, and avoids `ShowWin()` side effects.
- USS: no unsupported properties found; all `url()` sprite paths resolve to existing files; the HUD class rename (`oct-button` to `action-button`) is consistent between C# and USS with no leftovers.
- Audio clips for lock shake/unlock are wired in `Game.unity`; `PlaySfx` is null-safe.
- Ads, lives, hints: no logic changes beyond the key renames in finding 1.
