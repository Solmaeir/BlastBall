"""Güçlendirici efekt görselleri. Kullanım: python3 generate_powerup_sprites.py [çıktı klasörü]"""
import math, os, random, sys
from PIL import Image, ImageDraw, ImageFilter, ImageChops

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, "Sprites")
os.makedirs(OUT, exist_ok=True)
OUTLINE = (15, 10, 43, 255)
WHITE = (255, 255, 255, 255)


def lerp(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(len(a)))


def clamp01(v):
    return max(0.0, min(1.0, v))


def radial(name, size, fn):
    img = Image.new("RGBA", (size, size))
    px = img.load()
    c = (size - 1) / 2
    for y in range(size):
        for x in range(size):
            r = math.hypot(x - c, y - c) / c
            px[x, y] = fn(r) if r <= 1.0 else (0, 0, 0, 0)
    img.save(os.path.join(OUT, name + ".png"))


# ------------------------------------------------------------ blast
def flash(r):
    if r < 0.25:
        col = (255, 250, 215, 255)
    elif r < 0.6:
        col = lerp((255, 250, 215, 255), (255, 160, 60, 200), (r - 0.25) / 0.35)
    else:
        col = lerp((255, 160, 60, 200), (255, 90, 40, 0), (r - 0.6) / 0.4)
    return col


def ring(r):
    core = math.exp(-((r - 0.86) / 0.035) ** 2)
    halo = math.exp(-((r - 0.86) / 0.10) ** 2)
    color = lerp((255, 140, 50), (255, 246, 205), core)
    alpha = int(255 * min(1.0, core + 0.8 * halo))
    return color + (alpha,)


radial("blast_flash", 256, flash)
radial("blast_ring", 256, ring)


# ------------------------------------------------------------ reticle
def reticle(name, size=256, ss=4):
    S = size * ss
    c = S / 2
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse((c * 0.1, c * 0.1, S - c * 0.1, S - c * 0.1), fill=(255, 255, 255, 34))
    r = c * 0.88
    for i in range(18):
        a0 = i * 20 + 2
        a1 = a0 + 12
        for width, col in ((int(c * 0.095), OUTLINE), (int(c * 0.05), WHITE)):
            d.arc((c - r, c - r, c + r, c + r), a0, a1, fill=col, width=width)
    # cross hair
    for width, col in ((int(c * 0.06), OUTLINE), (int(c * 0.028), WHITE)):
        L = c * 0.09
        d.line([(c - L, c), (c + L, c)], fill=col, width=width)
        d.line([(c, c - L), (c, c + L)], fill=col, width=width)
    img.resize((size, size), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))


reticle("reticle_ring")


# ------------------------------------------------------------ frost edges (9-slice, 512x1024)
def frost(name, w=512, h=1024):
    rng = random.Random(7)
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = img.load()
    depth = 170.0
    for y in range(h):
        for x in range(w):
            e = min(x, w - 1 - x, y, h - 1 - y)
            t = clamp01(e / depth)
            a = (1 - t) ** 1.7 * 0.70
            if a <= 0.004:
                continue
            col = lerp((240, 252, 255), (110, 200, 250), t ** 0.7)
            px[x, y] = col + (int(255 * a),)
    img = img.filter(ImageFilter.GaussianBlur(2))
    layer = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)

    def shard(p0, p1, tip, shade):
        d.polygon([p0, p1, tip], fill=(shade, min(255, shade + 14), 255, 120))
        d.line([p0, tip], fill=(255, 255, 255, 170), width=2)

    def edge(count, length, base_fn, tip_fn, hmin, hmax):
        for _ in range(count):
            s = rng.uniform(0, length)
            wd = rng.uniform(10, 34)
            ht = rng.uniform(hmin, hmax)
            p0, p1 = base_fn(s), base_fn(s + wd)
            tip = tip_fn(s + wd * rng.uniform(0.2, 0.8), ht)
            shard(p0, p1, tip, rng.randint(200, 235))

    edge(13, w, lambda s: (s, 0), lambda s, ht: (s, ht), 28, 90)
    edge(13, w, lambda s: (s, h - 1), lambda s, ht: (s, h - 1 - ht), 28, 90)
    edge(20, h, lambda s: (0, s), lambda s, ht: (ht, s), 22, 70)
    edge(20, h, lambda s: (w - 1, s), lambda s, ht: (w - 1 - ht, s), 22, 70)
    # corner clusters
    for cx, cy, sx, sy in ((0, 0, 1, 1), (w - 1, 0, -1, 1), (0, h - 1, 1, -1), (w - 1, h - 1, -1, -1)):
        for k in range(4):
            ang = math.radians(rng.uniform(8, 82))
            ln = rng.uniform(60, 130)
            base = rng.uniform(14, 30)
            tip = (cx + sx * math.cos(ang) * ln, cy + sy * math.sin(ang) * ln)
            p0 = (cx + sx * rng.uniform(0, 30), cy + sy * rng.uniform(0, 30))
            p1 = (p0[0] + sx * base, p0[1] + sy * 4)
            shard(p0, p1, tip, rng.randint(205, 238))
    for _ in range(70):
        x, y = rng.randint(6, w - 7), rng.randint(6, h - 7)
        if min(x, w - 1 - x, y, h - 1 - y) > depth * 0.8:
            continue
        r = rng.uniform(1.5, 3.2)
        d.ellipse((x - r, y - r, x + r, y + r), fill=(255, 255, 255, 220))
    img.alpha_composite(layer.filter(ImageFilter.GaussianBlur(0.6)))
    img.save(os.path.join(OUT, name + ".png"))


