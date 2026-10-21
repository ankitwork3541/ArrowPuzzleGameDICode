# Prototype: five new level types (dev-only)

Status: prototype, not in the shipped campaign. Chosen by the developer on 2026-10-20 (open board, rocks,
linked lock, ice dragon, portal pair). Shipped `Pack_NN.bytes` (format `LDLC` v3) are untouched.

## Where it lives

- Data: `Assets/Project/Resources/DevProto/Proto_00.json` (5 levels, written by `LanternDragons/Proto/Bake 5 Prototype Levels`).
  JSON on purpose: it is a dev review pack, readable and diffable, and it keeps the v3 binary format unchanged.
- Runtime switch: `ProtoPack.Active`, on with `--proto` (Editor: menu `LanternDragons/Proto/Play Proto Levels`).
  While active the game plays only these 5 levels and never reads or writes the player's save.
- Rules shared by solver, carver and game: `StageRules` (rocks, portal pair, ray stepping, per-dragon ice and link).

## Shared solvability argument

The carver peels a board outside-in, so its output order is a solve order: dragon i's exit ray only crosses cells
of dragons with a lower index. Removing a dragon only frees cells. Every rule below keeps that invariant, and
`StageRating.Measure(..., rules)` re-solves each baked level with the rule modelled, so a violation is caught at bake.

## The five types

| # | Rule | Solvability argument | Data | Visual |
|---|------|----------------------|------|--------|
| 1 Open board | Scattered empty cells inside the board (about 22 percent). A ray passes over them, so blockers can sit far down a ray (decoys). | The carve partitions only the non-gap cells; rays over gaps are clear by definition. | `openCells` | Dots stay on the gap cells, so the holes are visible. |
| 2 Rocks | Fixed rock tiles that no dragon can pass. Tapping one does nothing. | Rocks are blockers in the carve (never carved, never cleared), so a head is only chosen when its ray misses every rock. | `rocks` | Warm grey rounded stone with an ink outline. |
| 3 Linked lock | Dragon A cannot move until one named dragon B has left (not a count). | Links only point to a lower solve index. When A is the lowest remaining dragon, B is already gone. A cycle or a forward link is rejected by `StageRules` validation and reported unsolvable. | `dragons[i].linkedTo` | Padlock on A, a gold ring on B, a gold line between them. |
| 4 Ice dragon | First tap cracks the ice (no move, no life lost); second tap shatters it and slides as normal. | Ice adds one free tap and changes no ray, so order feasibility is unchanged. Solver counts the extra taps. | `dragons[i].iced` | Pale blue frost shell over the body; a crack mark after tap one. |
| 5 Portal pair | A head's ray that enters one portal leaves the other in the same direction. The dragon slides into the first ring and slides out of the second. | The portal step is part of the ray definition used by carver, solver and game (`StageRules.Next`), so the carve order stays a valid solve order. The two portals share no row or column, so a ray cannot loop; the solver also caps ray length. | `portals` (2 cells) | Teal rings; the body is absorbed by one ring and emerges from the other. |

## Rule decisions taken (the brief did not cover them)

- Tapping an iced dragon is free, whatever its ray looks like (no life cost on the crack tap).
- A tap on a rock is ignored (no sound, no cost).
- Locked-by-link dragons ignore hint and guide lines (same as count padlocks).
- No new audio: crack reuses the lock-shake clip, shatter and link-open reuse the unlock clip.

## Risks

- Portal absorb/emerge animation is custom (two phases) and has only been checked on one board.
- Concurrent runs: two dragons using the same portal at once are not specially handled.
- Gaps only differ from "notched" boards by being scattered and visible; the player value is unproven.
- Art is procedural (runtime textures) for the prototype; real sprites would replace `ProtoArt`.
- Out-of-scope line in `design/game-brief.md` was amended for the prototype only.
