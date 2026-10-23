"""Düşman top görselleri ve "Nasıl Oynanır" çizimi. Kullanım: python3 generate_special_ball_sprites.py [çıktı klasörü]
Top sprite'larında top çapı 256 px'tir (Unity'de PPU 256 => 1 birim, normal toplarla aynı boy).
Parıltı / girdap gibi taşan katmanlar daha büyük tuvale çizilir, merkezleri topla çakışır.
Stil: oyundaki gezegen topları gibi düz renkler, yumuşak bantlar ve sol-alt hilal gölge."""
import math, os, sys
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageChops, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, "Sprites")
ASSETS = os.path.abspath(os.path.join(HERE, "..", ".."))
os.makedirs(OUT, exist_ok=True)

D = 256          # top çapı (px)
SS = 3           # süper örnekleme
OUTLINE = (20, 14, 50, 255)


def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


def canvas(size):
    return Image.new("RGBA", (size * SS, size * SS), (0, 0, 0, 0))


def save(img, name, size):
    img.resize((size, size), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))


def circle_mask(size, diameter, inset=0.0):
    """Tuval ortasında, çapı diameter (çıktı px) olan dairesel maske."""
    W = size * SS
    r = diameter * SS / 2 - inset * SS
    m = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m).ellipse((W / 2 - r, W / 2 - r, W / 2 + r, W / 2 + r), fill=255)
    return m


def paint(img, mask, color):
    layer = Image.new("RGBA", img.size, color) if isinstance(color, tuple) else color
    r, g, b, a = layer.split()
    img.alpha_composite(Image.merge("RGBA", (r, g, b, ImageChops.multiply(a, mask))))


def blur(mask, px):
    return mask.filter(ImageFilter.GaussianBlur(px * SS))


def scaled(mask, factor):
    return mask.point(lambda v: int(v * factor))


def crescent_shade(size, diameter, strength=95):
    """Gezegenlerdeki gibi sol-alt koyu hilal: daire eksi sağ-üste kaydırılmış daire."""
    W = size * SS
    full = circle_mask(size, diameter)
    off = Image.new("L", (W, W), 0)
    r = diameter * SS / 2
    dx, dy = r * 0.32, -r * 0.30
    ImageDraw.Draw(off).ellipse((W / 2 - r * 1.04 + dx, W / 2 - r * 1.04 + dy, W / 2 + r * 1.04 + dx, W / 2 + r * 1.04 + dy), fill=255)
    return scaled(ImageChops.subtract(full, off), strength / 255)


def spiral_mask(size, r0, r1, arms, turns, width0, width1, rot=0.0):
    """Merkezden dışa açılan spiral kollar (çıktı px cinsinden yarıçap/kalınlık)."""
    W = size * SS
    m = Image.new("L", (W, W), 0)
    d = ImageDraw.Draw(m)
    steps = 160
    for a in range(arms):
        base = rot + a * 2 * math.pi / arms
        for i in range(steps):
            t = i / steps
            rr = (r0 + (r1 - r0) * t) * SS
            ang = base + t * turns * 2 * math.pi
            w = (width0 + (width1 - width0) * t) * SS / 2
            x, y = W / 2 + rr * math.cos(ang), W / 2 + rr * math.sin(ang)
            d.ellipse((x - w, y - w, x + w, y + w), fill=255)
    return m


