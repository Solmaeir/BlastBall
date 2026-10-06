"""Ana menü sprite üreticisi. Kullanım: python3 generate_menu_sprites.py <çıktı klasörü>
Ölçüler tasarımdaki 390 px genişlikli telefon pikseli cinsindendir; çıktı 3x çözünürlüktür."""
import math, os, sys
from PIL import Image, ImageDraw, ImageFilter, ImageChops

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, "Sprites")
LOGO_SRC = os.path.join(HERE, "..", "..", "Resources", "Loading", "BlastBallLogo.png")
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


def rr_mask(w, h, box, r, corners=None):
    m = Image.new("L", (w, h), 0)
    ImageDraw.Draw(m).rounded_rectangle(box, radius=max(0, r), fill=255, corners=corners)
    return m


def fill_mask(canvas, mask, color_or_img):
    layer = Image.new("RGBA", canvas.size, color_or_img) if isinstance(color_or_img, tuple) else color_or_img.copy()
    r, g, b, a = layer.split()
    canvas.alpha_composite(Image.merge("RGBA", (r, g, b, ImageChops.multiply(a, mask))))


def new(w, h):
    return Image.new("RGBA", (int(w * K), int(h * K)), (0, 0, 0, 0))


def finish(img, name, w, h):
    out = img.resize((int(round(w * S)), int(round(h * S))), Image.LANCZOS)
    out.save(os.path.join(OUT, name + ".png"))
    return out


def ellipse_mask(W, H, box):
    m = Image.new("L", (W, H), 0)
    ImageDraw.Draw(m).ellipse(box, fill=255)
    return m


def thick_line(d, p0, p1, width, fill=255):
    d.line([p0, p1], fill=fill, width=int(width))
    r = width / 2
    for (x, y) in (p0, p1):
        d.ellipse((x - r, y - r, x + r, y + r), fill=fill)


def star_points(cx, cy, r_out, r_in, rot=-90, n=5):
    pts = []
    for i in range(n * 2):
        r = r_out if i % 2 == 0 else r_in
        a = math.radians(rot + i * 180 / n)
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts


def soften(mask, amount):
    return mask.filter(ImageFilter.GaussianBlur(amount)).point(lambda v: 255 if v > 110 else 0).filter(ImageFilter.GaussianBlur(K * 0.4))


# ------------------------------------------------------------------ panels & pills
def panel_top(name, w=120, h=120, r=22, lip=4):
    img = new(w, h); W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), hexc('#231a52'))
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1 - lip * K), r * K), hexc('#2e245c'))
    finish(img, name, w, h)


def nav_panel(name, w=120, h=120, r=24, rim=5):
    img = new(w, h); W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), hexc('#3d3179'))
    fill_mask(img, rr_mask(W, H, (0, rim * K, W - 1, H - 1), r * K), vgrad(W, H, [(0, hexc('#30256a')), (1, hexc('#2c2260'))], rim * K, H))
    finish(img, name, w, h)


def pill(name, w, h, fill, edge, ew=1.5):
    img = new(w, h); W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), H / 2), edge)
    e = ew * K
    fill_mask(img, rr_mask(W, H, (e, e, W - 1 - e, H - 1 - e), H / 2 - e), fill)
    finish(img, name, w, h)


panel_top("menu_panel_top")
nav_panel("nav_panel")
pill("menu_pill", 80, 40, hexc('#18113a'), hexc('#120c2c'))
pill("booster_pill", 80, 34, hexc('#2c225a'), hexc('#140e32'), 2)


# ------------------------------------------------------------------ 3D faces
def face(name, w, h, r, stops, hl, ow=2.5, outline=OUTLINE, corners=None, rim=3):
    img = new(w, h); W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K, corners), outline)
    o = ow * K
    inner = rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o + (o if corners and not corners[3] else 0)), (r - ow) * K, corners)
    fill_mask(img, inner, vgrad(W, H, stops, o, H - o))
    shifted = rr_mask(W, H, (o, o + rim * K, W - 1 - o, H - 1 - o + rim * K), (r - ow) * K, corners)
    rim_mask = ImageChops.multiply(ImageChops.subtract(inner, shifted).filter(ImageFilter.GaussianBlur(K * 0.3)), inner)
    fill_mask(img, rim_mask, hl)
    finish(img, name, w, h)


