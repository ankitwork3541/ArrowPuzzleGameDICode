# ArrowPuzzleGame — Project Context for Claude

This file persists project context across sessions/context-compaction. Keep it updated as work progresses — update the "Progress Log" section after finishing each meaningful step, don't wait to be asked.

## ⚠️ FULL RESET — 2026-08-06 — READ THIS FIRST

`Assets/Project` was **intentionally wiped back to empty folders** on this date, on the user's explicit, twice-confirmed instruction (after being shown exactly what would be lost, including working code that had no bugs). **Nothing in `Assets/Project` exists anymore except empty folders** (with their `.meta` files). Concretely, all of these are **gone**, not just "being reworked":

- All scripts: `ArrowController.cs`, `ArrowSegmentClick.cs`, `GridBuilder.cs`, `GridCell.cs`, `LevelLoader.cs`, `ArrowPathData.cs`, `LevelData.cs`
- All prefabs: `ArrowControllerPrefab.prefab`, `ArrowSegment.prefab`, `Cell.prefab`
- The level asset: `TestLevel1.asset`
- All 4 sprites: `segment.png`, `corner.png`, `head.png`, `cap.png` (even though `Docs/Game-Design.md` had said head/cap survive — that was superseded, all 4 are gone, art will be redone from scratch too)

**What's NOT deleted**: `Assets/Project/Scenes/Game.unity` (the scene file itself still exists, but its GameObjects now have missing/broken script references since the scripts they pointed to are gone — that's expected, not a bug to fix, it'll resolve itself as scripts get rewritten and reattached). `Assets/MultiTech Studio/` (reference template, untouched). `Docs/` (this file, `Game-Design.md`, `Unity-Learning-Notes.md`).

**Why**: this followed a long back-and-forth over rendering architecture (sprite pieces vs. `LineRenderer` — see the historical log below) that ended with the user wanting a completely clean slate to rebuild everything from zero, step by step, specifically *because* they want to actually learn it properly as a beginner rather than inherit a patched-together history. See `feedback-ask-dont-guess-game-design` and `feedback-beginner-teaching-style` memories for the exact moments this got corrected.

**What this means for resuming work**: there is currently **no game code at all**. The "Architecture Decisions" section below is now a *plan*, not a description of what exists — treat every item in it as **not yet built**. Start the rebuild the same way the project started the very first time: teach Unity fundamentals as needed, build in the smallest possible steps, the user types/applies each step themselves in the Editor, test before advancing. Do not skip ahead assuming any shortcut is safe because "we built this before" — we are rebuilding on purpose.

**Rebuild starting point**: read `Docs/Game-Design.md` in full first — it has the resolved architecture decisions (LineRenderer + head/tail sprite hybrid, multi-theme-ready, New Input System, bump+reverse blocked animation, hybrid level generation) that the *new* build should follow from the very first line of code, so we don't rebuild the old (now-deleted) sprite-piece approach by habit.

---

## What This Project Is

A 2D Unity arrow-path puzzle game targeting **50,000+ levels**. Mechanic: arrows sit on a grid as a chain of cells; tapping an arrow slides it forward one cell in its head's direction (like a snake), continuing automatically until it either exits the board (path clear all the way off-grid) or blocks (another arrow in the way — bumps forward slightly, flashes red, reverses back). Full product-level detail lives in `Docs/Game-Design.md`.

## CRITICAL — How To Teach/Work With This User

The user is a **complete Unity/programming beginner** ("hu beginner chu, khali Scene/Game view ane Object add karta avde che, bas" / later reconfirmed: "hu beginner chu, pro developer nathi"). This drives everything about how work should be delivered:

- **Step by step, one small piece at a time.** Never dump a large finished system at once — break it into the smallest teachable chunks.
- **The user types/applies the code themselves in Unity for anything that's a new system or a rebuild** — don't use Write/Edit to author game code for them. The line: small bug-fix patches to already-taught code are fine for Claude to apply directly *when explicitly asked to*; building or rebuilding a whole system (like this entire project, right now) is always taught step by step with the user doing the typing, even if a similar system existed before. Documentation files (this file, `Docs/*.md`) are fine for Claude to write/edit directly — this rule is about *game code*, not docs.
- **Simple, detailed language.** Explain the *why* behind each concept in plain terms before/alongside the code, not just "here's the code." Concepts must be taught, not just delivered.
- **Test-before-advancing.** After each step, the user tests it hands-on in the Unity Editor and reports back (often with a screenshot) before moving to the next step. Do not assume something works — wait for explicit confirmation, and don't infer "it worked" from an ambiguous short reply.
- **One concept, one code change per message.** Don't bundle multiple new concepts (e.g. two new methods plus Inspector wiring plus a test) into a single message.
- **Ask, don't guess or assume — on game design AND on implementation approach.** This has cost real, already-built work twice now (see memories). If a decision isn't written down in `Docs/Game-Design.md` or this file, stop and ask.
- **All conversational replies are in Gujarati written in Latin/English letters (romanized)** — global instruction, never Gujarati script.
- All code, comments, commands, file paths, identifiers stay in English (repo-English rule, also global).

