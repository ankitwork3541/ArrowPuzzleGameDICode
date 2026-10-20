# Level entrance: two new styles to compare (2026-10-20)

Style is `ui.entrance_style` in the config (`ink` default, `fly`, `classic` = the old look, for comparing only).
Frames come from a Windows development player (`--evidence-entrance all`), at 720x1280, slowed to 8% speed and
captured at roughly 10 / 35 / 65 / 100 percent of the entrance. Contact sheets in this folder:

| Sheet | What it shows |
|---|---|
| `ink-level6.png`, `ink-big.png` | Ink style on level 6 (17 dragons) and on the biggest baked level, 216 (198 dragons) |
| `fly-level6.png`, `fly-big.png` | Fly style, same two levels |
| `classic-level6.png`, `classic-big.png` | The old look as a reference |

Single frames are `<style>-<level>-<percent>.png` beside them.

## What each style does

- **ink**: an ink-brush stroke draws each dragon tail to head. The tip is dark and wet and the body behind it dries to
  the dragon's colour; the head pops (1.0 to 1.15 to 1.0) when the stroke ends; the padlock appears once the brush
  has passed it. The board unfolds first: faint lattice dots flash centre-out, a little ahead of the strokes, and
  dragons start in a centre-out ripple with overlap.
- **fly**: each dragon slides in from just outside the board along its own heading (the way it exits), nearest entry
  edge first, with ease-out and a 0.1-cell overshoot settle. The padlock pops in after its dragon has landed.
- **classic**: the first look, a plain pen stroke, centre-out, waiting dragons show as dots until their turn.

All three: a board tap during the entrance skips to the finished board (announced once), Reduce Motion shows the
finished board at once, no new sounds (the existing entrance sound hook is untouched).

## Numbers (Windows dev player, about 6 ms per frame at idle)

| Style | Level 6 (17 dragons) | Biggest, 198 dragons |
|---|---|---|
| ink | 0.90 s, worst frame 10.1 ms | 1.17 s, worst frame **32.5 ms** (first frame only) |
| fly | 0.90 s, worst frame 6.1 ms | 1.19 s, worst frame 11.2 ms |
| classic | 0.90 s, worst frame 6.2 ms | 0.90 s, worst frame 10.8 ms |

The one hitch is `ink` on a big board: starting it borrows about 1,700 lattice dots from the pool, and the first
time a board that large loads the pool has to create the missing ones (`LoadLevel` took 30 ms against 10 ms for
the other styles). Later loads reuse them. Not a crash or a stall, but it is the one spot to watch on a low-end phone.

## Checks (all PASS in the player log)

Outline and shadow stay on the body mid-entrance (0 of 17 dragons off, both styles); a skip finishes the board
at once and is announced once; Reduce Motion shows the finished board with no animation; after a full run every
dragon is playable. 270 of 270 EditMode tests pass (41 new in `EntranceMathTests`: ordering, timing budget, curves,
fly start distance, overshoot limit, skip state, config parse and fallback).

## Pros and cons

- **ink**: reads cleanly even on a 198-dragon board and the wet head is a nice touch; but it is still "dragons
  drawing themselves in", so it is the closer cousin of the old look (the old one is also a tail-to-head stroke).
- **fly**: clearly different from the old game and playful; on the biggest boards the dragons cross each other on the
  way in and the middle frames look busy for about half a second.
- **classic**: kept only as a reference; waiting dragons clutter the board as dots and padlocks float early.

## Recommendation

`fly` for small and medium boards, since the developer asked for something unlike the old game; `ink` is the safer
choice if the busy middle of a big board bothers you. A sensible product setting is `fly` by default and `ink` above
about 120 dragons (not built: say if you want it). The default in the build is still `ink`; switching is one config
value.

## Not verified

How either one feels on a real phone (speed, whether 0.9 s is too long for every level start), the first-frame hitch
on a low-end Android device, and the camera's own zoom-in intro on big boards (unchanged, 2 s, so on those boards it
is still moving after the entrance has finished). The cosmetic tuning left as notes: head pop size, ink tint,
overshoot amount.
