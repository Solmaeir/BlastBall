using UnityEngine;

// Tam ekran filtre (ör. buz kenarlığı + mavi ton). Yumuşakça açılıp kapanır, uyarı modunda yanıp söner.
// Zaman ölçeğinden bağımsız çalışır.
[RequireComponent(typeof(CanvasGroup))]
public class ScreenOverlay : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float fadeSpeed = 4f;
    [SerializeField, Min(1f)] private float blinkSpeed = 9f;

    private CanvasGroup group;
    private float shown;
    private float target;
    private bool warning;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        target = 1f;
    }

    public void Hide()
    {
        target = 0f;
        warning = false;
    }

    public void SetWarning(bool value)
    {
        warning = value;
    }

    private void Update()
    {
        shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * fadeSpeed);
        float blink = warning ? 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * blinkSpeed)) : 1f;
        group.alpha = shown * blink;

        if (shown <= 0f && target <= 0f) gameObject.SetActive(false);
    }
}