frost("frost_edges")


# ------------------------------------------------------------ shield line (9-slice, 256x64)
def shield_line(name, w=256, h=64):
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = img.load()
    cy = (h - 1) / 2
    for y in range(h):
        v = abs(y - cy)
        core = math.exp(-(v / 4.2) ** 2)
        band = clamp01((15 - v) / 5.0)
        mid = max(math.exp(-(v / 11.0) ** 2), band * 0.75)
        outer = math.exp(-(v / 22.0) ** 2)
        for x in range(w):
            end = min(x, w - 1 - x)
            fade = clamp01(end / 30.0) ** 0.8
            a = min(1.0, core + 0.5 * mid + 0.26 * outer) * fade
            if a <= 0.003:
                continue
            col = lerp((150, 95, 255), (255, 125, 238), clamp01(mid * 1.4))
            col = lerp(col, (255, 240, 255), clamp01(core))
            px[x, y] = col + (int(255 * a),)
    img.save(os.path.join(OUT, name + ".png"))


shield_line("shield_line")


# ------------------------------------------------------------ lock badge
def lock_badge(name, size=112, ss=4):
    S = size * ss
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse((0, 0, S - 1, S - 1), fill=OUTLINE)
    o = 6 * 2 * ss / 2
    grad = Image.new("RGBA", (S, S))
    gd = ImageDraw.Draw(grad)
    for y in range(S):
        gd.line([(0, y), (S, y)], fill=lerp((150, 144, 176, 255), (88, 82, 124, 255), y / S))
    mask = Image.new("L", (S, S), 0)
    ImageDraw.Draw(mask).ellipse((o, o, S - 1 - o, S - 1 - o), fill=255)
    img.paste(grad, (0, 0), mask)
    d = ImageDraw.Draw(img)
    c = S / 2
    s = S / 112
    col = (236, 232, 248, 255)
    d.rounded_rectangle((c - 17 * s, c - 2 * s, c + 17 * s, c + 22 * s), radius=5 * s, fill=col)
    d.arc((c - 11 * s, c - 22 * s, c + 11 * s, c + 8 * s), 180, 360, fill=col, width=int(6 * s))
    d.line([(c - 8 * s, c - 7 * s), (c - 8 * s, c)], fill=col, width=int(6 * s))
    d.line([(c + 8 * s, c - 7 * s), (c + 8 * s, c)], fill=col, width=int(6 * s))
    d.ellipse((c - 3.5 * s, c + 5 * s, c + 3.5 * s, c + 12 * s), fill=(62, 56, 96, 255))
    d.rectangle((c - 1.6 * s, c + 9 * s, c + 1.6 * s, c + 17 * s), fill=(62, 56, 96, 255))
    img.resize((size, size), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))


lock_badge("badge_lock")
print("done")
