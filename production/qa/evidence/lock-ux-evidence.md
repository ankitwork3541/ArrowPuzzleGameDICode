# Evidence: padlock number and the first-lock tutorial

> **Date**: 2026-10-20 · **Status**: compiled, tested and looked at in a Windows dev player (Fork V). Sign-off pending.
> Frames: `production/qa/evidence/lock-ux/` (720x1280, plus `lock-1-coach-360x640.png`) and
> `production/qa/evidence/story-008-v6/` (full set, including the 360x640 set).

## What was built

1. **Padlock number legibility.** `Padlock.png` redrawn (same 256 px, same `.meta`): one big flat gold rounded body, a
   small shackle, an ink outline. The digit is a `TextMesh` centred on the body (`DragonController`: `LockDigitSize`
   0.075, `LockDigitY` -0.164, ink).
2. **First-lock tutorial** (`LockCoach.cs`, `LockCoach.uss`, overlay in `PlayScreen.uxml`): one-time coach on the first
   padlock level, shown after the entrance; cream scrim (62%), a ghost padlock above the scrim, pulsing ring (static with
   Reduce Motion), a card ("LOCKED DRAGON", real count in the text, `N > ... > 1 > OPEN` row) and a GOT IT capsule.
   - First lock level is derived from the baked data (`StageLoader.FirstLockLevelIndex`, `LockTutorialRule`), not hard-coded.
   - Shown once per install (`ProgressData.lockTutorialSeen`); never if that level was already beaten
     (`highestLevelCompleted >= level`); never when `ui.lock_tutorial` is false (config key, default true).
   - Taps and pans stay off the board while it is up (`PlayScreenController.IsPointerOverUI`).
3. **Reminder**: tapping a locked dragon shows "Free N more" above its padlock for about a second.

## Defects found by looking, and fixed (3 rounds)

| Round | Defect (from the frames) | Fix |
|---|---|---|
| 1 | The digit was far too big (`characterSize` 0.26): about twice the padlock body, blurry, spilling over the dragon. | `LockDigitSize` 0.26 to 0.075. |
| 1 | The coach card (about 650 px tall, fixed offset) covered the padlock, and the 80% scrim hid the whole board, so the player could not see which dragon was meant. | Card is now compact (title 46, body 32, button 108) and placed from the ring: just below it when the padlock is in the upper half, just above it otherwise (`LockCoach.Place`). Scrim 80% to 62% so the board stays visible. |
| 2 | The digit rendered as a washed-out grey-brown on the gold (the TextMesh font shader converts linear to sRGB in this project). | The ink colour is fed as a linear value (`LockDigitColor`). |
| 3 | `ARROW DRAGON` on the splash ran almost edge to edge (about 10 px margin). The "Free N more" text was a little large. | `.intro-title--long` 84 px to 74 px; `LockHintSize` 0.11 to 0.085. |

## Acceptance

| # | Check | Result |
|---|---|---|
| 1 | Digit readable at a glance for 1, 4 and 12 on cream and over a coloured dragon line | **PASS** (`lock-digit-1/4/12.png`, `lock-3-board-after.png`): dark ink on the gold body. "12" fits the body. |
| 2 | Digit centred on the body, never cut by the shackle or the outline | **PASS** (close-ups). |
| 3 | Tutorial shows by itself on level 6 (first lock level) and only there | **PASS** (`[EvidenceCheck]`: first lock level index 5 = level 6, overlay up on its own). |
| 4 | Overlay keeps taps off the board; GOT IT restores them | **PASS** (`[EvidenceCheck]`: middle of the screen counts as UI while up, board again after GOT IT). |
| 5 | Never shows a second time; flag saved; old saves with level 6 beaten never see it | **PASS** for the run (`lockTutorialSeen` set, no overlay on re-entry); the old-save rule is covered by `LockTutorialRuleTests`. |
| 6 | `ui.lock_tutorial: false` disables it | **PASS** by EditMode test (not exercised in the player). |
| 7 | Ghost + ring sit on the real padlock; card on the opposite side; text fits at 360x640 | **PASS** at 720x1280 and 360x640 (`lock-1-coach.png`, `lock-1-coach-360x640.png`). Not exercised: the ring following a panning camera. |
| 8 | Reduce Motion: ring and row static | **PASS** by eye (`lock-2-coach-reduce-motion.png`: ring static, OPEN lit); the animation itself is not visible in a still. |
| 9 | "Free N more" visible about a second after a tap, readable over dragons | **PARTIAL** (`lock-hint-free-n-more.png`, a 3x crop): readable, but the text is wider than the padlock and sits over the dragon heads while it shows. |

## Checks run

- Compile: 0 errors. EditMode: **229 of 229** pass (212 + 17 `LockTutorialRuleTests`), twice, the second time after the last fix.
- `[EvidenceCheck]` lock run: 5 of 5 PASS + save restored. Full run: 11 of 11 PASS (reproduced once; see the note below).
- `PlayDragonExit` has no caller any more (grep): the dragon-exit whoosh no longer plays; tap, bump, unlock and lock
  shake sounds are still called (`DragonController` lines for each). Not heard, only checked in code.
- Product name: `PlayerSettings.productName` reads "Arrow Dragon Puzzle"; splash and Home show "ARROW DRAGON / PUZZLE"
  on one line each at 720x1280 and 360x640 without overflow or collision with the gear.

## Honest notes

- **One flaky full run**: in one run of the full evidence set a dragon had already left before frame `05-hud-board`
  (progress 5%, lock open, `locked tap` FAIL, `08-unlock` skipped). The identical build passed on the rerun and on the
  lock run. No code path taps a dragon at that moment, so the likely cause is a real mouse click landing on the freshly
  opened player window; not reproduced, not a code defect that I could find. The shipped frames are from the clean rerun.
- Not verified: the ring while the camera pans, "Free N more" timing on a device, haptics, any sound by ear.

Sign-off: [ ] Approved
