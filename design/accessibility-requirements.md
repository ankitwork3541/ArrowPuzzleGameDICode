# Accessibility Requirements: Lantern Dragons

> **Committed tier**: **Standard**
> **Status**: ASSUMED on 2026-10-20 (the developer asked to go fast and did not choose a tier). It can be raised or lowered by editing this file; until then every UI review gates on it.
> **Panel**: `MainPanelSettings.asset` is ScaleWithScreenSize with a **1080 x 1920** reference, so USS pixel values below are reference pixels.

## Standard tier: what it commits to

1. Touch targets at least **96px** (reference 1080 wide) in both directions.
2. Body and label text at least **4.5:1** against its background; large text and meaningful icons at least **3:1**.
3. No state is conveyed by colour alone: a shape, glyph or text always reinforces it.
4. Animations are listed and each has a plan to be skippable or reduced (see Motion).
5. Text sizes are set in the USS in reference pixels so they scale with the panel; no text is baked into a sprite.

Not committed at Standard: screen reader support, remappable input, colourblind palettes, in-game text scaling.

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
| Settings switch (`.switch`) | 104 x 56 | **gap**: the control is 56 high. It is only compliant if the whole row is the hit area. Not verified. |
| Bottom nav items (`.nav-item`) | icon 44px, hit area not measured | **gap**: not measured. |
| Board taps | one grid cell, size varies with level and zoom | **gap**: cells on big boards are smaller than 96px until the player zooms (existing design; pinch zoom is the mitigation). |

## 2. Contrast (computed, WCAG relative luminance)

Cream is `rgb(255,248,236)`, card is `rgb(255,252,244)`, ink is `rgb(58,36,24)`.

| Pair | Ratio | Verdict |
|---|---|---|
| ink on cream | 13.76 | pass |
| ink on card | 14.17 | pass |
| ink 72% (HUD "LEVEL" caption) on cream | 5.74 | pass |
| ink 60% (muted text, map count, "Loading...", nav labels) on cream | 3.96 | **gap**: under 4.5 for text; passes only as large text |
| ink 60% on card (nav labels) | 4.03 | **gap**, same reason |
| lantern red `#C43448` on cream | 5.06 | pass |
| cream on lantern red (PLAY / NEXT label) | 5.06 | pass |
| hint teal `#007A96` on cream | 4.71 | pass |
| cream on teal (hint badge digit) | 4.71 | pass |
| purple `#6842B0` on cream | 6.64 | pass |
| heart red `#D63031` on cream | 4.60 | pass |
| ink digit on gold badge `#FFC83D` (padlock) | 9.40 | pass |
| dark gold `#B07000` on cream (difficulty text, map counts) | 3.85 | **gap** for 20-22px text; passes as large text only |
| gold-orange `#E88C00` on cream (progress fill, node fill, borders) | 2.43 | **gap**: under 3:1. Used for fills and borders only, never for text. The progress track uses ink 12% (1.25:1), so the empty bar is barely visible. |
| ink 12% track vs cream | 1.25 | **gap**: decorative track |

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

Known gaps from contrast: muted ink text (3.96 to 4.03), dark gold small text (3.85), gold-orange fills and borders (2.43), ink-12% tracks (1.25). Each is a one-line USS change if the tier is raised (use ink at 0.72 for text, a darker gold for borders).

## 3. State is never colour alone

| State | Colour | Reinforced by |
|---|---|---|
| Locked dragon | gold badge | padlock shape and the remaining-count digit on it |
| Hint costs an ad | red badge | the text "AD" and a different icon (`IconAd` instead of `IconBulb`) |
| Hint disabled | 35% opacity | disabled behaviour; no badge |
| Active nav tab | red | a soft red capsule behind the item, plus the label |
| Locked nav tab | muted | a padlock glyph and the unlock level in the label ("Lvl 10") |
| Map node done / current / locked | gold / red / muted | star glyph / larger size and number only / padlock glyph |
| Grid guide on | red ring | fill and icon change (`.grid-button--on`) |
| Empty heart | ink 14% | outline shape differs from a filled heart only by tint: **gap**, a weak cue |
| Bump | ink flash | a life is lost (heart empties) and the arrow reverses |
| Hint pulse | blue | the swell in size |
| Win / lose | gold star / red heart badge | the badge glyph and the title text |

## 4. Motion and reduced motion

There is **no reduced-motion setting** in the game today (`SettingsManager` has sound, vibration and refresh rate only). Animations that exist:

| Animation | Where | Skippable / reducible now? |
|---|---|---|
| Arrow slide, bump and reverse | `ArrowController` (PrimeTween) | no (it is the core feedback, short) |
| Bump flash (ink) and hint pulse and swell | `ArrowController` | no |
| Padlock shake on a locked tap | `ShakeLock` | no |
| Padlock unlock swell and fade | `OnOtherArrowLeft` | no |
| Confetti on win | `ResultScreenController` | no |
| Result sheet slide-up (0.35s) | `.result-card--hidden` transition | no |
| HUD fade-in after the entrance | `.hud-hidden` | no |
| Level entrance | `LevelLoader` | no |
| Backdrop drift (BackdropArrows) | `BackdropArrows` | no |
| Splash progress tween | `SplashScreenController` | no |

Plan if the tier is raised: add a "Reduce motion" switch in Settings that stops backdrop drift and confetti, replaces the slide-up with a fade, and shortens the swell. Nothing is built yet.

## 5. Text scaling

All text sizes are fixed pixel sizes in USS inside a 1080 x 1920 ScaleWithScreenSize panel, so text scales with the screen but there is **no in-game text size setting** and the OS font scale is ignored. Smallest text in use: HUD difficulty chip 20px, action labels, ad placeholder caption (reference pixels, about 7 to 10 pt on a typical phone). This is a known gap at Standard (not committed).

## 6. Input

Touch only. The old UI rules mention gamepad and keyboard focus; this game is portrait mobile and has no focus navigation. Not committed.

## Revisit when

- Before any store release, re-check the four contrast gaps and the nav / switch hit areas on a real device.
- If a "Reduce motion" setting is wanted, raise the tier to Comprehensive and build section 4's plan.
