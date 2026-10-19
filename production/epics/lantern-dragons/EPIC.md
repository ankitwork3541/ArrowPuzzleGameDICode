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
| 001 | Locked arrows | Logic | Ready | N/A |
| 002 | New level pipeline | Logic | Ready | N/A |
| 003 | Festival board shapes | Config/Data | Ready | N/A |
| 004 | Lantern theme art | Visual/Feel | Ready | N/A |
| 005 | Level display and level map | UI | Ready | N/A |
| 006 | UI retheme, names, audio | UI | Ready | N/A |
| 007 | New identity and kept-systems check | Integration | Ready | N/A |
