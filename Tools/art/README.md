# Generated art

Every PNG these scripts write is checked in, so nothing here has to run for the project to build.
They exist so the art stays *tunable*: each of these looks is decided by a handful of numbers, and
those numbers are unrecoverable from a flattened PNG.

Outside `Assets/` on purpose — Unity would otherwise import the `.py` files as text assets.

```sh
pip install pillow
python3 Tools/art/make_ui_frames.py      # then let Unity reimport
```

| Script | Writes to `Assets/Project/Sprites/` | Knobs worth turning |
|---|---|---|
| `make_backdrop_arrows.py` | `BackdropArrow{Straight,Bend,Zigzag}` | `SHAPES` (the polylines themselves) |
| `make_ui_frames.py` | `UIOctagon*`, `UIHexPill*`, `UIBar*` | chamfer size, stroke width, glow radius |

## Why the UI frames are sprites at all

Two things the HUD design leans on have no USS equivalent: **chamfered corners** (USS has
`border-radius`, but no `clip-path` or polygon) and **outer glow** (`text-shadow` exists,
`box-shadow` does not). Both have to be baked.

They are drawn **white** so USS can recolour them with `-unity-background-image-tint-color` — one
frame file serves the cyan, magenta and purple variants. Fill and frame are separate sprites on
separate elements because they need opposite tints (a dark interior under a bright edge) and the
tint property applies to a whole image.

## Import settings

Unity keeps these in the `.meta` files, so they survive a regenerate. Worth knowing if a sprite is
ever added by hand:

- **All**: no mip maps, **no compression** — every one of these is a smooth gradient, and block
  compression bands them visibly.
- `UI*` frames: **Mesh Type: Full Rect**, and the `-unity-slice-*` values in `GameplayScreen.uss`
  assume the canvas sizes this script writes. Change one and check the other.
- `BackdropArrow*`: **Mesh Type: Tight** — the opposite of the UI frames, because these are large
  and mostly transparent, and a tight mesh cuts the wasted overdraw. Measured coverage of their
  256x256 square is only 17-28%.
