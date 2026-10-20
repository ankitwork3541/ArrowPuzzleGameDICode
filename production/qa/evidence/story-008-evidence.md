# Test Evidence: Light paper theme

> **Story**: `production/epics/lantern-dragons/story-008-light-paper-theme.md`
> **Story Type**: Visual/Feel
> **Date**: 2026-10-20
> **Tester**: Claude. Frames come from a built Development Windows player (720x1280, windowed), driven by `Assets/Project/Scripts/Managers/EvidenceCapture.cs` (`--evidence <dir>`). The Editor cannot be used: when it is not the active application, UI Toolkit panels never paint.
> **Build**: working tree on top of `ad4a449`, Unity 6000.6.3f1, save seeded to level 6 (5 levels beaten; level 6 is the first padlock level of the re-baked campaign). Frames re-captured for Layout v2 (positions redesigned) after the polish round.

---

## Acceptance Criteria Results

| # | Criterion | Result | Notes |
|---|-----------|--------|-------|
| AC-1 | Camera and HUD top panel are the same cream, no seam | PASS | `05-hud-board.png`: the header band and the board background are one colour; no seam. Panning under the HUD was not exercised. |
| AC-2 | Bloom off, board reads without it | PASS | No glow anywhere in the frames; edges are crisp. |
| AC-3 | Palette contrast on cream | PASS | All seven dragon colours are readable on cream in `05-hud-board.png`. Gold and lime are the tightest, but both still read. |
| AC-4 | Resting colours distinct from bump flash and hint pulse | PASS | `06-bump.png`: the bumped dragon flashes near-black ink, which no palette colour is. `07-hint.png`: the hinted head swells and turns strong blue, which no palette colour is either. |
| AC-5 | Head, pearl and padlock readable on cream | PASS for heads and padlock, pearl OPEN | Heads and outlines read well. The padlock digit is now ink on gold with the badge's dark outline (`05-hud-board.png`, `08-unlock.png`) and reads clearly. The pearl outline was raised from 1.2x to 1.3x, but at 720x1280 the pearl tail marker is still not clearly distinguishable in the frames, so it is not confirmed. |
| AC-6 | Backdrop retinted, faint, not scaling with pinch | PASS for the tint only | Lanterns, clouds and blossoms sit a few percent off cream on every screen. Drift and the pinch rule are unchanged code and were not exercised in stills. |
| AC-7 | HUD text, hearts, bar and buttons readable on cream | PASS | `05-hud-board.png`. Dark ink labels, red hearts, gold-orange progress, rounded-square buttons. The level badge is now a smaller capsule (112px, 4px border) with a clear gap to the lives pill, and the "LEVEL" caption is ink at 72% instead of 60%. It is still the lightest text in the header but reads. |
| AC-8 | Retained screenshots of board, bump, hint, unlock | PASS | Board, bump, hint and unlock are all in the frames. `08-unlock.png` now catches the padlock mid-board, translucent and swollen, with the progress bar part full. The earlier frame lost it to the hint's camera zoom; the capture now unlocks before it asks for the hint. |

## Screenshots (in `production/qa/evidence/story-008/`)

| # | File | Shows |
|---|------|-------|
| 1 | `01-splash.png` | Layout v2: "LANTERN / DRAGONS" and the logo at the top, empty middle, "Loading..." and a wide gold capsule bar at the bottom |
| 2 | `02-home.png` | Logo and name top-left, settings gear top-right, big gold-bordered level card, red PLAY capsule with a round LEVELS button, slim floating nav dock |
| 3 | `03-level-map.png` | Left-aligned header (back, LEVELS, count chip), winding path: done (gold), current (red), locked (muted padlock) |
| 4 | `04-settings.png` | Large left-aligned title, one grouped card with dividers, capsule switches, redrawn icons, no scrollbar |
| 5 | `05-hud-board.png` | Centred "LEVEL 6" with difficulty chip, lives capsule right, progress capsule under the title, one floating dock with round hint/grid/restart buttons |
| 6 | `06-bump.png` | Bumped dragon flashing ink, one life lost |
| 7 | `07-hint.png` | Hinted dragon pulsing blue |
| 8 | `08-unlock.png` | A padlock opening mid-board |
| 9 | `09-win.png` | Bottom sheet, gold star badge overlapping the top edge, red NEXT capsule |
| 10 | `10-lose.png` | Bottom sheet, red heart badge overlapping the top edge, red RETRY capsule |

