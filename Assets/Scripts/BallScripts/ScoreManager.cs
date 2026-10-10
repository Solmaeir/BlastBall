using UnityEngine;
using TMPro; // TextMeshPro kullanabilmek için ekledik

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Göstergeleri")]
    public TextMeshProUGUI scoreText; // Skor yazısı
    public TextMeshProUGUI levelText; // Seviye ve ilerleme yazısı

    [Tooltip("Atanırsa 'SEVİYE' kelimesi buraya, levelText'e sadece seviye numarası yazılır.")]
    public TextMeshProUGUI levelLabelText;
    [Tooltip("Atanırsa 'SKOR' etiketi buraya yazılır.")]
    public TextMeshProUGUI scoreLabelText;
    [Tooltip("Atanırsa 'HEDEF 175' buraya, scoreText'e sadece '85/175' yazılır.")]
    public TextMeshProUGUI targetText;

    [Header("İlerleme Çubuğu (skor plakası)")]
    [Tooltip("Dolgu objesinin RectTransform'u. anchorMax.x ile dolar (0 = boş, 1 = dolu).")]
    public RectTransform progressFill;
    [Min(0.1f)] public float progressFillSpeed = 2.5f;

    [Header("Seviye Eşiği")]
    [Tooltip("Seviye n'i geçmek için toplam skor: (5 + 6 + ... + (4+n)) x bu değer. 35 = eski 'seviyede 5, 6, 7... patlatma' temposuna yakın.")]
    [Min(1)] public int averagePointsPerPop = 35;

    [Header("Skor & Seviye Değerleri")]
    public int score = 0;
    public int currentLevel = 1;
    public int popsInCurrentLevel = 0;

    // Seviye atlamak için ulaşılması gereken toplam skor (Lvl 1 -> 175, Lvl 2 -> 385, Lvl 3 -> 630...)
    public int NextLevelScore => ScoreForLevel(currentLevel);

    // Kesintisiz seviye ilerlemesi: 0 = 1. seviye başı, 1.5 = 2. seviyenin yarısı.
    public float LevelProgress
    {
        get
        {
            int start = CurrentLevelStartScore;
            return currentLevel - 1 + Mathf.Clamp01((float)(score - start) / Mathf.Max(1, NextLevelScore - start));
        }
    }
    private int CurrentLevelStartScore => ScoreForLevel(currentLevel - 1);

    // Seviye 1..level arasını geçmek için gereken toplam skor: (5 + 6 + ... + (4+level)) x averagePointsPerPop
    private int ScoreForLevel(int level)
    {
        return averagePointsPerPop * (4 * level + level * (level + 1) / 2);
    }

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

    private float targetProgress;
    private float shownProgress;

    private void OnEnable()
    {
        Localization.LanguageChanged += UpdateUI;
    }

    private void OnDisable()
    {
        Localization.LanguageChanged -= UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
        shownProgress = targetProgress;
        ApplyProgress();
    }

    private void Update()
    {
        if (progressFill == null || Mathf.Approximately(shownProgress, targetProgress)) return;

        // Dolarken akıcı ilerler (oyun durmuş olsa bile), seviye atlayınca/sıfırlanınca anında boşalır.
        shownProgress = targetProgress < shownProgress
            ? targetProgress
            : Mathf.MoveTowards(shownProgress, targetProgress, progressFillSpeed * Time.unscaledDeltaTime);
        ApplyProgress();
    }

    private void ApplyProgress()
    {
        if (progressFill == null) return;

        // 0'da dolgu tamamen kaybolsun diye en az bir mikro değer veriyoruz.
        progressFill.anchorMax = new Vector2(Mathf.Clamp01(shownProgress), progressFill.anchorMax.y);
        progressFill.gameObject.SetActive(shownProgress > 0.001f);
    }

    // Bir grupta patlatma gerçekleştiğinde çalışır
    public void AddPop(int poppedBallCount)
    {
        // Her patlayan top 10 puan verir (Örn: 6 top patlarsa +60 puan)
        score += poppedBallCount * 10;

        // Mevcut seviyedeki patlatma sayısını 1 artır
        popsInCurrentLevel++;

        // Seviye atlama kontrolü (skor eşiğine göre; büyük bir patlatma birden fazla seviye atlatabilir)
        while (score >= NextLevelScore)
        {
            LevelUp();
        }

        PlayerProgress.Submit(score, currentLevel);

        UpdateUI();
    }

    public void ResetScore()
    {
        score = 0;
        currentLevel = 1;
        popsInCurrentLevel = 0;
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
        if (levelLabelText != null)
        {
            levelLabelText.text = Localization.Get("level");
        }

        if (levelText != null)
        {
            levelText.text = levelLabelText != null
                ? currentLevel.ToString()
                : $"{Localization.Get("level")} {currentLevel}";
        }

        if (scoreLabelText != null)
        {
            scoreLabelText.text = Localization.Get("score_label");
        }

        if (targetText != null)
        {
            targetText.text = $"{Localization.Get("target")} {NextLevelScore}";
        }

        // Skoru ve bir sonraki seviye için gereken skoru yazar (Örn: SKOR 120 / 175)
        if (scoreText != null)
        {
            scoreText.text = targetText != null
                ? $"{score}/{NextLevelScore}"
                : $"{Localization.Get("score").ToUpperInvariant()} {score} / {NextLevelScore}";
        }

        // Mevcut seviyenin başlangıcından bir sonraki eşiğe kadar ilerleme
        int levelStart = CurrentLevelStartScore;
        targetProgress = Mathf.Clamp01((float)(score - levelStart) / (NextLevelScore - levelStart));
    }
}