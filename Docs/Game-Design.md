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

## 4. Theming — multiple themes, planned from v1, more than the template long-term

**Decision: build for multiple swappable visual themes from the start** (e.g. Arrow, Snake, Fish, etc. — same idea as the template's theme list), not just the one Arrow look — and plan to eventually ship **more themes than the template has**.

Implication for architecture: whatever holds "how this arrow looks" (line width, line color/gradient, head sprite, tail sprite) needs to be **swappable data**, not hardcoded into `ArrowController`. Concretely (exact class names/structure TBD when we build it, not fixed yet):

- A theme = a set of: body width, body color/gradient, head sprite, tail sprite (and later, possibly per-theme sound/animation).
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

**Solvability checking applies to manual levels**: because reverse-construction *guarantees* solvability for dynamic levels by how they're built, but a manually-authored level has no such guarantee — it needs to be run through a **completability/solvability checker** (a simulator that tries to fully clear the level, same logic the generator already needs internally) before it ships. Whether dynamic levels are also re-verified through this same checker (belt-and-suspenders) or trusted by construction is an open implementation detail for when we build this — not decided yet, ask before assuming either way.

Not yet started — deferred until rendering (§3) and theming (§4) are settled, per the build order below.

---

## 8. Build Order (what's next, in order)

1. Rework `ArrowController` rendering to the hybrid `LineRenderer` + head/tail sprite approach (§3) — single theme (current Arrow look) first, prove it's smooth and gap-free.
2. Build the proper blocked-animation (bump forward → red flash → reverse to original position) — see §2 TODO.
3. Design the theming data structure (§4) once the single-theme version is solid — don't build theming *and* fix rendering at the same time, that's exactly the kind of combined change that causes churn.
4. Lives/Hearts + Hint system (§5).
5. Procedural level generator + manual level + solvability checker (§7) — independent of the above, can happen in parallel or after.
6. Object pooling — deferred until many levels are actually being cycled through in testing.
7. Monetization (§6) — whenever the user wants to pick this back up.

---

## Change Log

*(Newest at top — one line per decision change, so we can see when/why something flipped.)*

- **2026-08-06 (core loop built)**: Rebuilt the core arrow gameplay loop from scratch on `Assets/Project` (grid → level data → `ArrowController` move/block/exit) and hands-on-verified each piece in the Unity Editor. Re-validated against the template post-build — rewrote §9's comparison table since most rows were written *before* the code existed and are now stale; current code matches the template's movement/exit algorithms structurally, confirmed the bump+reverse deviation is built and working, and found the template's `GridManager` avoids Unity's `Awake`/`Start` ordering ambiguity entirely via explicit sequencing (a real bug we hit and fixed differently, worth adopting once a `LevelLoader` exists). See `CLAUDE.md` progress log for the play-by-play (including two script-execution-order/shader-color bugs found and fixed).
- **2026-08-06 (full reset)**: User chose to wipe all of `Assets/Project` (scripts, prefabs, levels, all 4 sprites) back to empty folders and rebuild everything from scratch, step by step, specifically to learn it properly rather than inherit patched-together code. See `CLAUDE.md`'s reset banner for exactly what was deleted. This doc's *decisions* (§1–9) still stand as the target architecture for the rebuild — only the old implementation/art files are gone, not the plan.
- **2026-08-06**: Confirmed bump+reverse blocked animation goes beyond the template on purpose (template only flashes color, verified by reading its code). Confirmed click detection uses New Input System, not the template's legacy `Input` class (architecture idea of one-check-per-arrow still borrowed). Added §9 fact-checked comparison table against the template. Confirmed `segment.png`/`corner.png` and the sprite-segment concept are being deleted everywhere (assets + code + docs), not just left unused.
- **2026-08-06**: Added explicit benchmark (match/exceed MultiTech template quality+performance, more themes long-term), hybrid level generation (dynamic + manual, with mandatory solvability checking for manual levels), clarified blocked-arrow animation is a bump-then-reverse (not just in-place flash), preserved user's original verbatim notes in §0.
- **2026-08-06**: Initial version. Locked: hybrid LineRenderer+sprite rendering, multi-theme from v1, Lives+Hints for v1 meta, monetization left open.
