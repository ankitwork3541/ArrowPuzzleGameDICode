# Game Brief: Lantern Dragons

**One-sentence pitch:** Tap paper dragons to slide them off a warm paper festival board, but padlocked dragons stay stuck until enough other dragons have escaped.

## Core loop
- Read the board, tap a free dragon: it slides forward and exits, or bumps another dragon (flashes red, costs a life).
- Each exit counts down any padlocked dragon; at zero the lock opens.
- Clear the board, move to the next level. Ads, hints and lives behave as in the previous game.

## Player goal & fail state — what "working" looks like
- Goal: every dragon leaves the board. Fail: lives reach 0.
- Tapping a locked dragon shakes its lock and costs no life.
- Every generated level is solvable with the locks respected.

## MVP — what must exist to be the game
1. Lantern theme: palette, dragon head/tail sprites, lantern-cloud backdrop (replaces the neon arrow silhouettes). **Light theme only** (decided 2026-10-20): the whole game is warm cream paper, with no dark mode — see Story 008.
2. Locked arrows: padlock visual, shake on tap, countdown, unlock animation.
3. New level pipeline: generator and solver respect locks (threshold never above the arrow's carve position), new seed constants and hash salts so no level matches the old game, new pacing ramp with an early lock introduction curve, binary format version bumped, levels re-baked.
4. New board shapes: about 20 festival silhouettes (AI-generated PNGs in `LevelShapeSources/`: dragon, lantern, koi, drum, coin...) replacing the old ones.
5. New level display: restyled level number and progress in the HUD, plus a level map / select screen on Home.
6. Retheme UI text and screens (Splash, Home, HUD, Settings), names, icon, package id, and audio swap (bells, drums, gong).
7. Verify kept systems still work: ads, lives, hints, save, settings.

## Out of scope — not building this
- No level editor, no hand-authored levels (all generated).
- No rocks, combo coins, new game modes, IAP, story, daily challenge, extra languages, new ad formats.

## Build order
1. Locked-arrow logic plus generator/solver support and format bump (the only real risk), placeholder art.
2. New seed constants and pacing ramp; bake a test set and playtest the locks.
3. Generate festival silhouettes, preview with `Preview Shape Masks`, bake with shapes.
4. Lantern art, palette, backdrop.
5. Level display: HUD level number/progress, then the level map screen.
6. UI retheme, names, audio.
7. New package id and AdMob units, then re-check ads.

---
**Who it's for / what they feel:** Casual mobile puzzle fans who want short, satisfying "I outsmarted it" sessions with a festive feel.

**Art & audio direction:** Warm cream paper board (~`#FFF8EC`), saturated red/gold/jade dragons with a darker outline and soft drop shadow (no bloom), faint drifting lantern clouds; bells, soft drums and a gong on clear. Light theme only, no dark mode.

**Reference game:** Our own previous arrow-path game: about 90% kept (grid, ads, lives, hints, generator); only the skin and the lock rule change.
