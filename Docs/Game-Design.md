# ArrowPuzzleGame — Game Design Document

This file locks down *product-level* decisions (what the game is, what's in v1, what's deliberately out) so the technical approach doesn't keep changing mid-build. Read this alongside `CLAUDE.md` (which tracks *how* the code is built, day to day) — this file tracks *what* we're building and why.

**Rule going forward: before starting any new system, check here first.** If a decision isn't recorded here yet, **stop and ask the user — do not assume or guess.** This is not optional: guessing wrong once already cost real, already-built work (see `feedback-ask-dont-guess-game-design` memory) — the original sprite-piece rendering approach had to be replaced because the rendering *approach itself* wasn't confirmed with the user before it was built.

---

## 0. User's Original Notes (verbatim, 2026-08-06)

Kept as-written so nothing gets lost in translation/restructuring. The structured sections below expand on these.

> Aa arrow puzzle game ma apde quality ane performance banne par dhyaan rakhvanu che jethi top-notch rahe. Aa mate sprite-segment approach use kari shakay, pan ema ek limitation aave che: jyare arrow animate thai ne aagal vadhe tyare sprite flicker thay che, ane corner vala sprite proper rite position badali shakta nथी. Etle apde eni jagyaae Line Renderer use karshu.
>
> Main gameplay: Apde app ma 50,000+ levels banavvana che. Levels dynamic hashe — arrows koi pan shape ma goothvai shakshe (animal, non-living things jem ke bulb, vagere). Aa levels manually banavvana nathi; badha dynamically generate thashe.
>
> Level flow: Level start thay etle badha arrows show thashe. Har arrow ne ek head (disha) hashe. Koi pan arrow par click kariye etle e arrow eni head ni disha ma continuous aagal vadhshe — screen ni bar java mate (etle ke puzzle complete karva mate). Jo e arrow ni disha ma koi avrodh hoy (etle ke biji koi arrow ni body aavi jay), to arrow smash jevi animation thashe, red color thashe, ane pachi reverse animation ma pachi eni mool position par aavi jashe — etle ke e move khoto hato. Jo koi avrodh na hoy, to e arrow screen ni bar nikari jashe (move successful).
>
> Life system: Apde 3 heart (lives) apshu. 3 wrong moves thay etle game over — pachi level restart kari shakay. Saathe hint system pan besadvanu che.
>
> Background dots: Attach karel image ma joto — arrows ni pachal nana dots dekhay che. E pan rakhvana che, jethi user ne easy rahe. Arrows apde white rakhshu (etle koi pan color na kari shakay).
>
> Themes: Aa game ma apde multiple themes rakhshu — etle arrow ni jagyaae biji koi vastu (snake, fish, ke biju kai) use kari shakay. Aa rite banavvu che jethi thoduk alag lage.
>
> Tech: Aa game banavva mate apde DOTween (animation), new Input System, ane biji modern vastuo use karshu jethi badha mobiles par game ni performance best rahe.

**Later clarification (same day)**: the "levels dynamic, not manual" note above is superseded by §7 below — level generation is **hybrid** (dynamic + manual), not dynamic-only. Also: "smash animation, red, reverse to original position" (mentioned above) means an actual forward **bump** motion before reversing, not just an in-place color flash — see §2.

---

## 1. Core Concept & Benchmark

A 2D grid puzzle game. Arrows (chains of cells) sit on a grid. Tapping an arrow sends it sliding forward, cell by cell, in its head's direction, continuing automatically until it either:

