# Rename Plan: make the project stop matching the old game

> **Status**: PLAN ONLY. Nothing has been renamed. Written 2026-10-20 by a read-only pass.
> **Scope chosen by the developer**: assets + scripts + classes + USS class names + save/prefs keys. **Naming style**: dragon / lantern theme words.
> **Base**: commit `0063ab5` (safety snapshot, already made by the developer). Full machine-readable map: `production/qa/rename-map.json`.

## Headline numbers

| What | Count |
|---|---|
| C# types renamed (file name = class name) | 63 files, 70 types with nested ones |
| Non-script assets/folders moved | 75 (of which 14 are "retire to `_Retired/`", not renamed) |
| USS classes renamed | 189 (no duplicates, no clash with an unchanged name) |
| UXML element names that C# reads, renamed on both sides | 44 |
| Persisted keys with one-time migration | 7 |
| Text files needing content edits | 105 (63 `.cs`, 6 `.uss`, 7 `.uxml`, 3 `.unity`, 2 `.prefab`, 3 `.asset`, 21 `.md`) |
| Existing serialized scene/prefab/asset values touched | 0 (field names are left alone) |

## 1. Rules that keep Unity from losing anything

1. **Move the `.meta` with the file, always.** GUIDs never change. Use `AssetDatabase.MoveAsset`, not `mv`, so the Editor moves both and updates references. Scenes, prefabs and assets store scripts by GUID, so renaming a class does not break them.
2. **A MonoBehaviour/ScriptableObject file name must equal its class name**, so file and class are renamed in the same step. 63 of 63 files follow this today.
3. **Serialized field names are never renamed** (they are the keys in `Game.unity`, the prefabs and `AdConfig.asset`). The full per-script list is in `rename-map.json` under `serializedFieldsPerScript_leaveAlone`. Exact class names reached by the scene: `AdConfig, AdConfigFetcher, AdLoadingOverlay, AdsManager, ArrowController (prefab), AudioManager, BackdropArrows, BannerAdUnit, CameraController, DeviceScaleCategory, Dot (prefab), GameFlowManager, GameplayHUDController, GridBuilder, GuideLines, HomeScreenController, InterstitialAdUnit, LevelLoader, LevelManager, LivesManager, NativeAdUnit, ResultScreenController, SettingsManager, SettingsScreenController, SplashScreenController`. Five of those fields still carry the old word (`arrowPrefab`, `arrowSprites`, `onlyArrowsThatCanExit`, `arrowClickSuccessClip`, `arrowClickBlockedClip`); renaming them is the optional **Phase B** below, using `[FormerlySerializedAs]`.
4. **JsonUtility types keep their field names** (`version, currentLevelIndex, highestLevelCompleted`, and every `AdRemoteConfig` field, which mirrors the backend JSON). Only the class name changes.
5. **UXML element `name`s that C# queries change on both sides together** (44 names listed in the map). A one-sided rename compiles and then silently breaks the UI.

## 2. The map (summary; the JSON has every row)

**Scripts** (file = class, folder unchanged)

