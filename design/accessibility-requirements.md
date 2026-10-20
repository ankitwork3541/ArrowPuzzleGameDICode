# Accessibility Requirements: Arrow Dragon Puzzle

> **Committed tier**: **Standard**
> **Status**: ASSUMED on 2026-10-20 (the developer asked to go fast and did not choose a tier). It can be raised or lowered by editing this file; until then every UI review gates on it.
> **Panel**: `MainPanelSettings.asset` is ScaleWithScreenSize with a **1080 x 1920** reference, so USS pixel values below are reference pixels.

## Standard tier: what it commits to

1. Touch targets at least **96px** (reference 1080 wide) in both directions.
2. Body and label text at least **4.5:1** against its background; large text and meaningful icons at least **3:1**.
3. No state is conveyed by colour alone: a shape, glyph or text always reinforces it.
4. Animations are listed and each has a plan to be skippable or reduced (see Motion).
5. Text sizes are set in the USS in reference pixels so they scale with the panel; no text is baked into a sprite.

Not committed at Standard: screen reader support, remappable input, colourblind palettes, in-game text scaling. **Still open and unbuilt:** an in-game text-size setting, screen reader support, and a colour-blind check of the seven dragon colours (they differ in lightness and every dragon also has a head sprite, but no simulation was run).

## 1. Touch targets (reference pixels)

| Control | Size | Meets 96px? |
|---|---|---|
| Back button (`.action-button--small`) | 120 x 120 | yes |
| Dock buttons hint / grid / restart (`.dock-button`) | 112 x 112 (inside a 180px-wide `.action` with its label) | yes |
| Home settings gear, round buttons (`.round-button`) | 120 x 120 | yes |
| Home LEVELS button (`.levels-button`) | 160 x 160 | yes |
| PLAY (`.play-button`) | 160 high, wide | yes |
| NEXT / RETRY (`.result-primary`) | 150 high, up to 620 wide | yes |
| Level map node (`.map-node`) | 132 x 132 | yes |
| Settings row (`.row`) | 132 high, full width | yes (row), see switch |
| Settings switch (`.toggle`) | 104 x 56 drawn; **tap area is the whole row, 132 high x full width** | yes, **closed 2026-10-20**: `OptionsScreenController.BindSwitch` registers the click on the row, so a tap anywhere on the row flips the switch (a tap on the switch bubbles up, fires once). Look unchanged. Needs one on-device check. |
| Bottom nav items (`.tab-item`) | min 96 wide x 100 high | yes, **closed 2026-10-20**: `min-width: 96px; min-height: 100px` (dock vertical padding cut by the same amount, so the dock keeps its height). |
| Board taps | one grid cell, size varies with level and zoom | **gap**: cells on big boards are smaller than 96px until the player zooms (existing design; pinch zoom is the mitigation). |

## 2. Contrast (computed, WCAG relative luminance)

Cream is `rgb(255,248,236)`, card is `rgb(255,252,244)`, ink is `rgb(58,36,24)`.

| Pair | Ratio | Verdict |
|---|---|---|
| ink on cream | 13.76 | pass |
| ink on card | 14.17 | pass |
| ink 72% (HUD "LEVEL" caption) on cream | 5.74 | pass |
| ink 72% (muted text, map count, "Loading...", nav labels, chevrons) on cream | 5.72 | pass, **closed 2026-10-20** (was ink 60%, 3.96) |
| ink 72% on card (nav labels) | 5.80 | pass, **closed 2026-10-20** (was 4.03) |
| lantern red `#C43448` on cream | 5.06 | pass |
| cream on lantern red (PLAY / NEXT label) | 5.06 | pass |
| hint teal `#007A96` on cream | 4.71 | pass |
| cream on teal (hint badge digit) | 4.71 | pass |
| purple `#6842B0` on cream | 6.64 | pass |
| heart red `#D63031` on cream | 4.60 | pass |
| ink digit on gold badge `#FFC83D` (padlock) | 9.40 | pass |
| dark gold `#965C00` (150,92,0) on cream (difficulty text, map counts, unlocked nav label, win "LEVEL" line) | 5.19 | pass, **closed 2026-10-20** (was `#B07000`, 3.85). On card 5.35. |
| ink on gold-orange `#E88C00` (done map node number and star, win badge glyph) | 5.66 | pass. Cream on gold-orange is only 2.43, so gold-orange fills now carry **ink**, never cream (`TrailMapScreen.uss`, `SheetScreen.uss`). |
| gold-orange `#E88C00` on cream (progress fill, node fill, borders) | 2.43 | **open, accepted**: fills and borders only, never text. Where it is the only state cue (map node "done") it also carries a star glyph and a darker `#B07000` ring. |
| ink 12% track vs cream | 1.25 | decorative track. **Closed for the level progress bar 2026-10-20**: it now has a 2px ink 55% ring (3.45:1) so the empty bar is visible (`PlayScreen.uss`, `.glow-track`). The splash loading bar track stays decorative (the "Loading..." text carries the state). |
| locked map node padlock (ink 60% on `rgb(239,232,220)`) | 3.77 | pass, **closed 2026-10-20** (was ink 45%, about 2.5) |

