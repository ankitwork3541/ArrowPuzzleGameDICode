# Story 003: Festival board shapes

> **Epic**: Lantern Dragons
> **Status**: Complete except commit (2026-10-20: preview and 1500-level bake pass, see `production/qa/smoke-2026-10-20.md`; AC1/AC5 wait for a git commit)
> **Layer**: Core
> **Type**: Config/Data
> **Estimate**: [fill before starting]
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-10-19

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 4 (New board shapes)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: HIGH (per `docs/engine-reference/unity/VERSION.md`)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal).

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*From `design/game-brief.md` (MVP feature 4), scoped to this story:*

- [ ] About 20 festival silhouettes (black on white, single connected shape, square canvas) sit in `LevelShapeSources/` at the repo root and are tracked in git.
- [ ] `ArrowPuzzle > Campaign > Preview Shape Masks` shows every shape as one connected region with no unreachable cells; failing shapes are redrawn or removed.
- [ ] A 250-level bake includes shaped levels using the new shapes, each shape used once per pass before repeating.
- [ ] Shaped levels stay solvable with locks (Story 002 rules apply).
- [ ] No shape from the old game remains in the folder.

---

## Implementation Notes

- Subjects: dragon, lantern, koi, drum, coin, fan, pagoda, lotus, firecracker, teapot, crane, rabbit, tiger, phoenix, cloud swirl, bell, dumpling, fish lantern, gate, moon.
- AI prompt ending: `solid black silhouette on pure white background, flat, no outlines, no details`. Avoid thin limbs.
- Source images are read as raw bytes by `LevelShapeMask`, so they are not Unity assets.

---

## Out of Scope

- Generator logic and ramp: Story 002.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level: minimal`: tests are waived (advisory); a Visual/Feel or UI story's retained screenshot is not.*

**Story Type**: Config/Data
**Required evidence**: smoke check (`production/qa/smoke-*.md`) is advisory at `qa.level: minimal`; keep a screenshot of the Preview Shape Masks window.

---

## Dependencies

- Depends on: Story 002
- Unlocks: None
