# Ads and the game config

Everything about ads (and the rest of the game's tunable settings) comes from one JSON document served by
the config API. The schema is `design/config/game-config.sample.json`, explained key by key in
`design/config/README.md`. Nothing that can change after release is hard-coded in the app.

## 1. Where the config comes from

- `PromoConfigFetcher` does one GET per app start with a 5 second timeout. The URL is the single constant
  `ConfigUrl` in that file. **It is empty until the developer supplies the API URL**; while it is empty no
  request is made.
- A valid payload is applied through `GameConfigService` and written to `lanterndragons_config_cache.json`
  in the persistent data path. At the next start the cached copy is applied first, so an offline session
  still runs on real settings.
- With no payload and no cache the game runs on `GameConfig.Defaults()`: **ads off, no links, lives 3,
  hints 3**. The game plays normally, with no gaps where ads would be.
- A payload is ignored (and the previous config kept) when it is not JSON, `code` is not `1`, or
  `config_version` is not `2`.

## 2. Ad placements

Each screen has its own entry under `ads.placements`:

| Key | Screen | Notes |
|---|---|---|
| `home` | Home | Anchored at the bottom, below the dock. |
| `options` | Settings | Its own placement; Home to Settings keeps the same banner instead of rebuilding it. |
| `play` | Gameplay | Anchored at the bottom under the action dock. |
| `win_sheet` | Win sheet | Native only. Uses the native ad preloaded at level start. |
| `lose_sheet` | Lose sheet | Native only. Same preload. |
| `splash`, `level_map` | Splash, level map | Parsed, but these screens have no ad slot today, so nothing renders. |

`type` is `none`, `banner`, `banner_large`, `native_small` or `native_big`. A banner type on a result
sheet resolves to none. `show_after_level` keeps a placement off until the player has cleared that many
levels.

Unit ids live in `ads.units`; an id must start with `ca-app-pub-` and contain `/`, otherwise it is
ignored. `banner_large` falls back to `banner`, `native_preload` falls back to `native`.

## 3. Switches, in the order the game checks them

1. `ads.enabled` is the master switch (`ads.test_mode`, or `PromoConfig.useTestIds` in the build, forces
   Google's test units and turns ads on).
2. `ads.formats.<banner|native|interstitial|rewarded>.enabled` turns one format off.
3. The screen's placement `type` and `show_after_level`.
4. A unit id for that format.
5. For a result sheet: an ad preloaded at level start is actually in hand, and is not older than
   `formats.native.max_age_min`.

If any check fails the screen looks exactly as it does with ads off: no ad and no empty space.

## 4. Interstitial rules (`ads.interstitial_rules`)

| Key | Meaning |
|---|---|
| `min_click_count` | Button clicks before a click-based interstitial. `0` turns the count trigger off. |
| `min_seconds_between` | Seconds between click-based interstitials. `0` turns the time trigger off. Either trigger is enough. |
| `every_n_levels_on_next` | Win NEXT shows one every Nth cleared level, with no click or time gate. `0` = off. |
| `on_back_to_home` | The HUD back button counts as a click. |
| `skip_first_levels` | No interstitial until this many levels are cleared. |
| `max_per_session` | Cap on interstitials actually shown since the app started. `0` = no cap. |

A skipped (not shown) interstitial keeps the click counter, so the next eligible click retries. The
counters reset when the app starts and after an interstitial is shown, not on an in-game Restart. Only a
preloaded interstitial is ever shown; the game never loads one on tap.

## 5. Rewarded ads

`ads.formats.rewarded.enabled` is the format switch and `ads.rewarded_rules.hint.enabled` lets the hint
button use it. After the free hints, if either is off, the hint button disables. A hint is unlocked only
after the ad closes with Google's reward callback; a skip, no-fill or load failure does not unlock it.
`extra_life` and `skip_level` are parsed for later and not used yet.

## 6. Game and UI settings from the same JSON

| Key | Effect |
|---|---|
| `game.lives.max` | Lives per level, 1 to 5 (the HUD pill fits five hearts), applied at level start. |
| `game.hints.free_count` | Free hints. A **changed** value resets everyone's pool, unless `game.hints.reset_on_change` is false. |
| `game.locks.intro_level`, `game.difficulty_offset` | Parsed, informational only: the 1,500 levels are baked. |
| `ui.options_rows.*` | Show or hide each Settings row. A link row (rate, privacy, terms) also needs its link. |
| `links.*` | Rate us, privacy, terms, support, share. Opened with `Application.OpenURL`. |
| `ui.confetti`, `ui.reduce_motion_default`, `ui.level_map` | Confetti on the win sheet, the Reduce Motion default (until the player chooses), the LEVELS button on Home. |
| `app.maintenance_mode`, `app.force_update` | A blocking sheet over Home (maintenance wins). |
| `message.banner_text` / `banner_url` | An optional tappable notice on Home. |

## 7. Staying inside AdMob's serving limits

These protections are unchanged and still apply:

1. **Back-off after a failed request**: 30 s, 60 s, 2 min, 4 min, then 5 min, reset on success.
2. **Room first, ad second**: the reserved box opens before the overlay is requested, so an ad never lands
   on top of a button (accidental clicks are invalid traffic).
3. **No live requests on the result sheets**: only the ad preloaded at level start is shown.
4. **One preload per level**, at level start.
5. **Stale ads are never shown**: older than `formats.native.max_age_min` (at least a minute) is dropped.
6. **Test devices**: `PromoConfig.testDeviceIds` plus `ads.test_device_ids` are registered before the SDK
   starts. The game warns when live ads can be served with no test device registered.
7. **Test units in development**: `PromoConfig.useTestIds` (and `ads.test_mode`) serve Google's test units.
   A red error is logged when it is on. **Turn both off before release.**
8. **Counters** (requests, fill, impressions, clicks, revenue) are logged per format.
9. **Ads close when a screen is left**, so one screen's ad never follows the player to the next.

## 8. Refresh

`ads.formats.native.refresh_sec` drives Home and Play native refresh. Result sheets are never refreshed.
Banners are not refreshed from code; that is left to the ad unit's own auto-refresh in AdMob.
`30` is the recommended value.

## 9. Still needed from the developer

- The config API URL for `ConfigUrl`, returning this schema.
- The real AdMob app id (Android and iOS) in `GoogleMobileAdsSettings.asset` and the manifest. The unit ids
  themselves arrive through `ads.units`.
- Real privacy, terms and support links for `links.*`.
