import math, os, sys
from PIL import Image, ImageDraw, ImageFilter, ImageChops

OUT = sys.argv[1] if len(sys.argv) > 1 else "out"
os.makedirs(OUT, exist_ok=True)

S = 2      # output pixels per design pixel
SS = 4     # supersampling
K = S * SS # working pixels per design pixel

OUTLINE = (15, 10, 43, 255)  # #0f0a2b


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
    """stops: list of (t, rgba); gradient spans rows y0..y1 (working px)."""
    if y1 is None:
        y1 = h
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


def fill_mask(canvas, mask, color_or_img):
    if isinstance(color_or_img, tuple):
        layer = Image.new("RGBA", canvas.size, color_or_img)
    else:
        layer = color_or_img.copy()
    r, g, b, a = layer.split()
    layer = Image.merge("RGBA", (r, g, b, ImageChops.multiply(a, mask)))
    canvas.alpha_composite(layer)


def finish(img, name, w_design, h_design):
    out = img.resize((int(round(w_design * S)), int(round(h_design * S))), Image.LANCZOS)
    out.save(os.path.join(OUT, name + ".png"))
    return out


# ---------------------------------------------------------------- 3D button parts
def button_face(name, w, h, r, stops, hl, ow=6):
    """Face of a candy button: dark outline, vertical gradient, light top rim."""
    W, H = w * K, h * K
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), OUTLINE)
    o = ow * K
    inner_box = (o, o, W - 1 - o, H - 1 - o)
    inner = rr_mask(W, H, inner_box, (r - ow) * K)
    fill_mask(img, inner, vgrad(W, H, stops, o, H - o))
    # top rim highlight: inner shape minus itself shifted down
    rim = 7 * K
    shifted = rr_mask(W, H, (o, o + rim, W - 1 - o, H - 1 - o + rim), (r - ow) * K)
    rim_mask = ImageChops.subtract(inner, shifted)
    rim_mask = rim_mask.filter(ImageFilter.GaussianBlur(K * 0.6))
    rim_mask = ImageChops.multiply(rim_mask, inner)
    fill_mask(img, rim_mask, hl)
    return finish(img, name, w, h)


def button_base(name, w, h, r, color, ow=6):
    W, H = w * K, h * K
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), OUTLINE)
    o = ow * K
    fill_mask(img, rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), (r - ow) * K), color)
    return finish(img, name, w, h)


BOOSTER_FACES = {
    "blue":   ([(0, hexc('#a8e8fe')), (0.45, hexc('#62cdf6')), (0.88, hexc('#33a9de')), (1, hexc('#238ec7'))], hexc('#e4f8ff'), hexc('#25578d')),
    "orange": ([(0, hexc('#ffb88a')), (0.45, hexc('#ff8a55')), (0.88, hexc('#ec6436')), (1, hexc('#d8562a'))], hexc('#fbe2d4'), hexc('#a3381d')),
    "pink":   ([(0, hexc('#f6b8f7')), (0.45, hexc('#e783f0')), (0.88, hexc('#cc5edd')), (1, hexc('#b847cb'))], hexc('#fde3ff'), hexc('#793096')),
    "gray":   ([(0, hexc('#a7a1bf')), (0.45, hexc('#8c86a8')), (0.88, hexc('#655f82')), (1, hexc('#4c4768'))], hexc('#c9c5d9'), hexc('#3f3960')),
}

for key, (stops, hl, base) in BOOSTER_FACES.items():
    button_face(f"booster_face_{key}", 160, 160, 46, stops, hl)
    button_base(f"booster_base_{key}", 160, 160, 46, base)

# level badge (purple) and pause (blue)
button_face("level_face", 124, 118, 38,
            [(0, hexc('#b8a6f6')), (0.3, hexc('#a48df2')), (0.85, hexc('#8169d6')), (1, hexc('#7159c3'))], hexc('#e2daff'))
button_base("level_base", 124, 118, 38, hexc('#463593'))
button_face("pause_face", 104, 104, 32,
            [(0, hexc('#9fdcff')), (0.35, hexc('#78c9fa')), (0.85, hexc('#4596dd')), (1, hexc('#3d8bd7'))], hexc('#d6f0ff'))
