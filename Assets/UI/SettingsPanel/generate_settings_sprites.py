"""Ayarlar paneli görselleri. Kullanım: python3 generate_settings_sprites.py [çıktı klasörü]
Ölçüler 390 px genişlikli telefon tasarım pikselidir (dp); çıktı 3x çözünürlüktür (menü sprite'larıyla aynı)."""
import math, os, sys
from PIL import Image, ImageDraw, ImageFilter, ImageChops

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, "Sprites")
os.makedirs(OUT, exist_ok=True)

S = 3
SS = 4
K = S * SS
OUTLINE = (20, 14, 50, 255)
WHITE = (255, 255, 255, 255)


def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


def lerp(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(len(a)))


def grad_color(stops, t):
    if t <= stops[0][0]:
        return stops[0][1]
    for i in range(1, len(stops)):
        if t <= stops[i][0]:
            t0, c0 = stops[i - 1]
            t1, c1 = stops[i]
            return lerp(c0, c1, (t - t0) / max(1e-6, t1 - t0))
    return stops[-1][1]


def vgrad(w, h, stops, y0=0, y1=None):
    y1 = h if y1 is None else y1
    img = Image.new("RGBA", (w, h))
    d = ImageDraw.Draw(img)
    for y in range(h):
        t = (y - y0) / max(1, (y1 - y0))
        d.line([(0, y), (w, y)], fill=grad_color(stops, min(1, max(0, t))))
    return img


def rr_mask(w, h, box, r):
    m = Image.new("L", (w, h), 0)
    ImageDraw.Draw(m).rounded_rectangle(box, radius=max(0, r), fill=255)
    return m


def ellipse_mask(w, h, box):
    m = Image.new("L", (w, h), 0)
    ImageDraw.Draw(m).ellipse(box, fill=255)
    return m


def fill_mask(canvas, mask, color_or_img):
    layer = Image.new("RGBA", canvas.size, color_or_img) if isinstance(color_or_img, tuple) else color_or_img.copy()
    r, g, b, a = layer.split()
    canvas.alpha_composite(Image.merge("RGBA", (r, g, b, ImageChops.multiply(a, mask))))


def new(w, h):
    return Image.new("RGBA", (int(w * K), int(h * K)), (0, 0, 0, 0))


def finish(img, name, w, h):
    img.resize((int(round(w * S)), int(round(h * S))), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))


# ------------------------------------------------------------------ kart çerçevesi (9-slice)
def card_frame(name, w=120, h=120, r=34):
    img = new(w, h)
    W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), OUTLINE)
    o = 2.5 * K
    rim = vgrad(W, H, [(0, hexc('#c1b0f8')), (0.5, hexc('#9a82ea')), (1, hexc('#6a50cd'))])
    fill_mask(img, rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), (r - 2.5) * K), rim)
    o2 = 12.5 * K
    fill_mask(img, rr_mask(W, H, (o2, o2, W - 1 - o2, H - 1 - o2), (r - 12.5) * K), hexc('#1c1346'))
    o3 = 14.5 * K
    inner = vgrad(W, H, [(0, hexc('#382c80')), (1, hexc('#261c5e'))], int(o3), int(H - o3))
    fill_mask(img, rr_mask(W, H, (o3, o3, W - 1 - o3, H - 1 - o3), (r - 14.5) * K), inner)
    finish(img, name, w, h)


card_frame("card_frame")


# ------------------------------------------------------------------ şeker butonlar (9-slice)
def candy_face(name, w, h, r, stops, hl, ow=2.5, rim=3.5):
    img = new(w, h)
    W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), OUTLINE)
    o = ow * K
    inner = rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), (r - ow) * K)
    fill_mask(img, inner, vgrad(W, H, stops, int(o), int(H - o)))
    shifted = rr_mask(W, H, (o, o + rim * K, W - 1 - o, H - 1 - o + rim * K), (r - ow) * K)
    rim_mask = ImageChops.multiply(ImageChops.subtract(inner, shifted).filter(ImageFilter.GaussianBlur(K * 0.3)), inner)
    fill_mask(img, rim_mask, hl)
    finish(img, name, w, h)


def candy_base(name, w, h, r, color, ow=2.5):
    img = new(w, h)
    W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), OUTLINE)
    o = ow * K
    fill_mask(img, rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), (r - ow) * K), color)
    finish(img, name, w, h)