| Folder | Old -> New |
|---|---|
| Core | ArrowController -> DragonController, Dot -> Spark, GridBuilder -> BoardBuilder, GridCell -> BoardCell, GuideLines -> LatticeGuides |
| Data | AdConfig -> PromoConfig (with AdSlotKind, NativeTemplateSize, NativeStyleMapping -> PromoSlotKind, PromoTemplateSize, PromoStyleMapping), AdRemoteConfig -> PromoRemoteConfig, AdTestIds -> PromoTestIds, ArrowPalette -> DragonPalette (nested Dealer -> Shuffler), ArrowPathData -> DragonPathData, LevelBinary -> StagePack, LevelData -> StageData (Difficulty -> StageTier), SaveData -> ProgressData, SaveSystem -> ProgressStore |
| Editor | LevelBinaryWriter -> StagePackWriter, LevelCampaignGenerator -> StageCampaignGenerator, LevelCarver -> StageCarver, LevelDifficulty -> StageRating, LevelLocks -> StageLocks, LevelShapeMask -> StageShapeMask |
| Managers | AdConfigFetcher -> PromoConfigFetcher, AdSettings -> PromoSettings, AdsManager -> PromoManager, AndroidDataBridge -> HostBridge, AudioManager -> SoundDirector, BackdropArrows -> LanternBackdrop, BannerAdUnit -> PromoBannerUnit, CameraController -> BoardCamera, EvidenceCapture -> ShotCapture, GameFlowManager -> FlowDirector, HintManager -> ClueManager, InterstitialAdUnit -> PromoInterstitialUnit, InterstitialManager -> PromoInterstitialManager, LevelLoader -> StageLoader, LevelManager -> StageManager, LevelTracker -> StageTracker, LivesManager -> EmberManager, NativeAdUnit -> PromoNativeUnit (NativeAdPlacement -> PromoNativePlacement), RewardedAdUnit -> PromoRewardedUnit, ScreenBackdrop -> SkyBackdrop, SettingsManager -> OptionsManager |
| UI | AdLoadingOverlay -> WaitSheet, AdPlaceholder -> PromoPlaceholder, BannerTestScreenController -> PromoLabScreenController, BottomNavBinder -> TabDockBinder, ConfettiView -> PetalBurstView, DeviceScaleCategory -> ScreenScaleProfile, GameplayHUDController -> PlayScreenController, HomeScreenController -> HallScreenController, LevelMapController -> TrailMapController, NativeAdSpace -> PromoSpace, ResultScreenController -> SheetScreenController, SettingsScreenController -> OptionsScreenController, SplashScreenController -> IntroScreenController, UIClickSound -> TapSound |
| Tests (`Assets/Tests/EditMode/Editor`) | ArrowPaletteTests -> DragonPaletteTests, BakedCampaignTests -> ShippedCampaignTests, LevelBinaryTests -> StagePackTests, LevelCarverTests -> StageCarverTests, LevelLocksTests -> StageLocksTests, LevelSolverTests -> StageSolverTests, SaveMigrationTests -> ProgressMigrationTests, TestLevels -> TestStages |

**Identifiers inside code** (about 62 distinct, all non-serialized): token rule `Arrow->Dragon, arrow->dragon, Qube->Ember` applied to identifiers only (for example `OnArrowLeft -> OnDragonLeft`, `PlayArrowClickBlocked -> PlayDragonClickBlocked`, `wantArrows -> wantDragons`). Before applying, grep every new identifier for collisions with existing `Dragon*` names (`DragonHead`, `DragonPearl` sprites).

