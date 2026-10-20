# Test Evidence: Level display and level map

> **Story**: `production/epics/lantern-dragons/story-005-level-display-and-map.md`
> **Story Type**: UI
> **Date**: 2026-10-20 (re-captured after the polish round)
> **Tester**: Claude, from a built Development Windows player at 720x1280 (see `story-008-evidence.md` for how the frames are produced). Save seeded to level 6 (5 beaten), so the first padlock level is the current one.

## Acceptance Criteria Results

| # | Criterion | Result | Notes |
|---|-----------|--------|-------|
| AC-1 | HUD shows level number and progress in the new style | PASS | `story-008/05-hud-board.png`: a smaller gold-bordered capsule with difficulty and "LEVEL 6" (about 55px clear of the lives pill), the progress capsule under the row, the hearts pill. The bar fills as dragons leave (`08-unlock.png` shows it part full). |
| AC-2 | Home has a level map with completed, current and future levels | PASS | `story-008/03-level-map.png`: 4 and 5 gold with a star, 6 large red, later levels muted with padlocks, header chip "5 CLEARED" (the campaign total is deliberately never shown). State is shown by shape and icon as well as colour. |
| AC-3 | Tapping a completed or current level loads it; future levels cannot be opened | PASS (logic), tap NOT TESTED | `[EvidenceCheck]` in the dev player drove the same path the map calls (`StageLoader.SelectLevel`): completed level 3 selected, current level 6 selected, locked level 46 refused (got 6, frontier 6). A real finger tap on a node, and a node Button inside a scrolling list on a touch device, were not tested. |
| AC-4 | Smooth with 1,500 levels (virtualised) | NOT TESTED | Built on a `ListView` with fixed row height and 1,500 plain ints (plus one spacer row), so only the visible rows exist as elements. Scrolling smoothness was not measured. |
| AC-5 | Progress survives an app restart | PASS (disk round-trip) | `[EvidenceCheck]` wrote the save through `ProgressStore.Save()` and read the file back from `persistentDataPath`: `highestLevelCompleted=42`. The old-name migration is covered by `LegacyKeyMigrationTests`. A real kill-and-relaunch was not performed. |
| AC-6 | Works at the portrait sizes in `DeviceScaleCategory` | PARTIAL | 720x1280 and 360x640 were captured (`story-008/13-small-*.png`, no overflow). Tablet scale was not captured. |
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
- **Home** (`02-home.png`): the big level card shows "LEVEL 06", the difficulty chip and "5 CLEARED" (read from `ProgressStore`; the total is never shown), with a round LEVELS button beside PLAY. PASS.
- **Level map** (`03-level-map.png`): left-aligned header with the count chip, winding path with done (gold), current (red) and locked (padlock) nodes, nodes clear of the nav dock. Map still opens scrolled to the current level. PASS. The first and last visible items are cut hard by the list edges.
- **HUD** (`05-hud-board.png`): level number and progress are in the new positions. PASS.
- Still NOT TESTED: tapping a node to load a level, scroll smoothness at 1,500 levels on a device, persistence across an app restart, other portrait sizes.


## After the rename pass (2026-10-20)

Frames recaptured from a rebuilt dev player after the rename and the re-bake (`story-008/`). The Home card and the map header both read "5 CLEARED"; no frame shows a total. The 4-digit title "LEVEL 1500" fits in the HUD (`story-008/12-hud-level-1500.png`). The five `[EvidenceCheck]` lines all passed (see `story-008-evidence.md`). Still NOT TESTED: a finger tap on a map node, scroll smoothness at 1,500 rows on a device, a real app restart, tablet sizes.

## Round 2 re-check (2026-10-20)

- Frontier rule verified in the player: replaying level 3 with level 11 as the frontier leaves PLAY on 11 and sends no analytics ping; beating the frontier advances to 12 and pings once (AC-5 progress rules, PASS).
- Map cold start at 900 cleared lands on level 901 (`story-008-v4/17-map-cold-start.png`); locked nodes are opaque (`18-locked-node-opaque.png`); header and Home card show "N CLEARED" and never a total.
- Map node selection: completed and current accepted, locked refused (PASS, logic level). A real finger tap and scroll smoothness at 1,500 rows are still NOT TESTED on a device.
