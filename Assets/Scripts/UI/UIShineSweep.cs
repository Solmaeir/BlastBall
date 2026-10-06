using UnityEngine;

// Bir parlama bandını bu RectTransform'un üzerinden periyodik olarak soldan sağa geçirir.
// Bant, bu objedeki Mask/RectMask2D ile kırpılmalıdır.
public class UIShineSweep : MonoBehaviour
{
    [SerializeField] private RectTransform shine;
    [SerializeField, Min(0.05f)] private float sweepDuration = 0.8f;
    [SerializeField, Min(0f)] private float interval = 2.1f;
    [SerializeField, Min(0f)] private float startDelay = 0.6f;

    private RectTransform area;
    private float timer;

    private void OnEnable()
    {
        area = (RectTransform)transform;
        timer = -startDelay;
        Place(1f);
    }

    private void Update()
    {
        if (shine == null) return;
        timer += Time.unscaledDeltaTime;
        if (timer < 0f) return;

        float cycle = sweepDuration + interval;
        float progress = Mathf.Repeat(timer, cycle) / sweepDuration;
        Place(Mathf.Min(progress, 1f));
    }

    private void Place(float progress)
    {
        if (shine == null || area == null) return;
        float half = area.rect.width * 0.5f + shine.rect.width;
        float eased = progress * progress * (3f - 2f * progress);
        shine.anchoredPosition = new Vector2(Mathf.Lerp(-half, half, eased), shine.anchoredPosition.y);
    }
}