def magma(size, diameter, seed, cells, seam, crust_a, crust_b, seam_core, seam_hot, glow, heat_bleed):
    """Voronoi magma: birbirine değmeyen kabuk plakaları ve aralarında kesintisiz lav damarları.
    Koordinatlar küre izlenimi için kenara doğru sıkıştırılır; damarlar plakaların ortak sınırıdır, kesişmez."""
    W = size * SS
    rng = np.random.default_rng(seed)
    # plakalar: jitter'lı ızgara (düzenli ama doğal dağılım)
    n = cells
    gx, gy = np.meshgrid(np.arange(n), np.arange(n))
    pts = np.stack([(gx + rng.uniform(0.15, 0.85, gx.shape)) / n, (gy + rng.uniform(0.15, 0.85, gy.shape)) / n], -1).reshape(-1, 2) * 2.4 - 1.2
    shade = rng.uniform(0, 1, len(pts))

    ys, xs = np.mgrid[0:W, 0:W].astype(np.float32)
    r = diameter * SS / 2
    px, py = (xs - W / 2) / r, (ys - W / 2) / r
    rr = px * px + py * py
    k = 1 + 0.55 * rr                        # kenarda plakalar küçülür (küre eğriliği)
    qx, qy = px * k, py * k

    d1 = np.full(qx.shape, 1e9, np.float32); d2 = np.full(qx.shape, 1e9, np.float32)
    idx = np.zeros(qx.shape, np.int32)
    for i, (sx, sy) in enumerate(pts):
        d = np.sqrt((qx - sx) ** 2 + (qy - sy) ** 2)
        closer = d < d1
        d2 = np.where(closer, d1, np.minimum(d2, d))
        idx = np.where(closer, i, idx)
        d1 = np.where(closer, d, d1)
    edge = d2 - d1                           # sınıra uzaklık (0 = damar ortası)

    def smooth(e, w):
        t = np.clip(1 - e / w, 0, 1)
        return t * t * (3 - 2 * t)

    cell = shade[idx][..., None]
    ca, cb = np.array(crust_a[:3], np.float32), np.array(crust_b[:3], np.float32)
    rgb = ca + (cb - ca) * cell
    # plaka kenarları ısıyla hafif aydınlanır
    bleed = smooth(edge, seam * heat_bleed)[..., None] * 0.55
    rgb = rgb + (np.array(glow[:3], np.float32) - rgb) * bleed
    # damar: geniş sıcak bant + ince parlak çekirdek
    hot = smooth(edge, seam)[..., None]
    rgb = rgb + (np.array(seam_hot[:3], np.float32) - rgb) * hot
    core = smooth(edge, seam * 0.45)[..., None]
    rgb = rgb + (np.array(seam_core[:3], np.float32) - rgb) * core

    alpha = np.array(circle_mask(size, diameter, inset=0.5), np.float32) / 255
    img = np.dstack([np.clip(rgb, 0, 255), alpha * 255]).astype(np.uint8)
    return Image.fromarray(img, "RGBA")


def soft_glow(size, diameter, color, inner, outer):
    """Topun çevresinde yumuşak parıltı halkası (top kenarında en parlak, dışa doğru söner)."""
    W = size * SS
    ys, xs = np.mgrid[0:W, 0:W].astype(np.float32)
    r = diameter * SS / 2
    d = np.sqrt((xs - W / 2) ** 2 + (ys - W / 2) ** 2) / r      # 1 = top kenarı
    a = np.clip((outer - d) / (outer - 1), 0, 1) ** 1.8          # dışa doğru sönüm
    a = np.where(d < inner, 0, a)                                # topun içi boş (gövde zaten orada)
    a = np.where((d >= inner) & (d < 1), np.clip((d - inner) / (1 - inner), 0, 1), a)
    img = np.zeros((W, W, 4), np.uint8)
    img[..., :3] = color[:3]
    img[..., 3] = (a * color[3]).astype(np.uint8)
    return Image.fromarray(img, "RGBA")


def outline_ring(size, diameter, width=5):
    return ImageChops.subtract(circle_mask(size, diameter), circle_mask(size, diameter, inset=width))


# ------------------------------------------------------------------ ortak gölge (tüm düşman topların üstünde, dönmez)
def ball_shade():
    img = canvas(D)
    paint(img, crescent_shade(D, D, 80), (25, 10, 60, 255))
    paint(img, outline_ring(D, D, 4), (20, 14, 50, 150))
    # sağ üst parlama
    W = D * SS
    hl = Image.new("L", (W, W), 0)
    ImageDraw.Draw(hl).ellipse((W * 0.58, W * 0.16, W * 0.74, W * 0.28), fill=255)
    paint(img, scaled(blur(hl, 3), 0.55), (255, 255, 255, 255))
    save(img, "ball_shade", D)


