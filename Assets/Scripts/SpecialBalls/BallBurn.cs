using UnityEngine;

// Normal topun "yanmış" durumu: üstüne kömür plakaları ve köz damarları gelir, çevresinde turuncu parıltı
// yanıp söner ve top renk eşleşmesine giremez. Yanan Top uygular, Dondur (Yanan Topu söndürünce) geri alır.
// Yanmıyorken bileşen kapalıdır, maliyeti yoktur.
[RequireComponent(typeof(BallMatching))]
public class BallBurn : MonoBehaviour, IPoolable
{
    [Tooltip("Topun üstünde duran yanık katmanı (varsayılan kapalı). Parıltı bunun alt objesidir.")]
    [SerializeField] private SpriteRenderer overlay;
    [Tooltip("Yanık topun çevresinde yanıp sönen parıltı.")]
    [SerializeField] private SpriteRenderer glow;
    [SerializeField, Min(0f)] private float pulseSpeed = 4f;

    private BallMatching matching;
    private float phase;

    public bool IsBurning { get; private set; }

    private void Awake()
    {
        matching = GetComponent<BallMatching>();
        SetBurning(false);
    }

    public void Ignite()
    {
        if (!IsBurning) SetBurning(true);
    }

    public void Extinguish()
    {
        if (IsBurning) SetBurning(false);
    }

    public void OnSpawnedFromPool() => SetBurning(false);

    public void OnReturnedToPool() => SetBurning(false);

    private void SetBurning(bool burning)
    {
        IsBurning = burning;
        matching.Matchable = !burning;
        overlay.gameObject.SetActive(burning);
        phase = Random.value * Mathf.PI * 2f;
        enabled = burning && glow != null;
    }

    // Parıltının yanıp sönmesi. Yalnızca yanarken çalışır.
    private void Update()
    {
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed + phase);
        Color color = glow.color;
        color.a = Mathf.Lerp(0.35f, 1f, pulse);
        glow.color = color;
        glow.transform.localScale = Vector3.one * Mathf.Lerp(0.96f, 1.06f, pulse);
    }
}
