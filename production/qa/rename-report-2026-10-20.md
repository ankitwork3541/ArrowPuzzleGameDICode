# Rename Report: 2026-10-20

> **Plan**: `production/qa/rename-plan.md`, map `production/qa/rename-map.json`.
> **Base**: snapshot commit `0063ab5` (tag `pre-rename-safety`). Nothing from this pass is committed.
> **Executed by**: Claude, in the open Unity 6000.6.3f1 Editor through the Pipeline CLI, plus a token-aware text tool (kept in the session scratchpad, not in the repo).

## What was done

| Step | Result |
|---|---|
| Assembly reload locked, then `AssetDatabase.MoveAsset` for everything | **138 of 138 moves, 0 errors** (2 folders, 63 scripts, 73 other assets; 14 of them retired into `_Retired/` folders, none deleted). One more clip renamed afterwards (`LD_ArrowTap.wav` to `LD_DragonTap.wav`). |
| GUIDs | All **235** `.meta` GUIDs under `Assets/` (outside third-party folders) compared before and after: **0 changed, 0 missing.** |
| Text pass (token-aware C# scanner, USS and UXML rewriter) | 63 `.cs`, 11 `.uss`, 11 `.uxml` files rewritten: 70 type names, identifier tokens (`Arrow` to `Dragon`, `Qube` to `Ember`), 189 USS classes, 44 UXML element names (both sides together), `url()` and `src` paths, menu paths (`LanternDragons/...`), Resources names. |
| Scene and prefabs | Scene is now `Scenes/LanternDragons.unity` (build settings point at it, same GUID). 16 scene GameObjects renamed (`GridBuilder` to `BoardBuilder`, `GameFlowManager` to `FlowDirector`, `UIToolkit_Home` to `UIToolkit_Hall`, ...). **0 missing scripts** in the scene and the three prefabs. |
| Recompile | One recompile after unlocking: **0 errors**. (The first pass left `ShotCapture` half-converted because the scanner mis-read `$"...{(a ? "x" : "y")}..."`; fixed in the tool and re-run, which also showed the other 62 files were already complete.) |
| Serialized fields (Phase B a) | 5 fields renamed with `[FormerlySerializedAs]` (`arrowPrefab` to `dragonPrefab`, `arrowSprites` to `ribbonSprites`, `onlyArrowsThatCanExit` to `onlyDragonsThatCanExit`, `arrowClickSuccessClip` to `dragonTapClip`, `arrowClickBlockedClip` to `dragonBumpClip`). The scene was reopened from disk and read back: `dragonPrefab=DragonPrefab`, tap clip `LD_ArrowTap` (now `LD_DragonTap`), bump clip `LD_Bump`, `ribbonSprites` length 3, 70 components, 0 missing. After a scene save the YAML carries the new field names. |
| Save and prefs keys | New names `lanterndragons_progress.json`, `lanterndragons_ad_config.json`, `lanterndragons.hint.*`, `lanterndragons.settings.*`. One-time **copy (never move)** migration from both older generations (`arrowqube_*`, and the shipped `progress.json`, `HintManager_RemainingHints`, `HintManager_LastFreeHintCount`, `settings.*`), newest first, only when the new key is absent: `ProgressStore.CopyLegacyFileIfMissing`, new `KeyMigration`. The ad-config cache is not migrated (refetched on every start). The stale "keeps the old arrowqube prefix" comments were rewritten. |
| Level file header (Phase B b) | `StagePack.Magic` `0x434C5041` (`APLC`) to `0x434C444C` (`LDLC`), `Version` 2 to 3. Re-baked: **1,500 levels, 298 shaped, 596 notched, 17,189 padlocks, every level re-solved with its locks, round-trip verified cell for cell**, 570 KB, 129 s. |

## Chunk hashes

| File | Before the rename | After Phase A (moves + text edits) | After Phase B (re-bake) |
|---|---|---|---|
| `Chunk_00.bytes` / `Pack_00.bytes` | `00825a39baa1e9c346b2147315a3e06108bf00ec7f3dae5b31b715c62b1d7883` | **identical** | `5e8dc61b3cb56e027c820ba42d5a4ad4e3ddc3116449f9d963ccf98fc6483fc7` |
| `Chunk_01.bytes` / `Pack_01.bytes` | `da68ee8fff43dc55349753109166be2644d9552394c595cada4c93e9cf7b8e55` | **identical** | `bc14f49e969c2067ab812ef8e6a91b4d494936812fbbcaffe8cfde439c449ac3` |

Phase A did not change a single baked byte. Phase B changed exactly three bytes per chunk (offsets 0, 1 and 4: the magic letters and the version); file sizes (300,495 and 283,331) and all level data are identical to the old files from git. Header now reads `4C 44 4C 43 03 00` ("LDLC", version 3).

## Tests

| Run | Result |
|---|---|
| EditMode, after Phase A | 76 of 76 passed (67 existing + 9 new `LegacyKeyMigrationTests`) |
| EditMode, after Phase B | **78 of 78 passed** (2 new `StagePackTests`: the new header, and a chunk with the old game's magic is rejected) |
| `ShippedCampaignTests` alone (explicit, `--include_explicit true`, detached, timeout 400) | **2 of 2 passed** (no Editor hang this time) |
| Windows dev player, `[EvidenceCheck]` | **5 of 5 PASS**: locked tap costs no life (3 to 3); map accepts completed level 3 and current level 6; map refuses locked level 46; progress read back from the disk file (42); one bump costs exactly one life (3 to 2). |
| Console | 0 errors. Only the pre-existing AdMob PluginImporter warnings. |
| UI consistency check (script) | 0 C# strings that reference a missing UXML name or USS class; all `url()` and `src` paths resolve (the 13 "missing" font hits were `%20` in a URL, the files exist). |

## Player frames (final, in `production/qa/evidence/story-008/`, copy in `story-008-v3/`)

A Development Windows player was rebuilt after the rename (detached build job, `Succeeded`, 0 errors) and run at 720x1280 and 360x640. I looked at every frame class:

| Frame | Verdict |
|---|---|
| `01-splash` | PASS. Name and logo at the top, loading bar at the bottom, no "my-progress" text. |
| `02-home` | PASS. Level card reads "5 CLEARED"; no total anywhere. The gear is a cog now. |
| `03-level-map` | PASS. Header chip "5 CLEARED"; done / current / locked nodes; the lowest node is partly under the dock as before. |
| `04-settings` | PASS. One grouped card, no scrollbar. |
| `05-hud-board`, `06-bump`, `07-hint`, `08-unlock` | PASS. Cream board, dock, dragons, ink bump flash, blue hint pulse, padlock opening mid-board. |
| `09-win`, `10-lose` | PASS. Bottom sheets with badges. |
| `11-ad-loading-modal` | PASS (new). |
| `12-hud-level-1500` | PASS (new). Four-digit title fits. |
| `13-small-1..4` (360x640) | PASS (new). No overflow. |
| `14-gear-home`, `15-pearl-zoom` | PASS for the gear. Pearl tail: PARTIAL, a small gold dot with an ink outline that is visible in the zoom but still small at full size. |

Nothing shows "/ 1500", nothing is blank or unstyled, nothing is dark.

## What was NOT renamed, and why

- **Third-party code**: GoogleMobileAds, PrimeTween, DOTween (Demigiant), TextMesh Pro, MultiTech Studio template, `Packages/`, ExternalDependencyManager.
- **Package id `in.dicodeinfotech.arrowqubepuzzle`, `companyName`, and the host bridge class string** (`HostBridge.BridgeClass`, `UnityAdBridge`): these are the developer's Story 007 decisions and the native host app must change in step.
- **Backend endpoint** in `StageTracker` (was `LevelTracker`): belongs to the previous game and needs the backend owner.
- **JSON field names** (`ProgressData`, `PromoRemoteConfig`): they mirror stored files and the backend; only the class names changed.
- **Level format order**: `StagePack.Dirs` order, `StageTier` member order and the header layout (only the magic and version values changed).
- **Inspector field names** other than the five above (the rest carry no old-game word).
- **Prose**: comments and log messages that say "arrow" as the game's ordinary word were left; identifiers were changed, prose was not.
- **Old assets** are in `_Retired/` folders, not deleted. Deleting them needs the developer's go-ahead.
- **Docs that describe history** (`production/`, `design/`, the CLAUDE.md progress log, the story files) still use the old names where they describe what happened at the time; `CLAUDE.md`'s folder section and the QA test plan were updated to the new names.

## Things to know

- `Assets/_Recovery/` (Unity's crash-recovery scene copies) appeared when the hung Editor was force-closed; it is untracked and can be deleted by the developer. The stray `Assets/production/` folder from the screenshot tool is still there too.
- `git status` shows many rename pairs because every `.meta` moved with its file; git detects most as renames.
- Renaming does not change generation logic, the level format layout, the save structure, the host bridge or the backend. If the aim is similarity at the level of code behaviour or binaries rather than names, those are the remaining overlaps.
- No commit was made. Rollback to the pre-rename state: tag `pre-rename-safety` (commit `0063ab5`), a developer decision.