PALETTE = {
    "blue":   ([(0, hexc('#a8e8fe')), (0.45, hexc('#62cdf6')), (0.88, hexc('#33a9de')), (1, hexc('#238ec7'))], hexc('#e4f8ff'), hexc('#25578d')),
    "purple": ([(0, hexc('#b8a6f6')), (0.3, hexc('#a48df2')), (0.85, hexc('#8169d6')), (1, hexc('#7159c3'))], hexc('#e2daff'), hexc('#463593')),
    "pink":   ([(0, hexc('#f6b8f7')), (0.45, hexc('#e783f0')), (0.88, hexc('#cc5edd')), (1, hexc('#b847cb'))], hexc('#fde3ff'), hexc('#793096')),
}
for key, (stops, hl, base) in PALETTE.items():
    candy_face(f"pill_face_{key}", 120, 52, 22, stops, hl)
    candy_base(f"pill_base_{key}", 120, 52, 22, base)

# yuvarlak butonlar (kapat / geri)
for key in ("pink", "blue"):
    stops, hl, base = PALETTE[key]
    for kind in ("face", "base"):
        img = new(40, 40)
        W, H = img.size
        fill_mask(img, ellipse_mask(W, H, (0, 0, W - 1, H - 1)), OUTLINE)
        o = 2.5 * K
        if kind == "face":
            inner = ellipse_mask(W, H, (o, o, W - 1 - o, H - 1 - o))
            fill_mask(img, inner, vgrad(W, H, stops, int(o), int(H - o)))
            sh = ellipse_mask(W, H, (o, o + 3 * K, W - 1 - o, H - 1 - o + 3 * K))
            fill_mask(img, ImageChops.multiply(ImageChops.subtract(inner, sh).filter(ImageFilter.GaussianBlur(K * 0.3)), inner), hl)
        else:
            fill_mask(img, ellipse_mask(W, H, (o, o, W - 1 - o, H - 1 - o)), base)
        finish(img, f"circle_{kind}_{key}", 40, 40)

# satır ikon diskleri
DISCS = {
    "pink": PALETTE["pink"],
    "blue": PALETTE["blue"],
    "orange": ([(0, hexc('#ffb88a')), (0.45, hexc('#ff8a55')), (0.88, hexc('#ec6436')), (1, hexc('#d8562a'))], hexc('#fbe2d4'), None),
}
for key, (stops, hl, _) in DISCS.items():
    img = new(38, 38)
    W, H = img.size
    fill_mask(img, ellipse_mask(W, H, (0, 0, W - 1, H - 1)), OUTLINE)
    o = 2.2 * K
    inner = ellipse_mask(W, H, (o, o, W - 1 - o, H - 1 - o))
    fill_mask(img, inner, vgrad(W, H, stops, int(o), int(H - o)))
    sh = ellipse_mask(W, H, (o, o + 2.6 * K, W - 1 - o, H - 1 - o + 2.6 * K))
    fill_mask(img, ImageChops.multiply(ImageChops.subtract(inner, sh).filter(ImageFilter.GaussianBlur(K * 0.3)), inner), hl)
    finish(img, f"disc_{key}", 38, 38)

# ------------------------------------------------------------------ anahtar ve slider
for state, stops in (("on", [(0, hexc('#8ff0ac')), (0.6, hexc('#49d079')), (1, hexc('#34b05f'))]),
                     ("off", [(0, hexc('#8a84ad')), (0.6, hexc('#6a648c')), (1, hexc('#4c4768'))])):
    img = new(64, 34)
    W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), H / 2), OUTLINE)
    o = 2.5 * K
    inner = rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), H / 2 - o)
    fill_mask(img, inner, vgrad(W, H, stops, int(o), int(H - o)))
    sh = rr_mask(W, H, (o, o + 3 * K, W - 1 - o, H - 1 - o + 3 * K), H / 2 - o)
    fill_mask(img, ImageChops.multiply(ImageChops.subtract(inner, sh).filter(ImageFilter.GaussianBlur(K * 0.3)), inner), (255, 255, 255, 70))
    finish(img, f"toggle_track_{state}", 64, 34)

img = new(30, 30)
W, H = img.size
fill_mask(img, ellipse_mask(W, H, (0, 0, W - 1, H - 1)), OUTLINE)
o = 2.4 * K
fill_mask(img, ellipse_mask(W, H, (o, o, W - 1 - o, H - 1 - o)), vgrad(W, H, [(0, WHITE), (1, hexc('#d9d3f2'))], int(o), int(H - o)))
finish(img, "toggle_knob", 30, 30)

