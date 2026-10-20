# Test Evidence: Lantern theme art

> **Story**: `production/epics/lantern-dragons/story-004-lantern-theme-art.md`
> **Story Type**: Visual/Feel
> **Date**: 2026-10-19
> **Tester**: Claude (driving the Unity Editor through the Pipeline CLI); lead sign-off by the developer below
> **Build / Commit**: working tree on top of `ad4a449`, Unity 6000.6.3f1, Game view captured from Main Camera (720x1280)

---

## What Was Tested

Level 6 of the baked campaign (17 arrows, one padlock) played in the Editor with the new lantern look: palette, dragon head and pearl tail sprites, padlock badge, lantern/cloud/blossom backdrop, and the two changed feedback colours (bump flash, hint pulse).

**Acceptance criteria covered**: AC-1 to AC-7

---

## Acceptance Criteria Results

| # | Criterion (from story) | Result | Notes |
|---|----------------------|--------|-------|
| AC-1 | `ArrowPalette` holds the lantern colours | PASS | Red, gold, jade, orange, rose, lime, violet (see board screenshot) |
| AC-2 | Resting colours never read as bump flash or hint pulse | PASS | Bump flash is white (orange dragon read back as 1,1,1 on tap); hint pulses ice cyan (jade dragon read back moving toward 0.49,0.98,1.0). No dragon is white or cyan; jade moved to hue 143 to keep distance from the hint colour |
| AC-3 | Dragon head/tail sprites ride the line ends, face correctly in all four directions | PASS | Heads visible facing up, down, left and right in the board screenshots; pearl tail marker at the tail |
| AC-4 | Padlock sprite and unlock effect replace placeholders | PASS | Gold padlock with dark outline and count; unlock = swell and fade (alpha 0.42 captured mid-fade) |
| AC-5 | Lantern-cloud backdrop replaces the neon arrow silhouettes, drifts, low contrast, does not scale with pinch | PASS (scale rule unchanged) | Lanterns, clouds and blossoms in plum on navy. The camera-child and per-frame rescale logic in `BackdropArrows` is untouched; pinch was not exercised in this run |
| AC-6 | Bloom still glows without washing out the board | PASS | Neon glow intact in screenshots; gold padlock blooms slightly but stays readable |
| AC-7 | Retained screenshot of a mid-level board | PASS | See table below |

---

## Screenshots / Video

| # | Filename | What It Shows | Acceptance Criterion |
|---|----------|--------------|----------------------|
| 1 | `story-004-board.png` | Full board, lantern backdrop, padlock "1" on the gold arrow top-left | AC-1, 3, 4, 5, 6, 7 |
| 2 | `story-004-board-zoom.png` | Same board at 2x, dragon heads and padlock in detail | AC-3, 4 |
| 3 | `story-004-bump.png` | Orange dragon flashing white after running into another arrow | AC-2 |
| 4 | `story-004-hint.png` | Hinted dragon (top right) pulsing ice cyan and swelling | AC-2 |
| 5 | `story-004-unlock.png` | Padlock mid-unlock, swelling and fading | AC-4 |

---

## Test Conditions

- **Game state at start**: Editor Play mode, campaign index forced to level 6 (no save written), 3 lives
- **Platform / hardware**: Windows 11 Editor, Game view; not tested on a device
- **Framerate during test**: not measured
- **Any special setup required**: bump, hint and unlock captured with `Time.timeScale` slowed (0.02 to 0.12) so a single frame lands inside the effect

---

## Observations

- The dragon head and padlock are generated placeholder art (drawn in code, white and tintable); they read well but a hand-made sprite would raise the quality.
- The padlock shake on tap and the feel/timing of the swell-and-fade are the *feel* half of this story: DEFERRED to a playtest.
- Backdrop script is still called `BackdropArrows`; renaming it was left alone to avoid breaking the scene reference.

---

## Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Lead (art-director / designer) | | | [ ] Approved |