button_base("pause_base", 104, 104, 32, hexc('#1b56a0'))


# ---------------------------------------------------------------- outlined shape helpers
def stroke_and_fill(draw_fn, w, h, fill, outline, ow):
    """draw_fn(draw, grow) draws the shape grown by `grow` working pixels, in white."""
    W, H = w * K, h * K
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    m_out = Image.new("L", (W, H), 0)
    draw_fn(ImageDraw.Draw(m_out), ow * K)
    m_in = Image.new("L", (W, H), 0)
    draw_fn(ImageDraw.Draw(m_in), 0)
    fill_mask(img, m_out, outline)
    fill_mask(img, m_in, fill)
    return img


def thick_line(d, p0, p1, width, fill=255):
    d.line([p0, p1], fill=fill, width=int(width))
    r = width / 2
    for (x, y) in (p0, p1):
        d.ellipse((x - r, y - r, x + r, y + r), fill=fill)


# ---------------------------------------------------------------- snowflake icon
def snowflake(name, fill, outline, size=96):
    W = size * K
    c = W / 2
    R = 35 * K

    def shape(d, g):
        lw = 9.5 * K + 2 * g
        for i in range(6):
            a = math.radians(90 + i * 60)
            ux, uy = math.cos(a), -math.sin(a)
            tip = (c + ux * R, c + uy * R)
            thick_line(d, (c, c), tip, lw)
            for frac, blen in ((0.55, 0.37), (0.30, 0.0)):
                if blen == 0:
                    continue
                bx, by = c + ux * R * frac, c + uy * R * frac
                for s in (-1, 1):
                    b = a + s * math.radians(48)
                    vx, vy = math.cos(b), -math.sin(b)
                    thick_line(d, (bx, by), (bx + vx * R * blen, by + vy * R * blen), lw)
        hr = 12 * K + g
        pts = [(c + hr * math.cos(math.radians(30 + 60 * i)), c - hr * math.sin(math.radians(30 + 60 * i))) for i in range(6)]
        d.polygon(pts, fill=255)

    img = stroke_and_fill(shape, size, size, fill, outline, 5)
    # dark hexagon hole in center like the reference
    d = ImageDraw.Draw(img)
    hr = 6 * K
    pts = [(c + hr * math.cos(math.radians(30 + 60 * i)), c - hr * math.sin(math.radians(30 + 60 * i))) for i in range(6)]
    d.polygon(pts, fill=outline)
    return finish(img, name, size, size)


# ---------------------------------------------------------------- star helper
def star_points(cx, cy, r_out, r_in, rot=-90):
    pts = []
    for i in range(10):
        r = r_out if i % 2 == 0 else r_in
        a = math.radians(rot + i * 36)
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts


def rounded_star(d, cx, cy, r_out, r_in, grow, rounding):
    pts = star_points(cx, cy, r_out + grow, r_in + grow * 0.6)
    d.polygon(pts, fill=255)
    # soften tips by drawing round joints
    for p in pts[::2]:
        pass


