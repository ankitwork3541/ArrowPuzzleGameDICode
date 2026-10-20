# ArrowPuzzleGame — Project Context

> Fresh start: 2026-08-15. The previous 100 KB history log was intentionally cleared.
> Old version is still in git history (`git show 2bc52c7:CLAUDE.md`) if anything needs looking up.

## What This Project Is

A 2D Unity arrow-path puzzle game, portrait mobile, targeting 50,000+ levels.

**Core mechanic**: arrows sit on a grid as a chain of cells. Tapping an arrow slides it forward
one cell in its head's direction (snake-style), continuing automatically until it either exits
the board (path clear all the way off-grid) or blocks (another arrow in the way — bumps forward
slightly, flashes red, reverses back and costs a life).

## How To Work With This User

The user is a **complete Unity/programming beginner**. This drives everything:

- **UPDATE 2026-10-20 — Claude does ALL the work.** The user said: "badhu work tare j karvanu che,
  have hu kai nai karu". Claude writes the code, edits scenes/prefabs/assets (through the Unity CLI
  when the Editor is open), runs the Editor, takes the screenshots and verifies the result itself.
  The user no longer types code and no longer has to test by hand. Do not hand steps back to them.
  Use parallel forks/agents on disjoint files to go fast. Only stop to ask for a real design
  decision the brief does not answer.
- **The old teaching mode below is superseded by the bullet above** (kept for history): step by
  step, smallest teachable chunks; the user typed new-system code themselves. If the user ever says
  they want to learn a part again, switch back for that part.
- **Explain the "why"** in simple language before/alongside the code.
- **Test before advancing.** Claude tests each step itself (Unity CLI: `unity status`, `unity
  command ...`, console read, screenshots) and reports the evidence. Don't claim it works without
  having looked. If a check cannot be run (e.g. UI Toolkit not captured), say so plainly.
- **Ask, don't guess** on game-design or implementation-approach decisions that aren't written down.
- **Replies in Gujarati written in Latin letters** (romanized), technical terms stay English.
  Anything written into the repo stays English.
- Pace: bundle setup + code + wiring into fewer messages where it makes sense, **without** cutting
  explanation depth.

## Tech Stack

- Unity **6000.6.3f1**, URP, 2D, portrait-locked
- **New Input System** (`UnityEngine.InputSystem`) — all input
- **UI Toolkit** (UXML + USS + `PanelRenderer`) for all UI — no UGUI, no TextMeshPro in `Assets/Project`
- **PrimeTween** for all tweening (zero-alloc; DOTween is installed under `Assets/Plugins/Demigiant`
  but unused). Always pass a target (`this`, `transform`) so tweens stop when the object is destroyed.
- **`UnityEngine.Pool.ObjectPool<T>`** for pooling (grid dots)

## Rendering Approach

- Arrow body = **`LineRenderer`** (+ `EdgeCollider2D` for click detection), with `numCornerVertices`
  giving free rounded corners. Separate head/tail sprite objects ride the line's ends.
- Grid background = one pooled `Dot` GameObject (`SpriteRenderer`) per empty cell.
- Product name (2026-10-20): **Arrow Dragon Puzzle** (two-line brand "ARROW DRAGON" / "PUZZLE"); the art
  direction keeps the lantern / paper theme (names like `LanternMixer`, `LanternBackdrop`, `LanternDragons.unity`
  and "lantern red" are theme or identifiers, not the product name).
- Art direction (changed 2026-10-20): **light paper lantern theme, no dark mode** — warm
  cream `#FFF8EC` base, ink `#3A2418`, lantern red `#C43448`, gold-orange `#E88C00`, hint teal
  `#007A96`. No bloom, no glow. The old neon cyan/magenta/navy look is gone. Full spec:
  `design/ux/ui-redesign.md`.

## Folder Structure

`Assets/Project/`
Names below are the current ones (renamed 2026-10-20, full old to new map in `production/qa/rename-map.json`
and `production/qa/rename-report-2026-10-20.md`; the progress log further down still uses the old names
because it is history).