If a context-compaction or new session loses the above nuance, **this section is the source of truth** — re-read it before resuming work.

## Reference Docs

- **`Docs/Game-Design.md`** — **read this FIRST, before anything else, before writing a single line of the rebuild.** Locks down *product-level* decisions: core mechanic, the resolved LineRenderer+sprite hybrid rendering architecture, multi-theme-from-v1, New Input System for click detection, the confirmed bump+reverse blocked-animation, v1 meta features (Lives/Hearts, Hints), monetization (left open), hybrid level generation (dynamic+manual with solvability checking), and a fact-checked comparison against the MultiTech template. Update its Change Log whenever a product decision flips — and if a decision isn't recorded there, **ask the user, don't guess**.
- **`Docs/Unity-Learning-Notes.md`** — comprehensive English-language notes covering Unity fundamentals taught so far (GameObject/Component model, MonoBehaviour lifecycle, New Input System, List/Dictionary, ScriptableObject, grid math, DOTween, Object Pooling). These fundamentals don't need re-teaching from zero just because the code was deleted — check here before re-explaining something already covered, and add to it when a genuinely new fundamental comes up during the rebuild.
- **`Assets/MultiTech Studio/`** — a purchased template, used **only as reference/study material** for architecture ideas. Not reused directly — all real game code is written from scratch by the user in `Assets/Project`, under guidance. License: Commercial use + modification allowed, redistribution/sublicensing not allowed.

## Architecture Decisions (PLANNED — nothing below this line exists in code yet, see reset notice above)

- **New Input System** (`UnityEngine.InputSystem`) for all of the user's own scripts.
- **Hybrid rendering**: `LineRenderer` (+ `EdgeCollider2D`) for the arrow body (smooth, gap-free, native rounded corners via `numCornerVertices`), with separate hand-drawn sprite objects for the head and tail riding the line's ends. Full rationale in `Docs/Game-Design.md` §3.
- **Click detection**: one check per arrow (New Input System), not per body segment.
- **Data-driven levels**: grid dimensions + arrow paths as data (ScriptableObject and/or JSON — exact format TBD when we rebuild level loading).
- **Multi-theme from the start**: whatever renders an arrow's look (body width/color, head/tail sprite) needs to be swappable data, not hardcoded.
- **50,000+ levels**: hybrid generation — procedural (reverse-construction technique, guarantees solvability by construction) + manual hand-crafted levels, with a solvability checker mandatory for manual ones. Not started.
- **Object pooling**: deferred until many levels are actually being cycled through in testing.

## Script Folder Structure

`Assets/Project/Scripts/` has these subfolders (created before the reset, kept empty until now) — put every new script in the matching one so the project stays organized as it grows:

- **`Core/`** — core gameplay logic: grid system, movement, blocking/exit logic (e.g. `GridCell.cs`, `GridBuilder.cs`). Plain data classes and MonoBehaviours that drive the actual game mechanic live here.
- **`Data/`** — data models: `ScriptableObject` classes and `[System.Serializable]` plain classes that hold level/theme/config data (e.g. `LevelData.cs`, `ArrowPathData.cs`).
- **`Managers/`** — manager-style MonoBehaviours that coordinate systems across the scene (e.g. a future `LevelManager`, `GameManager`) — not used yet.
- **`UI/`** — scripts driving UI elements (hearts/lives display, hint button, score/level text) — not used yet.
- **`Utils/`** — generic reusable helpers not tied to this game's specific mechanic — not used yet.

Other relevant top-level folders: `Assets/Project/Prefabs/`, `Assets/Project/Levels/` (for saved `LevelData` assets), `Assets/Project/Sprites/`, `Assets/Project/Scenes/` (contains `Game.unity`).

If a new script doesn't clearly fit one of the above, ask the user rather than guessing a new folder.

## Progress Log

*(Newest at top. Update this after each completed step.)*

