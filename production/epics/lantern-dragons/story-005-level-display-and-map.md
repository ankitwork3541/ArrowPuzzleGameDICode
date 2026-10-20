# Story 005: Level display and level map

> **Epic**: Lantern Dragons
> **Status**: In Review (2026-10-20: code and Layout v2 screenshots done, see `production/qa/evidence/story-005-evidence.md`; AC-3, AC-4, AC-5 NOT TESTED, AC-6 PARTIAL; waiting for the developer's sign-off)
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: [fill before starting]
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 5 (New level display)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: HIGH (per `docs/engine-reference/unity/VERSION.md`)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). UI Toolkit on 6.6 removed UXML Factory/Traits; use `UxmlElement` / `UxmlAttribute` for custom controls.

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*From `design/game-brief.md` (MVP feature 5), scoped to this story:*

- [ ] HUD shows the level number and progress in the new style.
- [ ] Home has a level map / select screen showing completed, current and future levels.
- [ ] Tapping a completed or current level loads it; future levels cannot be opened.
- [ ] The map stays smooth with 1,500 levels (virtualised list, not 1,500 elements at once).
- [ ] Progress survives an app restart (saved through `SaveSystem`).
- [ ] The map works at the portrait sizes handled by `DeviceScaleCategory`.
- [ ] A retained screenshot of the HUD and of the level map is saved to `production/qa/evidence/`.

---

## Implementation Notes

- UI Toolkit only (UXML + USS, `PanelRenderer`); no UGUI.
- Files: `GameplayHUDController`, `HomeScreenController`, `LevelTracker`, `SaveData`.

---

## Out of Scope

- Renaming, icon and audio: Story 006.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level: minimal`: tests are waived (advisory); a Visual/Feel or UI story's retained screenshot is not.*

**Story Type**: UI
**Required evidence**: retained screenshot of each screen touched in `production/qa/evidence/`.

---

## Dependencies

- Depends on: Story 002, Story 008 (light paper theme — the HUD and map are styled on the cream look)
- Unlocks: None
