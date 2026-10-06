using UnityEngine;
using UnityEngine.UI;

// Menüdeki sürekli "boşta" animasyonları: süzülme, ölçek nabzı, saydamlık, sallanma.
// Zaman ölçeğinden bağımsız çalışır (oyun duraklasa da UI canlı kalır).
[DisallowMultipleComponent]
public class UIIdleAnimator : MonoBehaviour
{
    public enum Waveform
    {
        // İleri-geri yumuşak salınım
        Sine,
        // Baştan sona gidip başa dönen döngü (ör. genişleyip sönen halka)
        Sawtooth
    }

    [SerializeField, Min(0.05f)] private float period = 3f;
    [SerializeField, Range(0f, 1f)] private float phase;
    [SerializeField] private bool randomPhase;
    [SerializeField] private Waveform waveform = Waveform.Sine;

    [Header("Süzülme (canvas birimi)")]
    [SerializeField] private Vector2 floatAmplitude;

    [Header("Ölçek")]
    [SerializeField] private float scaleFrom = 1f;
    [SerializeField] private float scaleTo = 1f;

    [Header("Saydamlık")]
    [SerializeField] private bool animateAlpha;
    [SerializeField, Range(0f, 1f)] private float alphaFrom = 1f;
    [SerializeField, Range(0f, 1f)] private float alphaTo = 1f;

    [Header("Sallanma (derece)")]
    [SerializeField] private float rotationAmplitude;

    private RectTransform rectTransform;
    private Vector2 basePosition;
    private Vector3 baseScale;
    private Quaternion baseRotation;
    private CanvasGroup canvasGroup;
    private Graphic graphic;
    private float phaseOffset;
    private bool cached;

    private void Awake()
    {
        Cache();
    }

    private void Cache()
    {
        if (cached) return;
        cached = true;
        rectTransform = (RectTransform)transform;
        basePosition = rectTransform.anchoredPosition;
        baseScale = rectTransform.localScale;
        baseRotation = rectTransform.localRotation;
        canvasGroup = GetComponent<CanvasGroup>();
        graphic = GetComponent<Graphic>();
        phaseOffset = randomPhase ? Random.value : phase;
    }

    private void OnDisable()
    {
        if (!cached) return;
        rectTransform.anchoredPosition = basePosition;
        rectTransform.localScale = baseScale;
        rectTransform.localRotation = baseRotation;
        SetAlpha(1f);
    }

    private void Update()
    {
        Cache();
        float t = Mathf.Repeat(Time.unscaledTime / period + phaseOffset, 1f);
        float wave = Mathf.Sin(t * Mathf.PI * 2f);
        float blend = waveform == Waveform.Sine ? wave * 0.5f + 0.5f : t;

        if (floatAmplitude != Vector2.zero) rectTransform.anchoredPosition = basePosition + floatAmplitude * wave;
        if (!Mathf.Approximately(scaleFrom, scaleTo)) rectTransform.localScale = baseScale * Mathf.LerpUnclamped(scaleFrom, scaleTo, blend);
        if (rotationAmplitude != 0f) rectTransform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, rotationAmplitude * wave);
        if (animateAlpha) SetAlpha(Mathf.Lerp(alphaFrom, alphaTo, blend));
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null) canvasGroup.alpha = alpha;
        else if (graphic != null) graphic.canvasRenderer.SetAlpha(alpha);
    }
}