img = new(120, 26)
W, H = img.size
fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), H / 2), hexc('#120c2c'))
fill_mask(img, rr_mask(W, H, (2 * K, 2 * K, W - 1 - 2 * K, H - 1 - 2 * K), H / 2 - 2 * K), hexc('#0e0926'))
finish(img, "slider_track", 120, 26)

img = new(120, 26)
W, H = img.size
fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), H / 2), hexc('#2a0b00'))
o = 2 * K
inner = rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), H / 2 - o)
fill_mask(img, inner, vgrad(W, H, [(0, hexc('#fff0a0')), (0.2, hexc('#ffe060')), (0.6, hexc('#ffcb3c')), (0.9, hexc('#f8af22')), (1, hexc('#d98a08'))], int(o), int(H - o)))
gl = rr_mask(W, H, (o + 6 * K, o + 2 * K, W - 1 - o - 6 * K, o + 6 * K), 2 * K)
fill_mask(img, ImageChops.multiply(gl, Image.new("L", (W, H), 110)), (255, 250, 225, 255))
finish(img, "slider_fill", 120, 26)


# ------------------------------------------------------------------ ikonlar (beyaz + koyu kontur)
def thick(d, p0, p1, width, fill=255):
    d.line([p0, p1], fill=fill, width=int(width))
    r = width / 2
    for x, y in (p0, p1):
        d.ellipse((x - r, y - r, x + r, y + r), fill=fill)


def poly(d, pts, g):
    d.polygon(pts, fill=255)
    if g > 0:
        n = len(pts)
        for i in range(n):
            thick(d, pts[i], pts[(i + 1) % n], 2 * g)


def glyph(name, size, shape_fn, detail_fn=None, fill=WHITE, ow=2.0):
    img = new(size, size)
    W, H = img.size
    mo = Image.new("L", (W, H), 0)
    shape_fn(ImageDraw.Draw(mo), ow * K, K)
    mi = Image.new("L", (W, H), 0)
    shape_fn(ImageDraw.Draw(mi), 0, K)
    fill_mask(img, mo, OUTLINE)
    fill_mask(img, mi, fill)
    if detail_fn:
        layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        detail_fn(ImageDraw.Draw(layer), K)
        r, g, b, a = layer.split()
        img.alpha_composite(Image.merge("RGBA", (r, g, b, ImageChops.multiply(a, mi))))
    finish(img, name, size, size)


def music(d, g, s):
    d.ellipse((4 * s - g, 17 * s - g, 14 * s + g, 25.5 * s + g), fill=255)
    d.rounded_rectangle((11.4 * s - g, 4.5 * s - g, 14.6 * s + g, 21.5 * s + g), radius=1 * s + g, fill=255)
    poly(d, [(14 * s, 4.5 * s), (23.5 * s, 9 * s), (23.5 * s, 14.5 * s), (14 * s, 10.5 * s)], g)


def speaker(d, g, s):
    poly(d, [(3.5 * s, 11 * s), (8.5 * s, 11 * s), (15 * s, 5.5 * s), (15 * s, 22.5 * s), (8.5 * s, 17 * s), (3.5 * s, 17 * s)], g)
    for rad in (6.5, 10.5):
        d.arc((16 * s - rad * s + 2 * s, 14 * s - rad * s, 16 * s + rad * s + 2 * s, 14 * s + rad * s), -48, 48, fill=255, width=int(2.6 * s + 2 * g))


def vibrate(d, g, s):
    d.rounded_rectangle((9.5 * s - g, 3.5 * s - g, 18.5 * s + g, 24.5 * s + g), radius=2.6 * s + g, fill=255)
    for sign in (-1, 1):
        cx = 14 * s + sign * 9.2 * s
        pts = [(cx, 8.5 * s), (cx - sign * 2.4 * s, 11.5 * s), (cx, 14.5 * s), (cx - sign * 2.4 * s, 17.5 * s), (cx, 20 * s)]
        for a, b in zip(pts, pts[1:]):
            thick(d, a, b, 2.4 * s + 2 * g)


