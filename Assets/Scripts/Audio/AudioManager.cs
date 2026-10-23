using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Tek seferlik (DontDestroyOnLoad) ses yöneticisi. Oyun hangi sahneden başlarsa başlasın, Resources/Audio/AudioManager
// prefab'ından kendiliğinden oluşur. Müzik yerleri ve ses efektleri prefab üzerinden sürükle-bırak ile atanır.
public class AudioManager : MonoBehaviour
{
    private const string PrefabPath = "Audio/AudioManager";

    [Serializable]
    public class MusicEntry
    {
        public MusicId id;
        [Tooltip("Boş bırakılırsa bu yer için müzik değişmez, çalan müzik devam eder.")]
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.6f;
        public bool loop = true;
        [Tooltip("Bu müziğe geçerken önceki müzikle çapraz geçiş süresi (saniye).")]
        [Min(0f)] public float fadeSeconds = 1f;
    }

    [Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public MusicId music;
    }

    [Serializable]
    public class SfxEntry
    {
        public SfxId id;
        [Tooltip("Birden fazla klip eklenirse her çalışta rastgele biri seçilir.")]
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        [Tooltip("Her çalışta perdeyi ± bu oranda rastgele değiştirir (monotonluğu azaltır).")]
        [Range(0f, 0.5f)] public float pitchVariation;
        [Tooltip("Bu sesin üst üste tekrar çalması için gereken en kısa süre (saniye).")]
        [Min(0f)] public float minInterval;
        [Tooltip("Aynı anda en fazla kaç tanesi çalabilir (0 = sınırsız). Dolunca yeni çağrılar yok sayılır.")]
        [Min(0)] public int maxVoices;
        [Tooltip("Doluluk sayılırken bu sesle birlikte sayılacak ikinci kimlik (ör. top-top ve top-duvar tek bütçeyi paylaşır).")]
        public SfxId sharedLimitWith;
    }

    [Header("Genel Ses Seviyesi (oyuncunun ayarlardaki müzik sesiyle çarpılır)")]
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    [Header("Müzik")]
    [SerializeField] private List<MusicEntry> music = new List<MusicEntry>();
    [Tooltip("Hangi sahne açılınca hangi müzik çalsın.")]
    [SerializeField] private List<SceneMusic> sceneMusic = new List<SceneMusic>();

    [Header("Ses Efektleri")]
    [SerializeField] private List<SfxEntry> sfx = new List<SfxEntry>();
    [Tooltip("Aynı anda çalabilen en fazla ses sayısı. Dolunca en eski ses kesilir.")]
    [SerializeField, Min(4)] private int voiceCount = 16;

    [Header("Otomatik Buton Sesi")]
    [Tooltip("Sahne açılınca ButtonSound bileşeni olmayan tüm butonlara varsayılan tık sesi eklenir.")]
    [SerializeField] private bool autoButtonSounds = true;

    public static AudioManager Instance { get; private set; }

    private readonly Dictionary<SfxId, SfxEntry> sfxMap = new Dictionary<SfxId, SfxEntry>();
    private readonly Dictionary<MusicId, MusicEntry> musicMap = new Dictionary<MusicId, MusicEntry>();
    private readonly List<Button> buttonBuffer = new List<Button>();
    private float[] lastPlayed;
    private int[] lastClip;
    private SfxId[] voiceSfx;
    private AudioSource[] voices;
    private int nextVoice;