def base(name, w, h, r, color, ow=2.5, outline=OUTLINE):
    img = new(w, h); W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), outline)
    o = ow * K
    fill_mask(img, rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), (r - ow) * K), color)
    finish(img, name, w, h)


face("play_face", 120, 82, 20, [(0, hexc('#ffe983')), (0.25, hexc('#fed344')), (0.75, hexc('#f8b029')), (0.92, hexc('#e69115')), (1, hexc('#d88307'))],
     hexc('#fff6cf'), ow=2.5, outline=hexc('#1a1430'), rim=4)
base("play_base", 120, 82, 20, hexc('#a55714'), outline=hexc('#1a1430'))
face("green_face", 26, 26, 8, [(0, hexc('#79e79b')), (0.6, hexc('#49d079')), (1, hexc('#36b461'))], hexc('#d5f7de'), ow=2, rim=2)
base("green_base", 26, 26, 8, hexc('#24854a'), ow=2)
face("nav_tab", 98, 110, 22, [(0, hexc('#c0b1fd')), (0.35, hexc('#9a7ff0')), (0.8, hexc('#7c61e2')), (1, hexc('#6b55ba'))],
     hexc('#e2dafd'), ow=2.5, corners=(True, True, False, False), rim=4)


def tag(name, w=69, h=33, r=7):
    img = new(w, h); W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), hexc('#1d1330'))
    o = 2 * K
    fill_mask(img, rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), (r - 2) * K), vgrad(W, H, [(0, hexc('#ff6680')), (1, hexc('#f2405f'))], o, H - o))
    finish(img, name, w, h)


tag("tag_red")


def logo_plate(name, w=245, h=94, r=13):
    img = new(w, h); W, H = img.size
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), hexc('#2a082f'))
    o = 2.5 * K
    fill_mask(img, rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), (r - 2.5) * K), vgrad(W, H, [(0, hexc('#8a0262')), (1, hexc('#66004a'))], o, H - o))
    finish(img, name, w, h)


logo_plate("logo_plate")


# ------------------------------------------------------------------ level nodes
def node(name, d, stops, hl, base_col, outline, base_h, glyph=None):
    w, h = d, d + base_h
    img = new(w, h); W, H = img.size
    D = d * K
    fill_mask(img, ellipse_mask(W, H, (0, base_h * K, D - 1, base_h * K + D - 1)), outline)
    ow = 2.5 * K
    fill_mask(img, ellipse_mask(W, H, (ow, base_h * K + ow, D - 1 - ow, base_h * K + D - 1 - ow)), base_col)
    fill_mask(img, ellipse_mask(W, H, (0, 0, D - 1, D - 1)), outline)
    inner = ellipse_mask(W, H, (ow, ow, D - 1 - ow, D - 1 - ow))
    fill_mask(img, inner, vgrad(W, H, stops, int(ow), int(D - ow)))
    shifted = ellipse_mask(W, H, (ow, ow + 3 * K, D - 1 - ow, D - 1 - ow + 3 * K))
    fill_mask(img, ImageChops.multiply(ImageChops.subtract(inner, shifted).filter(ImageFilter.GaussianBlur(K * 0.3)), inner), hl)
    if glyph:
        glyph(img, D / 2, D / 2)
    finish(img, name, w, h)


node("node_done", 44, [(0, hexc('#a993ff')), (0.5, hexc('#8a72f0')), (1, hexc('#6e56d6'))], hexc('#d6ccff'), hexc('#46368f'), hexc('#190b42'), 4)
node("node_current", 64, [(0, hexc('#fff3a0')), (0.35, hexc('#fde162')), (0.8, hexc('#f7b52b')), (1, hexc('#ee9d17'))], hexc('#fffbe0'), hexc('#b26d10'), hexc('#1c1409'), 6)


