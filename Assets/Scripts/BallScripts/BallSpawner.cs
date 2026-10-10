using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Prefab ve Konum Ayarları")]
    public GameObject[] ballPrefabs; // Oluşturduğun renkli top prefab'leri
    public Transform spawnPoint;     // Topun doğacağı nokta (boş obje konumu)

    [Header("Zamanlama ve Miktar Ayarları")]
    [Tooltip("Oyun başladıktan kaç saniye sonra ilk fırlatma başlayacak?")]
    public float startDelay = 1f;

    [Tooltip("Kaç saniyede bir fırlatma yapılacak? (Örn: 3.0 yaparsan 3 saniyede bir atar)")]
    public float spawnInterval = 3f;

    [Tooltip("Her fırlatma periyodunda aynı anda kaç adet top doğacak?")]
    [Min(1)] public int ballsPerSpawn = 1;

    [Header("Fırlatma Kuvveti Ayarları")]
    public float minUpForce = 6f;      // Yukarı doğru minimum fırlatma gücü
    public float maxUpForce = 9f;      // Yukarı doğru maksimum fırlatma gücü
    public float sideForce = 1.5f;     // Hafif sağa/sola sapma miktarı

    private float nextSpawnTime;

    private void Start()
    {
        nextSpawnTime = Time.time + startDelay;
    }

    // Time.time oyun duraklayınca (timeScale 0) durduğu için atışlar da durur.
    private void Update()
    {
        if (Time.time < nextSpawnTime) return;

        SpawnBalls();
        float factor = DifficultyDirector.Instance != null ? DifficultyDirector.Instance.SpawnIntervalFactor : 1f;
        nextSpawnTime = Time.time + spawnInterval * factor;
    }

    void SpawnBalls()
    {
        if (ballPrefabs == null || ballPrefabs.Length == 0) return;

        // Belirlenen miktar (ballsPerSpawn) kadar topu aynı anda fırlatır
        for (int i = 0; i < ballsPerSpawn; i++)
        {
            SpawnSingleBall();
        }
    }

    private void SpawnSingleBall()
    {
        // 1. Dizi içinden rastgele bir top prefab'i seç
        int randomIndex = Random.Range(0, ballPrefabs.Length);
        GameObject selectedPrefab = ballPrefabs[randomIndex];

        // 2. Seçilen topu spawnPoint konumunda oluştur
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject newBall = PoolManager.Spawn(selectedPrefab, spawnPos, Quaternion.identity);

        // 3. Topun Rigidbody2D bileşenine ulaş ve dikey itme kuvveti uygula
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomUpForce = Random.Range(minUpForce, maxUpForce);
            float randomSideForce = Random.Range(-sideForce, sideForce);

            // Yukarı ve hafif sağa/sola doğru anlık itme kuvveti (Impulse)
            float force = DifficultyDirector.Instance != null ? DifficultyDirector.Instance.LaunchForceFactor : 1f;
            Vector2 launchDirection = new Vector2(randomSideForce, randomUpForce) * force;
            rb.AddForce(launchDirection, ForceMode2D.Impulse);
        }
    }
}