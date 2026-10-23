using System.Collections.Generic;
using UnityEngine;

// 20. seviye düşmanı: Yanan Top. Değdiği normal topları yakar; yanan toplar kararır ve renk eşleşmesine giremez.
// Dondur kullanılınca söner ve yaktığı tüm toplar eski rengine döner. Bomba ve Kalkan ona işlemez.
public class BurningBall : SpecialBall
{
    [Header("Yakma")]
    [Tooltip("İki yakma arasındaki en kısa süre. Yığının bir anda kararmasını önler.")]
    [SerializeField, Min(0f)] private float igniteCooldown = 0.45f;

    [Header("Görsel")]
    [SerializeField] private SpriteRenderer aura;
    [SerializeField, Min(0f)] private float flickerSpeed = 5f;

    private readonly List<BallBurn> burned = new List<BallBurn>();
    private float nextIgniteTime;

    protected override void OnEnable()
    {
        base.OnEnable();
        burned.Clear();
        nextIgniteTime = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision) => TryIgnite(collision.collider);

    private void OnCollisionStay2D(Collision2D collision) => TryIgnite(collision.collider);

    private void TryIgnite(Collider2D other)
    {
        if (Time.time < nextIgniteTime) return;
        if (!other.TryGetComponent(out BallBurn burn) || burn.IsBurning) return;

        burn.Ignite();
        burned.Add(burn);
        nextIgniteTime = Time.time + igniteCooldown;
    }

    public override void Neutralize()
    {
        // Yaktığı toplar eski rengine döner (havuza dönenler zaten sıfırlanmıştır).
        foreach (BallBurn burn in burned) burn.Extinguish();
        burned.Clear();

        base.Neutralize();
    }

    private void Update()
    {
        if (aura == null) return;

        // İki dalganın toplamı: düzenli bir nabız ama alev gibi hafif titrek.
        float flicker = Mathf.Sin(Time.time * flickerSpeed) * 0.65f + Mathf.Sin(Time.time * flickerSpeed * 2.3f) * 0.35f;
        Color color = aura.color;
        color.a = 0.72f + 0.28f * flicker;
        aura.color = color;
        aura.transform.localScale = Vector3.one * (1f + 0.06f * flicker);
    }
}