- **Exits** the board (path ahead is fully clear all the way off-grid) → arrow is removed, contributes to level completion, or
- **Blocks** (another arrow is in the way) → bumps forward slightly, flashes red, reverses back to its original position (the move didn't happen).

Level is won when all arrows have exited. Target: **50,000+ levels**.

**Explicit benchmark**: the goal is to match — and exceed — the MultiTech Studio template's quality and, importantly, **performance**, while planning for **more themes than the template ships with**, longer-term. The template is reference/study material only (per the existing rule); we're not reusing its code, but its finished behavior is the bar to clear.

---

## 2. Core Mechanic — CONFIRMED, don't re-litigate

- Grid-based, `Vector2Int` coordinates, one `GridCell` per cell, tracks `Occupant`.
- One tap/click starts **continuous auto-movement** — the arrow keeps advancing on its own (no repeated tapping) until it blocks or exits. Matches reference apps and the MultiTech template's `autoRunToEnd` behavior.
- Movement is smooth/tweened (DOTween), `Ease.Linear`, so consecutive steps flow into each other without decelerate-then-reaccelerate stutter.
- **Blocking**: arrow **bumps forward slightly** (a small partial move toward the obstacle — a "smash" feel), **flashes red**, then **tweens back to its original position**. **CONFIRMED (2026-08-06)** — this is intentionally *more* than the template does (template only flashes color, no bump/position-change at all, confirmed by reading `ArrowLine.StepOnce()`'s blocked branch directly). Not yet built — current code only does an in-place red flash with no position change. **TODO when we resume implementation.**
- **Exiting**: slides off-grid then the arrow object is destroyed.
- Arrows render in **white** by default so per-arrow/per-theme color tinting works (`SpriteRenderer.color` / `LineRenderer.colorGradient`).
- **Background dots** behind the arrows/grid (visible in reference app screenshots) — kept for usability (helps the player read the grid). Not yet built.
- **Click detection: New Input System, not the template's approach.** **CONFIRMED (2026-08-06)** — the template's `ArrowLine.Update()` uses the legacy `Input` class (`Input.GetMouseButtonDown`, `Input.touchCount`) with `Physics2D.OverlapCircleAll`. We deliberately use `UnityEngine.InputSystem` instead (`Mouse.current`, `Touchscreen.current`, etc.) — this was already the standing rule for all of this project's own scripts, reconfirmed here specifically for click detection. What we should still borrow from the template's *architecture* (not its input API): **one click-check per arrow** (e.g. one `Update()`/collider on the whole `LineRenderer`+`EdgeCollider2D`), not one per body segment — our current per-segment `ArrowSegmentClick.Update()` is the old sprite-piece approach and goes away with the LineRenderer rework (§3).

---

## 3. Rendering Architecture — RESOLVED (this was the thing causing churn)

**Decision: hybrid rendering.**

- **Body** (the straight/curved tube connecting tail to head) → a `LineRenderer` (+ `EdgeCollider2D` for click detection), not discrete sprite pieces. `numCornerVertices` set high (e.g. 30–50) gives smooth, gap-free rounded corners for free. This is what the MultiTech Studio template does too (confirmed by reading `Assets/MultiTech Studio/Escape Game/Prefabs/Snake 1.prefab` directly) — every one of its themes (Arrow/Snake/Train) uses the same `LineRenderer`-based body.
- **Head** → a separate sprite object riding the tip of the line, repositioned/rotated in code every frame the line updates (same idea as the template's `headVisual`).
- **Tail** → same idea, a sprite riding the tail end.

**Why**: discrete rigid sprite pieces per grid-cell (the approach originally built) put a separate rigid GameObject in every cell (straight/corner/cap/head). This works fine visually at rest, but during a move, neighboring pieces near a *bend* travel in different cardinal directions simultaneously — since each piece is a fixed rigid shape sliding in its own straight line (not a continuously-redrawn line), they can visibly separate mid-transition. No amount of easing or rotation-smoothing fixes this — it's geometric, not a timing bug. A `LineRenderer` never has this problem because the line connecting any two points is redrawn fresh every frame.

**UPDATE (2026-08-06, later same day) — full reset, art included**: the original plan was to keep the already-built `head.png`/`cap.png` and only delete `segment.png`/`corner.png`. That's superseded — the user chose a **full reset of `Assets/Project`** (see `CLAUDE.md`'s reset banner), and **all 4 sprites were deleted**, not just the two body pieces. The head/tail art will be **redesigned and rebuilt from scratch** as part of the fresh rebuild, not restored from the old files. The architecture decision above (LineRenderer body + separate head/tail sprite objects) still stands — only the specific old PNG files are gone.

---

## 9. Validated Against Template (2026-08-06, re-checked same day after core loop built) — fact-checked, not guessed

Re-read `ArrowController.cs` (ours, now with working move/block/exit) and `ArrowLine.cs` (template) side by side. The core movement algorithm is now built and hands-on-verified — this table reflects the *current* state, superseding the first version written before the LineRenderer rework:

| Dimension | Ours (current code) | Template | Status |
|---|---|---|---|
| Body rendering | `LineRenderer` + `EdgeCollider2D` | `LineRenderer` + `EdgeCollider2D` | ✅ Matches — switched away from sprite pieces as planned |
| Click detection | New Input System (`Mouse.current`) + `EdgeCollider2D.OverlapPoint`, one check per arrow | Legacy `Input` + `Physics2D.OverlapCircleAll`, one check per arrow | ✅ Same architecture (one-check-per-arrow); intentionally different input API per §2 (confirmed, not a gap) |
| Movement algorithm | `MoveStep()`: `dir = head - prev` (grid space), shift path array (drop tail, append new head), tween all `LineRenderer` points `Ease.Linear` via `Vector3.Lerp`, `OnComplete` recurses into `TryAdvance()` | `StepOnce()`: identical structure — `dir = head.G - prev.G`, shift `nodes` array, tween via `DOTween.To` + `Lerp`, `Ease.Linear`, `OnComplete` recurses if `autoRunToEnd` | ✅ Matches — same snake-flow algorithm, confirmed by reading `StepOnce()` directly before building |
| Continuous auto-move | Recurses unconditionally every `MoveStep`/`ExitStepOnce` completion (equivalent to `autoRunToEnd = true` always) | `isMoving` + `queuedSteps` (max 3, queues extra taps) + `autoRunToEnd` (defaults true) | 🟡 Partial — same default behavior (single click runs to completion), we don't queue extra taps while mid-move (not yet decided if we want to, see §2) |
| Blocked animation | Bump forward 30% of a cell → `SetLineColor(red)` → tween back to the pre-attempt position → `SetLineColor(white)` | In-place red flash only via `DOTween.Sequence`, no position change | ✅ Built and matches the confirmed *intentional deviation* (bump+reverse goes beyond the template on purpose, §2) |
| Exit | `StartExit()` releases occupancy, `ExitStepOnce()` shifts the whole line forward by a world-space step (same shift-array pattern as movement) until every point leaves `Camera.main`'s viewport (`WorldToViewportPoint` 0–1 check), then `Destroy(gameObject)` | `ExitStepOnce()`: same shift-array + tween pattern, `IsCompletelyOffscreen()` uses viewport bounds **expanded by an `exitMargin`** (1.5 units) before declaring off-screen, also updates a head-sprite each frame | 🟡 Matches the core algorithm; missing the `exitMargin` safety buffer (ours checks the exact viewport edge, template checks past it) and head-sprite riding the tip (that's the next step, rendering polish) — worth adding the margin when polishing |
| Color flash helper | `SetLineColor(Color)`: sets `material.color` if the shader exposes `_Color`, else falls back to `LineRenderer.startColor/endColor` | `SetLineColor(Color)`: identical fallback logic | ✅ Matches — found and fixed the same vertex-color-vs-material-color gotcha the template already handles |
| Grid/occupant model | `GridCell.Occupant` / `GridBuilder` (grid built in `Awake()`) | `Dot.occupant` / `GridManager` (grid built on-demand via `RebuildSmart()`, called explicitly by a `LevelManager`, not relying on `Awake`/`Start` order) | ✅ Same concept; template avoids the `Awake`/`Start`-ordering question entirely via explicit sequencing — worth adopting once we build our own `LevelLoader` (see CLAUDE.md progress log) |

---

## 4. Theming — Neon Futuristic/Cyberpunk overall art direction + swappable piece-shape themes, planned from v1

**Full design system spec (colors, typography, spacing, component rules): see `Docs/Neon-Theme-Design-System.md`** (user-provided design export, 2026-08-10) — the source of truth for exact hex values and component styling going forward.

**Overall art direction (decided 2026-08-10): Neon Futuristic / Cyberpunk, applied to the WHOLE game** — Splash, Home, HUD/UI chrome, grid background, and arrow/piece rendering all share one visual language: dark backgrounds, neon glow colors (cyan/magenta/purple family), futuristic UI framing. This is not one of several selectable art styles — it is the single committed look for the entire game. Reference: user-provided mockup of a Splash screen ("NEON ROUTE ESCAPE" — glowing rounded-square app icon with a 4-direction arrow motif, neon gradient title text, circular percentage loading spinner + progress bar, dark starfield/neon-streaks background).

**Swappable "theme" system (still planned from v1, scope clarified 2026-08-10): controls PIECE SHAPE, not overall art style.** Originally (2026-08-06) this was meant to cover full alternate visual styles (color/palette per theme, template-style). Clarified: the multi-theme system now specifically means **piece-shape variety** — Arrow, Snake, Train, etc. (matching the range of shapes the MultiTech template offers as its "themes") — with every shape re-skinned in the *same* Cyberpunk neon palette, not each shape getting its own distinct color scheme/art style the way the template does it.

Implication for architecture: whatever holds "how this piece looks" (line width, line color/gradient, head sprite, tail sprite) needs to be **swappable data**, not hardcoded into `ArrowController`. Concretely (exact class names/structure TBD when we build it, not fixed yet):

- A theme = a set of: body width, body color/gradient (all within the neon palette), head sprite, tail sprite, **head sprite rotation offset** (and later, possibly per-theme sound/animation). The offset is needed because `ArrowController.UpdateHeadRotation()` computes a direction angle via `Atan2` and then corrects it to match the head sprite's own default-facing direction (Unity's built-in Triangle sprite points up by default, needing a `-90°` correction to align with a rightward-moving arrow) — a different head sprite drawn facing a different default direction will need a different offset, so this must be per-theme data, not a hardcoded constant.
- `ArrowController` reads "which theme" it should render as, rather than having sprites hardcoded as public fields the way it does today.
- We are **not** reusing the template's `ThemeData`/`ThemeManager` code directly (per the existing rule that `Assets/MultiTech Studio` is reference-only) — we'll build our own equivalent, informed by how theirs works.

