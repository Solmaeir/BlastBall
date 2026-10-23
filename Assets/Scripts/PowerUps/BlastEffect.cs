using UnityEngine;

// Bomba patlama efekti: parlama, genişleyen halka ve savrulan kıvılcımlar. PoolManager ile havuzlanır.
// Prefab kökünün çapı 1 birim kabul edilir; Play(radius) ile gerçek yarıçapa ölçeklenir.
public class BlastEffect : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer flash;
    [SerializeField] private SpriteRenderer ring;
    [SerializeField] private SpriteRenderer[] sparks;
    [SerializeField, Min(0.1f)] private float duration = 0.5f;

    private Vector2[] directions;
    private float[] spins;
    private float time;

    public void OnSpawnedFromPool()
    {
        time = 0f;
    }

    public void OnReturnedToPool() { }

    public void Play(float radius)
    {
        transform.localScale = Vector3.one * (radius * 2f);
        time = 0f;

        if (directions == null || directions.Length != sparks.Length)
        {
            directions = new Vector2[sparks.Length];
            spins = new float[sparks.Length];
        }

        float offset = Random.value * Mathf.PI * 2f;
        for (int i = 0; i < sparks.Length; i++)
        {
            float angle = offset + i * Mathf.PI * 2f / sparks.Length + Random.Range(-0.25f, 0.25f);
            directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            spins[i] = Random.Range(-240f, 240f);
        }

        Apply(0f);
    }

    private void Update()
    {
        time += Time.unscaledDeltaTime;
        float t = time / duration;

        if (t >= 1f)
        {
            PoolManager.Despawn(gameObject);
            return;
        }

        Apply(t);
    }

    private void Apply(float t)
    {
        float ease = 1f - (1f - t) * (1f - t);

        SetSprite(flash, Mathf.Lerp(0.45f, 1f, ease), Mathf.Pow(1f - t, 1.6f));
        SetSprite(ring, Mathf.Lerp(0.25f, 1.15f, ease), Mathf.Pow(1f - t, 1.1f));

        for (int i = 0; i < sparks.Length; i++)
        {
            Transform spark = sparks[i].transform;
            spark.localPosition = directions[i] * Mathf.Lerp(0.3f, 0.78f, ease);
            spark.localRotation = Quaternion.Euler(0f, 0f, spins[i] * time);
            SetSprite(sparks[i], Mathf.Lerp(0.26f, 0.1f, t), 1f - t);
        }
    }

    private static void SetSprite(SpriteRenderer renderer, float scale, float alpha)
    {
        renderer.transform.localScale = Vector3.one * scale;
        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }
}