**Assets** (full list in the JSON `assetMoves`)
- Prefabs: ArrowPrefab -> DragonPrefab, CommittedLineTemplate -> TrailLineTemplate, Dot -> Spark.
- Sprites from the old game: `Icon*` -> `Glyph*` (13), HomeIcon/LockIcon/SettingsIcon -> GlyphHall/GlyphLatch/GlyphCog, LogoSquare -> CrestSquare, Circle/Circle01/Flare01/Gradient01/Triangle -> DiscSolid/DiscSoft/SparkFlare/FadeRamp/WedgeSolid, UIBarFill/UIBarTrack -> InkBarFill/InkBarTrack, BackdropArrow{Bend,Straight,Zigzag} -> BackdropRibbon{...}.
- UXML + USS pairs: AdLoadingOverlay -> WaitSheet, AdPlaceholder -> PromoPlaceholder, BannerTestScreen -> PromoLabScreen, BottomNav -> TabDock, GameplayScreen -> PlayScreen, HomeScreen -> HallScreen, LevelMapScreen -> TrailMapScreen, SettingsScreen -> OptionsScreen, SplashScreen -> IntroScreen, ResultScreen.uss -> SheetScreen.uss, WinScreen/LoseScreen -> SheetWin/SheetLose, Typography -> TypeScale, MainPanelSettings -> LanternPanelSettings, OverlayPanelSettings -> OverlayPanelLantern.
- Scene: `Scenes/Game.unity` -> `Scenes/LanternDragons.unity`, folder `Scenes/Game/` -> `Scenes/LanternDragons/`.
- Resources: `AdConfig.asset` -> `PromoConfig.asset` (the `Resources.Load("AdConfig")` string changes with it), `Resources/Levels/Chunk_NN.bytes` -> `Resources/Stages/Pack_NN.bytes` (`StagePack.ResourceFolder` / `ChunkPrefix` and `StagePackWriter.OutputFolder` change to match).
- Audio: `MainMixer.mixer` -> `LanternMixer.mixer`. The old clips are no longer referenced by `Game.unity` (it points at `LD_*`), so they are **moved to `Audio/_Retired/`, not renamed**: `ArrowClick_Blocked.ogg, ArrowClick_Success.mp3, ArrowTap.wav, LevelComplete.wav, LevelFailed.wav, UIClick.wav`. Same for `NeonGradient.png`, `UIHexPill*`, `UIOctagon*` and the three `Mat_Neon*.mat` (no GUID references found). Verify "no references" with a GUID grep before each move. **Nothing is deleted**; deleting from `_Retired/` needs the developer's go-ahead.
- Left alone: `LD_*` audio, `Dragon*`, `Padlock`, `BackdropBlossom/Cloud/Lantern`, `UIRound*`, `UICapsule*`, `LogoRound`, `AppIcon/*` (made in this project), and the Oxanium fonts (third-party).

**USS classes and element names**: block-prefix table, applied to `.uss` selectors, `.uxml` `class=` and `name=`, and exact C# string literals. Examples: `top-panel -> crown-panel`, `bottom-bar -> lower-dock`, `action-button -> tool-button`, `hint-badge -> clue-badge`, `heart -> ember`, `result-* -> sheet-*`, `map-* -> trail-*`, `nav-* -> tab-*`, `neon-progress -> lantern-progress`, `splash-* -> intro-*`, `switch -> toggle`, `row -> entry`. Modifiers (`--x`, `__x`) are kept. Classes starting `unity-` are never touched. No dynamic class-name concatenation exists in the C# today (the grep for `"x-" + ...` only found log strings), so exact-token replacement is enough; re-grep for `$"` interpolations before running.
- UXML `<ui:Template name="AdPlaceholder">` / `template="AdPlaceholder"` / `LevelMap` / `BottomNav` are free strings and change together with the file they point to.
- The `project://database/Assets/...?guid=...` URLs carry a GUID, which Unity resolves first, but the path part must be rewritten too (rule 5 of the brief). Sprite-sheet fragments like `#HomeIcon_0`, `#IconStar_0`, `#LockIcon_0`, `#SettingsIcon_0` are **sub-sprite names stored inside the `.meta`**; they do not change when the file moves. Renaming them means editing the `name:` entries in those four `.meta` files and the URL fragments together; optional and listed as Phase B.

## 3. Persisted keys and one-time migration

The names that actually shipped (verified in `ad4a449` and `d4ac06b`) differ from the `arrowqube_*` names now in the working tree, which were added after the last commit. Both have to be covered.

| Where | New | Migrate from (first found wins) |
|---|---|---|
| progress file | `lanterndragons_progress.json` | `arrowqube_progress.json`, `progress.json` |
| ad-config cache file | `lanterndragons_ad_config.json` | `arrowqube_ad_config.json`, `ad_config.json` (cache only; dropping it is also fine) |
| PlayerPrefs int | `lanterndragons.hint.remaining` | `arrowqube.hint.remaining`, `HintManager_RemainingHints` |
| PlayerPrefs int | `lanterndragons.hint.lastFreeCount` | `arrowqube.hint.lastFreeCount`, `HintManager_LastFreeHintCount` |
| PlayerPrefs int | `lanterndragons.settings.sound` | `arrowqube.settings.sound`, `settings.sound` |
| PlayerPrefs int | `lanterndragons.settings.vibration` | `arrowqube.settings.vibration`, `settings.vibration` |
| PlayerPrefs int | `lanterndragons.settings.nativeRefreshRate` | `arrowqube.settings.nativeRefreshRate`, `settings.nativeRefreshRate` |
| legacy delete | none | keep the existing `DeleteKey("HintManager_Initialized")` |

