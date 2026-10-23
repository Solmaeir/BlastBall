using System.Collections.Generic;
using UnityEngine;

// Düşman topların ortak tabanı. Renk eşleşmesine girmez, sürüklenemez (prefab'da BallDragControl yoktur)
// ve yalnızca tanımındaki karşı güçlendiriciyle yok edilir. Davranış alt sınıftadır.
// Güçlendiriciler iki yoldan etkiler: alan etkisi (Bomba, Kalkan temizliği) Resists ile süzülür,
// kullanım anı etkisi (Dondur, Kalkan) CounterAll ile tüm uygun toplara iletilir.
[RequireComponent(typeof(Rigidbody2D))]
public abstract class SpecialBall : MonoBehaviour
{
    private static readonly List<SpecialBall> Active = new List<SpecialBall>();

    [SerializeField] private SpecialBallDefinition definition;

    protected Rigidbody2D Body { get; private set; }
    public SpecialBallDefinition Definition => definition;

    // Top çapı (dünya birimi). Collider yarıçapı 0.5 olduğundan ölçeğe eşittir.
    protected float Diameter => transform.lossyScale.x;

    protected virtual void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
    }

    // Havuzdan her çıkışta OnEnable, dönüşte OnDisable çalışır; kayıt ve sıfırlama burada yapılır.
    protected virtual void OnEnable()
    {
        Active.Add(this);
    }

    protected virtual void OnDisable()
    {
        Active.Remove(this);
    }

    public static int CountAlive(SpecialBallDefinition type)
    {
        int count = 0;
        foreach (SpecialBall ball in Active) if (ball.definition == type) count++;
        return count;
    }

    // Çarpışan obje bu güçlendiriciye dayanıklı bir düşman top mu? (Alan etkili güçlendiriciler atlamak için sorar.)
    public static bool Resists(Component hit, PowerUpDefinition booster)
    {
        return hit.TryGetComponent(out SpecialBall ball) && ball.definition.counter != booster;
    }

    // Güçlendirici kullanıldığında ona karşı zayıf olan tüm düşman toplar etkisizleşir.
    public static void CounterAll(PowerUpDefinition booster)
    {
        // Neutralize topu havuza döndürüp listeden çıkardığı için sondan başa gezilir.
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            if (i < Active.Count && Active[i].definition.counter == booster) Active[i].Neutralize();
        }
    }

    // Varsayılan: küçük bir patlama efektiyle havuza döner.
    public virtual void Neutralize()
    {
        if (definition.neutralizeEffect != null)
        {
            GameObject effect = PoolManager.Spawn(definition.neutralizeEffect.gameObject, transform.position, Quaternion.identity);
            effect.GetComponent<BlastEffect>().Play(Diameter * 0.9f);
        }

        AudioManager.PlaySfx(SfxId.BallPop);
        PoolManager.Despawn(gameObject);
    }
}
