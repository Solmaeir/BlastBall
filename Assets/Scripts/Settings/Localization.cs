using System;
using System.Collections.Generic;
using UnityEngine;

public enum Language
{
    Turkish,
    English
}

public static class Localization
{
    private const string PrefKey = "Language";

    private static readonly Dictionary<string, string[]> Texts = new Dictionary<string, string[]>
    {
        //                   Türkçe            English
        { "options",      new[] { "Ayarlar",      "Options" } },
        { "info",         new[] { "Bilgi",        "Info" } },
        { "volume",       new[] { "Ses",          "Volume" } },
        { "mute",         new[] { "Sesi Kapat",   "Mute" } },
        { "unmute",       new[] { "Sesi Aç",      "Unmute" } },
        { "language",     new[] { "Dil",          "Language" } },
        { "back",         new[] { "Geri",         "Back" } },
        { "loading",      new[] { "Yükleniyor",   "Loading" } },
        { "ok",           new[] { "Tamam",        "OK" } },
        { "cancel",       new[] { "İptal",        "Cancel" } },
        { "game_over",    new[] { "Oyun Bitti",   "Game Over" } },
        { "score",        new[] { "Skor",         "Score" } },
        { "level",        new[] { "SEVİYE",       "LEVEL" } },
        { "score_label",  new[] { "SKOR",         "SCORE" } },
        { "target",       new[] { "HEDEF",        "TARGET" } },
        { "booster_freeze", new[] { "DONDUR",     "FREEZE" } },
        { "booster_bomb",   new[] { "BOMBA",      "BOMB" } },
        { "booster_shield", new[] { "KALKAN",     "SHIELD" } },
        { "play",         new[] { "OYNA",         "PLAY" } },
        { "lives_full",   new[] { "DOLU",         "FULL" } },
        { "last_level",   new[] { "SON\nSEVİYE",  "LAST\nLEVEL" } },
        { "music",        new[] { "Müzik",        "Music" } },
        { "sound",        new[] { "Ses",          "Sound" } },
        { "vibration",    new[] { "Titreşim",     "Vibration" } },
        { "contact",      new[] { "İletişim",     "Contact" } },
        { "record_level", new[] { "EN YÜKSEK SEVİYE", "BEST LEVEL" } },
        { "record_score", new[] { "EN YÜKSEK SKOR",   "BEST SCORE" } },
        { "reward",       new[] { "ÖDÜL",         "REWARD" } },
        { "wheel",        new[] { "ÇARK",         "WHEEL" } },
        { "shop",         new[] { "MAĞAZA",       "SHOP" } },
        { "home",         new[] { "ANA SAYFA",    "HOME" } },
        { "ranking",      new[] { "SIRALAMA",     "RANKING" } },
        { "best_score",   new[] { "En Yüksek Skor", "Best Score" } },
        { "main_menu",    new[] { "Ana Menü",     "Main Menu" } },
        { "how_to_play",  new[] { "Nasıl Oynanır", "How to Play" } },
        { "how_to_body",  new[] {
            "<size=140%><color=#FFD23F>6 veya daha fazla</color></size>\naynı renk top birbirine değerse <color=#FFD23F>PATLAR!</color>\n\nTopları sürükle, fırlat ve aynı renkleri bir araya getir.\nBir top <color=#FF7FA8>üst çizgiye</color> 2 saniye değerse oyun biter.",
            "<size=140%><color=#FFD23F>6 or more</color></size>\nballs of the same color touching each other <color=#FFD23F>POP!</color>\n\nDrag and throw balls to group the same colors.\nIf a ball touches the <color=#FF7FA8>top line</color> for 2 seconds, the game is over." } },
        { "new_ball",     new[] { "YENİ TOP!",    "NEW BALL!" } },
        { "mixer_name",   new[] { "Karıştırıcı",  "Mixer" } },
        { "mixer_desc",   new[] {
            "Topların arasında yüzer ve ortalığı dağıtır.\nYalnızca <color=#FFD23F>BOMBA</color> ile yok edilir.",
            "Swims between the balls and scatters them.\nOnly a <color=#FFD23F>BOMB</color> can destroy it." } },
        { "burner_name",  new[] { "Yanan Top",    "Fire Ball" } },
        { "burner_desc",  new[] {
            "Değdiği topları yakar, renklerini karartır.\n<color=#7FD8FF>DONDUR</color> ile söndür, yanan toplar eski rengine döner.",
            "Burns the balls it touches and hides their color.\nPut it out with <color=#7FD8FF>FREEZE</color>, burnt balls get their color back." } },
        { "blackhole_name", new[] { "Kara Delik", "Black Hole" } },
        { "blackhole_desc", new[] {
            "Yukarıda süzülür ve çevresindeki topları kendine çeker.\nYalnızca <color=#E9A7FF>KALKAN</color> ile kapanır.",
            "Floats up high and pulls the nearby balls in.\nOnly a <color=#E9A7FF>SHIELD</color> can close it." } },
        { "turkish",      new[] { "Türkçe",       "Türkçe" } },
        { "english",      new[] { "English",      "English" } },
    };

    private static Language? current;

    public static event Action LanguageChanged;

    public static Language Current
    {
        get
        {
            if (!current.HasValue)
            {
                current = (Language)PlayerPrefs.GetInt(PrefKey, (int)Language.Turkish);
            }
            return current.Value;
        }
        set
        {
            if (current == value) return;
            current = value;
            PlayerPrefs.SetInt(PrefKey, (int)value);
            PlayerPrefs.Save();
            LanguageChanged?.Invoke();
        }
    }

    public static string Get(string key)
    {
        return Texts.TryGetValue(key, out var values) ? values[(int)Current] : key;
    }
}
