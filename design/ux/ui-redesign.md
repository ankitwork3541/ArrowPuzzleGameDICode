# UI Redesign: Paper Lantern (light theme)

> **Status**: Draft, nano spec (review skipped by developer choice, 2026-10-20)
> **Scope**: HUD, Home, Bottom nav, Settings, Win/Lose, Splash, ad popups
> **Accessibility tier**: ASSUMED Standard (not committed; `design/accessibility-requirements.md` is absent). Ink on cream text, touch targets at least 96px at the 1080-wide reference, state is never colour alone (icon/text reinforces it).

## Shape language

Soft rounded paper cards, like a paper lantern: rounded rectangles and capsules, a thin coloured border, a faint warm shadow. No octagons, no hexagons, no glows, no gradients.

| Element | Shape |
|---|---|
| Square action buttons (hint, grid, restart, back) | Rounded square, radius ~28% of side, cream-white fill, 6px coloured border |
| Level badge, lives pill, difficulty pill | Capsule (fully rounded ends) |
| Cards / popups / settings rows | Rounded rect, radius 40px, cream-white fill, 4px ink-12% border |
| Primary button (PLAY, NEXT, RETRY) | Capsule, solid lantern red, cream text, 5px darker-red bottom lip (pressed = lip shrinks) |
| Progress bar | Capsule track (ink 12%), capsule fill (gold-orange) |
| Switch | Capsule track, round knob |

## Colour tokens (use exactly)

| Token | Value |
|---|---|
| paper (screen background) | `rgb(255, 248, 236)` |
| card fill | `rgb(255, 252, 244)` |
| ink | `rgb(58, 36, 24)` |
| ink-muted | `rgba(58, 36, 24, 0.6)` |
| ink-line | `rgba(58, 36, 24, 0.12)` |
| lantern red | `rgb(196, 52, 72)` (pressed lip `rgb(150, 36, 54)`) |
| gold-orange | `rgb(232, 140, 0)` |
| dark gold | `rgb(176, 112, 0)` |
| hint teal | `rgb(0, 122, 150)` (hint only) |
| purple | `rgb(104, 66, 176)` (restart/back border) |
| heart red | `rgb(214, 48, 49)` |

## Sprite set (white, tintable, in `Assets/Project/Sprites/`)

Shape sprites are white on transparent, tinted in USS via `-unity-background-image-tint-color`. Slice values are in sprite pixels.

| File | Size | Use | USS slice |
|---|---|---|---|
| `UIRoundFill.png` | 128x128, radius 36 | fill for rounded squares and cards | all four 40 |
| `UIRoundFrame.png` | 128x128, radius 36, 6px ring | border layer over `UIRoundFill` | all four 40 |
| `UICapsuleFill.png` | 128x64 | capsule fill (pills, badges, buttons) | left 32, right 32, top 0, bottom 0 |
| `UICapsuleFrame.png` | 128x64, 5px ring | capsule border | left 32, right 32, top 0, bottom 0 |
| `UIBarTrack.png`, `UIBarFill.png` | existing names, redrawn as flat capsules, 128x32 | progress bar | left 16, right 16 |

No glow, no soft halo around any shape: the edge must be crisp so it reads on cream. The old `UIOctagon*` and `UIHexPill*` sprites are left on disk (unused after the redesign) and deleted in a later cleanup.

## Icon set (white, tintable, 128x128, overwritten in place under the existing names)

Rounded thick-stroke style, 10px stroke, round caps and joins, 12px safe margin: `IconBack`, `IconRestart`, `IconGrid`, `IconBulb`, `IconAd`, `IconHeart` (filled), `IconStar`, `IconSound`, `IconVibration`, `IconInfo`, `IconDocument`, `IconPencil`, `IconRefreshRate`. Overwrite the PNG only: the `.meta` files are kept so every existing reference and GUID still works.

## Screens

- **HUD**: top row = back (rounded square, 120px, purple border), level badge (capsule, gold-orange border, "LEVEL n" ink with difficulty above in dark gold), lives pill centre (capsule, red border, 3 hearts). Progress capsule under the row. Bottom bar = 3 rounded squares (hint teal, grid red, restart purple, 176px) with ink labels.
- **Home**: level label and difficulty capsule, PLAY primary capsule, bottom nav as a slim two-item capsule.
- **Settings**: cream cards with capsule switches.
- **Win / Lose**: rounded card on a cream scrim, title ink, primary capsule.
- **Splash / ad popups**: same card and capsule language.

## Layout v2: new positions (2026-10-20, developer: "fully different design from the old one")

The old game centred everything in a stack and put back / lives / level-badge in one top row with three separate action squares at the bottom. v2 moves things so no screen reads like the old one. All element `name`s that controllers query stay; only the position, grouping and styling change.

| Screen | New layout |
|---|---|
| **Splash** | Top: game name (two lines, "ARROW DRAGON" over "PUZZLE", large, lantern red) at about 12% from the top, the icon below it (about 300px, rounded square). Middle is empty so the drifting backdrop shows. Bottom: "Loading..." caption above a wide capsule progress bar (about 80% width) about 10% above the bottom edge. |
| **HUD** | Top: back button at the left; level title centred as plain text ("LEVEL 6", large ink) with the difficulty as a small coloured chip under it; lives as a vertical-free compact pill at the right. The progress capsule sits directly under the title, narrower (about 60% width), not full width. Bottom: ONE floating rounded dock (cream-white capsule card) holding the three action buttons (hint, grid, restart) as round buttons with the label under each inside the dock. |
| **Home** | Top-left: small logo plus the name; top-right: settings round button. Centre: one big rounded "level card" (cream-white, gold border) with "LEVEL 08" huge, the difficulty chip and an "N CLEARED" line (the campaign total is never shown). Bottom (above the nav): a wide red PLAY capsule with a round LEVELS (map) icon button at its left. The nav is a slim floating two-item capsule dock (Home, Settings); the Lvl 10 / 20 / 30 milestone items were removed. |
| **Level map** | Header left-aligned (back round button, "LEVELS" title, count chip below the title). Path and nodes stay; list gets bottom padding so the dock never covers nodes. |
| **Settings** | Full page: large left-aligned title under a round back button, rows as one grouped card with dividers (not separate bordered cards). |
| **Win / Lose** | Bottom sheet: the card slides up from the bottom edge, rounded top corners only, a big round icon/badge half-overlapping the top edge of the sheet, title, then the red primary capsule and the text button. The scrim stays cream. |
| **Ad loading / placeholder** | Keep the centred rounded card (a modal is expected to be centred); the placeholder stays a rounded card. |

## Out of scope

Text and names (Story 006), level map (Story 005), app icon.