Migration rules: **copy, never move** (the old value stays so a rollback keeps working); run once before the first read (in `ProgressStore.Load` for files, in `ClueManager`/`OptionsManager` `Awake` for prefs); only when the new key is absent (`!File.Exists(new)` / `!PlayerPrefs.HasKey(new)`); first match in the table order wins; log one line. Add tests to `ProgressMigrationTests` (new key present -> untouched; only `arrowqube_*` present -> copied; only the shipped name present -> copied; both -> new wins). `EvidenceCapture` (-> `ShotCapture`) writes `arrowqube_progress.json` at `ShotCapture.cs:259`; point it at the new constant.

## 4. Things that must NOT be renamed

See `doNotRename` in the JSON. The ones that bite: GoogleMobileAds, PrimeTween, Demigiant/DOTween, TextMesh Pro, MultiTech Studio, `Packages/*`; Unity message methods (`Awake`, `OnEnable`...); the host class string `in.dicodeinfotech.arrowqubepuzzle.UnityAdBridge` and the package id / `companyName` (the developer decides these, Story 007); AudioMixer parameter/group names; serialized field names; JsonUtility field names; enum order and `Dirs` order.

## 5. Baked levels: will a rename change the bytes?

**No.** `Chunk_00/01.bytes` are produced by `StageCampaignGenerator` from numeric constants only: `CampaignSalt 0x4C414E54`, `SeedStride 130363`, `Random.InitState(baseSeed + attempt)`, the grid/shape data and the layout in `StagePack`. No identifier name, type name or file name is written into or hashed into the output. Three things inside that code are format-defining and must keep their values: `Dirs` array order, `Difficulty`/`StageTier` member order (stored as a byte), and the chunk header. Proof to run after the rename: `sha256sum Assets/Project/Resources/Stages/Pack_0*.bytes` equals the hash of the current `Chunk_0*.bytes` (take the hashes first), and `ShippedCampaignTests` (re-solves all 1,500 levels) still passes. The `.meta` of the moved chunk files carries the same GUID.
One fingerprint remains: `StagePack.Magic = 0x434C5041` spells `APLC` ("Arrow Puzzle Level Chunk"). Changing it (suggested `0x434C444C`, "LDLC") changes the bytes, so it needs `Version` 3, a re-bake and test updates; treat it as optional Phase B.

## 6. Execution procedure (live Editor, Unity 6000.6.3f1)

Everything runs through the Pipeline CLI. Nothing here has been run.

