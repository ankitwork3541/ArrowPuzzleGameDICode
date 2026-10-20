# Story 008: Light paper theme

> **Epic**: Lantern Dragons
> **Status**: In Review (2026-10-20: light theme, paper-lantern redesign and Layout v2 done; 10 frames from a Windows dev player in `production/qa/evidence/story-008/`, see `story-008-evidence.md`; pearl tail marker still unconfirmed; waiting for the developer's sign-off)
> **Layer**: Presentation
> **Type**: Visual/Feel
> **Estimate**: [fill before starting]
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-10-20

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 1 (Lantern theme), revised 2026-10-20 to a light-only look. This replaces the dark navy / neon look built in Story 004.

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: HIGH (per `docs/engine-reference/unity/VERSION.md`)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). Check `docs/engine-reference/unity/breaking-changes.md` before using any API.

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Decisions (made with the developer, 2026-10-20)

- Background: warm cream paper, about `#FFF8EC`. Not pure white, not grey-blue.
- No dark mode and no theme switch. One light look for the whole game.
- No bloom. Dragons get a darker outline plus a soft drop shadow so they read on cream.
- The camera Solid Color, the opaque HUD top panel and the backdrop tint all move together. `.top-panel` must keep matching the camera colour exactly (see the note in `GameplayScreen.uss`).

## Acceptance Criteria

- [ ] Camera background and HUD top panel use the same cream colour, with no visible seam when the board is panned under the HUD.
- [ ] Bloom is off (Global Volume and camera Post Processing), and the board reads without it.
- [ ] `ArrowPalette` colours all have enough contrast on cream: no resting dragon is near-white or near-cream, and gold/lime are darkened or outlined until readable.
- [ ] Resting dragon colours never read as the bump flash or hint pulse. The old white bump flash and ice-cyan hint pulse are re-chosen for a light background (white cannot flash on cream).
- [ ] Dragon head, pearl tail and padlock stay readable on cream (outline or shadow added where needed).
- [ ] The lantern/cloud/blossom backdrop is retinted to a faint warm tone on cream, still drifting and still not scaling with pinch zoom.
- [ ] HUD text, hearts, progress bar and buttons are readable on cream (dark text, adjusted accent colours).
- [ ] Retained screenshots of a mid-level board, a bump, a hint and an unlock are saved to `production/qa/evidence/` with sign-off in `production/qa/evidence/story-008-evidence.md`.

---

## Implementation Notes

- Reuse the Story 004 sprites (`DragonHead`, `DragonPearl`, `Padlock`, `Backdrop*`); retint rather than redraw.
- Files likely touched: `ArrowPalette`, `ArrowController` (bump and hint colours), `BackdropArrows`, `Game.unity` (camera colour, Global Volume), `GameplayScreen.uss` and the other screen `.uss` files for the HUD.
- Full retheme of Splash, Home and Settings is Story 006, which now builds on this look.

---

## Out of Scope

- Level display and level map: Story 005.
- Splash, Home, Settings retheme, names, icon, audio: Story 006.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level: minimal`: tests are waived (advisory); a Visual/Feel story's retained screenshot is not.*

**Story Type**: Visual/Feel
**Required evidence**: retained screenshots in `production/qa/evidence/` + sign-off in `production/qa/evidence/story-008-evidence.md`.

---

## Dependencies

- Depends on: Story 004 (sprites)
- Unlocks: Stories 005, 006