Automated: `UiContrastTests` (EditMode) recomputes every pair above with the WCAG formula and lints all `.uss` files for text colours under ink 70% alpha or the old `rgb(176,112,0)`.

Dragon colours on cream (all are line art 28px or more thick, so 3:1 applies):

| Dragon | Hex | Ratio | Verdict |
|---|---|---|---|
| Lacquer Red | `#D62828` | 4.74 | pass |
| Festival Gold | `#BF8300` | 3.08 | pass, tightest |
| Jade | `#0F9D58` | 3.32 | pass |
| Ember Orange | `#E8590C` | 3.39 | pass |
| Peony Rose | `#E0457B` | 3.75 | pass |
| Lime Lantern | `#5E9400` | 3.48 | pass |
| Night Violet | `#7B4DFF` | 4.57 | pass |

Contrast gaps closed on 2026-10-20: muted ink text, dark gold small text, cream-on-gold-orange glyphs, the level progress track, the locked padlock. Opacity on locked and dim nav items was removed too (it multiplied the label alpha and pushed the text under 4.5:1). Still open: gold-orange fills and borders as pure decoration (2.43, accepted), and the empty-heart cue (see section 3).

## 3. State is never colour alone

| State | Colour | Reinforced by |
|---|---|---|
| Locked dragon | gold badge | padlock shape and the remaining-count digit on it |
| Hint costs an ad | red badge | the text "AD" and a different icon (`IconAd` instead of `IconBulb`) |
| Hint disabled | 35% opacity | disabled behaviour; no badge |
| Active nav tab | red | a soft red capsule behind the item, plus the label |
| Map node done / current / locked | gold / red / muted | star glyph / larger size and number only / padlock glyph |
| Grid guide on | red ring | fill and icon change (`.grid-button--on`) |
| Empty heart | ink 14% | outline shape differs from a filled heart only by tint: **gap**, a weak cue |
| Bump | ink flash | a life is lost (heart empties) and the arrow reverses |
| Hint pulse | blue | the swell in size |
| Win / lose | gold star / red heart badge | the badge glyph and the title text |

## 4. Motion and reduced motion

**Built 2026-10-20 (needs on-device and Editor verification):** Settings has a **Reduce Motion** switch (`OptionsManager.ReduceMotion`, key `lanterndragons.reduceMotion`, default off; Unity exposes no cheap read of the OS preference). Each animation reads `OptionsManager.ReduceMotionOn` when it starts, so there is no per-frame cost beyond a static bool read in the backdrop.

| Animation | Where | With Reduce Motion ON |
|---|---|---|
| Dragon slide, bump rewind | `DragonController` (PrimeTween) | unchanged: it is the core feedback and short |
| Bump: head overshoot and ink flash | `DragonController.BlockedBounce` | no overshoot; the ink colour is held for the same time, then the run rewinds |
| Hint pulse (swell plus blue) | `DragonController.PlayHintPulse` | blue colour pulse only, no swell |
| Padlock shake on a locked tap | `DragonController.ShakeLock` | no shake; a quick opacity dip instead |
| Padlock unlock swell and fade | `DragonController.OnOtherDragonLeft` | fade only, no scale |
| Confetti on win | `SheetScreenController`, `PetalBurstView` | not shown |
| Result sheet slide-up (0.35s) | `.sheet-card--hidden` transition | appears at once |
| Backdrop drift and spin | `LanternBackdrop` | frozen at the t = 0 pose |
| Map open auto-scroll | `TrailMapController.ScrollToCurrent` | already a jump (sets the offset directly), nothing animates |
| HUD fade-in after the entrance (opacity only) | `.hud-hidden` | **kept**: an opacity fade is not movement |
| Level entrance | `StageLoader` | **open**: not reduced yet |
| Splash progress tween | `IntroScreenController` | **open**: not reduced yet, it is a short bar fill |

The tier is still Standard (reduced motion is a bonus above it).

## 5. Text scaling

All text sizes are fixed pixel sizes in USS inside a 1080 x 1920 ScaleWithScreenSize panel, so text scales with the screen but there is **no in-game text size setting** and the OS font scale is ignored. Smallest text in use: HUD difficulty chip 20px, action labels, ad placeholder caption (reference pixels, about 7 to 10 pt on a typical phone). This is a known gap at Standard (not committed).

## 6. Input

Touch only. The old UI rules mention gamepad and keyboard focus; this game is portrait mobile and has no focus navigation. Not committed.

## Revisit when

- Before any store release, re-check the four contrast gaps and the nav / switch hit areas on a real device.
- Verify Reduce Motion on a device (toggle it on the Settings screen, then play a bump, a hint, a locked tap, an unlock, a win and a map open).