# ---------------------------------------------------------------- bomb icon
def bomb(name, body_top, body_bottom, outline, fuse, spark, highlight, size=110):
    W = size * K
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    bx, by, br = 47 * K, 63 * K, 30 * K
    ow = 5.5 * K
    d = ImageDraw.Draw(img)
    # fuse (behind cap): curve from cap to spark
    cap_c = (bx + br * 0.62, by - br * 0.62)
    path = []
    for i in range(21):
        t = i / 20
        x = cap_c[0] + 6 * K + t * 22 * K
        y = cap_c[1] - 4 * K - math.sin(t * math.pi) * 12 * K - t * 6 * K
        path.append((x, y))
    for w, col in ((7 * K + 2 * ow, outline), (7 * K, fuse)):
        d.line(path, fill=col, width=int(w), joint="curve")
        for p in (path[0], path[-1]):
            d.ellipse((p[0] - w / 2, p[1] - w / 2, p[0] + w / 2, p[1] + w / 2), fill=col)
    # cap: rotated rounded rect
    cap = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    cd = ImageDraw.Draw(cap)
    cw, chh = 22 * K, 16 * K
    cx, cy = W / 2, W / 2
    cd.rounded_rectangle((cx - cw / 2 - ow, cy - chh / 2 - ow, cx + cw / 2 + ow, cy + chh / 2 + ow), radius=5 * K, fill=outline)
    cd.rounded_rectangle((cx - cw / 2, cy - chh / 2, cx + cw / 2, cy + chh / 2), radius=3 * K, fill=body_top)
    cap = cap.rotate(-45, resample=Image.BICUBIC, center=(cx, cy), translate=(cap_c[0] - cx, cap_c[1] - cy))
    img.alpha_composite(cap)
    # body
    body = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    m_o = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_o).ellipse((bx - br - ow, by - br - ow, bx + br + ow, by + br + ow), fill=255)
    m_i = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_i).ellipse((bx - br, by - br, bx + br, by + br), fill=255)
    fill_mask(body, m_o, outline)
    fill_mask(body, m_i, vgrad(W, W, [(0, body_top), (1, body_bottom)], int(by - br), int(by + br)))
    # highlight
    hl = Image.new("L", (W, W), 0)
    ImageDraw.Draw(hl).ellipse((bx - br * 0.62, by - br * 0.66, bx - br * 0.12, by - br * 0.18), fill=255)
    hl = hl.rotate(25, center=(bx - br * 0.37, by - br * 0.42))
    fill_mask(body, hl, highlight)
    img.alpha_composite(body)
    # spark star
    sp = path[-1]
    sp = (sp[0] + 2 * K, sp[1] - 2 * K)
    m_o = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_o).polygon(star_points(sp[0], sp[1], 11 * K + ow * 0.8, 5 * K + ow * 0.6), fill=255)
    m_i = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_i).polygon(star_points(sp[0], sp[1], 9 * K, 4 * K), fill=255)
    fill_mask(img, m_o, outline)
    fill_mask(img, m_i, spark)
    return finish(img, name, size, size)


# ---------------------------------------------------------------- shield icon
def shield_path(cx, top, w, h, grow=0):
    """Classic heraldic shield: top edge rises to a soft center peak, straight sides, curved point."""
    w2 = w / 2 + grow
    t = top - grow
    hh = h + 2 * grow
    pts = []
    n = 32
    for i in range(n + 1):
        u = -1 + 2 * i / n
        pts.append((cx + u * w2, t + hh * 0.11 * abs(u) ** 1.4))
    side_end = t + hh * 0.48
    pts.append((cx + w2, side_end))
    for i in range(1, n + 1):
        s = i / n
        pts.append((cx + w2 * math.cos(s * math.pi / 2), side_end + (t + hh - side_end) * s))
    for i in range(n - 1, 0, -1):
        s = i / n
        pts.append((cx - w2 * math.cos(s * math.pi / 2), side_end + (t + hh - side_end) * s))
    pts.append((cx - w2, side_end))
    return pts


def shield(name, fill, shade, outline, star_fill, star_outline, size=110):
    W = size * K
    cx = W / 2
    top, sw, sh = 18 * K, 62 * K, 76 * K
    ow = 7 * K
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    m_o = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_o).polygon(shield_path(cx, top, sw, sh, ow), fill=255)
    m_i = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_i).polygon(shield_path(cx, top, sw, sh, 0), fill=255)
    m_o = m_o.filter(ImageFilter.GaussianBlur(5 * K)).point(lambda v: 255 if v > 100 else 0).filter(ImageFilter.GaussianBlur(K * 0.5))
    m_i = m_i.filter(ImageFilter.GaussianBlur(4 * K)).point(lambda v: 255 if v > 128 else 0).filter(ImageFilter.GaussianBlur(K * 0.5))
    fill_mask(img, m_o, outline)
    fill_mask(img, m_i, fill)
    # inner shade on right half (inside an inset shield)
    m_in2 = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_in2).polygon(shield_path(cx, top + 10 * K, sw - 20 * K, sh - 22 * K, 0), fill=255)
    half = Image.new("L", (W, W), 0)
    ImageDraw.Draw(half).rectangle((cx, 0, W, W), fill=255)
    fill_mask(img, ImageChops.multiply(m_in2, half), shade)
    # star
    scx, scy = cx, top + sh * 0.42
    m_o = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_o).polygon(star_points(scx, scy, 17 * K + 4.5 * K, 7.5 * K + 3.5 * K), fill=255)
    m_i = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_i).polygon(star_points(scx, scy, 16 * K, 7 * K), fill=255)
    fill_mask(img, m_o, star_outline)
    fill_mask(img, m_i, star_fill)
    return finish(img, name, size, size)


