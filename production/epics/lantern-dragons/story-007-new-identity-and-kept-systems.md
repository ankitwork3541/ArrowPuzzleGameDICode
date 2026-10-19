# Story 007: New identity and kept-systems check

> **Epic**: Lantern Dragons
> **Status**: Ready
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: [fill before starting]
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 7 (Verify kept systems)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: HIGH (per `docs/engine-reference/unity/VERSION.md`)
**Engine Notes**: Unity 6.5/6.6 raise the Android minimum API level to 26 and minimum OpenGL ES to 3.1; Gradle/AGP were upgraded (see `docs/engine-reference/unity/breaking-changes.md`).

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*From `design/game-brief.md` (Build order step 7 + MVP feature 7), scoped to this story:*

- [ ] New Android package id and bundle name are set.
- [ ] New AdMob app id and ad units are configured (test ids while developing); `AdConfigFetcher` points at the new game's API key/endpoint (confirm with the backend owner).
- [ ] Banner, interstitial, rewarded and native ads load and show on a real Android device with test ids; the rewarded-hint and interstitial rules from the CLAUDE.md log still hold.
- [ ] Full loop works on device: Splash, Home, level, win or lose, next level.
- [ ] Lives and hints behave as before, with locks (locked tap costs no life; hint skips locks).
- [ ] The save does not read the old game's data and survives a restart.
- [ ] Settings screen works (open item from CLAUDE.md: confirm scene wiring).
- [ ] No console errors in a 10-minute play session.

---

## Implementation Notes

- Files: `AdsManager`, `AdTestIds`, `AdConfigFetcher`, `AndroidDataBridge`, `SaveSystem`, ProjectSettings (package id).

---

## Out of Scope

- New features; this story only verifies and re-identifies.

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above.*

---

## Test Evidence

*Governed by `qa.level: minimal`: tests are waived (advisory); a Visual/Feel or UI story's retained screenshot is not.*

**Story Type**: Integration
**Required evidence**: playtest note in `production/qa/` (tests waived at `qa.level: minimal`).

---

## Dependencies

- Depends on: Stories 001-006
- Unlocks: None
