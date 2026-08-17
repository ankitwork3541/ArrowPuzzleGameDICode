# Level shape sources

Silhouette images that `LevelShapeMask` turns into carve masks for shaped levels.

Deliberately OUTSIDE `Assets/`: these are read as raw file bytes at bake time, so Unity never
imports them — no `.meta` files, no texture import cost, and no way for them to reach a build.

## What works

- PNG or JPG.
- Either a solid shape on a transparent background, or a dark shape on a light one. The importer
  detects which by looking for any non-opaque pixel.
- Source resolution barely matters; the image is cropped to its ink and resampled to the level's
  cell grid. A few hundred pixels across is plenty.
- Simple, chunky silhouettes read best. At ~30 cells across, thin details (whiskers, wires, text)
  collapse to nothing or to one-cell noise, which the cleanup pass then deletes.

## Checking one before baking

`ArrowPuzzle > Campaign > Preview Shape Masks` prints each mask as text plus its cell count, fill
ratio and region count. Aim for a single region and a fill around 0.5-0.6, which is where the
reference game's own shaped levels sit.
