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
