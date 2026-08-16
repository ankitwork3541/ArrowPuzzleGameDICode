from PIL import Image, ImageDraw, ImageFilter

# Frame art for the HUD. Two things the reference design leans on cannot be expressed in USS at
# all — chamfered (octagonal) corners, because USS has border-radius but no clip-path or polygon,
# and an outer glow, because USS has text-shadow but no box-shadow. Both have to be baked here.
#
# Everything is drawn WHITE so USS can recolour it with -unity-background-image-tint-color: one
# frame sprite serves the cyan, magenta and purple variants instead of one file per colour.
#
# Fill and frame are separate sprites rather than one, because they need different tints — a dark
# translucent interior under a bright neon edge — and tinting multiplies the whole image.

SS = 4                       # supersample: drawn at 4x, downsampled for antialiasing
DEST = "/Users/shiftxinfotech/ShiftXWorkSpace/Unity/ArrowPuzzleGame/Assets/Project/Sprites/"


def octagon(x0, y0, x1, y1, c):
    return [(x0 + c, y0), (x1 - c, y0), (x1, y0 + c), (x1, y1 - c),
            (x1 - c, y1), (x0 + c, y1), (x0, y1 - c), (x0, y0 + c)]


def hex_pill(x0, y0, x1, y1, p):
    my = (y0 + y1) / 2
    return [(x0 + p, y0), (x1 - p, y0), (x1, my),
            (x1 - p, y1), (x0 + p, y1), (x0, my)]


def render(size, points, stroke_w, glow_r, glow_a, filled):
    """One sprite. `points` is in final-pixel space; everything is scaled up internally."""
    w, h = size
    img = Image.new("L", (w * SS, h * SS), 0)
    d = ImageDraw.Draw(img)
    pts = [(x * SS, y * SS) for x, y in points]

    if filled:
        d.polygon(pts, fill=255)
        return Image.merge("RGBA", (Image.new("L", img.size, 255),) * 3 + (img,)) \
                    .resize((w, h), Image.LANCZOS)

    # Outline only. joint="curve" keeps the eight corners from growing spikes at this stroke width.
    d.line(pts + [pts[0]], fill=255, width=stroke_w * SS, joint="curve")

    # The glow is the same stroke, blurred and dimmed, sitting UNDER it. Drawing it as a separate
    # blurred layer rather than a wider soft stroke is what gives the falloff its exponential
    # shape — a linear ramp reads as a fat blurry border, not as light.
    glow = img.filter(ImageFilter.GaussianBlur(glow_r * SS))
    glow = glow.point(lambda v: int(v * glow_a))

    alpha = Image.new("L", img.size, 0)
    alpha.paste(glow)
    alpha.paste(img, mask=img)          # stroke punched back in at full strength over its glow

    return Image.merge("RGBA", (Image.new("L", img.size, 255),) * 3 + (alpha,)) \
                .resize((w, h), Image.LANCZOS)


# --- Octagon: LEVEL badge, back button, the three bottom action buttons -----------------------
# 128px canvas with a 16px margin all round for the glow to live in, and a 24px corner chamfer.
# Both fit inside the 52px 9-slice border set in USS, which is what lets one sprite stretch to
# every size from a 110px square button to a wide badge without the corners deforming.
OCT = octagon(16, 16, 112, 112, 24)
render((128, 128), OCT, 3, 5, 0.75, False).save(DEST + "UIOctagonFrame.png")
render((128, 128), OCT, 0, 0, 0, True).save(DEST + "UIOctagonFill.png")

# --- Hex pill: the LIVES capsule --------------------------------------------------------------
# Sliced left/right only. The middle section is a straight bar top and bottom, so stretching it
# vertically as well as horizontally stays correct.
HEXP = hex_pill(16, 14, 176, 82, 30)
render((192, 96), HEXP, 3, 5, 0.75, False).save(DEST + "UIHexPillFrame.png")
render((192, 96), HEXP, 0, 0, 0, True).save(DEST + "UIHexPillFill.png")

def render_bar(size, box, radius, glow_r, glow_a):
    """A rounded capsule, optionally with an outer glow. Used for the progress bar."""
    w, h = size
    img = Image.new("L", (w * SS, h * SS), 0)
    d = ImageDraw.Draw(img)
    x0, y0, x1, y1 = [v * SS for v in box]
    d.rounded_rectangle([x0, y0, x1, y1], radius=radius * SS, fill=255)

    alpha = img
    if glow_r > 0:
        glow = img.filter(ImageFilter.GaussianBlur(glow_r * SS)).point(lambda v: int(v * glow_a))
        alpha = Image.new("L", img.size, 0)
        alpha.paste(glow)
        alpha.paste(img, mask=img)

    return Image.merge("RGBA", (Image.new("L", img.size, 255),) * 3 + (alpha,)) \
                .resize((w, h), Image.LANCZOS)


# --- Progress bar -----------------------------------------------------------------------------
# Sliced left/right only, so the fill can be animated to any width while its two rounded ends keep
# their shape. The 12px margin round the capsule is where the glow lives, which is also why the
# track must NOT set overflow:hidden — that would clip the fill's glow off at both ends.
BAR = (12, 12, 84, 36)
render_bar((96, 48), BAR, 12, 6, 0.85).save(DEST + "UIBarFill.png")
render_bar((96, 48), BAR, 12, 0, 0).save(DEST + "UIBarTrack.png")

for n in ("UIOctagonFrame", "UIOctagonFill", "UIHexPillFrame", "UIHexPillFill",
          "UIBarFill", "UIBarTrack"):
    im = Image.open(DEST + n + ".png")
    print(f"{n:18s} {im.size}  alpha {im.split()[-1].getextrema()}")
