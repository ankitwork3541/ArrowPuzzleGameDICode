# Game config JSON (the only format; everything comes from the API)

`game-config.sample.json` is the single schema the game reads from the config API (`ConfigUrl`). There is no
legacy format: the old flat keys (`ad_enable`, `main_screen`, `banner_adunit_id`, ...) are gone. **Nothing
that can change after release is hard-coded in the app**: links, privacy/terms, ads, game tuning and UI
switches all come from this JSON. The app only ships safe defaults for when no payload (and no cached copy)
exists: ads off, links hidden, current game tuning.

## Top level

| Key | Meaning |
|---|---|
| `code` | Must be `1`, otherwise the payload is ignored. |
| `msg` | Free text, unused. |
| `config_version` | Schema version. A payload with a version the app does not know is ignored. |

## `app`

`name` (display name), `min_app_version`, `force_update` + `update_url` (blocking update prompt),
`maintenance_mode` + `maintenance_text` (blocking notice instead of the game).

## `links`

`rate_us`, `privacy`, `terms`, `support_email`, `more_games`, `share_text`, `share_url`. A row in Options that
needs a link is hidden when its link is empty. The Privacy and Terms rows open exactly what is here.

## `ads`

| Key | Meaning |
|---|---|
| `enabled` | Master switch. `false` turns every ad off. |
| `test_mode` | `true` = Google test units regardless of `units`. |
| `units.*` | AdMob unit ids (`banner`, `banner_large`, `native`, `native_preload`, `interstitial`, `rewarded`). Must start with `ca-app-pub-` and contain `/`. |
| `formats.<name>.enabled` | Turn one format off (`banner`, `native`, `interstitial`, `rewarded`). `formats.native` also has `refresh_sec` and `max_age_min`. |
| `placements.<screen>.type` | `none`, `banner`, `banner_large`, `native_small`, `native_big`. Screens: `splash`, `home`, `level_map`, `play`, `win_sheet`, `lose_sheet`, `options`. The result sheets only take native types. |
| `placements.<screen>.show_after_level` | Do not show that placement until this many levels are cleared. |
| `interstitial_rules` | `min_click_count`, `min_seconds_between`, `every_n_levels_on_next`, `on_back_to_home`, `skip_first_levels`, `max_per_session`. `0` on a count or time = that gate is off. |
| `rewarded_rules.<use>.enabled` | Which features may use a rewarded ad: `hint`, `extra_life`, `skip_level`. |
| `test_device_ids` | AdMob test device ids. |

## `game`

`lives.max` (1 to 5, applied at level start; the HUD pill grows to one heart per life),
`hints.free_count` (a **changed** value resets everyone's remaining hints, unless
`hints.reset_on_change` is false), `locks.intro_level` (first level with a padlock; applies to levels baked
without it only if the generator is data-driven, otherwise informational), `difficulty_offset`.

## `ui`

`options_rows.*` (show or hide each Options row), `reduce_motion_default`, `confetti`, `level_map`, `lock_tutorial`
(the one-time coach overlay on the first padlock level; `false` = it never shows).
(`nav_unlock_levels` was removed on 2026-10-20: the bottom dock is not configurable from the API.)

## `message`

Optional notice (`banner_text`, `banner_url`) shown on Home.

The unit ids and links in the sample are placeholders: Google's official test units and `example.com`.
