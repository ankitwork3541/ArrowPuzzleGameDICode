# ArrowPuzzleGame — Project Context for Claude

This file persists project context across sessions/context-compaction. Keep it updated as work progresses — update the "Progress Log" section after finishing each meaningful step, don't wait to be asked.

## What This Project Is

A 2D Unity arrow-path puzzle game targeting **50,000 levels**. Mechanic: arrows sit on a grid as a chain of cells; tapping an arrow slides it forward one cell in its head's direction (like a snake). If the cell ahead is empty, it moves; if the path ahead is fully clear all the way off the board, it exits and is removed; if blocked by another arrow, it bounces back (flash red) instead of moving.

## CRITICAL — How To Teach/Work With This User

The user is a **complete Unity/programming beginner** ("hu beginner chu, khali Scene/Game view ane Object add karta avde che, bas"). This drives everything about how work should be delivered:

- **Step by step, one small piece at a time.** Never dump a large finished system at once — break it into the smallest teachable chunks (this is why sprites were built one-at-a-time: segment → corner → head → cap, and why ArrowController was built in 3 separate incremental parts: spawn → move/block → exit).
- **Simple, detailed language.** Explain the *why* behind each concept in plain terms before/alongside the code, not just "here's the code." Concepts must be taught, not just delivered.
- **Test-before-advancing.** After each step, the user tests it hands-on in the Unity Editor and reports back (often with a screenshot) before moving to the next step. Do not assume something works — wait for confirmation.
- **All conversational replies are in Gujarati written in Latin/English letters (romanized)** — this is a *global* instruction (in the user's `~/.claude/CLAUDE.md`), repeated here because it's essential to how this project's teaching happens. Never use Gujarati script.
- All code, comments, commands, file paths, identifiers stay in English (repo-English rule, also global).
- The user wants to genuinely **learn Unity fundamentals**, not just receive a working game. Prioritize teaching over speed.

If a context-compaction or new session loses the above nuance, **this section is the source of truth** — re-read it before resuming work.

## Reference Docs

- **`Docs/Unity-Learning-Notes.md`** — comprehensive English-language notes covering all Unity fundamentals taught so far (GameObject/Component model, MonoBehaviour lifecycle, New Input System, List/Dictionary, ScriptableObject, grid math, DOTween, Object Pooling). Consult this before re-explaining a fundamental from scratch — check if it's already documented there first, and add to it when new fundamentals are taught.
- **`Assets/MultiTech Studio/`** — a purchased template, used **only as reference/study material** for architecture ideas (grid, level data, theming). It is explicitly NOT reused directly — all real game code is written from scratch by the user in `Assets/Project`, under guidance. License: Commercial use + modification allowed, redistribution/sublicensing not allowed.

## Architecture Decisions (already made, don't re-litigate)

- **New Input System** (`UnityEngine.InputSystem`) for all of the user's own scripts — not the legacy Input Manager. (`Mouse.current`, `Keyboard.current`, etc.)
- **Data-driven levels**: `LevelData` (ScriptableObject) holds `gridWidth/gridHeight/gridSpacing` + a list of `ArrowPathData` (each just a `List<Vector2Int> path`). `LevelLoader` reads a `LevelData` asset, builds the grid via `GridBuilder`, and spawns one `ArrowController` per arrow.
- **Grid**: `GridBuilder` manages a `Dictionary<Vector2Int, GridCell>`, converts grid↔world coords (`spacing` units apart). Cells track their `Occupant` (an `ArrowController` or null).
- **Arrow movement**: `ArrowController.TryAdvance()` — computes the head's direction from the last two path points, checks the next cell: null (off-grid) → exit, occupied by another arrow → flash red, else → advance (DOTween-animated) and update `path`/cell occupancy.
- **50,000 levels plan**: procedural generation via **reverse construction** (build a level by placing arrows in the reverse of their removal order, checked only against arrows placed so far — guarantees solvability with no separate solver needed). Difficulty controlled by tunable "recipe" parameters, decoupled from level quantity. Not yet started — deferred until after art polish.
- **Object pooling** for arrow spawn/destroy: explicitly deferred as premature optimization until many levels are actually being cycled through in testing.

## Sprite/Art Pipeline

- 4 tube pieces, each drawn on a 256×256 Figma canvas, exported at 4x (1024×1024 PNG), **pure white fill on transparent background** so `SpriteRenderer.color` can tint them any color at runtime.
- Pieces: `segment.png` (straight tube, flat both ends), `corner.png` (rounded outer bend / sharp inner bend), `head.png` (triangle only — the "shaft" look comes from layering `segment.png` underneath in Unity, not from the sprite itself), `cap.png` (flat top / rounded bottom — the tail's true end).
- All 4 imported with **Pixels Per Unit = 1024**, **Pivot = Center**.
- Verification technique for these (since white-on-transparent is invisible on a white background): `sips -g pixelWidth -g pixelHeight -g hasAlpha`, then a Python/PIL script that composites onto a dark background and checks `img.getchannel('A').getbbox()` against expected coordinates (scaled ×4 from the 256 canvas design math).

## Progress Log

*(Newest at top. Update this after each completed step — a couple of lines is enough: what got done, what's next.)*

- **2026-08-06 — Fixed a real movement bug**: `AdvanceStep()` was only tweening each segment's *position* on move, never recalculating which sprite (cap/straight/corner/head) or rotation it should show. Since a middle segment's straight-vs-corner role (and everyone's rotation) depends on the *current* path shape, not just spawn-time shape, segments looked broken/disconnected after a few moves. Fix: extracted the sprite+rotation decision logic out of `SpawnSegments()` into a shared `ApplySegmentSprite(i, sr, transform)` method, now called both at spawn AND every time `AdvanceStep()` repositions segments. Also refined the head's shaft child (added in the shaft-layering step) to only cover the bottom ~56% of the cell (`localScale.y=0.56`, `localPosition.y=-0.22`) instead of the full cell — the shaft was previously poking out past the triangle's narrow tip since the triangle is only ~44% of the cell height at its widest (base), making the tip look broken instead of sharp.

- **2026-08-06 — Art Part 6 fully complete and confirmed working in Unity.** Built as 5 small numbered sub-steps (sprite-swap → rotation → head/tail sprites → corner detection → head-shaft layering), each tested before the next. `ArrowController.SpawnSegments()` now renders the real tube art (cap/straight/corner/head, correctly rotated, head has a segment "shaft" layered underneath it as a child object) instead of the placeholder blue box. Two real bugs surfaced and got fixed along the way, worth remembering:
  - **Leftover dead code**: an old Step-2 rotation block was left in `SpawnSegments()` after Step 4 added its own per-branch rotation, silently overwriting the correct corner rotation every time. Lesson: when replacing a block during a later step, make sure the *old* block is actually deleted, not just superseded.
  - **Sprite pivot/trim bug**: Unity's default sprite import auto-trims to the alpha bounding box and centers the pivot on *that trimmed rect*, not the full 256×256(×4) design canvas. `segment.png`/`cap.png` happened to be symmetric so this never showed; `corner.png`/`head.png` are asymmetric (content lives in one part of the canvas), so their rotation pivot was off-center, causing visible gaps/notches. Fix: in Sprite Editor, manually set each sprite's rect to the full `X=0,Y=0,W=1024,H=1024` instead of the auto-trimmed one, so Pivot=Center lands on the true canvas center. **If more sprite pieces are added later, apply this same full-rect fix proactively** rather than waiting for a visible glitch.
  - The visible "flare" where the head's triangle (150px wide) is wider than the shaft (90px wide) is intentional — matches a classic arrowhead silhouette, not a bug.
  - Also re-confirmed [[feedback-beginner-teaching-style]] the hard way once mid-way through: dumped too much of Part 6 in one message before the user had tested it, got called out, restarted as small numbered sub-steps. Keep doing it that way.
- **2026-08-06**: All 4 sprites (`segment.png`, `corner.png`, `head.png`, `cap.png`) finished, imported into Unity, and verified (PPU=1024, Pivot=Center confirmed via Sprite Editor screenshots for all 4).
- **Earlier**: Built and hands-on-verified the full core game loop from scratch in `Assets/Project`: `GridCell`/`GridBuilder` (grid), `ArrowPathData`/`LevelData` (ScriptableObject level data), `ArrowController`/`ArrowSegmentClick` (movement/blocking/exit via DOTween), `LevelLoader` (automatic level spawning). Taught all 10 Unity-fundamentals steps first (see `Docs/Unity-Learning-Notes.md`).

## Next Steps (in order, unless the user redirects)

1. **Art cleanup**: revert the temporary debug colors (red `Cell` / red `Grid` background) used to make white sprites visible during Part 6 testing, back to normal. Also worth tinting/coloring the arrows themselves now (via `sr.color`) since the whole point of white-based sprites was per-arrow runtime tinting — not done yet, everything is still plain white.
2. Procedural level generator (reverse-construction technique) — deferred until art is fully working (it now is).
3. Object pooling for arrow spawn/destroy — deferred until many levels are being cycled through.
4. (Longer-term, deferred) Organic/animal-shaped level layouts — confirmed to need no architecture change, purely a level-design/generator concern.