- `Scripts/Core/` — board + dragon gameplay logic (`BoardBuilder`, `BoardCell`, `DragonController`, `Spark`,
  `LatticeGuides`)
- `Scripts/Data/` — `ScriptableObject` / serializable data models (`StageData`, `DragonPathData`, `StagePack`,
  `DragonPalette`, `ProgressStore`/`ProgressData`, `KeyMigration`, `PromoConfig`)
- `Scripts/Managers/` — scene-level coordinators (`FlowDirector`, `StageLoader`, `StageManager`,
  `EmberManager` (lives), `ClueManager` (hints), `SoundDirector`, `OptionsManager`, `BoardCamera`,
  `PromoManager` (ads), `LanternBackdrop`, `ShotCapture` (dev-build screenshot driver, `--evidence`))
- `Scripts/UI/` — UI Toolkit controllers (`IntroScreenController`, `HallScreenController`,
  `PlayScreenController`, `OptionsScreenController`, `SheetScreenController`, `TrailMapController`,
  `TabDockBinder`, `ScreenScaleProfile`, `WaitSheet`)
- `Scripts/Editor/` — editor-only tools (`StageCampaignGenerator`, `StagePackWriter`, `StageCarver`, ...);
  menu root is `LanternDragons/`
- `UXML/` — `.uxml` documents + `LanternPanelSettings.asset`
- `Themes/` — `.uss` stylesheets
- `Prefabs/`, `Sprites/`, `Audio/`, `Resources/Stages/` (baked `Pack_NN.bytes`, header `LDLC` v3), `Scenes/`
  (`LanternDragons.unity` — single-scene project). Retired old-game assets sit in `_Retired/` folders
  (nothing was deleted).
- Persisted names: save file `lanterndragons_progress.json`, PlayerPrefs `lanterndragons.*`. Older names
  (`arrowqube_*` and the shipped `progress.json`, `HintManager_*`, `settings.*`) are copied across once
  (`ProgressStore.CopyLegacyFileIfMissing`, `KeyMigration`), never moved.

Moving a Unity asset must move its paired `.meta` file too (references are GUID-based).

`Assets/MultiTech Studio/` is a purchased template kept **only as reference/study material** —
never reused directly.

## Current State (2026-08-15)

The game is a complete playable loop: Splash → Home → Gameplay → Win/Lose → next level, with
grid + arrow move/block/exit, Lives/Hearts, Hint, camera framing with pinch-zoom and pan,
level generators + solvability checking, audio + haptics, and a Settings screen.

