using UnityEngine;
using TMPro; // TextMeshPro kullanabilmek için ekledik

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Göstergeleri")]
    public TextMeshProUGUI scoreText; // Skor yazısı
    public TextMeshProUGUI levelText; // Seviye ve ilerleme yazısı

    [Header("Skor & Seviye Değerleri")]
    public int score = 0;
    public int currentLevel = 1;
    public int popsInCurrentLevel = 0;

    // Seviye atlamak için gereken patlatma sayısı (Lvl 1: 5, Lvl 2: 6, Lvl 3: 7...)
    public int RequiredPops => 4 + currentLevel;

    private void Awake()
    {
        // Kolay erişim için Singleton yapısı
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    // Bir grupta patlatma gerçekleştiğinde çalışır
    public void AddPop(int poppedBallCount)
    {
        // Her patlayan top 10 puan verir (Örn: 6 top patlarsa +60 puan)
        score += poppedBallCount * 10;

        // Mevcut seviyedeki patlatma sayısını 1 artır
        popsInCurrentLevel++;

        // Seviye atlama kontrolü
        if (popsInCurrentLevel >= RequiredPops)
        {
            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        popsInCurrentLevel = 0; // Yeni seviye için sayacı sıfırla
        currentLevel++;
        Debug.Log($"SEVİYE ATLADINIZ! Yeni Seviye: {currentLevel}");
    }

    private void UpdateUI()
    {
        // Sadece Seviye Numarasını yazar (Örn: SEVİYE 1)
        if (levelText != null)
        {
            levelText.text = $"SEVİYE {currentLevel}";
        }

        // Puanı yazar (Örn: SKOR: 0)
        if (scoreText != null)
        {
            scoreText.text = $"SKOR: {score}";
        }
    }
}