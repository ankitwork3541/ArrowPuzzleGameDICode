# Interaction Patterns: Paper Lantern UI

> **Status**: Written 2026-10-20 from the implemented UI (nothing here is aspirational).
> **Source of truth for shapes, colours and sprites**: `design/ux/ui-redesign.md`. Sizes are reference pixels on the 1080 x 1920 panel. Accessibility rules: `design/accessibility-requirements.md`.
> **Rule**: reuse a pattern before inventing one. A new pattern gets added here before its story closes.

Shared tokens: paper `rgb(255,248,236)`, card `rgb(255,252,244)`, ink `rgb(58,36,24)`, lantern red `rgb(196,52,72)`, gold-orange `rgb(232,140,0)`, hint teal `rgb(0,122,150)`, purple `rgb(104,66,176)`. No glow, no gradient, no text-shadow.

---

## P1. Round action button with ring

**When to use**: a single immediate action next to other round actions (back, settings gear, levels, hint, grid, restart).
**Look**: cream-white fill, a 4 to 6px coloured ring, a white tintable icon in the ring colour. Ring colour carries meaning: teal = hint only, red = guide/grid and home actions, purple = back and restart, gold-orange = map.
**Sizes**: 120 (back, settings gear), 112 (dock), 160 (LEVELS).
**States**: rest; `:active` scales to 0.93; disabled = 35% opacity plus `--disabled` class (never colour alone: the hint badge disappears); "on" for the grid toggle changes ring, fill and icon.
**USS**: `.action-button`, `.action-button--small`, `.action-button__icon`, `.action-button__icon--back|restart|grid|hint|hint-ad`, `.action-button__icon--teal|red|purple`, `.action-button--disabled`, `.round-button` (Home), `.round-button__icon--settings|levels`, `.levels-button`, `.card-fill` and `.card-frame--*` (back button on Gameplay).
**Rules**: always attach `UIClickSound.Attach(button)`; give the icon its own `picking-mode="Ignore"` so a tap on the glyph still hits the button.

## P2. Capsule primary with bottom lip

**When to use**: the one primary action on a screen (PLAY, NEXT, RETRY). One per screen.
**Look**: solid lantern-red capsule, cream label and chevron, a 5px darker-red bottom edge (`rgb(150,36,54)`) that reads as thickness.
**States**: rest; hover slightly lighter; `:active` drops the lip to 1px and moves the button down 4px; no disabled state (it is hidden or absent instead).
**USS**: `.play-button`, `.play-button__inner`, `.play-button__label`, `.play-button__chevron` (Home); `.result-primary`, `.result-primary__inner`, `.result-primary__label`, `.result-primary__chevron` (Win/Lose).
**Secondary partner**: a muted text button (`.result-home`, ink 60%, hover ink) below it, or a round button beside it (`.levels-button` on Home). Never a second red capsule.

## P3. Floating dock

**When to use**: a small group of persistent actions at the bottom of a screen (HUD actions, bottom navigation).
**Look**: cream-white capsule card with a 4px ink-12% border, margin from the screen edges (40px sides), items evenly spaced, label under each icon inside the dock.
**HUD dock**: `.bottom-bar` is the dock and is also the element the code measures for board insets and hit-testing, so it must stay the single container (`GameplayHUDController.BottomInsetPixels`, `IsPointerOverUI`). Ad height lifts it (`BottomBarBaseOffset` plus ad height).
**Nav dock**: a slim two-item capsule (Home, Settings), centred above the bottom edge. `.tab-dock`, items `.tab-item` (at least 240 x 100 reference pixels) with states `--active` (soft red capsule behind the item) and `--dim`. The three "Lvl 10 / 20 / 30" milestone items were removed from the game on 2026-10-20.
**USS**: `.bottom-bar`, `.action`, `.action__label`, `.dock-button`, `.dock-button--teal|red|purple`, `.hint-badge`, `.hint-badge--ad`, `.hint-badge__label`; `.tab-dock`, `.tab-item*`, `.tab-icon`, `.tab-label`.
**Hint badge**: a small round badge on the hint button, teal with the remaining count, red with the text "AD" when the next hint needs a rewarded ad.

## P4. Level title with difficulty chip (HUD header)

