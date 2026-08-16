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

- **Step by step, smallest teachable chunks.** Never dump a large finished system at once.
- **The user types new-system code themselves** in the Editor. Claude teaches the step; the user
  applies it. Exceptions: small bug-fix patches to already-taught code, temporary debug logging,
  documentation files, and any step where the user explicitly says "just do it yourself".
- **Explain the "why"** in simple language before/alongside the code.
- **Test before advancing.** The user tests each step hands-on in the Editor and reports back.
  Don't assume something works — wait for explicit confirmation.
- **Ask, don't guess** on game-design or implementation-approach decisions that aren't written down.
- **Replies in Gujarati written in Latin letters** (romanized), technical terms stay English.
  Anything written into the repo stays English.
- Pace: bundle setup + code + wiring into fewer messages where it makes sense, **without** cutting
  explanation depth.

## Tech Stack

- Unity **6000.5.2f1**, URP, 2D, portrait-locked
- **New Input System** (`UnityEngine.InputSystem`) — all input
- **UI Toolkit** (UXML + USS + `PanelRenderer`) for all UI — no UGUI, no TextMeshPro in `Assets/Project`
- **PrimeTween** for all tweening (zero-alloc; DOTween is installed under `Assets/Plugins/Demigiant`
  but unused). Always pass a target (`this`, `transform`) so tweens stop when the object is destroyed.
- **`UnityEngine.Pool.ObjectPool<T>`** for pooling (grid dots)

## Rendering Approach

- Arrow body = **`LineRenderer`** (+ `EdgeCollider2D` for click detection), with `numCornerVertices`
  giving free rounded corners. Separate head/tail sprite objects ride the line's ends.
- Grid background = one pooled `Dot` GameObject (`SpriteRenderer`) per empty cell.
- Art direction: **Neon Futuristic / Cyberpunk** (dark `#050816` base, cyan `#00E5FF`, magenta `#FF2BD6`).

## Folder Structure

`Assets/Project/`
- `Scripts/Core/` — grid + arrow gameplay logic (`GridBuilder`, `GridCell`, `ArrowController`, `Dot`)
- `Scripts/Data/` — `ScriptableObject` / serializable data models (`LevelData`, `ArrowPathData`, `LevelBinary`)
- `Scripts/Managers/` — scene-level coordinators (`GameFlowManager`, `LevelLoader`, `LevelManager`,
  `LivesManager`, `AudioManager`, `SettingsManager`, `CameraController`)
- `Scripts/UI/` — UI Toolkit controllers (`SplashScreenController`, `HomeScreenController`,
  `GameplayHUDController`, `SettingsScreenController`, `BottomNavBinder`, `DeviceScaleCategory`)
- `Scripts/Editor/` — editor-only tools (level generators, binary writer)
- `UXML/` — `.uxml` documents + `MainPanelSettings.asset`
- `Themes/` — `.uss` stylesheets
- `Prefabs/`, `Sprites/`, `Audio/`, `Levels/`, `Scenes/` (`Game.unity` — single-scene project)

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