**Phase 0 - safety (needs the developer's go-ahead for any git write).** The tree is already snapshotted in `0063ab5`. Recommended before starting: `git tag pre-rename 0063ab5` (a tag is local and reversible) and a clean `git status`. Record baselines: `sha256sum` of both chunk files, the EditMode test result (all green), and the current 10 frames in `production/qa/evidence/story-008/` as the visual reference. Close Play mode. Make sure no other agent is using the Editor.

**Phase 1 - stop the compiler, move assets.** `eval_file`: `EditorApplication.LockReloadAssemblies()`, `AssetDatabase.StartAssetEditing()`, create the `_Retired` folders with `AssetDatabase.CreateFolder`, then `AssetDatabase.MoveAsset(old, new)` for every row of `assetMoves` and `csharpTypes` (files first, folders last, check each returns an empty string; stop on the first error message). Then `StopAssetEditing()`. The reload lock stays on. Check afterwards that every moved file has its `.meta` beside it with the old GUID (compare against a GUID list taken in Phase 0).

**Phase 2 - text edits, in dependency order, on tracked files only** (a small Python tool driven by `rename-map.json`, skipping the `doNotRename` paths; dry-run prints a diff count first): (1) nested and top-level type names, whole-word, longest first; (2) the identifier token rules; (3) USS/UXML/C# exact-token class and element-name literals; (4) path and string constants (`Resources.Load` names, `StagePack.ResourceFolder/ChunkPrefix`, `StagePackWriter.OutputFolder`, menu paths, `Template name`, `project://database/...` path parts, `url("../Sprites/...")`); (5) persisted-key constants plus the migration helpers; (6) scene/prefab YAML needs **no** text edit (script GUIDs only) except GameObject `m_Name` values, which are optional; (7) docs (`CLAUDE.md`, `design/`, `production/`, 21 `.md` files).

**Phase 3 - one recompile.** `AssetDatabase.Refresh(ForceUpdate)`, then `EditorApplication.UnlockReloadAssemblies()`; wait with `recompile_status`; the console must show 0 errors. Leftover check: `grep -rniE "ArrowController|BackdropArrows|GridBuilder|arrowqube|NeonGradient|oct-|neon-"` should only hit `_Retired/`, third-party folders, the migration constants, and the docs that describe the old game.

**Phase 4 - Editor-side verification.** Run all EditMode tests including `Baked`; hash the chunks and compare with Phase 0; scan `Game.unity` and the three prefabs for missing scripts and for null serialized references that were non-null before (compare the `serializedFieldsPerScript` list through a small `eval` over the scene); confirm `EditorBuildSettings` still lists the renamed scene (if not, fix the path in `ProjectSettings/EditorBuildSettings.asset`); confirm no USS/UXML import warnings.

**Phase 5 - player verification.** Rebuild the Windows dev player with the detached build job, run it with `--evidence`, view the 10 frames and compare with the Phase 0 frames (same screens, nothing blank, no unstyled UI). Check the migration end to end: start once with a seeded old-format save and old PlayerPrefs, confirm the new files/keys appear and the progress, hints and settings carry over.

**Phase B (optional, separate commit).** `[FormerlySerializedAs]` renames of the 5 serialized fields (then re-save scene/prefab so the YAML key changes); sub-sprite names in four `.meta` files; GameObject names in the scene; `StagePack.Magic` + `Version 3` + re-bake; log tag renames. Retired-file deletion only on explicit approval.

## 7. Rollback

Phase 0 gives a point to return to (`0063ab5`, tagged `pre-rename`). If a phase fails before the Phase 3 recompile: unlock assemblies, then restore the tree from the snapshot (for example `git restore --source=0063ab5 --worktree --staged .` plus removal of the new `_Retired` folders). If it fails after: the same restore, then `AssetDatabase.Refresh`. Both are history-preserving but they discard uncommitted rename work, so they need the developer's go-ahead like every git write. Migration copies instead of moving, so an app built before the rename still finds its original save and prefs.

## 8. Expected size and top risks

Touched: 63 script files moved + about 75 asset moves + 105 text files edited, in one atomic pass, one recompile.

1. **Missing-script / lost values** if any `.meta` is moved outside the Editor or a serialized field is renamed. Mitigation: `MoveAsset` only, serialized names frozen, Phase 4 scan.
2. **Silent UI breakage** from one-sided string renames (element names, Resources paths, `Template name`, sprite `url()` paths). Mitigation: generated map, leftover grep, player screenshot comparison.
3. **Player data loss** if the migration is wrong. Mitigation: copy-not-move, tests for all four cases, seeded end-to-end run.
4. **Compile cascade** if text edits start before the reload lock is in place. Mitigation: Phase 1 lock, one recompile at the end.
5. **Residual fingerprints that renaming does not remove**: identical level-generation logic and numeric constants, the format layout, the `APLC` magic, the AdMob/host-bridge package id and backend endpoints (Story 007), and `Oxanium` fonts. Names are cosmetic to anything that compares behaviour or binaries; say so plainly if the goal is store-level similarity.
6. **Concurrent agents**: the rename touches almost every file, so no other fork may edit during Phases 1-3.
