# Story 002: New level pipeline

> **Epic**: Lantern Dragons
> **Status**: Complete
> **Layer**: Core
> **Type**: Logic
> **Estimate**: [fill before starting]
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: 2026-10-19

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 3 (New level pipeline)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: HIGH (per `docs/engine-reference/unity/VERSION.md`)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal). Check `docs/engine-reference/unity/breaking-changes.md` before using any API.

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*From `design/game-brief.md` (Player goal & fail state + MVP feature 3), scoped to this story:*

- [ ] Level data and the binary store a per-arrow lock threshold; the format version is bumped and the loader refuses old-format chunks with a clear error.
- [ ] The generator assigns each lock a threshold no greater than the arrow's carve position (arrows cleared before it in the solve order), so every level stays solvable.
- [ ] `LevelDifficulty` treats a locked arrow as unavailable until its threshold is met; a level with an unsatisfiable lock is reported unsolvable.
- [ ] New seed constants replace `7919` / `104729` and the `Hash01` salts; no baked layout matches a level from the old game's chunks (compare against the old `Chunk_*.bytes` in git history).
- [ ] New pacing ramp: the first lock appears at an early level (value chosen in dev, kept in a named constant) and the lock share grows gradually.
- [ ] `ArrowPuzzle > Campaign > Bake 50 Levels` and `Bake 250 Levels` finish with zero unsolvable levels and regenerate `Resources/Levels/Chunk_*.bytes`.
- [ ] Re-baking twice gives byte-identical output.

---

## Implementation Notes

- Files: `LevelCarver`, `LevelCampaignGenerator`, `LevelDifficulty`, `LevelBinaryWriter`, `LevelBinary`, `LevelData`, `ArrowPathData`.
- The carve order is already a valid solve order, so a threshold at or below the carve position guarantees solvability.
- Ramp tuning knobs (`OnboardingArrows`, peak and run lengths, bag mix) stay as named constants at the top of the generator.

---

## Out of Scope

- Board silhouettes: Story 003.
- Level map and HUD display: Story 005.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level: minimal`: tests are waived (advisory); a Visual/Feel or UI story's retained screenshot is not.*

**Story Type**: Logic
**Required evidence**: tests waived at `qa.level: minimal`; keep the bake summary log (levels baked, unsolvable count) in this story's notes.

---

## Dependencies

- Depends on: Story 001
- Unlocks: Stories 003, 005

---

## Completion Notes
**Completed**: 2026-10-19
**Criteria**: 7/7 passing
**Deviations**: Advisory only, all inside the pipeline's scope: (1) onboarding levels (first 10) are forced to plain rectangles, because the new campaign salt put a 120-arrow silhouette on level 3; (2) `SeedAttemptsFallback = 24` extra carve seeds, used only when the normal budget finds nothing (level 854 was dropped without it); (3) Story 001's temporary `debugLockCount` hook removed from `LevelLoader` as planned.
**Test Evidence**: Logic story at `qa.level: minimal` (tests waived). Verified with the Unity Pipeline CLI plus an independent Python re-implementation (`scratchpad/verify_chunks.py`, `verify_full.py`): 1500 levels, 0 unsolvable with locks, 0 deadlocks under low-/high-index play order, 0 locks above solve index or above 9, 0 levels identical to an old-game level, re-bake byte-identical, old v1 chunk refused with a clear error, unsatisfiable lock reported unsolvable. Screenshot: `production/qa/evidence/story-002-level6-locked.png`.
**Code Review**: Skipped (solo mode).
**Follow-ups**: Board shapes are still the old game's 132 silhouettes in `LevelShapeSources/` until Story 003; the campaign must be re-baked after that. The lock label is small on dense boards (Story 004). Baked chunks `Chunk_00/01.bytes` (1500 levels, format v2) are regenerated in this story.
