"""Oyun ses efektleri (saf Python, harici paket gerekmez).
Kullanım: python3 generate_game_sfx.py [çıktı klasörü]   (varsayılan: ../../Resources/Audio/SFX)

Güçlendirici sesleri (bomb_blast, freeze_start, shield_*) ayrı betikte: generate_powerup_sounds.py
"""
import math, os, random, struct, sys, wave

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, "..", "..", "Resources", "Audio", "SFX")
os.makedirs(OUT, exist_ok=True)
RATE = 32000
TAU = 2 * math.pi


def length(sec):
    return int(RATE * sec)


def silence(sec):
    return [0.0] * length(sec)


def mix(dst, src, start=0.0, gain=1.0):
    i0 = int(RATE * start)
    for i, v in enumerate(src):
        j = i0 + i
        if j < len(dst):
            dst[j] += v * gain


def tone(freq, sec, decay, attack=0.002, f_end=None, partials=()):
    """Sönümlenen sinüs. partials: (oran, genlik, ek_sönüm) listesi."""
    n = length(sec)
    out = [0.0] * n
    layers = [(1.0, 1.0, 0.0)] + list(partials)
    for ratio, amp, extra in layers:
        phase = 0.0
        for i in range(n):
            t = i / RATE
            f = freq if f_end is None else freq + (f_end - freq) * (i / n)
            phase += TAU * f * ratio / RATE
            a = min(1.0, t / attack) if attack > 0 else 1.0
            out[i] += math.sin(phase) * amp * a * math.exp(-(decay + extra) * t)
    return out


def noise(sec, decay=0.0, seed=None):
    r = random.Random(seed) if seed is not None else random
    return [(r.random() * 2 - 1) * math.exp(-decay * (i / RATE)) for i in range(length(sec))]


def lowpass(x, a):
    y, out = 0.0, []
    for v in x:
        y += a * (v - y)
        out.append(y)
    return out


def lowpass_sweep(x, a0, a1):
    y, out, n = 0.0, [], len(x)
    for i, v in enumerate(x):
        y += (a0 + (a1 - a0) * i / n) * (v - y)
        out.append(y)
    return out


def highpass(x, a=0.2):
    y, out = 0.0, []
    for v in x:
        y += a * (v - y)
        out.append(v - y)
    return out


def fade_out(x, sec):
    n = min(len(x), length(sec))
    for i in range(n):
        x[len(x) - 1 - i] *= i / n
    return x


def fade_in(x, sec):
    n = min(len(x), length(sec))
    for i in range(n):
        x[i] *= i / n
    return x


def save(name, samples, peak=0.8):
    m = max(1e-6, max(abs(v) for v in samples))
    data = b"".join(struct.pack("<h", int(max(-1.0, min(1.0, v / m * peak)) * 32767)) for v in samples)
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(data)


def bell(freq, sec, decay):
    return tone(freq, sec, decay, attack=0.004, partials=((2.01, 0.35, 3.0), (3.99, 0.12, 8.0)))


# --------------------------------------------------------------------------------------------- top-top (bilardo)
# Sert başlangıç tıkı + reçine topun kısa parlak tınısı + hafif gövde. Sönüm hızlı ve yumuşak, sonunda fade var.
def ball_hit(name, f0, seed):
    sec = 0.24
    out = silence(sec)
    click = fade_in(lowpass(noise(0.012, 260, seed), 0.38), 0.0004)
    mix(out, click, 0, 0.75)
    ring = tone(f0, sec, 30, attack=0.0006, partials=((2.54, 0.42, 22), (4.13, 0.2, 40), (5.9, 0.08, 60)))
    mix(out, ring, 0, 0.9)
    body = tone(f0 * 0.27, sec, 55, attack=0.0008, f_end=f0 * 0.22)
    mix(out, body, 0, 0.5)
    out = lowpass(out, 0.62)
    save(name, fade_out(out, 0.05))


ball_hit("ball_hit_1", 1240, 1)
ball_hit("ball_hit_2", 1360, 2)
ball_hit("ball_hit_3", 1480, 3)
ball_hit("ball_hit_4", 1580, 4)


# --------------------------------------------------------------------------------------------- top-duvar (yumuşak tok)
def ball_wall(name, f0, seed):
    sec = 0.18
    out = silence(sec)
    mix(out, tone(f0, sec, 38, attack=0.0015, f_end=f0 * 0.62), 0, 1.0)
    mix(out, lowpass(noise(0.02, 120, seed), 0.14), 0, 0.55)
    save(name, fade_out(lowpass(out, 0.5), 0.05))


ball_wall("ball_wall_1", 430, 11)
ball_wall("ball_wall_2", 380, 12)

# --------------------------------------------------------------------------------------------- arayüz
click = silence(0.1)
mix(click, tone(880, 0.1, 45, attack=0.002, partials=((2.0, 0.25, 30),)), 0, 1.0)
save("ui_click", fade_out(click, 0.03))

opn = silence(0.34)
mix(opn, bell(659.3, 0.3, 11), 0.0, 0.8)
mix(opn, bell(880.0, 0.3, 11), 0.075, 0.9)
mix(opn, lowpass_sweep(noise(0.2, 9, 5), 0.05, 0.4), 0.0, 0.22)
save("ui_open", fade_out(opn, 0.06))

