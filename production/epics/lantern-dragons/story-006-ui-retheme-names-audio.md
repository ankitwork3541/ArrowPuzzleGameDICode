# Story 006: UI retheme, names, audio

> **Epic**: Lantern Dragons
> **Status**: In Progress (2026-10-20: UI retheme done via Stories 008; name "Lantern Dragons", new logo and Android icon, 7 synthesised LD_* clips and lock shake/unlock sounds are in code. Open: the clips were never listened to, the Settings toggles were not run, and the old name still lives in save/pref keys, the package id and comments; no evidence doc of its own, screenshots are in `story-008/`)
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: [fill before starting]
> **Manifest Version**: N/A (minimal — no control manifest)
> **Last Updated**: [set by /dev-story when implementation begins]

## Context

**GDD**: `design/game-brief.md`
**Requirement**: Brief MVP feature 6 (UI retheme, names, audio)

**ADR Governing Implementation**: N/A (minimal — no ADRs)
**ADR Decision Summary**: N/A (minimal — no ADRs)
**ADR Version**: N/A (minimal — no ADRs)

**Engine**: Unity 6000.6.3f1 | **Risk**: HIGH (per `docs/engine-reference/unity/VERSION.md`)
**Engine Notes**: none (no ADR engine-compatibility analysis at minimal).

**Control Manifest Rules (this layer)**: N/A (minimal — no control manifest)

---

## Acceptance Criteria

*From `design/game-brief.md` (Art & audio direction + MVP feature 6), scoped to this story:*

- [ ] Splash, Home, HUD and Settings use the lantern look (USS) and new strings; the old game's name appears nowhere in UXML, USS or code strings.
- [ ] App name and icon are the new game's.
- [ ] Audio clips replaced for tap, bump, exit, win and lose, plus new lock-shake and unlock sounds (bells, drums, gong).
- [ ] Settings sound and haptics toggles still mute or enable everything.
- [ ] A retained screenshot of every screen touched is saved to `production/qa/evidence/`.

---

## Implementation Notes

- Files: `Themes/*.uss`, `UXML/*.uxml`, `AudioManager`, `SettingsScreenController`.
- Audio goes through `AudioManager` only; no new audio code path.

---

## Out of Scope

- Package id and ad units: Story 007.

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

- Depends on: Story 004
- Unlocks: Story 007