## Layout v2 check (`design/ux/ui-redesign.md`, "Layout v2")

Frames re-captured from a rebuilt dev player after the four layout forks; the Editor compiled with no errors and the console had no UXML/USS errors.

| Screen | Result | Notes |
|---|---|---|
| Splash | PASS | Name and logo at the top, loading bar at the bottom, no wrap or clipping ("DRAGONS" fits at 720 wide). Logo is a fixed 300px, so a very small phone is unchecked. |
| HUD | PASS | Title, chip, lives and progress are positioned as specified. The dock is about 190px and the board sits clear of it in `05-hud-board.png`. "LEVEL 1500" (4 digits) was not exercised. |
| Home | PASS | Matches the spec. The progress capsule for 5 / 1500 is a sliver, which is correct. The top-right gear is the shipped gear sprite and reads a little like a ship's wheel. |
| Level map | PASS with note | Left header and path as specified, nodes clear of the dock. The topmost locked node and the lowest path segment are cut hard by the list edges (no fade). |
| Settings | PASS | One grouped card with dividers. A default grey scrollbar showed down the right edge; fixed with `vertical-scroller-visibility="Hidden"` on the ScrollView (a USS `display: none` did not work because the ScrollView sets its own inline display). |
| Win / Lose | PASS | Bottom sheet fully visible, so the slide-up completes; the badge overlaps the top edge; text is not clipped. The same scrollbar defect was fixed the same way. Confetti sits over the sheet on Win. |
| Ad modal / placeholder | NOT TESTED | Not reachable in the Windows player (no Google Mobile Ads client). |

## Defects found in the frames and fixed during this run

- Six Settings icons (`IconVibration`, `IconSound`, `IconRefreshRate`, `IconInfo`, `IconDocument`, `IconPencil`) were still in Multiple sprite mode with the old small rects, so the new 128px art showed cropped. Reimported as Single sprites.
- The Win/Lose card was wider than the panel and rendered as two stripes. Now `width: 88%`, `max-width: 860px`; the NEXT/RETRY capsule is `width: 100%`, `max-width: 620px`.
- "LEVEL COMPLETE" overflowed the card. Title is now 56px.
- The lives pill touched the level badge. Fixed in the polish round: badge 150px to 112px high, 6px to 4px border, caption/number 30px, difficulty 20px, hearts 46px. The gap is now about 55px.
- The padlock digit was dark purple on gold. It is now ink `rgb(58,36,24)`.
- The unlock frame was cut at the top edge: `PlayHint()` refocuses and zooms the camera, and the capture asked for the hint first. The capture now runs the unlock before the hint.
- Level map: the lowest node sat under the bottom nav. A spacer row and a scroll target above the centre fix it (see `story-005-evidence.md`). The grey "triangle" at the top of the path is a locked node clipped by the list edge, not a drawing bug.
- The evidence build's demo level is now 6, because the 1,500-level re-bake changed level 8 (it had no padlock and no bumpable arrow).
- `ArrowController.BlockedBounce` called `Handheld.Vibrate()`, which does not exist in a Windows player, so no Windows build could compile. Now inside `#if UNITY_ANDROID || UNITY_IOS`.
- Settings, Win and Lose showed a default grey vertical scrollbar after the Layout v2 rebuild; hidden through the ScrollView's `vertical-scroller-visibility` attribute.
- `SplashScreen.uxml` had `title=""class=` (invalid XML), found by the Story 007 audit and fixed.

## Not verifiable here

- Animation and feel (swell timing, padlock shake, bump bounce): stills only.
- Backdrop drift and the pinch-zoom rule.
- The old gold and lime contrast numbers were hand-calculated, not measured on screen.
- On-device look, and anything on Android. The dev player logs an AdMob `ArgumentNullException` at start-up because GoogleMobileAds has no Windows client; that is an environment limit, not a game defect.
- Accessibility: NOT ASSESSED (no committed tier; `design/accessibility-requirements.md` is absent). Standard was assumed in `design/ux/ui-redesign.md`.

## Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Lead (art-director / designer) | | | [ ] Approved |