cls = silence(0.34)
mix(cls, bell(880.0, 0.3, 13), 0.0, 0.8)
mix(cls, bell(587.3, 0.3, 13), 0.07, 0.75)
mix(cls, lowpass_sweep(noise(0.18, 10, 6), 0.4, 0.05), 0.0, 0.18)
save("ui_close", fade_out(cls, 0.06))

play = silence(0.7)
mix(play, tone(170, 0.25, 24, attack=0.002, f_end=70), 0, 0.9)
for k, f in enumerate((523.3, 659.3, 784.0)):
    mix(play, bell(f, 0.5, 7), 0.04 + 0.055 * k, 0.8)
mix(play, highpass(noise(0.3, 14, 8), 0.45), 0.12, 0.14)
save("play_press", fade_out(play, 0.08))

start = silence(1.3)
for k, f in enumerate((523.3, 659.3, 784.0, 1046.5)):
    mix(start, bell(f, 1.0, 4.2), 0.09 * k, 0.75)
mix(start, tone(130.8, 1.2, 3.0, attack=0.01), 0.0, 0.35)
mix(start, highpass(noise(0.5, 8, 9), 0.5), 0.27, 0.1)
save("game_start", fade_out(start, 0.15))

over = silence(1.8)
for k, f in enumerate((392.0, 311.1, 261.6)):
    mix(over, tone(f, 0.9, 4.0, attack=0.012, partials=((2.0, 0.28, 2.0), (3.0, 0.1, 5.0))), 0.17 * k, 0.8)
mix(over, tone(130.8, 1.5, 2.4, attack=0.03, f_end=98.0), 0.3, 0.55)
save("game_over", fade_out(lowpass(over, 0.7), 0.25))

lvl = silence(1.0)
for k, f in enumerate((783.99, 987.77, 1174.66, 1567.98)):
    mix(lvl, bell(f, 0.8, 5.5), 0.07 * k, 0.7)
mix(lvl, highpass(noise(0.45, 7, 10), 0.55), 0.2, 0.14)
save("level_up", fade_out(lvl, 0.12))

# --------------------------------------------------------------------------------------------- oyun etkileşimleri
pop = silence(0.34)
mix(pop, tone(320, 0.2, 26, attack=0.001, f_end=1500), 0, 0.9)
mix(pop, lowpass(noise(0.01, 300, 13), 0.5), 0, 0.5)
mix(pop, tone(2600, 0.2, 40, attack=0.001), 0.02, 0.22)
mix(pop, tone(3300, 0.2, 44, attack=0.001), 0.035, 0.16)
save("ball_pop", fade_out(pop, 0.06))

grab = silence(0.06)
mix(grab, tone(1400, 0.06, 110, attack=0.001), 0, 0.7)
mix(grab, lowpass(noise(0.01, 250, 14), 0.4), 0, 0.3)
save("ball_grab", fade_out(grab, 0.02))

throw = noise(0.17, 0.0, 15)
throw = lowpass_sweep(throw, 0.5, 0.06)
throw = [v * math.sin(math.pi * i / len(throw)) ** 1.5 for i, v in enumerate(throw)]
save("ball_throw", throw)

denied = silence(0.28)
mix(denied, tone(200, 0.15, 24, attack=0.002, partials=((3.0, 0.2, 10),)), 0.0, 0.9)
mix(denied, tone(170, 0.15, 24, attack=0.002, partials=((3.0, 0.2, 10),)), 0.09, 0.85)
save("powerup_denied", fade_out(lowpass(denied, 0.6), 0.06))

unlock = silence(0.9)
for k, f in enumerate((1046.5, 1318.5, 1568.0, 2093.0)):
    mix(unlock, bell(f, 0.6, 6.5), 0.055 * k, 0.65)
mix(unlock, lowpass_sweep(noise(0.3, 6, 16), 0.05, 0.35), 0.0, 0.18)
save("powerup_unlock", fade_out(unlock, 0.1))

arm = silence(0.45)
fz = highpass(noise(0.4, 0.0, 17), 0.55)
arm_env = [(1 - math.exp(-i / (RATE * 0.02))) * math.exp(-4.5 * i / RATE) * (0.55 + 0.45 * math.sin(TAU * 34 * i / RATE)) for i in range(len(fz))]
mix(arm, [v * e for v, e in zip(fz, arm_env)], 0, 0.8)
mix(arm, tone(520, 0.25, 9, attack=0.005, f_end=960), 0, 0.22)
mix(arm, tone(95, 0.1, 40, attack=0.002), 0, 0.5)
save("bomb_arm", fade_out(arm, 0.08))

fend = silence(0.55)
mix(fend, tone(2400, 0.45, 8, attack=0.002, f_end=1100, partials=((1.5, 0.3, 6),)), 0, 0.6)
for k in range(3):
    mix(fend, highpass(noise(0.05, 70, 20 + k), 0.5), 0.05 + 0.09 * k, 0.45)
save("freeze_end", fade_out(fend, 0.1))

print("done")
