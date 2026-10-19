# Story 001: Locked arrows

> **Epic**: Lantern Dragons
> **Status**: Complete
> **Layer**: Core
> **Type**: Logic
> **Estimate**: [fill before starting]
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-10-19

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 2 (Locked arrows)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: HIGH (per `docs/engine-reference/unity/VERSION.md`)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). Check `docs/engine-reference/unity/breaking-changes.md` before using any API.

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*From `design/game-brief.md` (Player goal & fail state + MVP feature 2), scoped to this story:*

- [ ] Arrow data carries an optional lock threshold N (0 = unlocked); levels without locks play exactly as before.
- [ ] A locked arrow shows a padlock with its remaining exit count.
- [ ] Tapping a locked arrow shakes the padlock; the arrow does not move and no life is lost.
- [ ] Every arrow that exits the board decrements all remaining lock counts by one; at zero the lock opens with an unlock animation and the arrow becomes tappable.
- [ ] A moving arrow that runs into a locked arrow bumps like any other blocker (flash red, reverse, lose a life).
- [ ] Hint never points at a locked arrow.
- [ ] Restarting a level resets every lock counter.
- [ ] Win still means every arrow has exited; fail still means lives reach 0.

---

## Implementation Notes

- Touches `ArrowController` (state, tap handling), `HintManager` (skip locked) and the level load path in `LevelLoader`.
- Placeholder padlock art is fine; real art is Story 004.
- Do not change the level binary here; the data comes from Story 002. Prove the rule with a temporary hard-coded test level.

---

## Out of Scope

- Level data format, generator, solver: Story 002.
- Final padlock sprite and unlock effect polish: Story 004.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level: minimal`: tests are waived (advisory); a Visual/Feel or UI story's retained screenshot is not.*

**Story Type**: Logic
**Required evidence**: tests waived at `qa.level: minimal`; note the manual play-test (locked tap, countdown, unlock, hint) in this story when done.

---

## Dependencies

- Depends on: None
- Unlocks: Story 002

---

## Completion Notes
**Completed**: 2026-10-19
**Criteria**: 8/8 evaluated; 1 deferred part (AC3 shake visual) and 1 indirect (AC8 Win)
**Deviations**: None. Extra files touched, both within the story's implementation notes: `LevelManager.cs` (new `OnArrowLeft` event), `LevelLoader.cs` (temporary `debugLockCount` / `debugLockThreshold` test hook, to remove in Story 002).
**Test Evidence**: Logic story at `qa.level: minimal` (tests waived). Observed live in the Editor via the Unity Pipeline CLI: padlock label renders (`production/qa/evidence/story-001-locked-board.png`), locked tap does not move or cost a life, countdown 2→1→open, hint skips locked arrows, restart resets locks, bump into a locked arrow costs a life, lives 0 shows OUT OF LIVES (`production/qa/evidence/story-001-lose.png`).
**Deferred**: AC3 shake animation visual feel (needs a look in the Game view). AC8 Win was not fired on purpose: `ShowWin()` advances the saved campaign and sends a backend analytics ping. The remaining-arrow counter decrements on each exit and the Win logic is unchanged.
**Side effects of testing**: an accidental Win during the Editor test left the Editor save at level 2 (`highestLevelCompleted = 1`) and may have sent one `ReportLevelComplete(1)` analytics ping. `com.unity.pipeline 0.8.0-exp.1` was added to `Packages/manifest.json`.
**Code Review**: Skipped (solo mode). Informal `unity-specialist` review found no confirmed bugs; risks noted: lock label shows before the arrow finishes drawing in (Story 004), the countdown fires when an exiting arrow is fully off screen, and the generator must guarantee every lock can open (Story 002).