def lock_glyph(img, cx, cy):
    d = ImageDraw.Draw(img)
    s = K
    col = hexc('#cfcbe0')
    d.rounded_rectangle((cx - 8 * s, cy - 2 * s, cx + 8 * s, cy + 10 * s), radius=2.5 * s, fill=col)
    d.arc((cx - 5.5 * s, cy - 11 * s, cx + 5.5 * s, cy + 2 * s), 180, 360, fill=col, width=int(3 * s))
    d.line([(cx - 4 * s, cy - 4.5 * s), (cx - 4 * s, cy - 1 * s)], fill=col, width=int(3 * s))
    d.line([(cx + 4 * s, cy - 4.5 * s), (cx + 4 * s, cy - 1 * s)], fill=col, width=int(3 * s))
    d.ellipse((cx - 1.6 * s, cy + 2 * s, cx + 1.6 * s, cy + 5.2 * s), fill=hexc('#3f3a5a'))


node("node_locked", 44, [(0, hexc('#676384')), (1, hexc('#47435e'))], hexc('#8a86a6'), hexc('#2c2745'), hexc('#17102c'), 4, lock_glyph)


def ring_glow(name, d=110, thick=7):
    W = int(d * K)
    m = Image.new("L", (W, W), 0)
    dr = ImageDraw.Draw(m)
    t = thick * K
    dr.ellipse((t, t, W - 1 - t, W - 1 - t), outline=255, width=int(t))
    m = m.filter(ImageFilter.GaussianBlur(K * 1.5))
    img = Image.new("RGBA", (W, W), (255, 255, 255, 0))
    img.putalpha(m)
    finish(img, name, d, d)


ring_glow("ring_glow")


def small_star(name, fill, size=13):
    img = new(size, size); W, H = img.size
    c = W / 2
    m_o = Image.new("L", (W, H), 0)
    ImageDraw.Draw(m_o).polygon(star_points(c, c + 0.4 * K, 6.2 * K, 3 * K), fill=255)
    m_i = Image.new("L", (W, H), 0)
    ImageDraw.Draw(m_i).polygon(star_points(c, c + 0.4 * K, 4.6 * K, 2.1 * K), fill=255)
    fill_mask(img, soften(m_o, K * 0.4), OUTLINE)
    fill_mask(img, soften(m_i, K * 0.4), fill)
    finish(img, name, size, size)


small_star("star_small_gold", hexc('#ffd23f'))
small_star("star_small_dark", hexc('#3d3768'))


def dot(name, d=8):
    img = new(d, d); W, H = img.size
    fill_mask(img, ellipse_mask(W, H, (0, 0, W - 1, H - 1)), WHITE)
    finish(img, name, d, d)


dot("dot")


def sparkle(name, size=24):
    img = new(size, size); W, H = img.size
    c = W / 2
    pts = []
    for i in range(8):
        r = (c - K) if i % 2 == 0 else c * 0.22
        a = math.radians(-90 + i * 45)
        pts.append((c + r * math.cos(a), c + r * math.sin(a)))
    m = Image.new("L", (W, H), 0)
    ImageDraw.Draw(m).polygon(pts, fill=255)
    fill_mask(img, m.filter(ImageFilter.GaussianBlur(K * 0.4)), WHITE)
    finish(img, name, size, size)


sparkle("sparkle")


# ------------------------------------------------------------------ icons
def outlined(name, size, draw_fn, fill, ow=2.2, outline=OUTLINE, extra=None):
    img = new(size, size); W, H = img.size
    mo = Image.new("L", (W, H), 0); draw_fn(ImageDraw.Draw(mo), ow * K)
    mi = Image.new("L", (W, H), 0); draw_fn(ImageDraw.Draw(mi), 0)
    fill_mask(img, mo, outline)
    fill_mask(img, mi, fill if isinstance(fill, tuple) else fill(W, H))
    if extra:
        extra(img, ImageDraw.Draw(img), mi)
    finish(img, name, size, size)
    return img


def heart_shape(size):
    def fn(d, g):
        s = K
        cx = size * K / 2
        r = 8.6 * s + g
        d.ellipse((cx - 16 * s - g + 0.6 * s, 6 * s - g, cx - 16 * s + 2 * r - g + 0.6 * s, 6 * s + 2 * r - g), fill=255)
        d.ellipse((cx + 16 * s + g - 2 * r - 0.6 * s, 6 * s - g, cx + 16 * s + g - 0.6 * s, 6 * s + 2 * r - g), fill=255)
        d.polygon([(cx - 15.4 * s - g, 17.5 * s), (cx + 15.4 * s + g, 17.5 * s), (cx, 32 * s + g * 1.3)], fill=255)
    return fn