# ------------------------------------------------------------------ 10. seviye: Karıştırıcı (turkuaz girdap)
def mixer():
    body = canvas(D)
    full = circle_mask(D, D)
    W = D * SS
    grad = Image.new("RGBA", (W, W))
    gd = ImageDraw.Draw(grad)
    for y in range(W):
        t = y / W
        c = tuple(int(a + (b - a) * t) for a, b in zip(hexc('#6fe6e0'), hexc('#2f9fc4')))
        gd.line([(0, y), (W, y)], fill=c)
    paint(body, full, grad)
    save(body, "mixer_body", D)

    swirl = canvas(D)
    arms_dark = spiral_mask(D, 4, D * 0.52, 3, 0.85, 10, 34)
    arms_light = spiral_mask(D, 8, D * 0.5, 3, 0.85, 4, 12, rot=0.55)
    paint(swirl, ImageChops.multiply(arms_dark, full), hexc('#2a7fb0', 230))
    paint(swirl, ImageChops.multiply(arms_light, full), hexc('#d8fbff', 210))
    core = circle_mask(D, D * 0.2)
    paint(swirl, core, hexc('#effeff'))
    paint(swirl, outline_ring(D, D * 0.2, 3), hexc('#2a7fb0'))
    save(swirl, "mixer_swirl", D)


# ------------------------------------------------------------------ 20. seviye: Yanan Top (magma plakaları + parıltı)
def burner():
    body = magma(D, D, seed=7, cells=4, seam=0.075,
                 crust_a=hexc('#5a160f'), crust_b=hexc('#7d2414'),
                 seam_core=hexc('#fff4c2'), seam_hot=hexc('#ffb52e'), glow=hexc('#ff6a1a'), heat_bleed=3.2)
    out = canvas(D)
    out.alpha_composite(body)
    paint(out, outline_ring(D, D, 4), hexc('#ff8a2a', 200))
    save(out, "burner_body", D)

    size = int(D * 1.7)
    aura = canvas(size)
    aura.alpha_composite(soft_glow(size, D, hexc('#ff7a1f', 235), 0.86, 1.62))
    aura.alpha_composite(soft_glow(size, D, hexc('#ffc23a', 200), 0.9, 1.22))
    save(aura, "burner_aura", size)


# ------------------------------------------------------------------ yanmış normal top: kömür plakaları + sönük köz damarları
def burnt_overlay():
    body = magma(D, D, seed=21, cells=3, seam=0.05,
                 crust_a=hexc('#1f1722'), crust_b=hexc('#2f2431'),
                 seam_core=hexc('#ffb347'), seam_hot=hexc('#d9481a'), glow=hexc('#6b2a1c'), heat_bleed=2.5)
    out = canvas(D)
    out.alpha_composite(body)
    paint(out, outline_ring(D, D, 4), hexc('#14101a', 220))
    save(out, "burnt_overlay", D)

    size = int(D * 1.45)
    glow = canvas(size)
    glow.alpha_composite(soft_glow(size, D, hexc('#ff6a1a', 230), 0.9, 1.4))
    save(glow, "burnt_glow", size)


# ------------------------------------------------------------------ 30. seviye: Kara Delik (koyu çekirdek + mor girdap)
def black_hole():
    body = canvas(D)
    full = circle_mask(D, D)
    paint(body, full, hexc('#120a22'))
    rim = ImageChops.subtract(full, circle_mask(D, D * 0.84))
    paint(body, blur(rim, 5), hexc('#a65cff', 230))
    paint(body, outline_ring(D, D * 0.86, 3), hexc('#f0c8ff', 220))
    paint(body, circle_mask(D, D * 0.62), hexc('#05030b'))
    save(body, "blackhole_body", D)

    size = int(D * 1.9)
    swirl = canvas(size)
    arms = spiral_mask(size, D * 0.42, D * 0.95, 4, 0.55, 26, 4)
    paint(swirl, blur(arms, 6), hexc('#8a3cff', 210))
    arms2 = spiral_mask(size, D * 0.45, D * 0.88, 4, 0.55, 9, 2)
    paint(swirl, blur(arms2, 1.5), hexc('#ff9cf0', 230))
    paint(swirl, scaled(blur(circle_mask(size, D * 1.25), 16), 0.7), hexc('#7a2cff'))
    save(swirl, "blackhole_swirl", size)


