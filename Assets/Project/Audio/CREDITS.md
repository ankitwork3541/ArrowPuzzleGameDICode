# Third-party audio

Every sound in `Assets/Project/Audio/Lantern_*.wav` is derived from the CC0 (public domain) recordings below.
CC0 needs no attribution; the credits are given anyway. All files were retrieved on 2026-10-20 and then edited
(trimmed, pitch-shifted, layered, low-passed at 7 kHz, soft-saturated, level-matched; the build is described in
`production/qa/audio-research-2026-10-20.md`). Licence text for all of them:
https://creativecommons.org/publicdomain/zero/1.0/

| Source pack | Author | Page | Licence |
|---|---|---|---|
| 100 CC0 SFX | rubberduck | https://opengameart.org/content/100-cc0-sfx | CC0 1.0 |
| 100 CC0 metal and wood SFX | rubberduck | https://opengameart.org/content/100-cc0-metal-and-wood-sfx | CC0 1.0 |
| Interface Sounds 1.0 | Kenney (kenney.nl) | https://opengameart.org/content/interface-sounds | CC0 1.0 (the pack's own License.txt says the same; a credit to Kenney is optional) |
| Pleasing Bell Sound Effect | Spring Spring | https://opengameart.org/content/pleasing-bell-sound-effect | CC0 1.0 |

## What each shipped clip is made from

| Clip | Made from | Source |
|---|---|---|
| `Lantern_DragonBump.wav` | wood_hit_07 + a pitched-down copy (x0.7) | rubberduck, 100 CC0 metal and wood SFX |
| `Lantern_DragonBump_B.wav` | hit_04 + a pitched-down copy (x0.7) | rubberduck, 100 CC0 SFX |
| `Lantern_DragonExit.wav` | paper_02 (pitch x1.15) + pleasing-bell grace note D6 | rubberduck (100 CC0 SFX), Spring Spring |
| `Lantern_DragonTap.wav` | wood_hit_05 | rubberduck, 100 CC0 metal and wood SFX |
| `Lantern_DragonTap_B.wav` | wooden_03 | rubberduck, 100 CC0 SFX |
| `Lantern_DragonTap_C.wav` | wood_misc_08 | rubberduck, 100 CC0 metal and wood SFX |
| `Lantern_HeartLost.wav` | glass_02 low-passed + pleasing-bell note D4 | rubberduck (100 CC0 SFX), Spring Spring |
| `Lantern_HintPulse.wav` | pleasing-bell notes E5 then B5 | Spring Spring (Pleasing Bell) |
| `Lantern_LevelEntrance.wav` | paper_04 (pitch x0.9) + soft drum thump (wood_hit_07 x0.5) | rubberduck (both packs) |
| `Lantern_LockShake.wav` | three wood_hit_09 ticks (pitch 1.0 / 1.12 / 0.92) | rubberduck, 100 CC0 metal and wood SFX |
| `Lantern_LockShake_B.wav` | three wood_hit_05 ticks (pitch 1.1 / 1.25 / 1.0) | rubberduck, 100 CC0 metal and wood SFX |
| `Lantern_Lose.wav` | gong_02 (x0.7) + soft drum thump + pleasing-bell notes A5 then D5 | rubberduck (both packs), Spring Spring |
| `Lantern_MapNodeTap.wav` | Kenney click_003 low-passed + pleasing-bell A5 | Kenney (Interface Sounds), Spring Spring |
| `Lantern_SheetClose.wav` | paper_01 (pitch x0.8) low-passed at 3 kHz | rubberduck, 100 CC0 SFX |
| `Lantern_SheetOpen.wav` | paper_02 low-passed at 3.5 kHz | rubberduck, 100 CC0 SFX |
| `Lantern_ToggleOff.wav` | wood_hit_05 pitched x0.95, 90 ms | rubberduck, 100 CC0 metal and wood SFX |
| `Lantern_ToggleOn.wav` | wood_hit_05 pitched x1.25, 90 ms | rubberduck, 100 CC0 metal and wood SFX |
| `Lantern_UIClick.wav` | Kenney click_001, low-passed at 3 kHz | Kenney, Interface Sounds |
| `Lantern_Unlock.wav` | soft Kenney click + pleasing-bell notes A5 then D6 | Spring Spring (Pleasing Bell), Kenney (Interface Sounds) |
| `Lantern_Win.wav` | gong_02 (x0.65) + gong_01 (x0.7 at 0.58 s) + soft drum thumps + pleasing-bell run D5 F#5 A5 D6 | rubberduck (both packs), Spring Spring |

The earlier synthesised placeholder clips (`LD_*.wav`) sit unused in `Assets/Project/Audio/_Retired/`.
