using System;
using UnityEngine;

// Cihazda kalıcı oyuncu rekorları (PlayerPrefs). Oyun her zaman 1. seviyeden başlar;
// burada yalnızca ulaşılan en yüksek seviye ve skor tutulur.
public static class PlayerProgress
{
    private const string BestScoreKey = "BestScore";
    private const string BestLevelKey = "BestLevel";

    public static event Action Changed;

    public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);
    public static int BestLevel => Mathf.Max(1, PlayerPrefs.GetInt(BestLevelKey, 1));

    // Yeni rekor varsa hemen kaydeder; uygulama kapansa da kaybolmaz.
    public static void Submit(int score, int level)
    {
        bool changed = false;

        if (score > BestScore)
        {
            PlayerPrefs.SetInt(BestScoreKey, score);
            changed = true;
        }

        if (level > BestLevel)
        {
            PlayerPrefs.SetInt(BestLevelKey, level);
            changed = true;
        }

        if (!changed) return;

        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
