# Proto-5 Manual and Evidence Checklist

Story type: Visual/Feel + Integration (dev-only prototype). Gate: BLOCKING for evidence (retained screenshots), lead sign-off pending.
Output location: `production/qa/evidence/proto-5/`. Source rules: `production/levels/proto-5/design.md`.

## Setup

- Windows dev player with `--proto` (or Editor menu `LanternDragons/Proto/Play Proto Levels`). Only the 5 levels in `Proto_00.json` play. The player's save is never read or written (confirm: `lanterndragons_progress.json` timestamp unchanged after the session).
- Level order is expected to match the design table (1 open board, 2 rocks, 3 linked lock, 4 ice, 5 portal). If the order differs, record the actual order here and keep the screenshot names by type.
- Lives: note the starting heart count before each level. A blocked tap = bump, red/ink flash, reverse, minus 1 heart.
- Screenshot naming: `proto-5/NN-type-state.png` (full game view, 720x1280).

## Common checks (every level)

- C1 Free tap on a clear dragon: slides and exits, no life lost.
- C2 Blocked tap: bump forward, flash, reverse, exactly 1 heart lost. Pass = heart count drops by 1 and the dragon returns to its start cell.
- C3 Restart mid-level: board returns to the initial state (all dragons, ice, locks, links), hearts restored per normal restart rules.
- C4 Undo: no undo feature is documented. Pass = no undo control exists; if one appears, file a bug and test it.
- C5 Reduce Motion on (Options): level shows as finished board, motion shortened or skipped, no new sound, all rules still work.
- C6 Clear the level: win flow appears, no save write.

## Test Case: P5-01 Open board with gaps
**Precondition**: Proto level 1 loaded, full hearts.
**Steps**:
  1. Capture the untouched board.
  2. Tap a dragon whose ray crosses a gap and reaches the edge.
  3. Tap a dragon whose ray crosses a gap and then hits another dragon farther down.
  4. Clear the board in the solve order.
**Expected Result**: Scattered holes (about 22 percent of cells) show dots. Ray 2 exits over the gap. Ray 3 is blocked (the blocker may be far away), costs 1 heart. Level is solvable with no forced loss.
**Pass Criteria**: Gap cells visibly distinct from dragon cells; step 2 exits with 0 hearts lost; step 3 costs exactly 1 heart; level cleared.
**Screenshots**: `01-open-initial.png`, `01-open-blocked-bump.png`, `01-open-cleared.png`.
**Edge cases**: Reduce Motion (C5); restart after a blocked tap (C3); undo (C4).

## Test Case: P5-02 Rocks
**Precondition**: Proto level 2 loaded, full hearts.
**Steps**:
  1. Capture the initial board.
  2. Tap a rock.
  3. Tap a dragon whose ray hits a rock.
  4. Clear the board in the solve order.
**Expected Result**: Rocks are warm grey rounded stones with an ink outline. Step 2: nothing happens (no sound, no heart cost, no highlight). Step 3: bump, flash, reverse, minus 1 heart.
**Pass Criteria**: Rock tap leaves hearts and board unchanged; rock-blocked tap costs exactly 1 heart; no dragon ever passes through or onto a rock.
**Screenshots**: `02-rocks-initial.png`, `02-rocks-blocked-bump.png`, `02-rocks-cleared.png`.
**Edge cases**: Rapid double tap on a rock; hint (if shown) must not draw a path through a rock; Reduce Motion; restart.

## Test Case: P5-03 Linked lock
**Precondition**: Proto level 3 loaded, full hearts. Identify dragon A (padlock) and dragon B (gold ring), joined by a gold line.
**Steps**:
  1. Capture the initial board.
  2. Tap A early, before B has left.
  3. Tap B (clear path) so it exits.
  4. Watch A, then tap A.
  5. Finish the level.
