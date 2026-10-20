# Test Evidence: Level display and level map

> **Story**: `production/epics/lantern-dragons/story-005-level-display-and-map.md`
> **Story Type**: UI
> **Date**: 2026-10-20 (re-captured after the polish round)
> **Tester**: Claude, from a built Development Windows player at 720x1280 (see `story-008-evidence.md` for how the frames are produced). Save seeded to level 6 (5 beaten), so the first padlock level is the current one.

## Acceptance Criteria Results

| # | Criterion | Result | Notes |
|---|-----------|--------|-------|
| AC-1 | HUD shows level number and progress in the new style | PASS | `story-008/05-hud-board.png`: a smaller gold-bordered capsule with difficulty and "LEVEL 6" (about 55px clear of the lives pill), the progress capsule under the row, the hearts pill. The bar fills as dragons leave (`08-unlock.png` shows it part full). |
| AC-2 | Home has a level map with completed, current and future levels | PASS | `story-008/03-level-map.png`: 4 and 5 gold with a star, 6 large red, later levels muted with padlocks, "5 / 1500" counter. State is shown by shape and icon as well as colour. |
| AC-3 | Tapping a completed or current level loads it; future levels cannot be opened | NOT TESTED | Not driven in the player. Code review only: `LevelLoader.SelectLevel` refuses levels past `highestLevelCompleted + 1`. |
| AC-4 | Smooth with 1,500 levels (virtualised) | NOT TESTED | Built on a `ListView` with fixed row height and 1,500 plain ints (plus one spacer row), so only the visible rows exist as elements. Scrolling smoothness was not measured. |
| AC-5 | Progress survives an app restart | NOT TESTED | The run seeded the save in memory. Persistence goes through `SaveSystem` as before; a real restart was not performed. |
| AC-6 | Works at the portrait sizes in `DeviceScaleCategory` | PARTIAL | Only 720x1280 was captured. |
| AC-7 | Retained screenshots of the HUD and the map | PASS | `story-008/05-hud-board.png`, `story-008/03-level-map.png`. |

## Fixed in the polish round

- The lowest node was half hidden under the bottom nav. The list now has one empty spacer row after level 1, and the map opens scrolled so the current node and the two below it clear the nav. Level 4 is fully visible in the new frame; level 3 sits partly under the nav at rest, which is normal for a scrolling list.
- The "stray grey triangle" at the top of the path was not a drawing bug. It is a locked node cut by the top edge of the list, so only its lower arc shows. It is still there as a clipped arc; it is how any list looks at its clipping edge.

## Observations

- The path is smooth between rows with no visible seam where rows are recycled (static frame only; scrolling was not captured).
- The clipped node at the top edge could be softened with a fade, which USS cannot draw (no gradients); it would need a sprite or a Painter2D overlay.

## Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Lead | | | [ ] Approved |

## Layout v2 re-check (2026-10-20)

Frames in `production/qa/evidence/story-008/` were re-captured after the layout redesign.
- **Home** (`02-home.png`): the big level card shows "LEVEL 06", the difficulty chip and "5 / 1500" (read from `SaveSystem` and the loader), with a round LEVELS button beside PLAY. PASS.
- **Level map** (`03-level-map.png`): left-aligned header with the count chip, winding path with done (gold), current (red) and locked (padlock) nodes, nodes clear of the nav dock. Map still opens scrolled to the current level. PASS. The first and last visible items are cut hard by the list edges.
- **HUD** (`05-hud-board.png`): level number and progress are in the new positions. PASS.
- Still NOT TESTED: tapping a node to load a level, scroll smoothness at 1,500 levels on a device, persistence across an app restart, other portrait sizes.