This is a bigger architecture implication than it sounds — don't start building it casually; it's its own planned step (see §8).

---

## 5. Meta / UI Features — v1 scope

Confirmed for v1:

- **Lives / Hearts system** — 3 hearts. 3 wrong moves (blocked attempts) = game over → level can be restarted. (Matches reference app screenshot: heart icons in the HUD.)
- **Hint system** — lightbulb icon, tells the player which arrow to move next when stuck.

Not decided yet whether these are needed for the *very first* playable milestone or can wait until after core movement/theming is solid — default assumption is: get one theme fully polished and movement-perfect first, then layer these in. **If this assumption is wrong, say so — don't let it stand as a guess.**

---

## 6. Monetization — OPEN, not decided

Explicitly **not settled yet** — the user wants more discussion before committing to anything here (ads, IAP, theme unlock costs, etc. — the template has `ThemeShopManager`/pricing on `ThemeData` for reference, but we haven't decided if/how we're doing anything similar). **Do not build ad/IAP/shop hooks into the architecture pre-emptively** based on assumption — revisit this section together when the user wants to.

---

## 7. Level Generation — HYBRID (dynamic + manual), with mandatory solvability checking

**Decision**: level content comes from **both** sources, not dynamic-only:

1. **Dynamic/procedural** — levels generated via the **reverse-construction** technique (build a level by placing arrows in the reverse of their removal order, checked only against arrows placed so far — guarantees solvability by construction, no separate solver needed). Output format: **JSON** (or a JSON-backed data structure), not hand-authored — this is how we scale to 50,000+ levels.
2. **Manual** — hand-crafted levels (e.g. for organic/animal/object shapes that a procedural recipe might not produce well, or milestone/showcase levels). Also authored as JSON/data, just by a person instead of the generator.