**When to use**: the one place the HUD names the level. Plain ink text, large, centred; the difficulty sits under it in a small gold-bordered chip.
**Companions**: lives capsule at the right (`.lives-pill`, `.hearts`, `.heart`, `.heart--empty`), progress capsule under the title at about 60% width (`.progress-track`, `.progress-fill`, width set from code as a percent).
**USS**: `.level-title`, `.level-title__caption`, `.level-title__number`, `.difficulty-chip`, `.lives-pill`, `.hearts`, `.heart`, `.heart--empty`, `.progress-track`, `.progress-fill`.
**Rules**: the 4-digit title ("LEVEL 1500") must not touch the back button or the lives pill. The top panel is opaque and must equal the camera cream exactly.

## P5. Level card

**When to use**: a hero summary of one thing the player is about to act on (Home: the current level).
**Look**: one large rounded rectangle (radius about 56px), cream-white fill, 6px gold-orange border, huge red title ("LEVEL 08"), a difficulty chip and a labelled progress capsule ("5 / 1500").
**USS**: `.level-card`, `.card-progress`, `.card-progress__track`, `.card-progress__fill`, `.card-progress__text`; chip `.difficulty-pill`, `.difficulty-text`.
**Data**: progress comes from `SaveSystem.Data.highestLevelCompleted` and `LevelLoader.TotalLevels`.

## P6. Bottom sheet with overlapping badge

**When to use**: an outcome the player must acknowledge (Win, Lose). A centred modal is for system interruptions only (P8).
**Look**: full-width sheet anchored to the bottom, rounded top corners (48px), cream-white, ink-12% top border; a 168px round badge (gold-orange star for win, lantern red heart for lose, cream ring) centred on the top edge, half outside. Order inside: level line, title in ink, primary capsule (P2), text button.
**Motion**: slide up with a USS `translate` transition (0.35s) driven by the `result-card--hidden` class; `PlaySheetIn()` removes it about 60ms after the screen opens. A cream scrim sits behind.
**USS**: `.result-card`, `.result-card--hidden`, `.result-badge`, `.result-badge--win|lose`, `.result-badge__glyph`, `.result-badge__glyph--win|lose`, `.result-card__level`, `.result-card__level--lose`, `.result-card__title`, `.result-scrim`.
**Rules**: the sheet's ScrollView needs `vertical-scroller-visibility="Hidden"` as a UXML attribute (a USS `display: none` is overridden by the ScrollView's inline style). The ad box (`dialog-ad-space`) sits under the sheet.

## P7. Grouped settings card

**When to use**: a list of settings or links. One cream-white card holds all rows, separated by thin dividers, instead of a bordered card per row.
**Rows**: 132px high, red icon at the left, ink label, then either a capsule switch or a chevron.
**Switch**: capsule track 104 x 56; off = ink 12% track with a muted knob; on = solid gold-orange track with a cream knob. State is also the knob position.
**Header**: round back button (`OnHomeNavClicked`), then an 84px left-aligned title below it.
**USS**: `.settings-header`, `.settings-back`, `.settings-title`, `.settings-scroll`, `.settings-list`, `.card`, `.row`, `.row--sep`, `.row__icon`, `.row__icon--vibration|sound|refresh|star|document|info`, `.row__label`, `.row__chevron`, `.switch`, `.switch__knob`, `.switch--on`.

## P8. Modal card (system interruptions)

**When to use**: waiting on something outside the game (the rewarded-ad loading modal). Centred, because it blocks input.
**Look**: cream-white rounded card with a red frame on an ink 55% dimmer, a red arc spinner, title and subtitle. The dimmer stays dark on purpose: a cream dimmer would not dim anything.
**USS**: `.ad-loading-modal`, `.ad-loading-modal--hidden`, `.ad-loading-modal__backdrop`, `.ad-loading-modal__card`, `.ad-loading-spinner`, `.ad-loading-modal__title`, `.ad-loading-modal__subtitle`, `.round`, `.round__frame`, `.round__frame--red`.
**Rules**: while it shows, board taps are blocked (`AdLoadingOverlay.Showing`).

## P9. Virtualised path map