**Expected Result**: Step 2: A does not move, lock-shake feedback (shake clip and light haptic), no heart lost. Step 3: B exits; link opens with the unlock clip, padlock and gold line disappear. Step 4: A now moves as normal (exits or bumps by its ray).
**Pass Criteria**: Early A tap costs 0 hearts and A stays; link opens only after B leaves (not after any other dragon); A moves after the open.
**Screenshots**: `03-link-initial.png`, `03-link-early-tap-shake.png`, `03-link-unlock-moment.png`, `03-link-cleared.png`.
**Edge cases**: Tap A repeatedly while locked (still free, no stacking penalty); clear a different dragon first (A stays locked); hint and guide lines ignore A while locked; restart after B left (link and padlock restored); Reduce Motion (padlock state still readable).

## Test Case: P5-04 Ice dragon
**Precondition**: Proto level 4 loaded, full hearts. Identify the iced dragon (pale blue frost shell).
**Steps**:
  1. Capture the initial board.
  2. Tap the iced dragon once.
  3. Tap it a second time.
  4. Repeat on a second iced dragon if present, choosing one whose ray is blocked.
  5. Finish the level.
**Expected Result**: Tap 1: crack mark appears, dragon does not move, no heart lost (even when its ray is blocked). Tap 2: ice shatters (unlock clip) and the dragon slides normally; exit if clear, bump and minus 1 heart if blocked.
**Pass Criteria**: After one tap: crack visible, position unchanged, hearts unchanged. After two taps: shell gone and normal move rules apply.
**Screenshots**: `04-ice-initial.png`, `04-ice-cracked-after-tap1.png`, `04-ice-shatter.png`, `04-ice-cleared.png`.
**Edge cases**: Tap once then leave it (crack must persist); tap once then restart (ice fully restored, no crack); other dragons are unaffected by ice; Reduce Motion (crack still visible, shatter instant or short); hint on an iced dragon should not cost a heart.

## Test Case: P5-05 Portal pair
**Precondition**: Proto level 5 loaded, full hearts. Identify the two teal rings.
**Steps**:
  1. Capture the initial board.
  2. Tap a dragon whose ray enters portal 1 and leaves portal 2 to a clear path.
  3. Tap a dragon whose ray goes through the pair and then hits a dragon or a rock after emerging.
  4. Tap a dragon whose ray does not touch a portal.
  5. Finish the level.
**Expected Result**: Step 2: body is absorbed by ring 1 and emerges from ring 2 in the same direction, then exits. Step 3: the dragon enters, emerges, bumps the obstacle, reverses back through the pair to its start, minus 1 heart. Step 4: normal behavior.
**Pass Criteria**: Direction is preserved across the pair; the blocked-after-portal tap costs exactly 1 heart and the dragon fully returns with no body stranded inside or between rings; no body is drawn mid-board between the rings.
**Screenshots**: `05-portal-initial.png`, `05-portal-absorb-midframe.png`, `05-portal-emerge-midframe.png`, `05-portal-blocked-after-exit.png`, `05-portal-cleared.png`.
**Edge cases**: Ray through a portal into a rock (step 3 variant, rock behind the exit ring): must block and cost 1 heart; second tap on another dragon while the first is mid-portal (known risk, not specially handled: record any stuck state); restart during the absorb phase (board resets cleanly, no ghost segment); Reduce Motion (absorb/emerge still correct end state); no ray loops forever (rings share no row or column).

## Known risks to watch (from design.md)

- Portal two-phase animation checked on one board only.
- Two dragons using the same portal at once: unhandled, log any glitch as a bug.
- Gap boards may look like notched boards; note tester impression only, not a pass/fail item.

## Evidence index (expected files)

`production/qa/evidence/proto-5/`: 01-open-initial, 01-open-blocked-bump, 01-open-cleared, 02-rocks-initial, 02-rocks-blocked-bump, 02-rocks-cleared, 03-link-initial, 03-link-early-tap-shake, 03-link-unlock-moment, 03-link-cleared, 04-ice-initial, 04-ice-cracked-after-tap1, 04-ice-shatter, 04-ice-cleared, 05-portal-initial, 05-portal-absorb-midframe, 05-portal-emerge-midframe, 05-portal-blocked-after-exit, 05-portal-cleared (all `.png`). Sign-off: lead box unticked.