**Solvability checking applies to manual levels**: because reverse-construction *guarantees* solvability for dynamic levels by how they're built, but a manually-authored level has no such guarantee — it needs to be run through a **completability/solvability checker** (a simulator that tries to fully clear the level, same logic the generator already needs internally) before it ships. Whether dynamic levels are also re-verified through this same checker (belt-and-suspenders) or trusted by construction is an open implementation detail — still not decided, ask before assuming either way (the procedural generator does *not* currently call the checker, it relies on construction alone).

**Done, 2026-08-07** — both halves built and hands-on-verified, ahead of rendering-polish/theming per the re-prioritized order (see Change Log): `LevelGenerator.cs` (`Scripts/Editor/`, reverse-construction + random walk, menu item) and `LevelPainter.cs` (`Scripts/Managers/`, click-drag manual authoring in Play mode, `S` to save). Both output `LevelData` assets to `Assets/Project/Levels/`. The solvability checker (`LevelPainter.IsLevelSolvable()`) is a greedy fixed-point simulator — repeatedly remove any arrow whose exit ray is currently clear until nothing more can be removed; solvable iff every arrow got removed.

### 7b. Second generator: "carve" — 100% fill, for showcase/shaped levels (2026-08-08)

Random-place-and-check (§7 above) plateaus around **87% coverage** on a small shape and **~55%** on a large grid: as the board fills, a randomly-placed arrow almost never finds a clear exit ray. For levels that must be **completely filled** (shaped showcase levels — heart, and the big "needs scrolling" level), the generator is inverted:

> Treat the whole shape as already filled, then **carve arrows out one at a time in solve order**. An arrow may only be carved when its exit ray, from head to board edge, passes **exclusively through already-carved (empty) cells**. This peels the shape outside-in.

