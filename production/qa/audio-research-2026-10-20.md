# Audio research and build: 2026-10-20

> Fork AU (sound sourcing), working to `production/audio/lantern-dragons-sfx/sfx-spec.md`.
> **Nobody has listened to any of these clips yet.** Everything below is measured or reasoned from the
> files, never heard. Treat the whole set as "technically in spec, taste unchecked".

## Sources considered

| Source | Result |
|---|---|
| OpenGameArt: 100 CC0 SFX, 100 CC0 metal and wood SFX (rubberduck) | **Used.** CC0, direct zip download works. Recorded with an Android phone, so quality is uneven; wood, paper and gong are usable. |
| OpenGameArt: Pleasing Bell Sound Effect (Spring Spring) | **Used.** CC0 WAV, a warm vibraphone-like bell (fundamental measured at 737.5 Hz, about F#5). Very quiet source (peak 0.06), normalised before use. |
| Kenney Interface Sounds (via OpenGameArt) | **Used** for the two tiniest clicks. CC0, OGG, normalised. |
| OpenGameArt: Church Bell, Dings (Wandering Door Games) | Skipped: CC-BY-SA (share-alike). |
| ElevenLabs gong page | Skipped: the free plan requires attribution. |
| Dark Fantasy Studio bell pack (itch.io), Motion Array asian bell | Skipped: royalty-free licences, not CC0 / unclear terms. |
| Pixabay | Not usable from here: the site answered HTTP 403 to a plain download request. |
| Mixkit, Freesound | Reachable, not needed (Freesound downloads need a login). |

No network or download problem blocked the chosen sources.

## How the clips were built

No `ffmpeg` is installed, so the OGG sources were decoded, trimmed (8 ms lead-in), pitch-shifted by resampling,
layered, low-passed at 7 kHz (the spec bans energy above 8 kHz), soft-saturated (tanh, to bring the crest factor
near the spec's 14 dB), faded (2 ms in, spec-length out) and peak-normalised **inside the Unity Editor** with a
script. The pitched bells were retuned to the spec's D major pentatonic (D5, E5, F#5, A5, B5, D6). The gongs and
the wood sounds are not pitch-corrected, so how they sit against the bells is one of the things to listen for.

## Result against the spec

Peak is the true file peak in dBFS. RMS is over the whole file, "body" over the part above 10% of the peak.

| Clip | Format | Length | Peak | RMS | Body RMS | Spec peak / RMS | Spec length |
|---|---|---|---|---|---|---|---|
| Lantern_DragonBump | mono | 0.41 s | -6 | -19.0 | -14.7 | -6 / -17 | 250-500 ms |
| Lantern_DragonBump_B | mono | 0.48 s | -6 | -19.3 | -16.4 | -6 / -17 | 250-500 ms |
| Lantern_DragonExit | mono | 0.45 s | -9 | -23.5 | -22.3 | -9 / -21 | 300-600 ms |
| Lantern_DragonTap | mono | 0.26 s | -8 | -25.3 | -19.7 | -8 / -20 | 150-350 ms |
| Lantern_DragonTap_B | mono | 0.33 s | -8 | -21.9 | -18.9 | -8 / -20 | 150-350 ms |
| Lantern_DragonTap_C | mono | 0.29 s | -8 | -20.6 | -18.0 | -8 / -20 | 150-350 ms |
| Lantern_HeartLost | mono | 0.35 s | -11 | -19.9 | -19.6 | -11 / -23 | 200-350 ms |
| Lantern_HintPulse | mono | 0.69 s | -10 | -22.7 | -21.5 | -10 / -22 | 400-700 ms |
| Lantern_LevelEntrance | mono | 0.64 s | -12 | -29.5 | -28.0 | -12 / -24 | 400-800 ms |
| Lantern_LockShake | mono | 0.40 s | -9 | -18.6 | -16.8 | -9 / -21 | 200-400 ms |
| Lantern_LockShake_B | mono | 0.40 s | -9 | -26.2 | -23.2 | -9 / -21 | 200-400 ms |
| Lantern_Lose | stereo | 1.68 s | -5 | -16.0 | n/a | -5 / -18 | 1.2-2.0 s |
| Lantern_MapNodeTap | mono | 0.18 s | -11 | -22.4 | -22.0 | -11 / -24 | 80-200 ms |
| Lantern_SheetClose | mono | 0.20 s | -14 | -33.7 | -30.8 | -14 / -27 | 150-300 ms |
| Lantern_SheetOpen | mono | 0.28 s | -14 | -30.9 | -30.2 | -14 / -27 | 150-300 ms |
| Lantern_ToggleOff | mono | 0.09 s | -12 | -25.5 | -22.9 | -12 / -26 | 50-120 ms |
| Lantern_ToggleOn | mono | 0.09 s | -12 | -26.7 | -23.2 | -12 / -26 | 50-120 ms |
| Lantern_UIClick | mono | 0.10 s | -13 | -32.6 | -23.1 | -12 / -26 (placed at -13) | 40-120 ms |
| Lantern_Unlock | mono | 0.52 s | -7 | -20.9 | -19.6 | -7 / -19 | 400-700 ms |
| Lantern_Win | stereo | 2.60 s | -3 | -16.2 | n/a | -3 / -16 | 2.0-3.5 s |

Notes on the numbers:
- Every peak is at or below the spec's per-event value and no clip is above -3 dBFS.
- One-shot body RMS lands within about 2 dB of the spec targets; the UI click, entrance and sheet sounds are 2 to 4 dB
  quieter than the table (they are noise or very short), which keeps them at the bottom of the ladder where the spec
  wants them.
- The jingles' RMS is measured over the whole file: -16.2 dB (win, spec -16) and -16.0 dB (lose, spec -18).
- Win is delivered at -3 dBFS peak as 16-bit WAV; the in-game copy is Compressed In Memory Vorbis (quality 0.7), which can
  overshoot the source by a fraction of a dB. The automated test checks the delivered WAV.

## Import settings (as the spec)

One-shots: mono, Decompress On Load, ADPCM, preload on, sample rate preserved. Win and lose: stereo, Compressed In Memory,
Vorbis quality 0.7, preload off, Load In Background on.

## Code that came with the clips

- `SoundDirector` plays through an 8-voice pool, with per-event pitch spread, retrigger cooldown and voice limit
  (`SfxGate`, `SfxVariation`), round-robin alternates for tap (3 takes), bump (2) and lock shake (2), a separate Jingle
  `AudioSource` on a new `Jingle` mixer group, and effects ducked by 6 dB while a jingle plays. Turning Sound off stops
  a playing jingle at once (`OptionsManager.SoundChanged`).
- New events and where they fire: dragon exit (`DragonController`, when a dragon leaves the board), heart lost
  (`EmberManager.LoseLife`, 120 ms after the bump), hint (`StageLoader.PlayHint`), level entrance
  (`StageLoader.PlayEntrance`), map node tap (`TrailMapController`; locked nodes get the plain UI click), sheet open
  (`SheetScreenController`, and the maintenance / update sheet on Home), sheet close (the four result-sheet dismiss
  handlers in `FlowDirector`), Options toggles (on is the higher tick; turning Sound off is silent). The ad loading
  sheet intentionally has no sound.
- Haptics: `HapticPulse` replaces the 400 ms `Handheld.Vibrate()` with a 22 ms (bump) or 12 ms (padlock shake) Android
  vibrator one-shot, guarded and try/caught. iOS keeps the system buzz for the bump only.

## Deviations from the spec, and why

- **File names** are `Lantern_<Name>.wav`, not the spec's `LD_<Name>.wav`, so they cannot be confused with the retired
  synthesised `LD_*` placeholders that now live in `_Retired/`.
- **No per-clip volume fields**: the loudness ladder is baked into the files at the spec peaks, so SoundDirector needs
  no trim values.
- **No Master limiter**: Unity's mixer has no true limiter effect (only compressor and duck effects); peaks are kept at
  -3 dBFS or lower instead.
- **Ducking is code-side** (a 6 dB volume factor on effects while a jingle plays), not a mixer side-chain: building a
  Duck Volume send and receive from editor code was not reliable. The `Jingle` group exists so the mixer owner can add
  one later.
- **Sheet close** has no natural "sheet disappears" event, so it plays when the player taps NEXT, RETRY or HOME.
- Looping ambience or music candidates were not searched for: the brief asks for effects only.

## Tests

`AudioClipsTests` (clips exist, length and channel count per the spec, peak under -3 dBFS read from the WAV, loudness
ladder, import settings, every SoundDirector field and alternate wired in the scene), `SfxGateTests` and
`SfxVariationTests` (cooldown, voice limit, ten rapid taps, pitch bounds, round-robin never repeats). Full EditMode
suite: **212 of 212 pass**. Windows dev player: `[EvidenceCheck]` 11 of 11 PASS and no audio warnings in the log (the
AdMob `ArgumentNullException` at start-up is the known Windows-only one).

## What to listen to first (the spec's checklist, on a phone speaker and then headphones)

1. The **bump** (`Lantern_DragonBump`), then the **win jingle** (loudest, most startling), then ten rapid dragon taps.
2. Is everything warm and gentle, nothing startling on first hearing?
3. Can you tell tap, bump, lock shake and unlock apart with eyes closed?
4. Is the bump "disappointed" and not an alarm; is the lock shake different from it and not punishing?
5. Do ten rapid taps sound pleasant with no obvious repeat and no stacking mush?
6. Does each gameplay sound finish before the next tap would land?
7. Win: festive (gong, bells, soft drum)? Lose: a sigh, not a loss?
8. Is the loudness order right: UI quietest, jingles loudest?
9. Is anything sci-fi or synthetic? (The gongs are phone recordings and may sound thin.)
10. Do the pitched sounds sit in one key with no clash, and is there no click at the start or end of any clip?

Most likely to need a swap after listening: the gong (`gong_01`, `gong_02`, phone recordings) in the win and lose jingles,
and the lock shake (wood ticks, not an actual padlock rattle).
