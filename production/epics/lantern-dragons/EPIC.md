# Epic: Lantern Dragons

> **Source**: `design/game-brief.md` (minimal tier: no GDD, ADRs or TR registry)
> **Goal**: Tap paper dragons to slide them off a midnight festival board, but padlocked dragons stay stuck until enough other dragons have escaped.

## Scope
Reskin of the previous arrow-path game. Keep: core gameplay, ads, level generator. Change: theme, art, names, levels, level display. Add: locked arrows.

## Ordering
Brief build order: locked-arrow logic and generator/solver first (only real risk), then seeds and ramp, shapes, art, level display, UI/audio, package id and ads.

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | Locked arrows | Logic | Complete | N/A |
| 002 | New level pipeline | Logic | Complete | N/A |
| 003 | Festival board shapes | Config/Data | Complete except git commit (19 shapes, 1,500 levels baked) | N/A |
| 004 | Lantern theme art | Visual/Feel | Superseded by 008 (dark look; head/pearl/padlock sprites are reused) | N/A |
| 005 | Level display and level map | UI | In Review (sign-off pending; AC-3/4/5 not tested) | N/A |
| 006 | UI retheme, names, audio | UI | In Progress (audio unheard; old name remains in keys and package id) | N/A |
| 007 | New identity and kept-systems check | Integration | In Progress (blocked on package id, AdMob units, host app) | N/A |
| 008 | Light paper theme | Visual/Feel | In Review (sign-off pending; pearl tail open) | N/A |

Last refreshed 2026-10-20 from the story files and `production/qa/evidence/*`. Sprint plan: `production/sprints/sprint-2026-10-20.md`.