Two properties fall out for free, which is why this is the better algorithm and not just a tuning win:
- **Solvability** — the carve order *is* the solve order, so it is correct by construction (same guarantee as reverse-construction, obtained differently).
- **100% coverage** — we are *partitioning* the shape, not randomly filling it, so coverage is total by definition rather than by luck.
- It also **eliminates the self-block bug class entirely**: body cells are by definition still `remaining`, and the ray is only allowed to cross non-`remaining` cells, so an arrow structurally cannot block itself. (§7's generator needed an explicit `ownBody` check bolted on after that exact bug shipped.)

The one real failure mode is **stranded isolated cells** (a cell with no remaining neighbour can't form a ≥2-cell arrow) — first version hit 99.3% for this reason. Fixed by absorbing would-be-isolated neighbours during path growth, plus a repair pass that appends any straggler to an adjacent arrow's **tail** — provably safe, since the tail never affects the head's ray, and a never-carved cell is one that no ray ever crossed.

Both generators are kept: §7's for ordinary sparse/easy levels, this one for filled/shaped levels. Menu: `Generate Level`, `Generate Heart Level`, `Generate Huge Level`.

**Note for the Hint system (§5)**: this generator emits arrows in **exact solve order**. Persisting that order into `LevelData` is all a real hint needs.

---

## 8. Build Order (what's next, in order)

**Re-prioritized 2026-08-07** — timeline is tight, so the order now favors reaching a playable end-to-end loop first; theming/pooling/monetization move later. Steps 1–2 are done:

1. ~~Rework `ArrowController` rendering to the hybrid `LineRenderer` + head/tail sprite approach (§3)~~ — **done** 2026-08-07, single theme (placeholder Triangle/Knob sprites), smooth and gap-free.
2. ~~Build the proper blocked-animation (bump forward → red flash → reverse to original position)~~ — **done** 2026-08-06, see §2.
3. **Lives/Hearts + Hint system (§5)** — next up.
4. **Procedural level generator + manual level + solvability checker (§7)** — the actual 50,000-levels content pipeline; needed for a genuinely playable game, so pulled forward ahead of theming.
5. Theming data structure (§4) — deferred from its original position (was step 3) specifically to get a playable loop faster; single-theme (Arrow, placeholder sprites) is enough until this point.
6. Object pooling — still deferred until many levels are actually being cycled through in testing.
7. Monetization (§6) — whenever the user wants to pick this back up.

---

## 10. Camera Framing & Navigation — RESOLVED (2026-08-08), don't re-litigate

This caused real churn (two wrong implementations before the right one), so the model is locked here.

**Decision: constant cell size, zoom-OUT only, plus pinch-zoom and 4-way pan.**

- **A grid cell is the same size on screen in every level.** `CameraController.cellsVisibleVertically` (≈32) is the design constant: it defines how many cells fit vertically at default zoom, and therefore how many pixels a cell occupies. This number was derived by **measuring the reference app's screenshots in pixels** (≈57px per cell on 1080×1920), not guessed.
- **The camera only ever zooms OUT to fit, never IN.** A level smaller than the default view **stays small and centred with empty space around it** — it is *not* blown up to fill the screen. This is precisely what makes the reference app feel premium: arrow thickness relative to the grid reads identically whether the level has 5 arrows or 300.
- **Line width stays constant in world units** (0.25 of a cell — also measured from the reference). Never scale line width or head/tail sprites with camera zoom; that destroys the constant proportion above. *(A previous attempt did exactly this and had to be reverted.)*
- **Framing centres on the actual arrow bounding box**, not the declared grid rect, so sparse levels still centre correctly.
- **`maxCellsVisibleVertically` caps zoom-out**, which is what makes very large levels genuinely require panning instead of being auto-fitted into an unreadable thumbnail.
- **Pan is clamped to content bounds + a `panPadding` margin**, so scrolling to an edge shows some empty space past the shape (reference behaviour) rather than jamming content against the screen edge.
- **Tap vs. drag arbitration is mandatory, not polish.** Input is read **once** in `CameraController`; only a genuine tap (movement below `dragThresholdPixels`, no active pinch) is forwarded to arrows. Without this, panning drags across arrows and fires every one of them. Bonus: hundreds of arrows no longer each poll input every frame.

**Rejected alternatives** (both were built and reverted — do not revisit without a reason):
1. *Fit-to-screen every level* (`Clamp(contentFitZoom, min, max)`) — zooms IN on small levels, making arrows look enormous and inconsistent between levels.
2. *Scaling line width/sprites by zoom to compensate* — treats the symptom, breaks the arrow-vs-cell proportion that the whole look depends on.

**The template does none of this automatically** — it stores a hand-tuned `origin` and `camSize` per level, authored in its Level Editor. That cannot scale to 50,000 levels, so ours is computed at load time. This is a deliberate, documented deviation.

---

## 11. Responsive UI Architecture — LOCKED (2026-08-11)

Splash/Home panels shipped without a real multi-device strategy, and it showed: title text pushed off-screen on some aspect ratios, notch/cutout devices clipped edge content, bottom-nav icons looked oversized or distorted depending on device, and some icons/text looked blurry. Root-caused by reading the actual `Game.unity` RectTransform values (not guessed) and cross-checked against current (2026) Unity UI guidance before deciding. Four pillars, all UGUI (see rejected-alternative note below):

1. **Anchors, not fixed pixel offsets.** Every UI element must use an edge/corner-relative Anchor Preset appropriate to where it visually belongs (top, bottom, center, stretch) instead of a center-anchor + hardcoded `anchoredPosition`/`sizeDelta` tuned to one assumed resolution. This was the direct cause of `TitleText`/`PlayButton` drifting off-screen on non-reference aspect ratios.
2. **Safe Area handling is mandatory**, via a reusable `SafeArea` component built on `Screen.safeArea` (recomputed on resolution/orientation change), wrapping any panel content that hugs a screen edge (e.g. `BottomNav`, top title bar). Same API covers both iOS notch/Dynamic Island and Android cutouts — no per-platform branching needed.
3. **Icon/text sizing is proportional, not fixed-pixel.** Icons use `AspectRatioFitter` + a parent Layout Group with Control Child Size enabled, so size is a function of the container, not a hardcoded `sizeDelta` (e.g. `40x40`) that becomes a larger or smaller fraction of the screen as `CanvasScaler`'s effective canvas dimensions shift per device aspect ratio. TextMeshPro Auto Size is enabled for **static** labels only (title, level number, nav labels) — never for frequently-changing text, which would repeatedly rebuild the font atlas and cost performance.
4. **Performance budget alongside the visual fix**: all UI icons/shapes packed into one Sprite Atlas (Tight packing, under 2048×2048), Mip Maps stay off for all UI textures, small icon/shape sprites use Texture Compression None/High Quality (negligible memory cost, removes block-compression softening that was reading as "blurry"), and `Raycast Target` is disabled on every non-interactive `Image` (decorative icons, background panels) to cut `GraphicRaycaster` overhead.

### 11a. SUPERSEDED (2026-08-11, same day) — reversed in favor of UI Toolkit, see §11b

The original §11 kept UGUI and rejected UI Toolkit, reasoning that this game's UI isn't dynamic/data-heavy enough to justify a rebuild. That call is reversed after a second, more targeted R&D pass specifically on graphics/performance (not just architecture): UI Toolkit's **textureless vector rendering** structurally eliminates the blur problem this section spent most of its effort working around (sprite compression tuning, mip map/DPI fiddling) — a PNG icon upscaled on a high-DPI or tablet screen can blur no matter how well its import settings are tuned, but a vector shape is resolution-independent by construction. Given the user's top priority is graphics/performance quality, this structural advantage outweighs the rebuild cost. User made the call explicitly after being shown the trade-off (rebuild cost + new-language learning curve for a beginner, vs. the structural fix + Unity's active 2026 investment in UI Toolkit while UGUI is in maintenance mode).

**What carries over conceptually** (the four pillars' *goals* are unchanged, only the implementation mechanism changes): no element should ever be pushed off-screen by aspect ratio, safe-area-obstructed zones must be respected, icon/text sizing must be proportional not fixed-pixel, and small/tablet devices must read at a comparable perceived size. **What does NOT carry over**: `CanvasScalerAutoFit.cs`, `SafeArea.cs`, `BottomNavBackgroundFitter.cs`, and every `RectTransform`/`Image`/`TextMeshProUGUI` UGUI component in the existing Splash/Home/Gameplay/Win/Lose hierarchy — all UGUI-specific, to be replaced by UI Toolkit's own primitives (see §11b).

### 11b. UI Toolkit Migration — LOCKED (2026-08-11)

**Decision: full rebuild of all UI (Splash, Home, Gameplay HUD, Win/Lose) in UI Toolkit (UXML + USS + `UIDocument`), replacing UGUI entirely.** Not a partial/hybrid adoption — mixing both systems mid-project would mean maintaining two UI mental models and two responsive strategies at once, which defeats the point of switching.

How the same underlying goals get solved in UI Toolkit's own terms (research-backed, not guessed — this needs verifying against official Unity 6 UI Toolkit docs as the rebuild reaches each point, since exact API names should be confirmed against the installed Editor version before locking further):
- **Aspect-ratio safety**: UI Toolkit's layout is Flexbox-based (USS `flex-grow`/`flex-shrink`/percentage sizes), which is natively more responsive than UGUI's anchor system — closer to how responsive web layouts work. Still needs verifying there's an equivalent guarantee to UGUI's `ScreenMatchMode.Expand` "never crops" behavior; `PanelSettings` has its own scale-mode options (mirroring `CanvasScaler`'s Constant Pixel / Scale With Screen Size / Constant Physical Size) and the same DPI-reliability caveats found for UGUI likely apply here too — confirm before relying on Constant Physical Size.
- **Safe Area**: no automatic handling in UI Toolkit either — needs the same `Screen.safeArea` read, applied as USS padding/margin on the root `VisualElement` instead of `RectTransform` anchors. Conceptually the same fix as `SafeArea.cs`, different mechanism.
- **Icon/text sizing**: vector graphics (where source art allows) resolve the proportional-sizing and blur problems simultaneously — no `AspectRatioFitter` equivalent needed if icons are true vector shapes rather than raster PNGs re-imported as sprites. Text rendering approach (whether UI Toolkit's default text engine is SDF-based/crisp-at-scale like TextMeshPro, or whether TextMeshPro integration is still needed/possible) needs confirming during the rebuild, not assumed.
- **Tablet perceived-size boost**: same diagonal-inches device-category concept from §11a still applies — implementation moves to adjusting `PanelSettings` scale or a root-level USS transform instead of a `Canvas.scaleFactor` multiplier.

**Rebuild order** (proposed, confirm with user before starting each): teach UI Toolkit fundamentals first (UXML structure, USS styling, `UIDocument`/`PanelSettings`, C# `VisualElement` query/binding — none of this exists in `Docs/Unity-Learning-Notes.md` yet, all net-new for the beginner-teaching approach), then rebuild Splash (simplest, fewest elements) end-to-end as the first real example before touching Home/Gameplay/Win-Lose. PrimeTween's UGUI-tween usage (splash progress bar, hint pulse) needs an equivalent for UI Toolkit — UI Toolkit supports USS transitions and direct style-property animation, but whether PrimeTween integrates directly or a different animation approach is needed is to be confirmed when that point in the rebuild is reached, not assumed now.

### 11c. UI Toolkit technical findings (confirmed 2026-08-11, real project-specific gotchas)

Answers to the "to be confirmed during rebuild" questions in §11b, now verified through actually building Splash + Home:

- **Unity version matters for setup**: on Unity 6.5 (6000.5.x), the `UIDocument` component is superseded by a new **`PanelRenderer`** component (`GameObject > UI Toolkit > Panel Renderer`) — `UIDocument` still works but is labeled "Legacy" in the Editor menus. Always check the actual installed Editor version before giving UI Toolkit setup steps; menu paths and recommended components have changed between Unity versions.
- **`PanelSettings` needs the SAME reference-resolution care as `CanvasScaler`** — it is not configured sensibly by default. Unity's default `PanelSettings` reference resolution is `1200x800` (landscape!), completely wrong for a portrait mobile game. Fixed to `1080x1920` with `screenMatchMode: Expand` (mirrors `CanvasScaler.ScreenMatchMode.Expand` from §11 — never crops/pushes content off-screen, only ever adds extra space). This is a required setup step, not an optional tune-up — skipping it caused a real, confusing "content way too small in the corner" / bottom-nav-overflow bug in this session.
- **9-slice background images on a plain `VisualElement` are unreliable in practice — avoid them for critical shapes.** Chased a real bug for multiple rounds: (1) `-unity-slice-scale` looked like a reasonable property to set explicitly but is a **multiplier**, not a literal size — worse, for Sprite-typed images Unity auto-multiplies it by `spritePixelsToUnits ÷ PanelSettings.referenceSpritePixelsPerUnit`, so a sprite imported at PPU 256 against a panel's default reference PPU of 100 gets its slice regions silently scaled ~2.56× larger than intended, even leaving `-unity-slice-scale` unset doesn't disable this auto-adjustment. (2) Even after fixing the sprite's PPU to match (100) and adding the required `-unity-background-scale-mode: stretch-to-fill` (9-slicing does not apply correctly without this — confirmed via Unity Discussions, this is a real prerequisite, not optional), the slicing still rendered as a stretched oval instead of a proper 9-sliced stadium. **Root cause not fully identified** — abandoned the approach rather than keep burning time on it (explicit user call: stop chasing an intermittent bug, ship something that reliably works).
  - **Practical resolution used instead**: for shapes needing a stadium/pill with a border (PLAY button, difficulty pill), use a **plain `VisualElement` with explicit `height` + `border-radius` set to exactly half that height + a solid-color `border-width`/`border-color`** — no image at all. This is fully reliable (proven working for both the difficulty pill and the PLAY button) and costs only the loss of the cyan→magenta *gradient* border (now solid cyan) — acceptable tradeoff given USS has no native gradient support at all anyway (confirmed: no `linear-gradient()` equivalent exists in USS as of this Unity version). A true gradient border, if wanted later, needs a different technique (e.g. two overlapping elements, or a custom shader) — not attempted this session.
- **`border-radius` needs an exact pixel value matching half the element's actual (not assumed) height — large "escape hatch" values like `9999px` (the common CSS trick for "always a pill") do NOT reliably clamp the same way in USS.** Confirmed via Unity Discussions: Unity's clamping can produce **elliptical, not circular, corners** when the value is large relative to a non-square element, especially when the element's true rendered height isn't precisely known upfront (padding-driven auto-height is harder to predict than an explicit `height`). Fix pattern: give the element an explicit `height`, then set `border-radius` to precisely `height / 2` — don't rely on oversized values or padding-derived auto-sizing to "just clamp correctly."
- **Text rendering**: not yet specifically stress-tested for crispness/SDF-equivalence vs TextMeshPro — still an open item, not confirmed either way.
- **PrimeTween × UI Toolkit**: confirmed working via the Splash screen (`SplashScreenController.cs`) — `Tween.Custom(0f, 1f, ...)` driving a `ProgressBar.value`/`.title` inside a `PanelRenderer.RegisterUIReloadCallback` works identically in spirit to the old UGUI pattern, just retargeted at UI Toolkit's control properties instead of `Image.fillAmount`.

Sources: [Unity UI Toolkit vs UGUI: 2025 Developer Guide — Angry Shark Studio](https://www.angry-shark-studio.com/blog/unity-ui-toolkit-vs-ugui-2025-guide/), [Unity — Comparison of UI systems](https://docs.unity3d.com/Manual//UI-system-compare.html), [Unity — Migrate from uGUI to UI Toolkit](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-Transitioning-From-UGUI.html), [Turiyaware — Resolution Independent UI in Unity](https://www.turiyaware.com/the-quest-for-a-resolution-independent-ui-in-unity/)

---

## Change Log

*(Newest at top — one line per decision change, so we can see when/why something flipped.)*

- **2026-08-11 (reversed same day: full UI Toolkit migration, UGUI rejected)**: Added §11b, superseding §11's "stay on UGUI" call from earlier the same day (renamed to §11a). A second, graphics/performance-specific R&D pass (user explicitly asked for this after the tablet-scaling research kept surfacing patch-over-patch fixes) found UI Toolkit's textureless vector rendering structurally eliminates the blur problem §11a spent most of its effort compensating for (sprite compression, mip maps, DPI-based scaling) — a PNG icon can blur when upscaled no matter how well tuned; a vector shape can't. User chose the full rebuild after seeing the honest trade-off (beginner + tight timeline + already-built UGUI screens, vs. Unity's 2026 active investment in UI Toolkit while UGUI is maintenance-only). All UGUI-specific work from this same day (`CanvasScalerAutoFit.cs`, `SafeArea.cs`, `BottomNavBackgroundFitter.cs`, the Safe Area Content wiring) does not carry forward — to be replaced by UI Toolkit's own mechanisms once the rebuild reaches each point (exact APIs to be confirmed against the installed Unity 6 Editor version during the rebuild, not assumed in advance).
- **2026-08-11 (responsive UI architecture locked)**: Added §11 after real multi-device bugs surfaced on Splash/Home (title text off-screen, notch clipping, bottom-nav icons inconsistently sized/distorted, blurry icons/text) — root-caused against actual `Game.unity` values, not guessed. Locked on 4 pillars, staying on UGUI: Anchor Presets (not fixed pixel offsets), a `SafeArea` component (`Screen.safeArea`-based), proportional icon/text sizing (`AspectRatioFitter` + Layout Group Control Child Size + TMP Auto Size for static labels only), and a performance pass (Sprite Atlas, no UI mip maps, Texture Compression None/High for small icons, Raycast Target off on non-interactive images). Explicitly rejected migrating to UI Toolkit for this project (more "modern" in the abstract, not worth a full UI rebuild at this stage). `CanvasScalerAutoFit` (an earlier same-day attempt using a binary 0/1 match-mode switch) is superseded — it fixed the text-drift bug but caused the bottom-nav icon-sizing regression that triggered this fuller pass; kept in the codebase pending the Anchor/SafeArea/proportional-icon work landing.
- **2026-08-10 (art direction locked: Neon Futuristic/Cyberpunk)**: Rewrote §4. The whole game (not just one selectable theme) commits to a Neon Futuristic/Cyberpunk look — dark backgrounds, neon glow, futuristic UI — starting with the Splash screen (user provided a reference mockup: "NEON ROUTE ESCAPE", glowing app icon, neon gradient title, circular % loading spinner). The previously-planned "multiple swappable themes" system is NOT dropped, but its scope is clarified: it now means piece-SHAPE variety (Arrow/Snake/Train, matching the template's shape range), all re-skinned in the same neon palette — not separate art styles/palettes per theme like the template does. Confirmed via direct question after an initial misread (first guess was "Cyberpunk replaces the multi-theme plan entirely," which the user corrected — both ideas coexist, just with theme scope narrowed to shape).
- **2026-08-08 (camera framing + 100%-fill generator)**: Locked the camera model in new §10 after two rejected implementations (fit-to-screen-every-level, and zoom-compensated line widths) — the resolved model is constant cell size + zoom-out-only + pinch/pan, with tap-vs-drag arbitration, derived by measuring the reference app rather than guessing. Added §7b: a second, inverted generator ("carve in solve order") that reaches **100% fill** on shaped levels where the original random-place-and-check plateaued at ~87%, and which gets solvability and self-block-immunity for free. Both generators are kept for different level types. Also noted that this generator's output is in exact solve order, which is the missing ingredient the Hint system (§5) was always waiting on. Confirmed with user: the huge showcase level stays a solid rectangle, and the reference's white/dark palette waits for the theming step (§4) rather than being hacked in now.
- **2026-08-07 (build order re-prioritized)**: User flagged a tight timeline ("apde ochi time line che") and asked to move faster without cutting explanation depth. Re-ordered §8: theming (previously step 3) moved to step 5, after Lives/Hearts+Hint (§5) and level generation (§7) — goal is a playable end-to-end loop sooner; single-theme placeholder sprites are enough until theming is actually reached. Rendering polish (head/tail sprites riding the `LineRenderer`, §3) completed and verified same day.
- **2026-08-06 (core loop built)**: Rebuilt the core arrow gameplay loop from scratch on `Assets/Project` (grid → level data → `ArrowController` move/block/exit) and hands-on-verified each piece in the Unity Editor. Re-validated against the template post-build — rewrote §9's comparison table since most rows were written *before* the code existed and are now stale; current code matches the template's movement/exit algorithms structurally, confirmed the bump+reverse deviation is built and working, and found the template's `GridManager` avoids Unity's `Awake`/`Start` ordering ambiguity entirely via explicit sequencing (a real bug we hit and fixed differently, worth adopting once a `LevelLoader` exists). See `CLAUDE.md` progress log for the play-by-play (including two script-execution-order/shader-color bugs found and fixed).
- **2026-08-06 (full reset)**: User chose to wipe all of `Assets/Project` (scripts, prefabs, levels, all 4 sprites) back to empty folders and rebuild everything from scratch, step by step, specifically to learn it properly rather than inherit patched-together code. See `CLAUDE.md`'s reset banner for exactly what was deleted. This doc's *decisions* (§1–9) still stand as the target architecture for the rebuild — only the old implementation/art files are gone, not the plan.
- **2026-08-06**: Confirmed bump+reverse blocked animation goes beyond the template on purpose (template only flashes color, verified by reading its code). Confirmed click detection uses New Input System, not the template's legacy `Input` class (architecture idea of one-check-per-arrow still borrowed). Added §9 fact-checked comparison table against the template. Confirmed `segment.png`/`corner.png` and the sprite-segment concept are being deleted everywhere (assets + code + docs), not just left unused.
- **2026-08-06**: Added explicit benchmark (match/exceed MultiTech template quality+performance, more themes long-term), hybrid level generation (dynamic + manual, with mandatory solvability checking for manual levels), clarified blocked-arrow animation is a bump-then-reverse (not just in-place flash), preserved user's original verbatim notes in §0.
- **2026-08-06**: Initial version. Locked: hybrid LineRenderer+sprite rendering, multi-theme from v1, Lives+Hints for v1 meta, monetization left open.
