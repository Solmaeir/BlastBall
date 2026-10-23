using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Seviye ilerledikçe düşman top türlerini açar ve BallSpawner'ın atışlarına seyrek, dengeli biçimde karıştırır.
// Tür açıldığı an ilk topu bir sonraki atışta garanti düşer, karşı güçlendiricisinden bir hak verilir ve
// oyuncu türü ilk kez görüyorsa tanıtım penceresi açılır. Sonrasında her tür kendi olasılık, süre ve adet sınırına uyar.
public class SpecialBallDirector : MonoBehaviour
{
    private const string SeenKeyPrefix = "SpecialBallSeen_";

    public static SpecialBallDirector Instance { get; private set; }

    [SerializeField] private List<SpecialBallDefinition> types = new List<SpecialBallDefinition>();
    [Tooltip("Herhangi iki düşman top arasındaki en kısa süre (oynanan saniye). Türler üst üste binmesin diye.")]
    [SerializeField, Min(0f)] private float globalMinInterval = 10f;
    [Tooltip("Yeni tür ilk kez açıldığında gösterilen pencere.")]
    [SerializeField] private InfoPopup introPopup;

    [Header("Test")]
    [Tooltip("GEÇİCİ: tüm türler oyunun başında açılır. Yayından önce kapat.")]
    [SerializeField] private bool unlockAllAtStart;

    private readonly List<SpecialBallDefinition> unlocked = new List<SpecialBallDefinition>();
    private readonly Queue<SpecialBallDefinition> guaranteed = new Queue<SpecialBallDefinition>();
    private readonly Dictionary<SpecialBallDefinition, float> lastSpawn = new Dictionary<SpecialBallDefinition, float>();
    private float lastAnySpawn = float.NegativeInfinity;

    private static float Now => GameManager.Instance != null ? GameManager.Instance.PlayTime : Time.time;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private IEnumerator Start()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.LevelChanged += OnLevelChanged;
        if (!unlockAllAtStart) yield break;

        // Oyun ilk karede başlatılır; tanıtım penceresinin duraklatması ezilmesin diye bir kare beklenir.
        yield return null;
        foreach (SpecialBallDefinition type in types) if (type != null && !unlocked.Contains(type)) Unlock(type);
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;
        if (ScoreManager.Instance != null) ScoreManager.Instance.LevelChanged -= OnLevelChanged;
    }

    // BallSpawner her atışta sorar: atış düşman topa dönüşecekse onun prefab'ını, değilse normal topu döner.
    public GameObject Pick(GameObject regular)
    {
        float now = Now;
        if (guaranteed.Count > 0) return Take(guaranteed.Dequeue(), now);
        if (unlocked.Count == 0 || now - lastAnySpawn < globalMinInterval) return regular;

        // Rastgele bir türden başlanır ki listede öndeki tür ayrıcalık kazanmasın.
        int start = Random.Range(0, unlocked.Count);
        for (int i = 0; i < unlocked.Count; i++)
        {
            SpecialBallDefinition type = unlocked[(start + i) % unlocked.Count];
            if (now - LastSpawn(type) < type.minInterval) continue;
            if (SpecialBall.CountAlive(type) >= type.maxAlive) continue;
            if (Random.value < type.spawnChance) return Take(type, now);
        }
        return regular;
    }

    private GameObject Take(SpecialBallDefinition type, float now)
    {
        lastSpawn[type] = now;
        lastAnySpawn = now;
        return type.prefab;
    }

    private float LastSpawn(SpecialBallDefinition type)
    {
        return lastSpawn.TryGetValue(type, out float time) ? time : float.NegativeInfinity;
    }

    private void OnLevelChanged(int level)
    {
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver) return;

        foreach (SpecialBallDefinition type in types)
        {
            if (type != null && level >= type.unlockLevel && !unlocked.Contains(type)) Unlock(type);
        }
    }

    private void Unlock(SpecialBallDefinition type)
    {
        unlocked.Add(type);
        guaranteed.Enqueue(type);

        if (type.counter != null && type.counterGrantOnUnlock > 0 && PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.Grant(type.counter, type.counterGrantOnUnlock);
        }

        string seenKey = SeenKeyPrefix + type.id;
        if (introPopup == null || PlayerPrefs.GetInt(seenKey, 0) == 1) return;

        PlayerPrefs.SetInt(seenKey, 1);
        PlayerPrefs.Save();
        introPopup.Show(type.Intro);
    }
}