    private AudioSource[] musicSources;
    private int activeMusicSource;
    private MusicEntry activeMusic;
    private readonly float[] fadeLevel = new float[2];   // 0..1: yalnızca geçiş (fade) seviyesi
    private readonly float[] clipVolume = new float[2];  // kaynağın klibine ait Inspector ses seviyesi
    private float fadeRate = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        AudioManager prefab = Resources.Load<AudioManager>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("AudioManager prefab'ı bulunamadı: Resources/" + PrefabPath);
            return;
        }

        Instantiate(prefab);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        name = "AudioManager";
        DontDestroyOnLoad(gameObject);

        foreach (SfxEntry entry in sfx) sfxMap[entry.id] = entry;
        foreach (MusicEntry entry in music) musicMap[entry.id] = entry;
        lastPlayed = new float[Enum.GetValues(typeof(SfxId)).Length];
        lastClip = new int[lastPlayed.Length];
        for (int i = 0; i < lastPlayed.Length; i++)
        {
            lastPlayed[i] = -100f;
            lastClip[i] = -1;
        }

        voices = new AudioSource[voiceCount];
        voiceSfx = new SfxId[voiceCount];
        for (int i = 0; i < voices.Length; i++) voices[i] = CreateSource("Voice " + i, false);

        musicSources = new[] { CreateSource("Music A", true), CreateSource("Music B", true) };

        ApplySettings();
        GameSettings.Changed += ApplySettings;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;
        GameSettings.Changed -= ApplySettings;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Oyuncu "Ses" anahtarını kapattıysa her şey susar.
    private static void ApplySettings()
    {
        AudioListener.volume = GameSettings.SoundEnabled ? 1f : 0f;
    }

    private AudioSource CreateSource(string sourceName, bool loop)
    {
        var go = new GameObject(sourceName);
        go.transform.SetParent(transform, false);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.loop = loop;
        return source;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (SceneMusic entry in sceneMusic)
        {
            if (entry.sceneName != scene.name) continue;

            // Sahnenin müziği henüz atanmadıysa önceki müzik kendi Fade Seconds süresince sönerek biter.
            if (musicMap.TryGetValue(entry.music, out MusicEntry target) && target.clip == null) StopMusic(activeMusic != null ? activeMusic.fadeSeconds : 1f);
            else PlayMusic(entry.music);
        }

        if (autoButtonSounds) AddButtonSounds();
    }

    private void AddButtonSounds()
    {
        buttonBuffer.Clear();
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!button.TryGetComponent(out ButtonSound _)) buttonBuffer.Add(button);
        }

        foreach (Button button in buttonBuffer) button.gameObject.AddComponent<ButtonSound>();
    }

    // ---------------------------------------------------------------- ses efektleri

    // Instance yoksa (ör. prefab eksikse) sessizce hiçbir şey yapmaz; çağıran kodun null kontrolüne gerek kalmaz.
    public static void PlaySfx(SfxId id, float volumeScale = 1f, float pitchScale = 1f, float pan = 0f)
    {
        if (Instance != null) Instance.Play(id, volumeScale, pitchScale, pan);
    }

    public static void PlayMusicTrack(MusicId id)
    {
        if (Instance != null) Instance.PlayMusic(id);
    }

    public void Play(SfxId id, float volumeScale = 1f, float pitchScale = 1f, float pan = 0f)
    {
        if (id == SfxId.None || !sfxMap.TryGetValue(id, out SfxEntry entry)) return;
        if (entry.clips == null || entry.clips.Length == 0) return;
        if (IsSaturated(entry)) return;

        float now = Time.unscaledTime;
        int index = (int)id;
        if (now - lastPlayed[index] < entry.minInterval) return;
        lastPlayed[index] = now;

        int pick = PickClipIndex(entry.clips.Length, lastClip[index]);
        lastClip[index] = pick;
        AudioClip clip = entry.clips[pick];
        if (clip == null) return;

        float pitch = pitchScale * (1f + UnityEngine.Random.Range(-entry.pitchVariation, entry.pitchVariation));
        StartVoice(clip, entry.volume * volumeScale * sfxVolume, pitch, pan, id);
    }

    // Çağıranın boşuna hesap yapmaması için: bu sesin eş zamanlı sınırı doluysa true döner.
    public static bool IsSaturated(SfxId id)
    {
        return Instance != null && Instance.sfxMap.TryGetValue(id, out SfxEntry entry) && Instance.IsSaturated(entry);
    }

    private bool IsSaturated(SfxEntry entry)
    {
        if (entry.maxVoices <= 0) return false;

        int playing = 0;
        for (int i = 0; i < voices.Length; i++)
        {
            if (!voices[i].isPlaying) continue;
            if (voiceSfx[i] == entry.id || (entry.sharedLimitWith != SfxId.None && voiceSfx[i] == entry.sharedLimitWith)) playing++;
        }

        return playing >= entry.maxVoices;
    }

    // Rastgele klip seçer ama bir önceki seçilenle aynı klibi vermez (art arda aynı ses tekrarı monoton duyulur).
    public static int PickClipIndex(int count, int last)
    {
        if (count <= 1) return 0;
        if (last < 0 || last >= count) return UnityEngine.Random.Range(0, count);

        int pick = UnityEngine.Random.Range(0, count - 1);
        return pick >= last ? pick + 1 : pick;
    }

    // Kütüphanede olmayan, doğrudan bir klip çalar (ör. güçlendirici tanımındaki sesler).
    public void PlayClip(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null) StartVoice(clip, volumeScale * sfxVolume, 1f, 0f, SfxId.None);
    }

    private void StartVoice(AudioClip clip, float volume, float pitch, float pan, SfxId id)
    {
        int index = -1;
        for (int i = 0; i < voices.Length; i++)
        {
            int candidate = (nextVoice + i) % voices.Length;
            if (voices[candidate].isPlaying) continue;

            index = candidate;
            break;
        }

        if (index < 0) index = nextVoice;
        nextVoice = (index + 1) % voices.Length;

        AudioSource source = voices[index];
        voiceSfx[index] = id;

        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = pitch;
        source.panStereo = Mathf.Clamp(pan, -1f, 1f);
        source.Play();
    }

    // ---------------------------------------------------------------- müzik

    public void PlayMusic(MusicId id)
    {
        if (!musicMap.TryGetValue(id, out MusicEntry entry) || entry.clip == null) return;
        if (activeMusic == entry && musicSources[activeMusicSource].isPlaying) return;

        activeMusicSource = 1 - activeMusicSource;
        AudioSource source = musicSources[activeMusicSource];
        source.clip = entry.clip;
        source.loop = entry.loop;
        fadeLevel[activeMusicSource] = 0f;
        clipVolume[activeMusicSource] = entry.volume;
        source.volume = 0f;
        source.Play();

        activeMusic = entry;
        fadeRate = 1f / Mathf.Max(0.01f, entry.fadeSeconds);
    }

    public void StopMusic(float fadeSeconds = 1f)
    {
        activeMusic = null;
        fadeRate = 1f / Mathf.Max(0.01f, fadeSeconds);
    }

    private void Update()
    {
        for (int i = 0; i < musicSources.Length; i++)
        {
            AudioSource source = musicSources[i];
            if (!source.isPlaying) continue;

            // Geçiş (fade) ile oyuncunun müzik ayarı ayrıdır: ayar anında uygulanır, geçiş yalnızca sahne/müzik değişiminde işler.
            float fadeTarget = i == activeMusicSource && activeMusic != null ? 1f : 0f;
            fadeLevel[i] = Mathf.MoveTowards(fadeLevel[i], fadeTarget, Time.unscaledDeltaTime * fadeRate);
            source.volume = fadeLevel[i] * clipVolume[i] * musicVolume * GameSettings.MusicVolume;

            if (fadeTarget <= 0f && fadeLevel[i] <= 0f) source.Stop();
        }
    }
}
