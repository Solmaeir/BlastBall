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
        { "best_score",   new[] { "En Yüksek Skor", "Best Score" } },
        { "main_menu",    new[] { "Ana Menü",     "Main Menu" } },
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