def heart_extra(img, d, mi):
    s = K
    hl = Image.new("L", img.size, 0)
    ImageDraw.Draw(hl).ellipse((7 * s, 9 * s, 13 * s, 15 * s), fill=255)
    fill_mask(img, hl, (255, 255, 255, 140))


outlined("icon_heart", 36, heart_shape(36), lambda W, H: vgrad(W, H, [(0, hexc('#ff7088')), (1, hexc('#ee3d5c'))]), ow=2.4,
         outline=hexc('#3a091d'), extra=heart_extra)


def coin(name, size=34):
    img = new(size, size); W, H = img.size
    fill_mask(img, ellipse_mask(W, H, (0, 0, W - 1, H - 1)), hexc('#2d1600'))
    o = 2.2 * K
    fill_mask(img, ellipse_mask(W, H, (o, o, W - 1 - o, H - 1 - o)), vgrad(W, H, [(0, hexc('#ffd65a')), (1, hexc('#f39c1c'))]))
    i = 6 * K
    fill_mask(img, ellipse_mask(W, H, (i, i, W - 1 - i, H - 1 - i)), hexc('#e8961a'))
    i2 = 7.4 * K
    fill_mask(img, ellipse_mask(W, H, (i2, i2, W - 1 - i2, H - 1 - i2)), vgrad(W, H, [(0, hexc('#ffd04a')), (1, hexc('#fbbd2a'))]))
    hl = ellipse_mask(W, H, (5 * K, 4.5 * K, 12 * K, 10 * K))
    fill_mask(img, hl, (255, 255, 255, 120))
    finish(img, name, size, size)


coin("icon_coin")


def plus_glyph(name, size=14):
    img = new(size, size); W, H = img.size
    d = ImageDraw.Draw(img)
    c = W / 2
    L = 4.8 * K
    thick_line(d, (c - L, c), (c + L, c), 3 * K, WHITE)
    thick_line(d, (c, c - L), (c, c + L), 3 * K, WHITE)
    finish(img, name, size, size)


plus_glyph("plus_glyph")


def gear(name, size=30):
    def fn(d, g):
        c = size * K / 2
        R, r = 11 * K + g, 8.4 * K + g
        pts = []
        teeth = 8
        for i in range(teeth * 4):
            a = math.radians(i * 360 / (teeth * 4) - 90 + 360 / (teeth * 8))
            rad = R if (i % 4) in (0, 1) else r
            pts.append((c + rad * math.cos(a), c + rad * math.sin(a)))
        d.polygon(pts, fill=255)

    def extra(img, d, mi):
        c = size * K / 2
        h = 4.2 * K
        fill_mask(img, ellipse_mask(img.size[0], img.size[1], (c - h - 2.2 * K, c - h - 2.2 * K, c + h + 2.2 * K, c + h + 2.2 * K)), OUTLINE)
        fill_mask(img, ellipse_mask(img.size[0], img.size[1], (c - h, c - h, c + h, c + h)), hexc('#4fa6ec'))
    outlined(name, size, fn, WHITE, ow=2.4, extra=extra)


gear("gear_glyph")


