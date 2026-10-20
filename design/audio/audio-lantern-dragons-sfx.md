# Audio Design: Lantern Dragons SFX

> **Status**: Implemented, **not yet listened to by a human** (2026-10-20). Compiled from the sound-designer spec
> (`production/audio/lantern-dragons-sfx/sfx-spec.md`) and the sourcing/implementation report
> (`production/qa/audio-research-2026-10-20.md`). Clip licences: `Assets/Project/Audio/CREDITS.md`.
> **Gap**: no `design/audio/sound-bible.md` exists; the five proposed bible rules below stand in for it until the
> audio-director signs them off.

## Direction

Festive paper-lantern board, warm and gentle, short casual sessions: bells, a felt-mallet gong, soft taiko and
wood block. Failure is a sigh, never a punishment. Nothing synthetic, sci-fi or neon-sounding (that was the old game).

Proposed sound-bible rules:
1. One voice: hand-played festival percussion and bells (small bell, wood block, soft taiko, paper).
2. Warm and gentle at all times; no sound may startle.
3. Loudness ladder: UI quietest, dragon actions in the middle, locks and bump slightly above, result jingles loudest but never above -3 dBFS peak.
4. Gameplay one-shots end before the next likely tap (under ~600 ms audible tail); only result jingles exceed 1 s.
5. All pitched sounds in one pentatonic set (D major pentatonic: D E F# A B).

## Events and shipped clips (all CC0)

| Event | Code hook | Clip | Format | Length | Peak (dBFS) |
|---|---|---|---|---|---|
| Dragon tap | `PlayDragonClickSuccess` | `Lantern_DragonTap` (+ `_B`, `_C` round-robin) | mono | 0.26 s | -8 |
| Bump (blocked) | `PlayDragonClickBlocked` | `Lantern_DragonBump` (+ `_B`) | mono | 0.41 s | -6 |
| UI click | `PlayUIClick` / `TapSound` | `Lantern_UIClick` | mono | 0.10 s | -13 |
| Padlock shake | `PlayLockShake` | `Lantern_LockShake` (+ `_B`) | mono | 0.40 s | -9 |
| Unlock | `PlayUnlock` | `Lantern_Unlock` | mono | 0.52 s | -7 |
| Level complete | `PlayLevelComplete` | `Lantern_Win` | stereo, Vorbis | 2.60 s | -3 |
| Level failed | `PlayLevelFailed` | `Lantern_Lose` | stereo, Vorbis | 1.68 s | -5 |
| Dragon exit | `DragonController` | `Lantern_` exit clip | mono | | -9 |
| Heart lost | `EmberManager` (120 ms after the bump) | heart-lost clip | mono | | -11 |
| Hint | `StageLoader.PlayHint` | hint clip | mono | | -10 |
| Level entrance | `StageLoader.PlayEntrance` | entrance clip | mono | | -12 |
| Map node tap | `TrailMapController` (locked nodes use the plain click) | map-node clip | mono | | -11 |
| Sheet open / close | result sheet, maintenance and update sheets | sheet open and close clips | mono | | -14 |
| Options toggle on / off | `OptionsScreenController` (turning Sound off is silent) | toggle on and off clips | mono | | -12 |
| Ad-loading sheet | | **silent by decision** (ads bring their own audio) | | | |

Clip file names, sources and authors per file are in `Assets/Project/Audio/CREDITS.md`. Sources used:
rubberduck (OpenGameArt, CC0), Kenney "Interface Sounds" (CC0), Spring Spring "Pleasing Bell" (CC0). Rejected for
licence reasons: Church Bell and Dings (CC-BY-SA), an ElevenLabs gong (attribution), royalty-free but not CC0 packs.

## Mixing and playback (SoundDirector)

- 8-voice pool, per-event pitch spread, round-robin alternates (tap x3, bump x2, lock shake x2), per-event
  cooldown and voice limit so ten rapid taps do not stack.
- Separate jingle source on a new `Jingle` mixer group (`LanternMixer`). Effects are ducked 6 dB in code while a
  jingle plays (not a mixer side-chain); the group exists so a mixer owner can add one later.
- Turning Sound off stops a playing jingle at once. Master limiter: Unity's mixer has none, so peaks are kept at
  -3 dBFS or lower instead.
- Haptics: `HapticPulse` replaces the ~400 ms `Handheld.Vibrate()` with a 22 ms pulse (bump) and a 12 ms pulse
  (padlock shake) on Android; iOS keeps the system buzz for the bump only.
- Import: clips under 0.7 s mono, Decompress On Load, ADPCM; the two jingles stereo, Compressed In Memory, Vorbis.
  Estimated total well under 0.5 MB (cap 1 MB).

## Accessibility

No gameplay information is conveyed by audio alone: bump = ink flash + heart removed + haptic; lock shake = visual
shake (+ light haptic); unlock = visible lock opening; win/lose = result sheet. The Sound option is respected
everywhere. No BLOCKING gaps.

## Verification so far

- EditMode **212 of 212** pass (clip length / channels / peak / import settings per clip, every SoundDirector field
  wired in the scene, gate, pitch-range and round-robin tests). Windows dev player `[EvidenceCheck]` 11 of 11 PASS,
  no audio warnings.
- **Not verified**: how any of it sounds (nobody has listened), haptics on a phone, ducking by ear.

## Listening checklist (phone speaker, then headphones)

Listen first to the bump, then the win jingle, then 10 rapid dragon taps.

1. Does every sound feel warm and gentle, with nothing that startles on first hearing?
2. Can you tell tap, bump, lock shake and unlock apart with eyes closed?
3. Is the bump "disappointed" without sounding like an alarm or buzzer?
4. Is the lock shake clearly different from the bump, and not punishing?
5. Do 10 rapid taps sound pleasant, with no obvious repeat and no stacking mush?
6. Is each gameplay sound finished before the next tap would land (under ~600 ms)?
7. Does the win jingle sound festive and the lose jingle a sigh, not a loss?
8. Is the loudness order right: UI quietest, jingles loudest, nothing jumping out?
9. Is there nothing sci-fi, synthetic or neon-like in any clip?
10. Do overlapping pitched sounds stay in key, with no click at the start or end of any clip?

Most likely to need a swap: the two gongs (`gong_01`, `gong_02`) are Android-phone recordings and may sound thin,
and the lock shake is wood ticks rather than a real padlock rattle.

## Open items

- Audio-director sign-off on the bible rules, the key and the Jingle group.
- The synthesised placeholders (`LD_*`) now live in `Assets/Project/Audio/_Retired/`.
- A real mixer side-chain ducking and limiter (if wanted) need a mixer owner change.