def vibrate_detail(d, s):
    d.rounded_rectangle((11 * s, 7 * s, 17 * s, 19.5 * s), radius=1.2 * s, fill=(20, 14, 50, 255))
    d.ellipse((13 * s, 21 * s, 15 * s, 23 * s), fill=(20, 14, 50, 255))


def globe(d, g, s):
    d.ellipse((2.5 * s - g, 2.5 * s - g, 21.5 * s + g, 21.5 * s + g), fill=255)


def globe_detail(d, s):
    c = (20, 14, 50, 255)
    w = int(1.5 * s)
    d.line([(2.5 * s, 12 * s), (21.5 * s, 12 * s)], fill=c, width=w)
    d.ellipse((7.5 * s, 2.5 * s, 16.5 * s, 21.5 * s), outline=c, width=w)
    d.arc((2.5 * s, 6.5 * s, 21.5 * s, 17.5 * s), 0, 180, fill=c, width=w)
    d.arc((2.5 * s, 6.5 * s, 21.5 * s, 17.5 * s), 180, 360, fill=c, width=w)


def info(d, g, s):
    d.ellipse((2 * s - g, 2 * s - g, 22 * s + g, 22 * s + g), fill=255)


def info_detail(d, s):
    c = (20, 14, 50, 255)
    d.ellipse((10.1 * s, 5.2 * s, 13.9 * s, 9 * s), fill=c)
    d.rounded_rectangle((10.4 * s, 10.5 * s, 13.6 * s, 18.5 * s), radius=1.2 * s, fill=c)


def help_detail(d, s):
    c = (20, 14, 50, 255)
    w = int(2.8 * s)
    d.arc((8 * s, 4.6 * s, 16 * s, 12.6 * s), 180, 400, fill=c, width=w)
    d.line([(13.4 * s, 11.6 * s), (12 * s, 13 * s), (12 * s, 15 * s)], fill=c, width=w, joint="curve")
    d.ellipse((10.3 * s, 16.4 * s, 13.7 * s, 19.8 * s), fill=c)


def back(d, g, s):
    poly(d, [(3.5 * s, 12 * s), (11.5 * s, 3.5 * s), (11.5 * s, 8.5 * s), (20.5 * s, 8.5 * s), (20.5 * s, 15.5 * s), (11.5 * s, 15.5 * s), (11.5 * s, 20.5 * s)], g)


def cross(d, g, s):
    thick(d, (5.5 * s, 5.5 * s), (18.5 * s, 18.5 * s), 4.4 * s + 2 * g)
    thick(d, (18.5 * s, 5.5 * s), (5.5 * s, 18.5 * s), 4.4 * s + 2 * g)


def instagram(d, g, s):
    d.rounded_rectangle((3 * s - g, 3 * s - g, 21 * s + g, 21 * s + g), radius=6 * s + g, outline=255, width=int(2.8 * s + 2 * g))
    d.ellipse((7.4 * s - g, 7.4 * s - g, 16.6 * s + g, 16.6 * s + g), outline=255, width=int(2.6 * s + 2 * g))
    d.ellipse((15.6 * s - g, 5.2 * s - g, 18.4 * s + g, 8 * s + g), fill=255)


def xlogo(d, g, s):
    thick(d, (4.5 * s, 3.5 * s), (18.5 * s, 20.5 * s), 3.8 * s + 2 * g)
    thick(d, (18.5 * s, 3.5 * s), (4.5 * s, 20.5 * s), 2.4 * s + 2 * g)


def mail(d, g, s):
    d.rounded_rectangle((2.5 * s - g, 5 * s - g, 21.5 * s + g, 19 * s + g), radius=2.4 * s + g, fill=255)


def mail_detail(d, s):
    c = (20, 14, 50, 255)
    w = int(1.7 * s)
    d.line([(3.5 * s, 6.5 * s), (12 * s, 13 * s), (20.5 * s, 6.5 * s)], fill=c, width=w, joint="curve")


glyph("icon_music", 28, music)
glyph("icon_speaker", 28, speaker)
glyph("icon_vibrate", 28, vibrate, vibrate_detail)
glyph("icon_globe", 24, globe, globe_detail)
glyph("icon_info", 24, info, info_detail)
glyph("icon_help", 24, info, help_detail)
glyph("icon_back", 24, back)
glyph("icon_cross", 24, cross)
glyph("icon_instagram", 24, instagram)
glyph("icon_xlogo", 24, xlogo)
glyph("icon_mail", 24, mail, mail_detail)
print("done")
