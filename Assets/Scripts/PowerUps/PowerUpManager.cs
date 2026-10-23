using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Tüm güçlendiricilerin tek merkezi: hak sayıları, seviye kilidi, buton etkileşimi ve diğer sistemlerle köprü.
public class PowerUpManager : MonoBehaviour
{
    [Serializable]
    private class SlotConfig
    {
        public PowerUpDefinition definition;
        [Tooltip("Boş bırakılırsa Button Prefab'dan otomatik oluşturulur.")]
        public BoosterButtonView view;
    }

    public static PowerUpManager Instance { get; private set; }

    [Header("Güçlendiriciler")]
    [SerializeField] private List<SlotConfig> slots = new List<SlotConfig>();
    [SerializeField] private BoosterButtonView buttonPrefab;
    [SerializeField] private RectTransform buttonContainer;

    [Header("Sahne Referansları")]
    [Tooltip("Topların oynandığı kutunun içi (dokunma alanı sınırı).")]
    [SerializeField] private Renderer playfield;
    [SerializeField] private Collider2D topFrame;
    [Tooltip("Tam ekran filtrelerin (ör. buz) oluşturulacağı overlay canvas.")]
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private PlayfieldCameraFit cameraFit;

    [Header("Test")]
    [Tooltip("GEÇİCİ: tüm güçlendiriciler seviye beklemeden açık başlar. Yayından önce kapat.")]
    [SerializeField] private bool unlockAllForTesting;

    private readonly List<PowerUpSlot> runtimeSlots = new List<PowerUpSlot>();
    private readonly List<PowerUpSlot> pickBuffer = new List<PowerUpSlot>();
    private Camera cam;
    private float ballDiameter;

    // Hakkı bitmiş (ve aktif olmayan) bir butona basıldı. Reklam / mağaza sistemi buraya bağlanır.
    public event Action<PowerUpSlot> EmptyPressed;

    public Bounds Field => playfield.bounds;
    public Collider2D TopFrame => topFrame;
    public bool IsGameRunning => GameManager.Instance == null || GameManager.Instance.IsPlaying;

    public bool SpawningBlocked
    {
        get
        {
            foreach (PowerUpSlot slot in runtimeSlots) if (slot.IsActive && slot.Runtime.BlocksSpawning) return true;
            return false;
        }
    }

    public bool GameOverPaused
    {
        get
        {
            foreach (PowerUpSlot slot in runtimeSlots) if (slot.IsActive && slot.Runtime.PausesGameOver) return true;
            return false;
        }
    }

    public bool InputLocked
    {
        get
        {
            foreach (PowerUpSlot slot in runtimeSlots) if (slot.IsActive && slot.Runtime.LocksBallInput) return true;
            return false;
        }
    }

    // Top çapı (dünya birimi). Ekrandaki ilk toptan okunur.
    public float BallDiameter
    {
        get
        {
            if (ballDiameter > 0f) return ballDiameter;

            BallMatching ball = FindFirstObjectByType<BallMatching>();
            if (ball == null || !ball.TryGetComponent(out CircleCollider2D circle)) return 0.42f;

            Vector3 scale = ball.transform.lossyScale;
            ballDiameter = circle.radius * 2f * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            return ballDiameter;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        cam = Camera.main;

        foreach (SlotConfig config in slots)
        {
            if (config.definition == null) continue;

            BoosterButtonView view = config.view != null ? config.view : Instantiate(buttonPrefab, buttonContainer);
            view.Bind(config.definition);

            var slot = new PowerUpSlot(this, config.definition, view);
            slot.Runtime = config.definition.CreateRuntime(slot);
            runtimeSlots.Add(slot);

            if (view.TryGetComponent(out Button button)) button.onClick.AddListener(() => Press(slot));
        }
    }

    private void Start()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.LevelChanged += OnLevelChanged;
        RefreshUnlocks(false);
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;
        if (ScoreManager.Instance != null) ScoreManager.Instance.LevelChanged -= OnLevelChanged;
        foreach (PowerUpSlot slot in runtimeSlots) slot.Runtime.Dispose();
    }

    private void Update()
    {
        foreach (PowerUpSlot slot in runtimeSlots)
        {
            if (slot.IsActive) slot.Runtime.Tick();
        }
    }

    private void OnLevelChanged(int level)
    {
        RefreshUnlocks(true);
    }

    private void RefreshUnlocks(bool animate)
    {
        int level = ScoreManager.Instance != null ? ScoreManager.Instance.currentLevel : 1;

        foreach (PowerUpSlot slot in runtimeSlots)
        {
            bool unlocked = unlockAllForTesting || level >= slot.Definition.unlockLevel;
            bool justUnlocked = unlocked && !slot.IsUnlocked;
            slot.IsUnlocked = unlocked;
            slot.Refresh();
            if (justUnlocked && animate)
            {
                slot.View.PlayUnlock();
                AudioManager.PlaySfx(SfxId.PowerUpUnlock);
            }
        }
    }

    private void Press(PowerUpSlot slot)
    {
        if (!IsGameRunning) return;

        if (!slot.IsUnlocked)
        {
            AudioManager.PlaySfx(SfxId.PowerUpDenied);
            Haptics.Play(HapticType.Light);
            return;
        }

        if (slot.Count <= 0)
        {
            if (slot.IsActive) return;

            AudioManager.PlaySfx(SfxId.PowerUpDenied);
            Haptics.Play(HapticType.Light);
            EmptyPressed?.Invoke(slot);
            return;
        }

        slot.Runtime.Press();
    }

    // Ödül / reklam / mağaza gibi kaynaklardan hak eklemek için.
    public void Grant(PowerUpDefinition definition, int amount)
    {
        foreach (PowerUpSlot slot in runtimeSlots)
        {
            if (slot.Definition == definition) slot.Add(amount);
        }
    }

    // Açık güçlendiricilerden rastgele biri (ödüller için). Hiçbiri açık değilse null.
    public PowerUpSlot RandomUnlockedSlot()
    {
        pickBuffer.Clear();
        foreach (PowerUpSlot slot in runtimeSlots) if (slot.IsUnlocked) pickBuffer.Add(slot);
        return pickBuffer.Count > 0 ? pickBuffer[UnityEngine.Random.Range(0, pickBuffer.Count)] : null;
    }

    public bool TryAbsorbGameOver()
    {
        foreach (PowerUpSlot slot in runtimeSlots)
        {
            if (slot.IsActive && slot.Runtime.TryAbsorbGameOver()) return true;
        }
        return false;
    }

    public Vector2 ScreenToWorld(Vector2 screen) => cam.ScreenToWorldPoint(screen);

    public bool FieldContains(Vector2 world)
    {
        Bounds b = Field;
        return world.x >= b.min.x && world.x <= b.max.x && world.y >= b.min.y && world.y <= b.max.y;
    }

    // Oyun dünyasını dondurur (fizik, atışlar, süre). Duraklatma menüsü açıkken GameManager zaten kontrol eder.
    public void SetWorldFrozen(bool frozen)
    {
        if (frozen) Time.timeScale = 0f;
        else if (IsGameRunning) Time.timeScale = 1f;
    }

    public void PlaySound(AudioClip clip)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClip(clip);
    }

    public void Shake(float strength, float duration)
    {
        if (cameraFit != null) cameraFit.Shake(strength, duration);
    }

    public T CreateOverlay<T>(T prefab) where T : Component
    {
        return Instantiate(prefab, overlayRoot);
    }
}
