using UnityEngine;

// Oyunun zorluğunu oynanan süre ve seviye ilerlemesinden hesaplar.
// Eğri yumuşaktır ve doyuma ulaşır: başta yavaş artar, hiçbir zaman ani sıçramaz, üst sınırı aşmaz.
// Zorluğa bağlı sistemler (ör. BallSpawner) değerleri buradan okur.
public class DifficultyDirector : MonoBehaviour
{
    public static DifficultyDirector Instance { get; private set; }

    [Header("Zorluk Eğrisi")]
    [Tooltip("Yalnızca süreyle zorluğun yarıya ulaşması için gereken süre (saniye).")]
    [SerializeField, Min(1f)] private float timeHalfLife = 240f;
    [Tooltip("Yalnızca seviyeyle zorluğun yarıya ulaşması için gereken seviye sayısı.")]
    [SerializeField, Min(0.1f)] private float levelHalfLife = 6f;

    [Header("Etkiler (zorluk 0 -> 1)")]
    [Tooltip("En zor durumda atış aralığı başlangıcın bu katına iner.")]
    [SerializeField, Range(0.1f, 1f)] private float minSpawnIntervalFactor = 0.4f;
    [Tooltip("En zor durumda fırlatma gücü başlangıcın bu katına çıkar.")]
    [SerializeField, Min(1f)] private float maxLaunchForceFactor = 1.3f;

    // 0 = başlangıç, 1'e asimptotik olarak yaklaşır.
    public float Difficulty { get; private set; }
    public float SpawnIntervalFactor => Mathf.Lerp(1f, minSpawnIntervalFactor, Difficulty);
    public float LaunchForceFactor => Mathf.Lerp(1f, maxLaunchForceFactor, Difficulty);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        float time = GameManager.Instance != null ? GameManager.Instance.PlayTime : 0f;
        float level = ScoreManager.Instance != null ? ScoreManager.Instance.LevelProgress : 0f;
        Difficulty = 1f - Mathf.Pow(0.5f, time / timeHalfLife + level / levelHalfLife);
    }
}
