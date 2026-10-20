# Lantern Dragons - SFX Specification

Status: Draft v1, 2026-10-20. Author: sound-designer. Reports to: audio-director (review pending).
Audience: the asset-sourcing agent (Fork AU) and the developer, who judge candidate clips against this.

Sources read: `Assets/Project/Scripts/Managers/SoundDirector.cs`, `LanternMixer.mixer` (metadata), `LD_Win.wav.meta`, call sites (`DragonController`, `FlowDirector`, `TapSound`, `OptionsManager`), `design/game-brief.md` direction as supplied in the task.

## 0. Governing-document gap

`design/audio/sound-bible.md` does NOT exist. Direction is taken only from the game brief (bells, soft drums, a gong on clear; warm and gentle; casual, short sessions). Everything in this file marked "proposed" is a sound-designer recommendation that needs audio-director sign-off.

Five rules a one-page sound bible for this game would need (proposed):

1. **Voice**: one family of sounds - hand-played festival percussion and bells (small bell, wood block, soft taiko, paper). Nothing synthetic-sounding.
2. **Mood ceiling**: warm and gentle at all times. Failure is a soft sigh, never a punishment; no sound is allowed to startle.
3. **Loudness ladder**: UI quietest, dragon actions middle, locks/bump slightly above, result jingles loudest but under -3 dBFS peak. Exact numbers in section 1.
4. **Length discipline**: gameplay one-shots end before the next likely tap (under 600 ms audible tail); only result jingles may exceed 1 s.
5. **Pitch and key**: all pitched sounds sit in one pentatonic set (proposed D major pentatonic: D E F# A B) so overlapping sounds never clash.

## 1. Audio event list

Mixer group: all events route to `SFX` (via `sfxMixerGroup`), under `Master`. The jingle source also uses `sfxMixerGroup` (see section 3 for the recommended split). Peak = true peak of the delivered file, dBFS. RMS = short-term over the sound body.

### 1a. Wired events (exist in code today)

| ID | Code method | Trigger | Priority | Sonic description | Length | Peak / RMS dBFS | Pitch var | Voice limit / cooldown |
|---|---|---|---|---|---|---|---|---|
| SFX_DragonTap | `PlayDragonClickSuccess` (DragonController ~768) | Tap on a free dragon, slide begins | High | Soft wood-block or small taiko tap with a tiny paper rustle; round, no click transient | 150-350 ms | -8 / -20 | +/-4% (about +/-70 cents), 3 variants round-robin | 4 voices; 40 ms retrigger guard |
| SFX_DragonBump | `PlayDragonClickBlocked` (~660) | Slide hits another dragon, ink flash, life lost | Highest gameplay | Low muffled thud (drum skin, felt mallet) plus a short dull wood knock; disappointed, not alarming | 250-500 ms | -6 / -17 | +/-3%, 2 variants | 2 voices; 150 ms guard |
| SFX_LockShake | `PlayLockShake` (~404) | Tap on a locked dragon (free, no life lost) | Medium | Dry wooden rattle of a small padlock or bamboo clack, 2-3 quick ticks; neutral, clearly not the bump | 200-400 ms | -9 / -21 | +/-5%, 2 variants | 1 voice; 250 ms cooldown (rapid taps must not machine-gun) |
| SFX_Unlock | `PlayUnlock` (~149) | Lock countdown reaches zero and the lock opens | High | One bright small bell or chime plus a soft click, rising; the reward moment before the win | 400-700 ms | -7 / -19 | none (fixed pitch, in key) | 1 voice; if two locks open within 150 ms, play once |
| SFX_UIClick | `PlayUIClick` (`TapSound`) | Any UI button, nav item, settings row | Low | Tiny soft paper-tap or wood tick; the quietest sound in the game | 40-120 ms | -12 / -26 | +/-3% | 2 voices; 60 ms guard |
| JNG_LevelComplete | `PlayLevelComplete` (FlowDirector 201) | Last dragon exits, result sheet opens | Highest | Gong strike (soft mallet) into 3-4 ascending bell notes and a soft drum roll-off; festive, resolved | 2.0-3.5 s | -3 / -16 | none | Jingle source, single voice; new jingle stops previous |
| JNG_LevelFailed | `PlayLevelFailed` (FlowDirector 217) | Lives reach zero, result sheet opens | Highest | Two descending soft bell notes over a low gentle drum; a sigh, not a buzzer; ends in a major-leaning resolution | 1.2-2.0 s | -5 / -18 | none | As above |

Both jingles are stopped by `StopJingle()` on sheet dismiss; the tail must therefore fade naturally and a hard cut must not click (files end at silence, last 20 ms zero-crossing faded).

### 1b. Proposed new events (no code hooks yet; each needs a `SoundDirector` method from the programmer)

| ID | Trigger | Priority | Sonic description | Length | Peak / RMS dBFS | Pitch var | Limit / cooldown | In first pass? |
|---|---|---|---|---|---|---|---|---|
| SFX_DragonExit | Dragon fully clears the board (per dragon) | Medium | Airy paper whoosh, soft, with a tiny rising bell grace note | 300-600 ms | -9 / -21 | +/-4% | 3 voices; 80 ms guard (several can exit in a chain) | Yes, highest value of the new set |
| SFX_HeartLost | Heart/life visually removed (follows bump) | Medium | Small soft glassy pop, one low bell note; must sit under the bump, not stack with it | 200-350 ms | -11 / -23 | none | 1 voice; play 120 ms after bump | Yes |
| SFX_HintPulse | Hint reveals a dragon (after ad/free hint unlock) | Low-medium | Gentle two-note bell shimmer, rising | 400-700 ms | -10 / -22 | none | 1 voice; 500 ms cooldown | Yes |
| SFX_LevelEntrance | Board builds in at level start | Low | Soft paper-unfold swish, optional single low drum | 400-800 ms | -12 / -24 | +/-3% | 1 voice | Later |
| SFX_MapNodeTap | Tap on a level-map node | Low | Same family as UIClick but slightly warmer, a touch of bell | 80-200 ms | -11 / -24 | +/-3% | 1 voice; 100 ms guard | Later (may reuse UIClick) |
| SFX_SheetOpen / SFX_SheetClose | Bottom sheet or modal slides in / out (result, update, maintenance, ad-loading wait sheet) | Low | Soft paper slide; close is the same sound shorter and lower | 150-300 ms | -14 / -27 | +/-3% | 1 voice; sheets share one slot | Later |
| SFX_ToggleOn / SFX_ToggleOff | Options Sound / Vibration switch | Low | Short wood tick, on slightly higher than off. Note: toggling Sound off must be silent (see section 4) | 50-120 ms | -12 / -26 | none | 1 voice | Later (UIClick is acceptable) |
| SFX_AdLoading | Ad modal / wait sheet | None | Intentionally NO sound. Ads bring their own audio; our sound must not overlap | - | - | - | - | Decision: silent |

Notes: update and maintenance sheets reuse SFX_SheetOpen; they must never use the failure sound (they are not player errors).

## 2. Sonic palette

| Use | Avoid |
|---|---|
| Small tuned bells (temple bell, hand bell, kalimba-like pluck), felt-mallet gong, soft taiko and frame drum, wood block, bamboo clack, paper rustle and slide | Harsh alarms, buzzers, error beeps |
| Warm mids (200 Hz - 4 kHz body), soft attack (3-15 ms) | Sci-fi bleeps, laser zaps, digital glitches, synth sweeps (reads as the old neon game) |
| Natural short decay, dry or very small room (under 0.3 s reverb) | Long reverb or delay tails that mask the next tap |
| One pentatonic key across all pitched sounds | Content above 8 kHz carrying energy (cymbal sizzle, sharp transients) |
| Rounded, slightly imperfect hand-made feel | Cartoon boings, coin-pickup clones, loud sub-bass over 80 Hz (phone speakers cannot play it, only distort) |

## 3. Mixing and technical rules

**Levels (peak dBFS, matching the tables above):** UI -12 to -14, locks/exit/hint -9 to -10, tap -8, unlock -7, bump -6, failed jingle -5, complete jingle -3. Never any clip above -3 dBFS true peak. Normalise by RMS first, then set peaks; keep crest under ~14 dB so mobile speakers do not distort.

**Headroom/clipping:** Master and SFX mixer group at 0 dB default; no master gain above 0. Recommend (config change, audio-director to approve) a Limiter at -1 dB on Master as a safety net; the mixer currently has only Attenuation effects.

**Ducking (proposed):** Mixer currently has no ducking. Jingles (complete/failed) should duck gameplay sfx by -6 dB for the jingle length. Because gameplay sfx should have ended by the result sheet, a simpler approach is acceptable: split a `Jingle` group under Master and give it its own volume, so jingles can be tuned independently of sfx. Needs a mixer change by the middleware owner, not by this spec.

**Stereo/mono:**

| Category | Format | Reason |
|---|---|---|
| Gameplay one-shots, UI, locks | Mono, center (spatialBlend 0) | Halves memory, phone speakers are mono, 2D game |
| Jingles | Stereo | Width only matters on headphones for the longer pieces |

**Format and import (Unity 6, mobile):** Deliver 44.1 kHz, 16-bit WAV (or lossless source); Unity does the compression.

| Clip type | Length | Load Type | Compression | Preload | Sample rate override |
|---|---|---|---|---|---|
| UI click, tap, bump, lock, exit, hint, ticks | under 0.7 s | Decompress On Load | ADPCM | on | Preserve (44.1) or 32 kHz if no content above 12 kHz |
| Unlock, entrance, sheet | 0.4-0.8 s | Decompress On Load | ADPCM | on | Preserve |
| Win/Lose jingles | over 1 s | Compressed In Memory | Vorbis, quality ~70% | off ("Load In Background" on) | Preserve |

Current `LD_Win.wav` is Compressed In Memory, Vorbis (loadType 1, compressionFormat 1), preload off: correct for its length, keep it.

**Memory estimate (decompressed, mono 44.1 kHz 16-bit = ~88 KB/s; ADPCM roughly 1/3.5 of that):**
Short sfx: about 15 clips x avg 0.3 s = 4.5 s mono = ~400 KB PCM, ~115 KB as ADPCM. Jingles: 2 clips, about 5 s stereo Vorbis in memory ~ 120 KB. Total under 0.5 MB; budget cap proposed at 1 MB for all audio.

**Variations (round-robin, no immediate repeat):** Tap 3, Bump 2, LockShake 2, Exit 2, rest 1. Pitch randomisation as per tables. Note: `SoundDirector` does not implement round-robin, random pitch or voice limits today (it calls `PlayOneShot` with no pitch change); these need a programmer task. Until then, source distinct variant files and tolerate repetition.

## 4. Accessibility check

| Gameplay-critical event | Visual fallback | Haptic fallback | Verdict |
|---|---|---|---|
| Bump / blocked | Ink-dark flash, heart removed | `Handheld.Vibrate()` gated by Vibration toggle (DragonController ~662) | OK |
| Lock shake (tap on locked) | Lock shakes | None | OK for audio (visual present). Gap: no haptic; non-blocking, optional light tick |
| Unlock | Lock visibly opens (assumed from design; verify) | None | OK, non-critical |
| Level complete / failed | Result sheet | None | OK |
| Dragon exit, hint, heart lost | Visible motion / pulse / heart UI | None | OK |

BLOCKING gaps: none found.

Other points:
- Sound toggle is respected in `PlaySfx` and `PlayJingle` (checked via `OptionsManager.SoundEnabled`). Gap (non-blocking): turning Sound off while a jingle is already playing does not stop it; `StopJingle()` should be called on toggle-off. Programmer task.
- Vibration is a long generic `Handheld.Vibrate()` (about 400 ms on Android), not a short tick; this is heavy for a "gentle" game. Concern for the developer, outside audio scope.
- Sudden loud/high-frequency risk: the bump (highest sfx) and the complete jingle's gong strike. Both must have attack of 5 ms or more, no energy above 8 kHz, and peaks capped as above. Candidate clips with a sharp transient or sizzle fail.
- No safety-critical audio exists; nothing is audio-only. No sound is used to convey information that has no visual equivalent. Keep it that way.

## 5. Acceptance checklist (listen on a phone speaker, then headphones)

Listen first to: SFX_DragonBump, then the complete jingle (loudest and most startling), then 10 rapid dragon taps in a row.

1. Does every sound feel warm and gentle, with nothing that startles on first hearing?
2. Can you tell tap, bump, lock shake and unlock apart with eyes closed?
3. Is the bump clearly "disappointed" without sounding like an alarm or buzzer?
4. Is the lock shake clearly different from the bump, and not punishing?
5. Do 10 rapid taps sound pleasant, with no obvious repeat and no stacking mush?
6. Is each gameplay sound finished before the next tap would land (under ~600 ms)?
7. Does the complete jingle sound festive (gong, bells, soft drum) and the failed jingle a sigh, not a loss?
8. Is the loudness order right: UI quietest, jingles loudest, nothing jumping out between sounds?
9. Is there nothing sci-fi, synthetic or neon-like in any clip?
10. Do all pitched sounds sit in the same key, with no clash when two overlap, and no click at the start or end of any clip?

## 6. Open items for audio-director

- Approve the proposed bible rules (section 0) and the D major pentatonic key.
- Approve the Jingle mixer group, ducking, and Master limiter (needs a mixer owner change).
- Programmer tasks: new `SoundDirector` methods for section 1b events, round-robin plus pitch variation, per-event cooldown and voice limit, stop jingle on Sound toggle-off.
- Naming: clip files follow existing `LD_<Name>.wav`; no audio naming convention document exists to cite.
