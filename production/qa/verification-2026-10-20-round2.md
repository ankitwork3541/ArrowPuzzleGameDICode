# Verification, round 2 (2026-10-20)

> **Scope**: Y1 (frontier rule, map scroll retry, analytics on frontier only, opaque locked nodes, ShotCapture save safety) and Y2 (Reduce Motion, contrast, touch targets, outline buffer) edits, compiled and run for the first time.
> **Method**: Unity Editor through the Pipeline CLI (compile, EditMode tests) and a Windows development player driven by `ShotCapture` (`--evidence`). Frames in `production/qa/evidence/story-008-v4/`.

## Compile and tests

| Check | Result |
|---|---|
| Script compile after Y1 + Y2 | PASS, 0 errors (the `Every().Until()` scheduler call, `GeometryChangedEvent` and the `Tween.Custom` bump/lock code all compiled as written) |
| EditMode suite | PASS, 101 of 101 (78 earlier + 14 FrontierRule + 9 UiContrast) |
| `ShippedCampaignTests` (explicit, run alone, detached) | PASS, 2 of 2; the Editor stayed responsive |
| Scene dirty flag | `dirty=False` after recompile, after tests, after a player build and after the explicit test. Nothing in this round dirtied `LanternDragons.unity`. The earlier "Scene(s) Have Been Modified" modal was not reproduced; if it appears again, never click Save. |

## `[EvidenceCheck]` (player log), 13 of 13 PASS

- Reduce Motion ON freezes the backdrop (moved 0), OFF lets it drift (moved 4.2e-2).
- Locked tap costs no life (3 to 3, still locked).
- Map: completed level 3 selectable, current level selectable, locked level 46 refused (got 6).
- Restart persistence: progress 42 read back from the file on disk.
- One bump costs exactly one life (3 to 2).
- Frontier: with highest = 10, a replay win of level 3 leaves PLAY on 11 (highest stays 10) and sends no ping; beating level 11 advances to 12 (highest 11) and sends exactly one ping. Pings are counted through `StageTracker.ReportCount` with `SuppressNetworkForTests`, so nothing was posted to the real endpoint.
- Save file restored after the run (contents equal the pre-run file).

## Defect found and fixed in this round

- **Star glyph (`GlyphSeal.png`) was cropped** (win badge and the map's done nodes showed a star with a square notch). Cause: the `.meta` still held the old sprite rect (8,14,80x75) from the 96px original, in Multiple mode, so the new 128px star was cut. Fix: five redrawn glyphs (`GlyphSeal`, `GlyphRedo`, `GlyphReturn`, `GlyphEmber`, `GlyphLattice`) re-imported as Single through the importer, and the one USS URL that named the old sub-sprite (`TabDock.uss`, `#IconStar_0`) now points at the file. Because the old crop had been scaling those glyphs up, their USS got `scale` (back 1.6, restart 1.2, heart 1.2, map star 1.4, options star 1.3, options restart 1.2). Re-captured and checked: star, hearts, back arrow, restart and lose-badge heart all read correctly.

## Per-frame verdicts (all looked at by eye; 5 earlier runs plus this one)

| Frame | Verdict |
|---|---|
| 01 splash, 02 home, 14 gear | PASS (name and icon top, loading bar bottom; "5 CLEARED", no total; gear is a clean cog) |
| 03 level map | PASS (done/current/locked states, star fixed; the topmost locked node is cut hard by the list edge, accepted) |
| 17 map cold start | PASS (900 cleared lands on 901 with 899 and 900 below) |
| 18 locked node opaque | PASS (the path no longer shows through) |
| 04/16 options | PASS with a note: the Reduce Motion row is present, grouped, and the switch is 56px with the whole 132px row tappable; the 16 frame shows the switch OFF because it was set from code and the row does not refresh on `ReduceMotionChanged` (only a real tap updates it) |
| 05 HUD board, 07 hint, 06 bump, 08 unlock | PASS (darker label text reads well, nav dock height unchanged, outline and shadow correct after the `SetPositions` buffer change, progress capsule ring visible, padlock swell and fade mid-board) |
| 09 win, 10 lose | PASS (bottom sheet complete, badge glyph correct, ink on gold) |
| 11 ad loading modal | PASS (centred card) |
| 12 HUD level 1500 | PASS (title fits between back button and lives) |
| 13 small phone 360x640 (4 frames) | PASS (re-captured after the icon change) |
| 15 pearl zoom | PARTIAL (gold dot with an ink outline is visible in the zoom, still small at full size) |

## Still unverified

- A real finger on a map node, scroll feel at 1,500 rows on a device, and the Android look. Dragging from a node Button still scrolling the map needs a device.
- The ad placeholder inside a page (no Google Mobile Ads client on Windows). The dev player logs a known `ArgumentNullException` from the AdMob bridge at start-up.
- Draw calls on the biggest level (each dragon now has three line renderers). Needs a low-end Android device.
- Animation feel (shake, swell, bump) and the synthesised audio.
- The Reduce Motion sheet slide: the backdrop freeze is measured, but the instant-sheet and confetti-off behaviour was not captured frame by frame.
- Accessibility: tier is assumed Standard (see `design/accessibility-requirements.md`); no screen reader or colour-blind pass.
- `Assets/_Recovery/` and `Assets/production/` are untracked leftovers the developer can delete.