WHITE = (255, 255, 255, 255)
snowflake("icon_freeze", WHITE, hexc('#0d3f74'))
bomb("icon_bomb", hexc('#3a3462'), hexc('#1d1838'), OUTLINE, hexc('#ece6f6'), hexc('#ffd23f'), hexc('#6d6694'))
shield("icon_shield", WHITE, hexc('#f0cbf7'), OUTLINE, hexc('#ffd23f'), OUTLINE)


def grayify(src_name, dst_name, dark=hexc('#3e3959'), light=hexc('#d3d0e2')):
    im = Image.open(os.path.join(OUT, src_name + ".png")).convert("RGBA")
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            L = (0.299 * r + 0.587 * g + 0.114 * b) / 255
            c = lerp(dark, light, L)
            px[x, y] = (c[0], c[1], c[2], a)
    im.save(os.path.join(OUT, dst_name + ".png"))


grayify("icon_freeze", "icon_freeze_gray")
grayify("icon_bomb", "icon_bomb_gray")
grayify("icon_shield", "icon_shield_gray")


# ---------------------------------------------------------------- badges
def circle_badge(name, d_design, fill_top, fill_bottom, ow=6):
    W = d_design * K
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    m = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m).ellipse((0, 0, W - 1, W - 1), fill=255)
    fill_mask(img, m, OUTLINE)
    o = ow * K
    m2 = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m2).ellipse((o, o, W - 1 - o, W - 1 - o), fill=255)
    fill_mask(img, m2, vgrad(W, W, [(0, fill_top), (1, fill_bottom)], o, W - o))
    return img


img = circle_badge("badge_red", 56, hexc('#ff5a72'), hexc('#e2475f'))
finish(img, "badge_red", 56, 56)

for kind in ("plus", "check"):
    img = circle_badge("badge_green", 56, hexc('#4ad879'), hexc('#38c266'))
    d = ImageDraw.Draw(img)
    W = 56 * K
    c = W / 2
    lw = 7 * K
    if kind == "plus":
        L = 13 * K
        thick_line(d, (c - L, c), (c + L, c), lw, WHITE)
        thick_line(d, (c, c - L), (c, c + L), lw, WHITE)
    else:
        p0, p1, p2 = (c - 12 * K, c + 1 * K), (c - 3 * K, c + 10 * K), (c + 13 * K, c - 9 * K)
        thick_line(d, p0, p1, lw, WHITE)
        thick_line(d, p1, p2, lw, WHITE)
    finish(img, f"badge_{kind}", 56, 56)


# ---------------------------------------------------------------- active ring, glow
def ring(name, size, r_out, thick, color=WHITE):
    W = size * K
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    m = rr_mask(W, W, (0, 0, W - 1, W - 1), r_out * K)
    t = thick * K
    m2 = rr_mask(W, W, (t, t, W - 1 - t, W - 1 - t), (r_out - thick) * K)
    fill_mask(img, ImageChops.subtract(m, m2), color)
    return finish(img, name, size, size)


ring("booster_ring", 192, 62, 16)


def glow(name, size, inner, r, blur):
    W = size * 2  # low-res is fine for blur
    img = Image.new("RGBA", (W, W), (255, 255, 255, 0))
    m = Image.new("L", (W, W), 0)
    p = (size - inner) / 2 * 2
    ImageDraw.Draw(m).rounded_rectangle((p, p, W - 1 - p, W - 1 - p), radius=r * 2, fill=255)
    m = m.filter(ImageFilter.GaussianBlur(blur * 2))
    img.putalpha(m)
    img = img.resize((int(size * S), int(size * S)), Image.LANCZOS)
    img.save(os.path.join(OUT, name + ".png"))


glow("booster_glow", 260, 196, 64, 14)


