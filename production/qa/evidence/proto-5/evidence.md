# Prototype 5 level types: evidence (2026-10-20)

Windows development player (720x1280), `--proto --evidence-proto`, driven by `ShotCapture.ProtoRun`. The run uses the
real Play button and the real tap path; the save file was restored afterwards (`[EvidenceCheck]` PASS).

| Level | Frames | Checks (all PASS) |
|---|---|---|
| 1 Open board | `proto-1-start.png` | 13 dragons solved by tapping, 13 taps, no life lost |
| 2 Rocks | `proto-2-start.png` | 6 rocks hold their cell and are not dragons; 11 dragons solved, no life lost |
| 3 Linked lock | `proto-3-start.png`, `proto-3-link-tapped.png`, `proto-3-link-opened.png` | tap on a linked dragon with a clear ray costs no life and stays locked; 11 dragons solved |
| 4 Ice dragon | `proto-4-start.png`, `proto-4-cracked.png` | first tap: no move, ice cracked, no life; second tap shatters and slides; 9 dragons / 11 taps solved |
| 5 Portal pair | `proto-5-start.png`, `proto-5-before-transit.png`, `proto-5-transit-1..6.png` | blocked ray through the rings bounces back in place and costs 1 life; a bent ray leaves through the rings (remaining 6 to 5); 11 dragons solved |

Not verified: sound (crack and shatter reuse existing clips), Reduce Motion variants, Android, two dragons using the rings at
once, a hand-played feel pass. Art is procedural placeholder (`ProtoArt`). Release builds exclude the pack
(`ProtoBuildGuard`; checked with a non-Development build: pack absent from `resources.assets`, file restored afterwards).