**When to use**: any list that can be 1,000 or more items long. Only the visible rows exist as elements.
**Look**: a winding path drawn per row with Painter2D; round nodes along it; the whole map opens scrolled so the current node sits in view, clear of the dock.
**Node states** (shape and glyph reinforce colour): `.map-node--done` gold with a star, `.map-node--current` larger red with only the number, `.map-node--locked` muted with a padlock glyph and not tappable.
**Header**: left-aligned: round back button, "LEVELS" title, a gold count chip ("5 / 1500").
**USS**: `.map-header`, `.map-back`, `.map-title`, `.map-progress`, `.map-list`, `.map-row`, `.map-node`, `.map-node__number|star|lock`, `.map-node--done|current|locked`, plus `.map-slot`, `.map-slot--open` and `.content--hidden` on Home.
**Rules**: row height is fixed (`FixedHeight`), items are plain ints, there is one empty spacer row near the first level, and the scrollbar is hidden. Open and close go through `HomeScreenController` and `GameFlowManager.TryCloseMap` (Android back closes the map first). A tap on a level past `highestLevelCompleted + 1` is refused in `LevelLoader.SelectLevel`.

## P10. Splash layout

**When to use**: the first screen only. Brand at the top (name over the logo), empty middle so the backdrop drifts through, a wide capsule progress bar near the bottom with a "Loading..." caption above it. The progress bar's own title text is hidden through `.unity-progress-bar__title`.
**USS**: `.splash-root`, `.splash-top`, `.splash-title`, `.splash-icon`, `.splash-bottom`, `.splash-caption`, `.neon-progress` (the class name is a leftover; it is only a name).

## P11. Coach overlay (one-time tutorial)

**When to use**: teaching one rule the first time it matters, on the screen where it matters. Used once, for the first padlock level.
**Look**: a cream scrim (80%) over the whole screen, a ghost copy of the thing being taught drawn ABOVE the scrim (a world-space object sits under the UI, so the scrim would dim the real one), a pulsing lantern-red ring around it, and one rounded paper card with a title, one or two sentences using the real numbers, an illustrative row, and one red capsule button (GOT IT). The card sits on the side of the screen the subject is not on.
**Illustrative row** (padlock): `N > ... > 1 > OPEN`, shortened when N is small; each step lights in turn.
**Reduce Motion**: the ring and the row do not pulse or cycle; everything is simply shown.
**Rules**: the overlay swallows every tap and pan (`PlayScreenController.IsPointerOverUI` returns true while it is up); it shows once per install (`ProgressData.lockTutorialSeen`), never when the player already beat that level before the feature existed, and never when `ui.lock_tutorial` is false in the game config. Hiding it for a level restart does not count as "seen"; only GOT IT does.
**Reminder** after the tutorial: tapping a locked dragon shows "Free N more" above its padlock for about a second (static, no sound change).
**USS**: `LockCoach.uss`: `.coach`, `.coach--hidden`, `.coach__ring`, `.coach__ring--big`, `.coach__ghost`, `.coach__ghost-count`, `.coach__card`, `.coach__card--top|bottom`, `.coach__title`, `.coach__body`, `.coach__steps`, `.coach__step`, `.coach__step--dim`, `.coach__mini`, `.coach__mini-count`, `.coach__dots`, `.coach__arrow`, `.coach__open`, `.coach__open-label`, `.coach__button`.

## P12. Level entrance (how a level builds itself in)

**When to use**: the one moment a board appears. The style is `ui.entrance_style` in the config: `ink` (ink-brush stroke tail to head, wet dark tip that dries to the dragon colour, head pop, faint lattice dots flashing centre-out first), `fly` (each dragon slides in from outside the board along its own heading, nearest entry edge first, 0.1-cell overshoot) or `classic` (the first look, for comparing only). Total about 0.9 s, never more than about 1.2 s on the biggest boards (the starts overlap inside a fixed window). **Rules**: a board tap during it skips to the finished board once; Reduce Motion shows the finished board at once; no sound of its own beyond the existing entrance hook; outline and shadow follow the body every frame; the padlock appears only once its dragon is there. Logic: `EntranceMath` (pure, tested); evidence: `production/qa/evidence/entrance-evidence.md`.

---

## Not yet a pattern (open)

- A fade at the edges of the map list (needs a sprite or a Painter2D overlay; USS has no gradients).
- A reduce-motion variant of P6 and of the backdrop (see `design/accessibility-requirements.md`).
- The faint warm card shadow named in `ui-redesign.md` is not drawn by any pattern.