def gift(name, size=36):
    s = K

    def box(d, g):
        d.rounded_rectangle((7 * s - g, 15 * s - g, 29 * s + g, 31 * s + g), radius=2 * s + g, fill=255)
        d.rounded_rectangle((5 * s - g, 10 * s - g, 31 * s + g, 16.5 * s + g), radius=2 * s + g, fill=255)
        d.ellipse((9 * s - g, 3 * s - g, 18.5 * s + g, 11.5 * s + g), fill=255)
        d.ellipse((17.5 * s - g, 3 * s - g, 27 * s + g, 11.5 * s + g), fill=255)

    def extra(img, d, mi):
        W, H = img.size
        red = Image.new("L", (W, H), 0); rd = ImageDraw.Draw(red)
        rd.rounded_rectangle((7 * s, 15 * s, 29 * s, 31 * s), radius=2 * s, fill=255)
        rd.rounded_rectangle((5 * s, 10 * s, 31 * s, 16.5 * s), radius=2 * s, fill=255)
        fill_mask(img, red, vgrad(W, H, [(0, hexc('#ff6a7f')), (1, hexc('#e83857'))]))
        # bow loops (gold) with holes
        bow = Image.new("L", (W, H), 0); bd = ImageDraw.Draw(bow)
        bd.ellipse((9.8 * s, 3.8 * s, 17.7 * s, 10.7 * s), fill=255)
        bd.ellipse((18.3 * s, 3.8 * s, 26.2 * s, 10.7 * s), fill=255)
        fill_mask(img, bow, hexc('#ffce41'))
        holes = Image.new("L", (W, H), 0); hd = ImageDraw.Draw(holes)
        hd.ellipse((12 * s, 6 * s, 16.5 * s, 9.5 * s), fill=255)
        hd.ellipse((19.5 * s, 6 * s, 24 * s, 9.5 * s), fill=255)
        fill_mask(img, holes, hexc('#d9901a'))
        knot = Image.new("L", (W, H), 0)
        ImageDraw.Draw(knot).rounded_rectangle((15.5 * s, 7 * s, 20.5 * s, 11.5 * s), radius=1.5 * s, fill=255)
        fill_mask(img, knot, hexc('#ffce41'))
        # ribbon
        rib = Image.new("L", (W, H), 0); rb = ImageDraw.Draw(rib)
        rb.rectangle((16 * s, 10 * s, 20 * s, 31 * s), fill=255)
        fill_mask(img, rib, hexc('#ffce41'))
        line = Image.new("L", (W, H), 0); ld = ImageDraw.Draw(line)
        ld.line([(5 * s, 16.5 * s), (31 * s, 16.5 * s)], fill=255, width=int(2 * s))
        fill_mask(img, line, OUTLINE)
    outlined(name, size, box, WHITE, ow=2.2, extra=extra)


gift("icon_gift")


def wheel(name, size=36):
    s = K
    c = size * K / 2
    R = 13 * s

    def shape(d, g):
        d.ellipse((c - R - g, c + 1.5 * s - R - g, c + R + g, c + 1.5 * s + R + g), fill=255)
        d.polygon([(c - 4 * s - g, 1 * s - g), (c + 4 * s + g, 1 * s - g), (c, 8 * s + g)], fill=255)

    def extra(img, d, mi):
        W, H = img.size
        cy = c + 1.5 * s
        cols = [hexc('#f2445f'), WHITE, hexc('#4db2f0'), hexc('#ffb347'), WHITE, hexc('#c46bf0')]
        for i, col in enumerate(cols):
            seg = Image.new("L", (W, H), 0)
            ImageDraw.Draw(seg).pieslice((c - R, cy - R, c + R, cy + R), -90 + i * 60, -30 + i * 60, fill=255)
            fill_mask(img, seg, col)
        ring = Image.new("L", (W, H), 0)
        ImageDraw.Draw(ring).ellipse((c - R, cy - R, c + R, cy + R), outline=255, width=int(2 * s))
        fill_mask(img, ring, OUTLINE)
        hub = Image.new("L", (W, H), 0)
        ImageDraw.Draw(hub).ellipse((c - 3.5 * s, cy - 3.5 * s, c + 3.5 * s, cy + 3.5 * s), fill=255)
        fill_mask(img, hub, OUTLINE)
        hub2 = Image.new("L", (W, H), 0)
        ImageDraw.Draw(hub2).ellipse((c - 1.8 * s, cy - 1.8 * s, c + 1.8 * s, cy + 1.8 * s), fill=255)
        fill_mask(img, hub2, hexc('#ffce41'))
        ptr = Image.new("L", (W, H), 0)
        ImageDraw.Draw(ptr).polygon([(c - 3 * s, 2 * s), (c + 3 * s, 2 * s), (c, 7 * s)], fill=255)
        fill_mask(img, ptr, hexc('#ffce41'))
    outlined(name, size, shape, WHITE, ow=2.2, extra=extra)


wheel("icon_wheel")