# ---------------------------------------------------------------- progress bar
def bar_track(name, w=120, h=68):
    W, H = w * K, h * K
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), H / 2), hexc('#0e082a'))
    o = 2 * K
    fill_mask(img, rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), H / 2 - o), hexc('#110d2b'))
    return finish(img, name, w, h)


bar_track("bar_track")

P = 46  # stripe period (design px) == tiled center width


def bar_fill(name, cap=30, h=56):
    w = cap * 2 + P
    W, H = w * K, h * K
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    shape = rr_mask(W, H, (0, 0, W - 1, H - 1), H / 2)
    fill_mask(img, shape, hexc('#2a0b00'))
    o = 2 * K
    inner = rr_mask(W, H, (o, o, W - 1 - o, H - 1 - o), H / 2 - o)
    base = vgrad(W, H, [(0, hexc('#fff0a0')), (0.14, hexc('#ffe060')), (0.5, hexc('#ffcb3c')),
                        (0.8, hexc('#f8af22')), (0.9, hexc('#d98a08')), (1, hexc('#c27d2e'))], o, H - o)
    # diagonal stripes, period P horizontally, 45deg, aligned so the center tile [cap, cap+P) tiles seamlessly
    stripes = Image.new("L", (W, H), 0)
    sd = ImageDraw.Draw(stripes)
    pk = P * K
    for k in range(-6, 8):
        x0 = cap * K + k * pk
        sd.polygon([(x0, H), (x0 + pk * 0.5, H), (x0 + pk * 0.5 + H, 0), (x0 + H, 0)], fill=255)
    stripe_col = Image.new("RGBA", (W, H), (255, 236, 150, 255))
    stripe_alpha = ImageChops.multiply(stripes, Image.new("L", (W, H), 70))
    base = Image.composite(Image.blend(base, stripe_col, 0.0), base, Image.new("L", (W, H), 0))
    base.alpha_composite(Image.merge("RGBA", (*stripe_col.split()[:3], stripe_alpha)))
    fill_mask(img, inner, base)
    # top gloss
    gl = rr_mask(W, H, (o + 6 * K, o + 3 * K, W - 1 - o - 6 * K, o + 9 * K), 3 * K)
    fill_mask(img, ImageChops.multiply(gl, Image.new("L", (W, H), 120)), (255, 250, 225, 255))
    return finish(img, name, w, h)


bar_fill("bar_fill")


def bar_star(name, size=72):
    W = size * K
    c = W / 2
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    ow = 6 * K
    ro, ri = 30 * K, 14 * K
    m_o = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_o).polygon(star_points(c, c + 2 * K, ro + ow, ri + ow * 0.7), fill=255)
    m_o = m_o.filter(ImageFilter.GaussianBlur(K * 1.2)).point(lambda v: 255 if v > 110 else 0)
    m_i = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m_i).polygon(star_points(c, c + 2 * K, ro, ri), fill=255)
    m_i = m_i.filter(ImageFilter.GaussianBlur(K * 1.2)).point(lambda v: 255 if v > 110 else 0)
    fill_mask(img, m_o, OUTLINE)
    fill_mask(img, m_i, vgrad(W, W, [(0, hexc('#6a6198')), (1, hexc('#4a4078'))], int(c - ro), int(c + ro)))
    # small gloss stroke on upper-left arm
    d = ImageDraw.Draw(img)
    thick_line(d, (c - 10 * K, c - 12 * K), (c - 4 * K, c - 22 * K), 3 * K, (190, 186, 214, 255))
    return finish(img, name, size, size)


bar_star("bar_star")


# ---------------------------------------------------------------- pause glyph
def pause_glyph(name, w=56, h=56):
    W, H = w * K, h * K
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    bw, bh, gap, ow = 9 * K, 38 * K, 10 * K, 5 * K
    cx, cy = W / 2, H / 2
    for s in (-1, 1):
        x = cx + s * (gap / 2 + bw / 2)
        box_o = (x - bw / 2 - ow, cy - bh / 2 - ow, x + bw / 2 + ow, cy + bh / 2 + ow)
        box_i = (x - bw / 2, cy - bh / 2, x + bw / 2, cy + bh / 2)
        fill_mask(img, rr_mask(W, H, box_o, (bw / 2 + ow)), OUTLINE)
        fill_mask(img, rr_mask(W, H, box_i, bw / 2), WHITE)
    return finish(img, name, w, h)


