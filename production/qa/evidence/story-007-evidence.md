# Test Evidence: New identity and kept-systems check

> **Story**: `production/epics/lantern-dragons/story-007-new-identity-and-kept-systems.md`
> **Story Type**: Integration
> **Date**: 2026-10-20
> **Tester**: Claude (Unity Editor through the Pipeline CLI, plus a Windows development player driven by `ShotCapture`)
> **Build**: working tree on top of `8369eba`, Unity 6000.6.3f1

## Decisions applied (from the developer, 2026-10-20)

- New Android package id and iOS bundle id: `in.dicodeinfotech.arrowdragonpuzzle`. Set through `PlayerSettings.SetApplicationIdentifier` for Android, Standalone and iOS and read back; `AndroidResolverDependencies.xml` and the `rateUsUrl` in `LanternDragons.unity` use the same id.
- No host/launcher app any more: `HostBridge` (the `UnityAdBridge` JNI call, paid-event forwarding, reading `game_config_json` from the launching Intent) was deleted, together with its four call sites in the banner, interstitial, native and rewarded units. `AndroidJavaObject` remains only in `PromoSpace.cs` for display metrics.
- Ad config now comes from an HTTP GET: `PromoConfigFetcher.Fetch` (coroutine, 5 s timeout) uses one `const string ConfigUrl`. It is EMPTY (TODO in the code): with no URL no request is made, the cached copy of the last good config is used, else the baked `PromoConfig` defaults. `PromoManager.Start` is now a coroutine that waits for the fetch before `Initialize()`; with an empty URL it returns on the same frame, so ad start-up is unchanged.
- Level-complete analytics removed entirely: `StageTracker` deleted, its call in `FlowDirector.ShowWin` removed, the test hooks and the ping assertions in `ShotCapture` removed. `FrontierRule` and its tests are kept.
- New Play listing: `AndroidBundleVersionCode = 1`, `bundleVersion` (version name) `1.0`, iOS build number `1`.

## Results

| Check | Result |
|---|---|
| Compile | PASS: `recompile_status` completed, 0 errors |
| EditMode tests | PASS: 101 of 101 |
| Windows development player build | PASS: `Succeeded errors=0` with the new identifier |
| `[EvidenceCheck]` in the player log | PASS: 11 of 11 (reduce motion x2, locked tap, map x3, disk persistence, bump, frontier x2, save restored) |
| Console | No errors. Known: the AdMob `ArgumentNullException` at start-up on Windows (no Google Mobile Ads client there) |

## Acceptance criteria

| # | Criterion | Result | Notes |
|---|---|---|---|
| AC-1 | New Android package id and bundle name set | PASS | `in.dicodeinfotech.arrowdragonpuzzle` on Android, Standalone and iPhone; `productName` is Lantern Dragons. |
| AC-2 | New AdMob app id and ad units configured; fetcher points at the new endpoint | BLOCKED-ON-DEVELOPER | Fetcher now reads one `ConfigUrl` that is empty until the API URL is supplied. `GoogleMobileAdsSettings.asset` and the Android manifest still hold the Google TEST app ids; the real AdMob app id is needed before release. Unit ids arrive through the API payload (or the baked `PromoConfig`). |
| AC-3 | All four ad formats load and show on a real Android device | NOT TESTED | Needs a device. The Windows player has no Google Mobile Ads client. |
| AC-4 | Full loop on device | PARTIAL | Splash, Home, level, win and lose were exercised in the Windows player (frames in `story-008-v4/`); not on a device. |
| AC-5 | Lives and hints with locks | PASS (logic) | `[EvidenceCheck]`: locked tap costs no life, bump costs exactly one life; hint skips locks by code review and tests. Hint pulse and counts not re-run on a device. |
| AC-6 | Save does not read the old game's data and survives a restart | PASS | A new package id gives a fresh data folder. The migration from the old key names only copies when an old key exists in the new folder, which a clean install never has. `[EvidenceCheck]`: progress read back from disk (42), save file restored. |
| AC-7 | Settings screen works | PASS | Options frame in `story-008-v4/` (new Reduce Motion row), switch state persists. |
| AC-8 | No console errors in a 10-minute play session | NOT TESTED | Only the automated run was observed. |

## Remaining hits for the removed identifiers (grep of the repo, excluding Library/Temp/.git)

- `arrowqube` in `ProgressStore.cs`, `ClueManager.cs`, `OptionsManager.cs`, `LegacyKeyMigrationTests.cs`: the legacy key names that the one-time copy reads. Kept on purpose; harmless with the new package id.
- `AndroidJavaObject` in `PromoSpace.cs:184-185`: Android display metrics, not the host.
- `newvids.xyz` in `Docs/NativeAds.md:36`: documentation of the previous game's backend; not code. Update or delete the doc once the new API is known.
- `HostBridge` in `production/session-logs/session-log.md`: history.
- Planning and review documents under `production/` mention the old names as history.
- No hits for `UnityAdBridge`, `game_config_json`, `completelevel` or `StageTracker` in code.

## Still blocked on the developer

1. The config API URL for `PromoConfigFetcher.ConfigUrl` (and its JSON schema confirmed as the existing `PromoRemoteConfig`).
2. The real AdMob app ids (Android and iOS) for `GoogleMobileAdsSettings.asset` and the Android manifest.
3. The signing keystore and the Play Console listing (store icon, feature graphic).
4. A real Android device pass for AC-3, AC-4 and AC-8.

