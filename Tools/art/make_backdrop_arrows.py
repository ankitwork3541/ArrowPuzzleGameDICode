import math
from PIL import Image, ImageDraw

# Drawn at 4x and downsampled — PIL has no antialiased polygon fill, and a hard-edged silhouette
# would crawl badly once the scatter script rotates it to an arbitrary angle.
OUT = 256
S = OUT * 4
k = S / 1024.0        # everything below is authored in a 1024-unit design space

SHAFT = 150
HEAD_HALF = 165
HEAD_LEN = 255
SOFTEN = 34           # stroke width used to round the head's corners

DEST = "/Users/shiftxinfotech/ShiftXWorkSpace/Unity/ArrowPuzzleGame/Assets/Project/Sprites/"

# Silhouettes mirroring the shapes the game's own arrows take: a straight run, a single bend, and
# a double bend. Same family as the playfield, but filled rather than outlined — the playable
# arrows are LineRenderer strokes, and the two reads have to stay separable at a glance.
SHAPES = {
    "BackdropArrowStraight": [(512, 880), (512, 430)],
    "BackdropArrowBend":     [(200, 830), (620, 830), (620, 430)],
    "BackdropArrowZigzag":   [(200, 880), (200, 620), (720, 620), (720, 420)],
}


def draw(points):
    img = Image.new("RGBA", (S, S), (255, 255, 255, 0))
    d = ImageDraw.Draw(img)
    white = (255, 255, 255, 255)

    pts = [(x * k, y * k) for x, y in points]
    w = int(SHAFT * k)

    d.line(pts, fill=white, width=w, joint="curve")

    # joint="curve" rounds the bends but leaves both ends square, so cap the tail by hand.
    tx, ty = pts[0]
    r = w // 2
    d.ellipse([tx - r, ty - r, tx + r, ty + r], fill=white)

    # Head: a triangle standing on the end of the shaft, aimed along its last segment.
    (px, py), (qx, qy) = pts[-2], pts[-1]
    length = math.hypot(qx - px, qy - py)
    dx, dy = (qx - px) / length, (qy - py) / length
    nx, ny = -dy, dx                                  # perpendicular

    hh, hl = HEAD_HALF * k, HEAD_LEN * k
    head = [(qx + nx * hh, qy + ny * hh),
            (qx - nx * hh, qy - ny * hh),
            (qx + dx * hl, qy + dy * hl)]
    d.polygon(head, fill=white)

    # Soften the head's three points. The design system asks for slightly rounded terminals rather
    # than sharp corners, and a needle tip is the first thing to break up under downsampling.
    d.line(head + [head[0]], fill=white, width=int(SOFTEN * k), joint="curve")
    for hx, hy in head:
        rr = SOFTEN * k / 2
        d.ellipse([hx - rr, hy - rr, hx + rr, hy + rr], fill=white)

    return img


def centre(img):
    # The scatter script spins each copy to a random angle, and Unity rotates a sprite about its
    # texture's middle — an off-centre shape would orbit that point rather than turn on the spot,
    # which reads as a wobble once the copies start drifting.
    shape = img.crop(img.split()[-1].getbbox())

    fit = S * 0.90
    scale = min(fit / shape.width, fit / shape.height)
    shape = shape.resize((max(1, round(shape.width * scale)),
                          max(1, round(shape.height * scale))), Image.LANCZOS)

    out = Image.new("RGBA", (S, S), (255, 255, 255, 0))
    out.paste(shape, ((S - shape.width) // 2, (S - shape.height) // 2))
    return out.resize((OUT, OUT), Image.LANCZOS)


for name, pts in SHAPES.items():
    img = centre(draw(pts))
    img.save(DEST + name + ".png")

    alpha = img.split()[-1]
    covered = sum(1 for v in alpha.getdata() if v > 8) / (OUT * OUT)
    print(f"{name:26s} alpha {alpha.getextrema()}  coverage {covered:.0%}")