def house(name, size=36):
    s = K

    def shape(d, g):
        d.polygon([(18 * s, 4 * s - g * 1.3), (33 * s + g * 1.2, 17 * s + g * 0.3), (3 * s - g * 1.2, 17 * s + g * 0.3)], fill=255)
        d.rounded_rectangle((7 * s - g, 14 * s - g, 29 * s + g, 32 * s + g), radius=2 * s + g, fill=255)

    def extra(img, d, mi):
        W, H = img.size
        door = Image.new("L", (W, H), 0)
        ImageDraw.Draw(door).rounded_rectangle((14.5 * s, 21 * s, 21.5 * s, 32 * s), radius=1.2 * s, fill=255)
        fill_mask(img, door, OUTLINE)
        door2 = Image.new("L", (W, H), 0)
        ImageDraw.Draw(door2).rectangle((16.3 * s, 23 * s, 19.7 * s, 30 * s), fill=255)
        fill_mask(img, door2, hexc('#fac123'))
    outlined(name, size, shape, WHITE, ow=2.6, extra=extra)


house("icon_home")


def bag(name, size=30):
    s = K

    def shape(d, g):
        d.rounded_rectangle((5 * s - g, 11 * s - g, 25 * s + g, 28 * s + g), radius=3 * s + g, fill=255)

    def extra(img, d, mi):
        W, H = img.size
        fill_mask(img, mi, vgrad(W, H, [(0, hexc('#7fdcf8')), (1, hexc('#2fa6dc'))], int(11 * s), int(28 * s)))
        handle = Image.new("L", (W, H), 0)
        ImageDraw.Draw(handle).arc((9.5 * s, 3 * s, 20.5 * s, 16 * s), 180, 360, fill=255, width=int(2.4 * s))
        fill_mask(img, handle, hexc('#7fdcf8'))
        top = Image.new("L", (W, H), 0)
        ImageDraw.Draw(top).rectangle((5 * s, 11 * s, 25 * s, 14 * s), fill=255)
        fill_mask(img, ImageChops.multiply(top, mi), hexc('#b9eefc'))
    outlined(name, size, shape, WHITE, ow=0.01, outline=(0, 0, 0, 0), extra=extra)


bag("icon_shop")


def trophy(name, size=30):
    s = K

    def shape(d, g):
        d.pieslice((7 * s - g, -6 * s - g, 23 * s + g, 18 * s + g), 0, 180, fill=255)
        d.rectangle((7 * s - g, 2 * s - g, 23 * s + g, 6 * s), fill=255)
        d.rectangle((13 * s - g, 17 * s, 17 * s + g, 22 * s), fill=255)
        d.rounded_rectangle((9 * s - g, 21 * s - g, 21 * s + g, 26 * s + g), radius=1.5 * s, fill=255)
        d.arc((2 * s - g, 3 * s - g, 11 * s + g, 13 * s + g), 90, 270, fill=255, width=int(2.4 * s + g * 2))
        d.arc((19 * s - g, 3 * s - g, 28 * s + g, 13 * s + g), 270, 90, fill=255, width=int(2.4 * s + g * 2))

    def extra(img, d, mi):
        W, H = img.size
        fill_mask(img, mi, vgrad(W, H, [(0, hexc('#ffd84a')), (1, hexc('#f0a51a'))]))
        hl = Image.new("L", (W, H), 0)
        ImageDraw.Draw(hl).rectangle((10 * s, 4 * s, 12 * s, 11 * s), fill=255)
        fill_mask(img, ImageChops.multiply(hl, mi), (255, 255, 255, 150))
    outlined(name, size, shape, WHITE, ow=0.01, outline=(0, 0, 0, 0), extra=extra)


trophy("icon_trophy")


# mini inventory icons
def mini_snow(name, size=18):
    img = new(size, size); W, H = img.size
    c = W / 2
    d = ImageDraw.Draw(img)
    for col, w in ((hexc('#153660'), 3.6 * K), (hexc('#a0ebff'), 2 * K)):
        for i in range(3):
            a = math.radians(90 + i * 60)
            dx, dy = math.cos(a) * 7 * K, math.sin(a) * 7 * K
            thick_line(d, (c - dx, c - dy), (c + dx, c + dy), w, col)
    finish(img, name, size, size)