- **2026-08-06 — Rebuild started, Grid system done, Level data model in progress.** Confirmed starting point with user: Grid system first (per `Docs/Game-Design.md` §8 order, adapted for full reset — grid/level-data now come before the LineRenderer rework since nothing exists yet). Built and hands-on-tested: `GridCell.cs` (plain data class, `Scripts/Core/`) with `Position`+`Occupant`; `GridBuilder.cs` (MonoBehaviour, `Scripts/Core/`) building a `Dictionary<Vector2Int, GridCell>` via nested loop, `OnDrawGizmos()` visualizing cells, plus `GetCell`/`IsCellOccupied`/`SetOccupant`/`ClearOccupant` helper methods — all tested via Console logs and Scene view gizmos. Matches template's `Dot.cs`/`GridManager.cs` concept (per `feedback-verify-against-template-each-step` memory — checking this every step now, not just once). Currently mid-step on `ArrowPathData.cs`/`LevelData.cs` (`Scripts/Data/`) for the level data model. Added a `Script Folder Structure` section to this file per user request, so file placement doesn't need re-asking each time.
- **2026-08-06 — FULL RESET.** See banner at the top of this file. Deleted all of `Assets/Project`'s scripts/prefabs/levels/sprites (kept empty folder structure) after a long rendering-architecture debugging saga (condensed history below) ended with the user wanting to rebuild everything from scratch, step by step, to actually learn it. Next actual step: start rebuilding, beginning with whatever the user wants to tackle first per `Docs/Game-Design.md` (grid, then movement, then rendering, etc. — ask, don't assume the order).

<details>
<summary><b>History before the full reset (2026-08-06 and earlier) — kept for lessons learned only. None of this code exists anymore.</b></summary>

- Spent a long session chasing a corner-turn visual gap/flicker in a discrete-sprite-piece rendering approach for `ArrowController` (segment/corner/head/cap sprites, one GameObject per grid cell). Tried: recalculating sprite+rotation on every move, deferring sprite swap to `.OnComplete()`, `Ease.Linear`, continuous auto-move via `isMoving`+`DOVirtual.DelayedCall`, smooth `DORotate`. None fully fixed it — **root cause was geometric, not timing**: independent rigid sprites near a bend travel in different cardinal directions during the same step and can visually separate mid-tween; a `LineRenderer` never has this because the line between any two points is redrawn fresh every frame.
- Deep-dived the MultiTech Studio template's actual technique (`Assets/MultiTech Studio/Escape Game/Prefabs/Snake 1.prefab`, `ArrowLine.cs`): confirmed every theme uses the same `LineRenderer`+`EdgeCollider2D` body (`numCornerVertices: 50` for free smooth rounding) with a separate sprite-based head object riding the line's tip. This is where the resolved hybrid architecture decision (now in `Docs/Game-Design.md`) came from.
- Did a fact-checked line-by-line comparison of `ArrowController.cs` vs. the template's `ArrowLine.cs` before deciding anything — found the template's blocked-animation is just a color flash (no bump), confirming the user's "bump + reverse" idea is intentionally beyond the template, not a misunderstanding of it.
- Two real bugs worth remembering for the rebuild: (1) leftover dead code silently overwriting a later fix — when replacing a code block during a later step, make sure the *old* block is actually deleted; (2) Unity's sprite import auto-trims to the alpha bounding box and centers the pivot on *that trimmed rect*, not the full design canvas — symmetric sprites hid this, asymmetric ones (a corner piece, a triangle) showed visible gaps until the sprite rect was manually set to the full canvas size.
- Before the reset: built and hands-on-verified a full working core loop from scratch (grid, ScriptableObject level data, arrow movement/blocking/exit, automatic level loading, all 4 tube sprites imported and rendering) — all 10 Unity-fundamentals lessons that produced that work are still valid and documented in `Docs/Unity-Learning-Notes.md`, only the *code* is gone.

</details>

## Next Steps

1. **Ask the user where they want to start.** Likely candidates, in a sensible order, but confirm rather than assume: (a) grid system (`GridBuilder`/`GridCell` equivalents), (b) level data model, (c) arrow movement/blocking/exit core logic, (d) the `LineRenderer`+head/tail-sprite rendering, (e) New Input System click detection. Since fundamentals like GameObject/Component/MonoBehaviour/ScriptableObject/grid-math were already taught and are documented in `Docs/Unity-Learning-Notes.md`, it's fine to move a bit faster through concepts the user has already demonstrably learned — but still teach each *new* system step by step, and still let the user type/apply the code.
2. Everything else in `Docs/Game-Design.md` §8 (Build Order) applies once the rebuild reaches that point again.
