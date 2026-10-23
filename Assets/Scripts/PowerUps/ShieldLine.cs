using UnityEngine;

// Kalkanın oyun alanı içindeki yarı saydam çizgisi. Kurulunca nabız atar, kırılınca parlayıp dağılır.
public class ShieldLine : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bar;
    [SerializeField] private SpriteRenderer[] sparks;
    [SerializeField, Min(0.02f)] private float thickness = 0.26f;
    [SerializeField, Min(0.1f)] private float breakDuration = 0.5f;

    private enum State { Hidden, Armed, Breaking }

    private State state;
    private float time;
    private float baseWidth;

    public void Show(Bounds field, float topEdgeY)
    {
        baseWidth = field.size.x;
        transform.position = new Vector3(field.center.x, topEdgeY - thickness * 0.5f - 0.02f, transform.position.z);
        bar.size = new Vector2(baseWidth, thickness);
        bar.color = Color.white;
        foreach (SpriteRenderer spark in sparks) spark.gameObject.SetActive(false);

        state = State.Armed;
        time = 0f;
        gameObject.SetActive(true);
    }

    public void Break()
    {
        state = State.Breaking;
        time = 0f;
        foreach (SpriteRenderer spark in sparks) spark.gameObject.SetActive(true);
    }

    private void Update()
    {
        time += Time.unscaledDeltaTime;

        if (state == State.Armed)
        {
            SetAlpha(bar, 0.72f + 0.28f * Mathf.Sin(time * 4f));
            return;
        }

        if (state != State.Breaking) return;

        float t = time / breakDuration;
        if (t >= 1f)
        {
            state = State.Hidden;
            gameObject.SetActive(false);
            return;
        }

        bar.size = new Vector2(baseWidth, thickness * Mathf.Lerp(1f, 2.6f, t));
        SetAlpha(bar, (1f - t) * (1f - t));

        for (int i = 0; i < sparks.Length; i++)
        {
            float side = (i % 2 == 0 ? -1f : 1f);
            float spread = (i / 2 + 1) / (sparks.Length / 2f + 1f);
            sparks[i].transform.localPosition = new Vector3(side * baseWidth * 0.5f * spread * t * 1.2f, Mathf.Sin(t * Mathf.PI) * 0.35f * (i % 3 - 1), 0f);
            sparks[i].transform.localRotation = Quaternion.Euler(0f, 0f, 220f * t * side);
            sparks[i].transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 0.1f, t);
            SetAlpha(sparks[i], 1f - t);
        }
    }

    private static void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        Color color = renderer.color;
        color.a = Mathf.Clamp01(alpha);
        renderer.color = color;
    }
}