def mini_bomb(name, size=18):
    img = new(size, size); W, H = img.size
    s = K
    fill_mask(img, ellipse_mask(W, H, (2 * s, 5 * s, 15 * s, 18 * s - 1)), hexc('#2d0611'))
    fill_mask(img, ellipse_mask(W, H, (3.2 * s, 6.2 * s, 13.8 * s, 16.8 * s)), vgrad(W, H, [(0, hexc('#ffad8a')), (0.4, hexc('#fd7542')), (1, hexc('#d65428'))]))
    fill_mask(img, ellipse_mask(W, H, (5 * s, 8 * s, 8 * s, 11 * s)), (255, 220, 200, 200))
    d = ImageDraw.Draw(img)
    d.line([(11.5 * s, 6 * s), (14 * s, 2.5 * s)], fill=hexc('#2d0611'), width=int(1.8 * s))
    finish(img, name, size, size)


def mini_shield(name, size=18):
    img = new(size, size); W, H = img.size
    s = K

    def pts(g):
        return [(9 * s, 1.5 * s - g), (16 * s + g, 4 * s - g * 0.3), (15 * s + g, 11 * s), (9 * s, 17 * s + g), (3 * s - g, 11 * s), (2 * s - g, 4 * s - g * 0.3)]
    mo = Image.new("L", (W, H), 0); ImageDraw.Draw(mo).polygon(pts(1.5 * s), fill=255)
    mi = Image.new("L", (W, H), 0); ImageDraw.Draw(mi).polygon(pts(0), fill=255)
    fill_mask(img, soften(mo, K * 0.6), hexc('#140c38'))
    fill_mask(img, soften(mi, K * 0.6), hexc('#d65fea'))
    half = Image.new("L", (W, H), 0); ImageDraw.Draw(half).rectangle((9 * s, 0, W, H), fill=255)
    fill_mask(img, ImageChops.multiply(soften(mi, K * 0.6), half), hexc('#f2a6fb'))
    finish(img, name, size, size)


mini_snow("mini_freeze")
mini_bomb("mini_bomb")
mini_shield("mini_shield")


# ------------------------------------------------------------------ shine band
def shine(name, w=60, h=140):
    W, H = int(w * K / 2), int(h * K / 2)
    img = Image.new("RGBA", (W, H), (255, 255, 255, 0))
    m = Image.new("L", (W, H), 0)
    d = ImageDraw.Draw(m)
    d.polygon([(W * 0.35, 0), (W * 0.75, 0), (W * 0.65, H), (W * 0.25, H)], fill=150)
    d.polygon([(W * 0.82, 0), (W * 0.92, 0), (W * 0.82, H), (W * 0.72, H)], fill=110)
    m = m.filter(ImageFilter.GaussianBlur(K * 0.6))
    img.putalpha(m)
    img.resize((w * S, h * S), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))


shine("shine_band")


# ------------------------------------------------------------------ logo split
def split_logo():
    src = Image.open(LOGO_SRC).convert("RGBA")
    planets = {
        "planet_earth": (202, 306, 70),
        "planet_purple": (820, 296, 68),
        "planet_sun": (328, 176, 60),
        "planet_orange": (724, 164, 48),
    }
    core = src.copy()
    for name, (cx, cy, r) in planets.items():
        crop = src.crop((cx - r, cy - r, cx + r, cy + r))
        m = Image.new("L", crop.size, 0)
        ImageDraw.Draw(m).ellipse((0, 0, 2 * r - 1, 2 * r - 1), fill=255)
        crop.putalpha(ImageChops.multiply(crop.getchannel("A"), m))
        crop.save(os.path.join(OUT, name + ".png"))
        # patch the hole in the core with the surrounding halo colour
        ring = []
        px = src.load()
        for i in range(48):
            a = 2 * math.pi * i / 48
            x, y = int(cx + (r + 8) * math.cos(a)), int(cy + (r + 8) * math.sin(a))
            if 0 <= x < src.width and 0 <= y < src.height:
                ring.append(px[x, y])
        wsum = sum(p[3] for p in ring) or 1
        avg = tuple(int(sum(p[k] * p[3] for p in ring) / wsum) for k in range(3)) + (int(sum(p[3] for p in ring) / len(ring)),)
        hole = Image.new("L", src.size, 0)
        ImageDraw.Draw(hole).ellipse((cx - r - 2, cy - r - 2, cx + r + 2, cy + r + 2), fill=255)
        hole = hole.filter(ImageFilter.GaussianBlur(3))
        core = Image.composite(Image.new("RGBA", src.size, avg), core, hole)
    core.save(os.path.join(OUT, "logo_core.png"))


split_logo()
print("done")
