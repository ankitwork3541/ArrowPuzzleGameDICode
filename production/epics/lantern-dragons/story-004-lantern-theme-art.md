# Story 004: Lantern theme art

> **Epic**: Lantern Dragons
> **Status**: Superseded by Story 008 (2026-10-20: the game moved to a light paper theme; the sprites are reused)
> **Layer**: Presentation
> **Type**: Visual/Feel
> **Estimate**: [fill before starting]
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-10-19

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 1 (Lantern theme)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: HIGH (per `docs/engine-reference/unity/VERSION.md`)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). Check `docs/engine-reference/unity/breaking-changes.md` before using any API.

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*From `design/game-brief.md` (Art & audio direction + MVP feature 1), scoped to this story:*

- [ ] `ArrowPalette` holds the lantern colours (red, gold, jade and companions on navy).
- [ ] Resting dragon colours never read as the bump-flash red or the hint pulse. If red or gold are used for dragons, the bump flash and hint pulse are changed so they stay distinct (the old palette avoided red and yellow for exactly this reason).
- [ ] Dragon head and tail sprites ride the `LineRenderer` ends and face correctly in all four directions.
- [ ] Padlock sprite and unlock effect replace the Story 001 placeholders.
- [ ] A lantern-cloud backdrop replaces the neon arrow silhouettes, drifts slowly, stays low contrast and does not scale with pinch zoom.
- [ ] Bloom still gives the glow without washing out the board.
- [ ] A retained screenshot of a mid-level board is saved to `production/qa/evidence/`.

---

## Implementation Notes

- Bloom threshold stays below 1.0 (vertex colours are LDR); see the CLAUDE.md progress log.
- `BackdropArrows` is the file to rework; `ScreenBackdrop` stays unused.

---

## Out of Scope

- Level display and UI text: Stories 005, 006.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level: minimal`: tests are waived (advisory); a Visual/Feel or UI story's retained screenshot is not.*

**Story Type**: Visual/Feel
**Required evidence**: retained screenshot in `production/qa/evidence/` + sign-off in `production/qa/evidence/story-004-evidence.md`.

---

## Dependencies

- Depends on: Story 001 (padlock placeholder)
- Unlocks: Story 006
