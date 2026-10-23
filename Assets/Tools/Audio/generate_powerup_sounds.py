"""Güçlendirici sesleri (saf Python, harici paket gerekmez). Kullanım: python3 generate_powerup_sounds.py [çıktı klasörü]"""
import math, os, random, struct, sys, wave

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, "..", "..", "Resources", "Audio", "SFX")
RATE = 22050
rng = random.Random(11)


def silence(sec):
    return [0.0] * int(RATE * sec)


def mix(dst, src, start=0.0, gain=1.0):
    i0 = int(RATE * start)
    for i, v in enumerate(src):
        j = i0 + i
        if j < len(dst):
            dst[j] += v * gain


def sine(freq, sec, decay, f_end=None):
    out = []
    phase = 0.0
    n = int(RATE * sec)
    for i in range(n):
        t = i / RATE
        f = freq if f_end is None else freq + (f_end - freq) * (i / n)
        phase += 2 * math.pi * f / RATE
        out.append(math.sin(phase) * math.exp(-decay * t))
    return out


def noise(sec, decay):
    return [(rng.random() * 2 - 1) * math.exp(-decay * (i / RATE)) for i in range(int(RATE * sec))]


def lowpass_sweep(x, a0, a1):
    y, out = 0.0, []
    for i, v in enumerate(x):
        a = a0 + (a1 - a0) * (i / len(x))
        y += a * (v - y)
        out.append(y)
    return out


def highpass(x, a=0.15):
    y, out = 0.0, []
    for v in x:
        y += a * (v - y)
        out.append(v - y)
    return out


def save(name, samples, peak=0.85):
    m = max(1e-6, max(abs(v) for v in samples))
    data = b"".join(struct.pack("<h", int(max(-1, min(1, v / m * peak)) * 32767)) for v in samples)
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(RATE); w.writeframes(data)


# bomb: low thump + rumbling noise sweeping down
bomb = silence(0.95)
mix(bomb, sine(140, 0.9, 5.5, 38), 0, 1.0)
mix(bomb, lowpass_sweep(noise(0.95, 4.2), 0.55, 0.03), 0, 1.3)
mix(bomb, highpass(noise(0.12, 28)), 0, 0.5)
save("bomb_blast", bomb)

# freeze: glassy arpeggio + shimmer
freeze = silence(1.15)
for k, f in enumerate((1318.5, 1760.0, 2093.0, 2637.0, 3136.0)):
    mix(freeze, sine(f, 0.7, 5.0), k * 0.07, 0.55)
    mix(freeze, sine(f * 2.01, 0.4, 9.0), k * 0.07, 0.15)
mix(freeze, highpass(noise(1.0, 3.5), 0.4), 0.05, 0.12)
save("freeze_start", freeze)

# shield on: rising sweep + bell
sh = silence(0.7)
mix(sh, sine(320, 0.28, 2.0, 880), 0, 0.8)
mix(sh, sine(1180, 0.6, 5.5), 0.22, 0.6)
mix(sh, sine(1770, 0.5, 7.0), 0.22, 0.28)
save("shield_start", sh)

# shield break: glass shatter
br = silence(0.75)
for k in range(14):
    mix(br, highpass(noise(0.07, 55), 0.5), rng.uniform(0, 0.28), rng.uniform(0.3, 0.9))
mix(br, sine(900, 0.45, 6.0, 260), 0, 0.5)
mix(br, highpass(noise(0.6, 6.0), 0.35), 0, 0.35)
save("shield_break", br)
print("done")