pause_glyph("pause_glyph")


# ---------------------------------------------------------------- HUD panels
def panel_top(name, w=200, h=200, r=40, lip=12):
    W, H = w * K, h * K
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), hexc('#221854'))
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1 - lip * K), r * K), hexc('#2a205e'))
    return finish(img, name, w, h)


def panel_bottom(name, w=200, h=200, r=40, rim=10):
    W, H = w * K, h * K
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    fill_mask(img, rr_mask(W, H, (0, 0, W - 1, H - 1), r * K), hexc('#392f6d'))
    fill_mask(img, rr_mask(W, H, (0, rim * K, W - 1, H - 1), r * K),
              vgrad(W, H, [(0, hexc('#2c1f62')), (1, hexc('#2a205e'))], rim * K, H))
    return finish(img, name, w, h)


panel_top("hud_panel_top")
panel_bottom("hud_panel_bottom")


# ---------------------------------------------------------------- playfield frame (world space, sliced)
def frame_ring(name, size=256, r_out=60, thick=22):
    """Rendered in design px; Unity PPU decides world size."""
    W = size * K
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    outer = rr_mask(W, W, (0, 0, W - 1, W - 1), r_out * K)
    t = thick * K
    inner = rr_mask(W, W, (t, t, W - 1 - t, W - 1 - t), (r_out - thick) * K)
    ringm = ImageChops.subtract(outer, inner)
    col = vgrad(W, W, [(0, hexc('#a08de0')), (0.3, hexc('#937ce4')), (0.7, hexc('#8a72e0')), (1, hexc('#6b52cf'))], 0, W)
    fill_mask(img, ringm, col)
    # thin darker inner edge
    edge = ImageChops.subtract(rr_mask(W, W, (t - 2 * K, t - 2 * K, W - 1 - t + 2 * K, W - 1 - t + 2 * K), (r_out - thick + 2) * K), inner)
    fill_mask(img, edge, hexc('#4b3a92'))
    return finish(img, name, size, size)


def frame_field(name, size=256, r=40):
    W = size * K
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    fill_mask(img, rr_mask(W, W, (0, 0, W - 1, W - 1), r * K), (255, 255, 255, 255))
    return finish(img, name, size, size)


frame_ring("playfield_ring")
frame_field("playfield_field")

print("done")


# ---------------------------------------------------------------- clock icon (game timer)
def clock_icon(name, size=52):
    W = size * K
    c = W / 2
    img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
    ow = 4.5 * K
    r = 16 * K
    # top knob
    knob = Image.new("L", (W, W), 0)
    ImageDraw.Draw(knob).rounded_rectangle((c - 4 * K - ow, 3 * K, c + 4 * K + ow, 8 * K + ow), radius=2 * K, fill=255)
    fill_mask(img, knob, OUTLINE)
    knob2 = Image.new("L", (W, W), 0)
    ImageDraw.Draw(knob2).rounded_rectangle((c - 4 * K, 5 * K, c + 4 * K, 8 * K), radius=1.5 * K, fill=255)
    fill_mask(img, knob2, hexc('#ffd23f'))
    cy = c + 3.5 * K
    m = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m).ellipse((c - r - ow, cy - r - ow, c + r + ow, cy + r + ow), fill=255)
    fill_mask(img, m, OUTLINE)
    m = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m).ellipse((c - r, cy - r, c + r, cy + r), fill=255)
    fill_mask(img, m, vgrad(W, W, [(0, WHITE), (1, hexc('#dcd6f5'))], int(cy - r), int(cy + r)))
    d = ImageDraw.Draw(img)
    hand = 3.2 * K
    d.line([(c, cy), (c, cy - 10 * K)], fill=OUTLINE, width=int(hand))
    d.line([(c, cy), (c + 7 * K, cy + 3 * K)], fill=OUTLINE, width=int(hand))
    d.ellipse((c - 2.6 * K, cy - 2.6 * K, c + 2.6 * K, cy + 2.6 * K), fill=hexc('#f2445f'))
    return finish(img, name, size, size)


clock_icon("icon_clock")