Known open items:
- Settings screen scene setup (`PanelRenderer` GameObject + controller wiring) not verified yet
- Win/Lose screens are temporary stubs — not built in UI Toolkit
- Mouse-wheel zoom not verified in the Editor
- Hint button tap may also register as a board tap (`CameraController` doesn't check for UI hits)

## Progress Log

Keep this updated as work progresses — add an entry after each meaningful step, don't wait to be asked.

- **2026-08-15** — Context/memory reset. This file rewritten minimal.
- **2026-08-15** — Neon arrow colours, step 1 of the multi-colour + glow work. Verified in the
  Editor. Reference: `~/Downloads/stitch_neon_route_ui_design`.
  - New `Scripts/Data/ArrowPalette.cs` — seven LDR neon colours spread over hues 157°-340° (green
    `#00FF9D`, cyan `#00E5FF`, blue `#2979FF`, purple `#8B5CFF`, violet `#C24DFF`, magenta
    `#FF2BD6`, pink `#FF4D8D`). The red/yellow side of the wheel is left empty on purpose — those
    are the blocked-bounce and hint-pulse state colours, and a resting arrow wearing one would read
    as feedback that never happened. Deliberately **not** HDR: `LineRenderer` writes
    `startColor`/`endColor` into 8-bit vertex colours, which clamp at 1.0, so the glow has to come
    from Bloom with a sub-1.0 threshold rather than from colour intensity.
  - `ArrowController`: `activeColor` (one shared cyan) replaced by per-arrow `baseColor` +
    `activeBoost`, so a travelling arrow lifts towards white instead of losing its own colour.
    Every `Color.white` reset now restores `baseColor`.
  - `ArrowPalette.Dealer`, seeded on the level index by `LevelLoader`, hands each arrow its colour:
    a level's palette is stable across restarts but differs level to level. It deals from a
    shuffled deck rather than rolling per arrow — independent rolls collide ~85% of the time on a
    five-arrow level, which read as a bug on small boards. Deck order is *spawn* order, so two
    spatially adjacent arrows can still match on a large board; a spatial fix would need graph
    colouring and is not worth it unless it actually shows.
- **2026-08-16** — Step 2, glow + backdrop. Verified in the Editor.
  - URP Bloom is live: a `Global Volume` in the scene (threshold 0.55, intensity 0.9, scatter 0.75)
    plus Post Processing enabled on the camera. Threshold sits *below* 1.0 on purpose — see the
    LDR note above; at 1.0 nothing would ever bloom.
  - New `Scripts/Managers/BackdropArrows.cs` on a `Backdrop` child of the Main Camera. Scatters a
    few huge filled arrow silhouettes (`Sprites/BackdropArrow{Straight,Bend,Zigzag}.png`, generated,
    `Mesh Type: Tight`) and drifts them on slow sine paths.
    - It is a camera child *and* rescales itself from `orthographicSize` every `LateUpdate`. Being
      a child alone is not enough: an orthographic camera's zoom **is** its world-to-screen scale,
      so a child still grows and shrinks under a pinch unless its own scale cancels that out.
    - Placement is a jittered grid (one arrow per cell, centre cells left empty), not free random.
      Independent random positions clump, and every overlap is paid for twice in alpha blending.
    - Tint lives or dies on contrast. `#080C1C` is about 1.5% above the `#050816` base. The failure
      mode is always "too visible" — three passes were needed to get it low enough.
  - The `Starfield` / `ShootingStars` particle backdrop is gone, along with `GameFlowManager.spaceFx`
    and its six `SetActive()` calls. `CameraController.FrameMenu()` stays: its starfield reason is
    obsolete, but it also clears `hasContent`, which is what stops pan/zoom working on menu screens.
  - `Scripts/Managers/ScreenBackdrop.cs` is written but **unused** — it was for a full-screen
    backdrop image, an approach that was abandoned. Delete it if no base gradient is ever added.
  - Still to do: head/tail sprite atlas batching; a Settings "Effects" toggle covering bloom *and*
    the backdrop together (both are comparable cost and both are decoration, not gameplay);
    blocked-bounce red is still pure `Color.red`, which is dull against neon and barely blooms —
    something like `#FF1744` would sit in the palette better.
    - Open perf question, not yet measured: `LineRenderer` is not SRP-Batcher compatible (Unity
    supports meshes only, "not lines and trails"), so each arrow is its own draw call. Measure
    batches on the biggest level before considering the real fix — one procedural mesh for all
    arrow bodies. Input no longer depends on `EdgeCollider2D`, which makes that refactor viable.
- **2026-08-24** — Hint rewarded ads match the product rules. `ad_enable` is the master switch;
  `rewarded_ad_enable` only toggles rewarded while other formats can stay on. `free_hint_count`
  from the API is the free pool; a changed value resets remaining hints, an unchanged value keeps
  leftovers. Hint tap loads a rewarded ad on demand (Unity `RewardedAd.Load`, not preload), shows
  the themed loading modal, and unlocks the hint only after the ad closes with Google's reward
  callback. Skip / no-fill / load fail does not unlock. After free hints, if rewarded is off the
  hint button disables. Banner / native units were not changed.
- **2026-08-24** — Interstitial policy matches the API rules. Click-based ads use one global
  counter plus `ad_click_time_sec`; both must pass, then a *preloaded* interstitial is shown
  (official load-ahead, never load-on-tap). Skip keeps the counter so the next eligible click
  retries. `ad_level_count` is a separate Next-button path with no click/time gate. `back_ad_on`
  folds the HUD back button into the same click counter. Tracking resets on process start (app
  kill) and on a successful show — not on in-game Restart. `ad_click_count` / `ad_level_count` of
  0 turn that trigger off.

- **2026-10-20** — Direction change: the game is a **light paper theme only** (brief updated, Story
  008 added, Story 004 superseded; its dragon head/pearl/padlock/backdrop sprites are reused).
  - Done by forks, not yet looked at in the Editor: camera + HUD `.top-panel` cream; Bloom off in
    `Global Volume Profile`; palette darkened for cream (`ArrowPalette`); bump flash = near-black
    ink, hint pulse = blue `#0077B6`; travelling arrow darkens toward ink; outline + soft shadow
    LineRenderers built at runtime in `ArrowController`; backdrop tint `#FAEEDC`; all UI Toolkit
    screens retinted cream (Splash, Home, Nav, Settings, Win/Lose, ad popups, native ad template).
  - Then `/team-ui` redesign to the **paper-lantern shape language** (`design/ux/ui-redesign.md`):
    octagon/hex shapes replaced by rounded squares and capsules, new sprites
    `UIRound*`/`UICapsule*`, 13 icons redrawn (same filenames/GUIDs), PLAY/NEXT/RETRY = red capsule
    with a bottom lip. HUD controller class names renamed to `action-button*`.
  - Console after reimport: no errors (only the AdMob plugin-importer and obsolete
    `RegisterUIReloadCallback` warnings, both pre-existing).
  - New festival shape PNGs added to `LevelShapeSources/` (20 files; Story 003 next: preview + bake).
  - Story 003 done: 19 festival silhouettes, final **1500-level campaign baked** (298 shaped, 596
    notched, 17,189 padlocks, all re-solved with locks, 570 KB). Smoke note
    `production/qa/smoke-2026-10-20.md`. Story 005 level map (`LevelMapController`, virtualised
    `ListView`, Painter2D path) and Story 006 (name "Lantern Dragons", new logo/app icon, 7 synthesised
    `LD_*` bell/drum/gong clips, lock shake/unlock sounds wired) are in code and compile clean but
    **not seen on screen yet**. Story 007 audit: `production/qa/evidence/story-007-audit.md`; package
    id / AdMob units / `LevelTracker` endpoint are still the old game's and need the user's input.
  - Screenshots solved: the Editor never paints UI Toolkit unless it is the active app, so evidence
    comes from a **Windows development player** driven by `EvidenceCapture.cs` (`--evidence <dir>`),
    built with a detached job (`--detach`, poll `unity job status`). 10 frames in
    `production/qa/evidence/story-008/`, evidence docs `story-008-evidence.md` and
    `story-005-evidence.md` (sign-off boxes still unticked, the lead's call). Polish round done:
    smaller level badge, ink padlock digit, map bottom padding, mid-board unlock frame. Fixed a real
    Windows build error (`Handheld.Vibrate()` now guarded).
  - **Rename pass (afternoon)**: snapshot commit `0063ab5` (tag `pre-rename-safety`) first, then 63 script
    files / 70 types, 75 asset moves (14 retired to `_Retired/`), 189 USS classes and 44 UXML element names
    renamed on both sides, scene `Game.unity` to `LanternDragons.unity`, `Levels/Chunk_NN` to
    `Stages/Pack_NN`. Done with `AssetDatabase.MoveAsset` (all 235 GUIDs unchanged, 0 missing scripts),
    then a token-aware text pass, one recompile. Chunk hashes were byte-identical after the rename; Phase B
    then changed the level file header to `LDLC` v3 and re-baked (only 3 header bytes differ, level data is
    identical). Five serialized fields renamed with `[FormerlySerializedAs]` (values kept). New save keys
    `lanterndragons_*` with copy-not-move migration from both older generations (tests:
    `LegacyKeyMigrationTests`). Total level count is never shown anywhere (Home and map say "N CLEARED").
    EditMode: 78 of 78 pass, plus `ShippedCampaignTests` (explicit, run alone) 2 of 2. Windows dev player
    rebuilt, 18 frames recaptured, `[EvidenceCheck]` 5 of 5 PASS. Not renamed on purpose: third-party code,
    package id / host bridge class (Story 007 answers pending), Inspector field names, JSON field names.
    Report: `production/qa/rename-report-2026-10-20.md`.
  - **Story 007 identity (afternoon)**: package id and iOS bundle id are now
    `in.dicodeinfotech.arrowdragonpuzzle` (set through `PlayerSettings`), version code 1 (new Play
    listing). **No host/launcher app any more**: `HostBridge` (`UnityAdBridge` JNI, paid-event forwarding,
    Intent config) was deleted. Ad config comes from an HTTP GET in `PromoConfigFetcher.Fetch`
    (`const string ConfigUrl`, EMPTY until the developer supplies it: then no request, cached copy, else
    the baked `PromoConfig`); `PromoManager.Start` is a coroutine that waits for the fetch. The level-complete
    tracker (`StageTracker`) was removed entirely; `FrontierRule` stays. 101 of 101 EditMode tests, Windows
    player build ok, `[EvidenceCheck]` 11 of 11. Blocked on the developer: config API URL, real AdMob app
    ids, keystore, and an Android device pass. Evidence: `production/qa/evidence/story-007-evidence.md`.
  - **Dynamic game config (2026-10-20, round 3)**: one nested JSON (`design/config/game-config.sample.json`,
    README beside it) now drives ads (per-screen placements, interstitial/rewarded rules), links
    (privacy/terms/rate), lives, hints, Options rows, confetti, LEVELS button, maintenance/force-update
    sheets and a Home notice. Code: `GameConfig`/`GameConfigParser`, `AdRules`, `GameConfigService`
    (cache `lanterndragons_config_cache.json`), `PromoSettings` reads it; `PromoRemoteConfig` deleted.
    `PromoConfigFetcher.ConfigUrl` is still empty (developer to supply). The Lvl 10/20/30 dock items were
    removed (dock = Home, Settings). EditMode 167 of 167; evidence in `production/qa/evidence/story-007/`
    and `story-008-v5/`; doc `Docs/NativeAds.md` rewritten. Dev player flags: `--evidence-config <file>
    --evidence-config-run --evidence-prefix <name>`.
  - **Real audio (2026-10-20, Fork AU, spec `production/audio/lantern-dragons-sfx/sfx-spec.md`)**: the synthesised
    `LD_*` placeholders are replaced by 20 `Lantern_*.wav` clips cut from CC0 packs (rubberduck's two OpenGameArt
    SFX packs, Kenney Interface Sounds, Spring Spring's bell), credits in `Assets/Project/Audio/CREDITS.md`, build and
    measurements in `production/qa/audio-research-2026-10-20.md`. 7 original events + 9 new ones (dragon exit,
    heart lost, hint, level entrance, map node, sheet open/close, toggles), each hooked where it happens.
    `SoundDirector` now has an 8-voice pool, pitch spread, round-robin alternates, per-event cooldown/voice limit
    (`SfxGate`), a separate Jingle source + `Jingle` mixer group, code-side 6 dB ducking, Sound-off stops a playing
    jingle; `HapticPulse` replaces the 400 ms `Handheld.Vibrate()` with 12-22 ms Android pulses and adds a light
    pulse for the padlock shake. EditMode 212 of 212, `[EvidenceCheck]` 11 of 11. **Nobody has listened to it yet**
    (gongs are phone recordings; the lock shake is wood ticks): listen first to bump, win jingle, 10 rapid taps.
  - **Padlock readability + first-lock tutorial (verified 2026-10-20)**: `Padlock.png` redrawn with a big flat
    body for the digit (ink, `LockDigitSize` 0.075, fed as a linear colour because the TextMesh shader converts to sRGB);
    `LockCoach` overlay (UXML in `PlayScreen.uxml`, `Themes/LockCoach.uss`) shows once on the first padlock level
    (derived from the baked data via `StageLoader.FirstLockLevelIndex`, flag `ProgressData.lockTutorialSeen`, config key
    `ui.lock_tutorial`); the card is placed from the ring so it never covers the padlock; tapping a locked dragon shows
    "Free N more". The game's display name is now **Arrow Dragon Puzzle** (splash/Home fit at 720x1280 and 360x640);
    the dragon-exit whoosh no longer plays. EditMode 229 of 229, `[EvidenceCheck]` 11 of 11 + lock run 5 of 5; one
    flaky full run (a dragon left before frame 05) did not reproduce. Evidence: `production/qa/evidence/lock-ux-evidence.md`.
    Editor hang rule: one Editor operation at a time, everything long detached (two hangs came from overlapping a
    recompile with a test job).
  - **Level entrance styles** (`ui.entrance_style`: `ink` default / `fly` / `classic` for comparing; `EntranceMath`,
    tap skips, Reduce Motion = finished board, no new sound): 0.9 s small, about 1.17 s on 198 dragons; developer to
    pick one, see `production/qa/evidence/entrance-evidence.md` (270 of 270 EditMode tests, player checks all PASS).
  - Still open: pearl tail marker is hard to see; animation feel, backdrop drift and Android look
    unverified.
  - (superseded) Open: screenshots for Story 008 evidence. `capture_game_view` with a size drops UI Toolkit, and
    `ScreenCapture` fails because the Game view reports 2560x36 — fix the Game view size first.
    The capture tool also writes under `Assets/` (stray `Assets/production/` folder to delete).
    Nav icons (`HomeIcon`, `LockIcon`, `SettingsIcon`) are not yet in the new icon set; the padlock
    badge digit colour and the spec's faint warm card shadow are not done.

---

## Claude Code Game Studios (CCGS) Framework

Installed 2026-10-19 from `Donchitos/Claude-Code-Game-Studios` v1.1.3: `.claude/` (agents, skills,
hooks, rules, docs) plus `project.yaml` at the repo root. The `src/` folder was intentionally not
copied — Unity's code root is `Assets/`. Godot and Unreal agents were removed; only the Unity
specialists (`unity-specialist`, `unity-ui-specialist`, `unity-shader-specialist`,
`unity-addressables-specialist`, `unity-dots-specialist`) remain.

**Precedence:** the "How To Work With This User" section above always wins over the framework
rules below — replies are romanized Gujarati and everything written into the repo stays English.
Since 2026-10-20 Claude does all the implementation itself (see the top of that section), so a CCGS
agent or skill writing a finished system is fine; the user no longer types code.

- **Engine**: Unity 6000.6.3f1 (C#), URP 2D — see "Tech Stack" above
- **Version control**: Git
- **Config**: `project.yaml` is the primary config store (`modes.rigor: minimal`). Skills resolve it
  per `.claude/docs/config-resolution.md`. Do not add the six knobs `rigor` fronts
  (`modes.review_mode`, `modes.workflow`, `docs.density`, `qa.level`, `modes.story_granularity`,
  `team.size`) to `project.yaml` — that pins them and shadows the rigor expansion.
- **Code root**: `Assets/Project/Scripts/` (not `src/`); tests belong in `Assets/Tests/EditMode/`
  and `Assets/Tests/PlayMode/`.

### Engine Version Reference

<!-- ENGINE-REFERENCE-IMPORT -->
@docs/engine-reference/unity/VERSION.md

### Project Structure

@.claude/docs/directory-structure.md

### Coordination Rules

@.claude/docs/coordination-rules.md

### Collaboration Protocol

Every task follows: **Question -> Options -> Decision -> Draft -> Approval**. Agents ask before
writing files, show drafts first, and make no commits without user instruction.

### Coding Standards

@.claude/docs/coding-standards.md

### Context Management

Read `.claude/docs/context-management.md` on demand. `production/session-state/active.md` is the
session checkpoint — read it first after any compaction, crash, or `/clear`. Helpers in
`.claude/scripts/` emit observations, never verdicts.