## Sign-Off

| Role | Name | Date | Signature |
|---|---|---|---|
| Lead | | | [ ] Approved |

## Dynamic game config (added 2026-10-20, round 3)

The flat ads-only config (`PromoRemoteConfig`: `ad_enable`, `main_screen`, ...) was replaced by one nested
JSON for the whole game, `design/config/game-config.sample.json` (explained in `design/config/README.md`).
There is no legacy format: `PromoRemoteConfig.cs` was deleted (via `AssetDatabase.DeleteAsset`; it is in git).

**New code**: `GameConfig` (+ `GameConfigParser`), `AdRules`, `GameConfigService` (current config, cache file
`lanterndragons_config_cache.json`, `Changed` event). `PromoSettings` now reads the config; `PromoManager`,
`PromoInterstitialManager`, `ClueManager`, `EmberManager`, `OptionsManager`, `HallScreenController`,
`OptionsScreenController`, `SheetScreenController`, `PlayScreenController` follow it. `PromoConfig.asset` was
re-serialised and now carries only the development switches (it held no ad unit ids).

| Config key | What it now controls |
|---|---|
| `ads.enabled`, `ads.test_mode` | Master switch / Google test units (`PromoConfig.useTestIds` also forces test units) |
| `ads.units.*` | AdMob unit ids; invalid ids (no `ca-app-pub-` prefix or no `/`) are dropped; `banner_large` falls back to `banner`, `native_preload` to `native` |
| `ads.formats.*` | Per-format on/off; `native.refresh_sec`, `native.max_age_min` |
| `ads.placements.<screen>` | `type` + `show_after_level` for home, options, play, win_sheet, lose_sheet (win/lose are native-only) |
| `ads.interstitial_rules` | click count, seconds between, every N levels on NEXT, back button, **skip_first_levels, max_per_session** (new) |
| `ads.rewarded_rules.hint` | Whether the hint button may use a rewarded ad |
| `game.lives.max` | Lives per level (1 to 5), applied at level start; the HUD pill grows to one heart per life |
| `game.hints.free_count`, `reset_on_change` | Free hints; a changed count resets the pool only when `reset_on_change` is true |
| `links.*` | Rate / Privacy / Terms open the config link; a row with an empty link is hidden |
| `ui.options_rows.*`, `ui.confetti`, `ui.level_map`, `ui.reduce_motion_default` | Options rows, win confetti, the LEVELS button, the Reduce Motion default until the player chooses |
| `app.maintenance_mode`, `app.force_update` | A blocking sheet over Home (maintenance wins) |
| `message.banner_text`, `banner_url` | A tappable notice on Home |

**Parsed but not applied** (the 1,500 levels are baked): `game.locks.intro_level`, `game.difficulty_offset`.
`ads.placements.splash` and `level_map` are parsed but those screens have no ad slot. `rewarded_rules.extra_life`
and `skip_level` and `links.support_email / more_games / share_*` are parsed for later.

**The bottom dock** now has two items (Home, Settings); the Lvl 10 / 20 / 30 milestone items, `TabDockBinder`
milestone logic and their USS states were removed. `LockIcon`/`GlyphLatch.png` and `GlyphSeal.png` stay on disk,
unused by the dock.

**Tests**: EditMode 167 of 167 (66 new in `GameConfigTests`: sample parse, every placement type, defaults,
partial sections, unknown version, bad payload, invalid ids, clamps, interstitial rules, `show_after_level`,
link rows, options rows, both directions of `reset_on_change`, service + cache round trip).

**Player run with `--evidence-config <file> --evidence-config-run`** (dev builds only), frames in
`production/qa/evidence/story-007/`:

| Frame | Config | Verdict |
|---|---|---|
| `cfg-a-home.png`, `cfg-a-options.png` | the sample | PASS. Home and Settings show the slim two-item dock (Home, Settings); all Options rows and the three links present |
| `cfg-b-options.png` | vibration, sound, reduce motion rows off; terms link empty | PASS. Only Native Refresh Rate, Rate us and Privacy remain, dividers correct |
| `cfg-b-home.png` | `message.banner_text` set | PASS. Tappable notice under the header |
| `cfg-b-hud.png` | `game.lives.max` 5 | PASS. Five hearts, smaller, clear of the level title (a first capture overlapped the title; fixed with `.embers--many`) |
| `cfg-c-home.png` | `app.maintenance_mode` | PASS. "BACK SOON" sheet, no button, text from the config |
| `cfg-d-home.png` | `app.force_update` | PASS. "UPDATE REQUIRED" with an UPDATE button |

`[EvidenceCheck]`: evidence config loaded PASS, a payload is applied PASS, `game.lives.max` reaches the lives
manager PASS (3/3 and 5/5), save file restored PASS; full run `story-008-v5/`: 11 of 11 PASS (reduce motion x2,
locked tap, map x3, disk persistence, bump, frontier x2, save restored).

With **no** config (first launch, offline) the game runs on `GameConfig.Defaults()`: ads off, and the Rate /
Privacy / Terms rows are hidden because there are no links. That is intended.

**Still blocked on the developer**: the config API URL (`PromoConfigFetcher.ConfigUrl`, empty), the real AdMob
app id (Android and iOS) for `GoogleMobileAdsSettings.asset` and the manifest, real `links.*` URLs, the signing
keystore and Play listing, and an Android device pass for the four ad formats.