# ------------------------------------------------------------------ tanıtım penceresi ikonları (katmanlar birleşik)
def compose_icon(name, layers):
    size = 512
    out = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    for file, scale in layers:
        im = Image.open(os.path.join(OUT, file + ".png")).convert("RGBA")
        px = int(round(im.width / D * size * scale))
        im = im.resize((px, px), Image.LANCZOS)
        out.alpha_composite(im, ((size - px) // 2, (size - px) // 2))
    out.save(os.path.join(OUT, name + ".png"))


# ------------------------------------------------------------------ "Nasıl Oynanır" çizimi: 6 aynı renk top + 6+ rozeti
def how_to_illustration():
    w, h = 720, 400
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    planet = Image.open(os.path.join(ASSETS, "UI", "Shared", "Planets", "venus-512.png")).convert("RGBA").crop((50, 50, 462, 462))
    other = Image.open(os.path.join(ASSETS, "UI", "Shared", "Planets", "earth-512.png")).convert("RGBA").crop((50, 50, 462, 462))
    pr = 92
    ball = planet.resize((pr, pr), Image.LANCZOS)
    cx, cy = 300, 215
    step = pr * 0.98
    # sıkı bir altıgen küme: 2 + 3 + 1
    spots = [(-0.5, -0.87), (0.5, -0.87), (-1, 0), (0, 0), (1, 0), (0.5, 0.87)]
    glow = Image.new("L", (w, h), 0)
    gd = ImageDraw.Draw(glow)
    for sx, sy in spots:
        x, y = cx + sx * step, cy + sy * step
        gd.ellipse((x - pr * 0.62, y - pr * 0.62, x + pr * 0.62, y + pr * 0.62), fill=255)
    out.alpha_composite(Image.merge("RGBA", (*Image.new("RGB", (w, h), (255, 210, 63)).split(), glow.filter(ImageFilter.GaussianBlur(14)).point(lambda v: int(v * 0.85)))))
    for sx, sy in spots:
        x, y = cx + sx * step, cy + sy * step
        out.alpha_composite(ball, (int(x - pr / 2), int(y - pr / 2)))
    # yan tarafta farklı renk bir top (eşleşmez)
    out.alpha_composite(other.resize((78, 78), Image.LANCZOS), (int(cx - 2.25 * step), int(cy + 0.55 * step)))
    # patlama yıldızları
    d = ImageDraw.Draw(out)
    for ang in range(0, 360, 30):
        a = math.radians(ang)
        r0, r1 = 150, 176 if ang % 60 else 190
        d.line([(cx + r0 * math.cos(a), cy + r0 * 0.78 * math.sin(a)), (cx + r1 * math.cos(a), cy + r1 * 0.78 * math.sin(a))], fill=(255, 226, 120, 230), width=7)
    # 6+ rozeti
    bx, by, br = 560, 150, 92
    d.ellipse((bx - br, by - br, bx + br, by + br), fill=OUTLINE)
    d.ellipse((bx - br + 8, by - br + 8, bx + br - 8, by + br - 8), fill=hexc('#ffd23f'))
    d.ellipse((bx - br * 0.62, by - br * 0.78, bx + br * 0.62, by - br * 0.18), fill=hexc('#ffe27a'))
    font = ImageFont.truetype(os.path.join(ASSETS, "UI", "Shared", "Fonts", "Baloo2-ExtraBold.ttf"), 112)
    text = "6+"
    tb = d.textbbox((0, 0), text, font=font, stroke_width=10)
    tx = bx - (tb[0] + tb[2]) / 2
    ty = by - (tb[1] + tb[3]) / 2 + 4
    d.text((tx, ty), text, font=font, fill=(255, 255, 255, 255), stroke_width=10, stroke_fill=OUTLINE)
    out.save(os.path.join(OUT, "how_to_play.png"))


ball_shade()
mixer()
burner()
burnt_overlay()
black_hole()
compose_icon("mixer_icon", [("mixer_body", 0.78), ("mixer_swirl", 0.78), ("ball_shade", 0.78)])
compose_icon("burner_icon", [("burner_aura", 0.58), ("burner_body", 0.58), ("ball_shade", 0.58)])
compose_icon("blackhole_icon", [("blackhole_swirl", 0.52), ("blackhole_body", 0.52), ("ball_shade", 0.52)])
how_to_illustration()
print("done")
