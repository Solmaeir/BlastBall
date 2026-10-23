using System;
using UnityEngine;

// Oyuncunun cihazda saklanan tercihleri. Ayarlar paneli yazar; AudioManager ve Haptics okur.
public static class GameSettings
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundEnabledKey = "SoundEnabled";
    private const string HapticsEnabledKey = "HapticsEnabled";

    public static event Action Changed;

    // Müzik ses seviyesi (0-1).
    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
            Changed?.Invoke();
        }
    }

    // Kapalıysa tüm sesler (müzik + efektler) susar.
    public static bool SoundEnabled
    {
        get => PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(SoundEnabledKey, value ? 1 : 0);
            Changed?.Invoke();
        }
    }

    public static bool HapticsEnabled
    {
        get => PlayerPrefs.GetInt(HapticsEnabledKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(HapticsEnabledKey, value ? 1 : 0);
            Changed?.Invoke();
        }
    }

    // Slider sürüklenirken diske her seferinde yazmamak için değerler bellekte tutulur; panel kapanınca kaydedilir.
    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
