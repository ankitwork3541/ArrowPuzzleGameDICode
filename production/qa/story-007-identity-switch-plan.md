# Story 007: identity switch plan

> **Story**: `production/epics/lantern-dragons/story-007-new-identity-and-kept-systems.md`
> **Updated**: 2026-10-20 after the developer's answers. Evidence: `production/qa/evidence/story-007-evidence.md`.

## Decisions

- Package id and iOS bundle id: `in.dicodeinfotech.arrowdragonpuzzle`.
- No host/launcher app: the game is standalone.
- Ad unit ids and game config come from an HTTP API (URL not supplied yet).
- Level-complete analytics: removed.
- New Play listing: version code restarts at 1.

## Rows (names are the current, renamed ones)

| # | Item | State |
|---|---|---|
| 1 | `applicationIdentifier` Android / Standalone / iPhone | APPLIED: `in.dicodeinfotech.arrowdragonpuzzle`, set through `PlayerSettings` and read back |
| 2 | `companyName: dicodeinfotech` | Kept |
| 3 | `productName: Lantern Dragons` | Done (Story 006) |
| 4 | `bundleVersion 1.0`, `AndroidBundleVersionCode 1`, iOS build number `1` | APPLIED (new listing) |
| 5 | `ProjectSettings/AndroidResolverDependencies.xml` `bundleId` | APPLIED. Re-run Android Resolver (Force Resolve) before the first Android build |
| 6 | `HostBridge` (`UnityAdBridge` JNI call, `game_config_json` Intent read) | REMOVED, file deleted, plus the four `OnAdPaid` forwards in the banner, interstitial, native and rewarded units |
| 7 | `rateUsUrl` in `LanternDragons.unity` (on `OptionsScreenController`) | APPLIED: Play Store URL with the new id |
| 8 | AdMob app ids in `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` | OPEN, needs the developer's real AdMob app ids (still Google test ids) |
| 9 | `Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib/AndroidManifest.xml` `APPLICATION_ID` | OPEN, must equal the Android id from row 8 |
| 10 | `AdTestIds.cs` | Left as is (test fallbacks) |
| 11 | Ad unit ids and flags | Now from the API through `PromoConfigFetcher.Fetch` (`const string ConfigUrl`, EMPTY, TODO). Empty URL: no request, cached copy, else baked `PromoConfig` |
| 12 | Level-complete endpoint | REMOVED: `StageTracker` deleted, call in `FlowDirector.ShowWin` removed, `ShotCapture` ping checks removed |
| 13 | Save and prefs legacy keys | Kept as one-time copy sources (`KeyMigration`); a new package id gives a fresh data folder so a clean install never has them |
| 14 | Keystore, Play Console listing, store icon and feature graphic | OPEN, developer |
| 15 | `Docs/NativeAds.md` mentions the previous game's backend (`newvids.xyz/.../strtdata`) | OPEN, rewrite once the new API is known |

## What is left for the developer

1. The config API URL (and confirmation that it returns the existing `PromoRemoteConfig` JSON: `code == 1` plus the ad unit ids and flags). Put it in `PromoConfigFetcher.ConfigUrl`.
2. Real AdMob app ids (Android, iOS) for rows 8 and 9.
3. Keystore and Play Console listing.
4. An Android device pass: all four ad formats, the full loop, a 10-minute session.
