using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BoosterVisualState
{
    Ready,
    Active,
    Empty,
    // Seviye henüz açılmadı
    Locked
}

public enum BoosterActiveStyle
{
    // Basılı + kalan süreyi gösteren halka (Dondur)
    Timer,
    // Basılı + parlak çerçeve (Bomba)
    Highlight,
    // Çerçeve + onay rozeti (Kalkan)
    Equipped
}

// Güçlendirici butonunun yalnızca görsel durumlarını yönetir; oyun mekaniği burada değil.
public class BoosterButtonView : MonoBehaviour
{
    [Header("Durum")]
    [SerializeField] private BoosterVisualState state = BoosterVisualState.Ready;
    [SerializeField] private BoosterActiveStyle activeStyle = BoosterActiveStyle.Highlight;
    [SerializeField, Min(0)] private int count = 1;
    [SerializeField, Range(0f, 1f)] private float timer = 1f;

    [Header("Görseller")]
    [SerializeField] private Image face;
    [SerializeField] private Image baseImage;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite faceSprite;
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite iconSprite;
    [SerializeField] private Sprite faceEmptySprite;
    [SerializeField] private Sprite baseEmptySprite;
    [SerializeField] private Sprite iconEmptySprite;

    [Header("Rozetler")]
    [SerializeField] private GameObject countBadge;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject plusBadge;
    [SerializeField] private GameObject checkBadge;
    [SerializeField] private GameObject lockBadge;

    [Header("Aktif Durum")]
    [SerializeField] private Image glow;
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField] private Image ring;
    [SerializeField] private Image ringTrack;
    [SerializeField] private UIButtonPressEffect pressEffect;

    [Header("Tanımdan Görünüm")]
    [Tooltip("İkon sprite'ının piksel boyutu bu katsayıyla canvas birimine çevrilir.")]
    [SerializeField, Min(0.01f)] private float iconScale = 0.449f;

    public BoosterVisualState State => state;
    public int Count => count;

    // Görünümü (renk, ikon, parlama rengi, aktif stili, etiket) güçlendirici tanımından alır.
    public void Bind(PowerUpDefinition definition)
    {
        faceSprite = definition.faceSprite;
        baseSprite = definition.baseSprite;
        iconSprite = definition.icon;
        iconEmptySprite = definition.iconDisabled;
        glowColor = definition.glowColor;
        activeStyle = definition.activeStyle;

        if (icon != null && definition.icon != null) icon.rectTransform.sizeDelta = definition.icon.rect.size * iconScale;

        LocalizedText label = GetComponentInChildren<LocalizedText>(true);
        if (label != null) label.SetKey(definition.labelKey);

        Apply();
    }

    public void SetState(BoosterVisualState newState)
    {
        state = newState;
        Apply();
    }

    // Kilit açılınca butonu kısa süre büyütüp eski haline getirir.
    public void PlayUnlock()
    {
        StopAllCoroutines();
        StartCoroutine(UnlockPop());
    }

    private System.Collections.IEnumerator UnlockPop()
    {
        const float duration = 0.4f;
        Vector3 rest = Vector3.one;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            transform.localScale = rest * (1f + 0.22f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }
        transform.localScale = rest;
    }

    public void SetCount(int newCount)
    {
        count = Mathf.Max(0, newCount);
        Apply();
    }

    public void SetTimer(float normalized)
    {
        timer = Mathf.Clamp01(normalized);
        if (ring != null && activeStyle == BoosterActiveStyle.Timer) ring.fillAmount = timer;
    }

    private void OnEnable()
    {
        Apply();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) Apply();
        };
    }
#endif

    private void Apply()
    {
        bool locked = state == BoosterVisualState.Locked;
        bool empty = state == BoosterVisualState.Empty || locked;
        bool active = state == BoosterVisualState.Active;

        if (face != null) face.sprite = empty ? faceEmptySprite : faceSprite;
        if (baseImage != null) baseImage.sprite = empty ? baseEmptySprite : baseSprite;
        if (icon != null) icon.sprite = empty ? iconEmptySprite : iconSprite;

        if (countText != null) countText.text = count.ToString();
        SetActive(countBadge, state == BoosterVisualState.Ready && count > 0);
        SetActive(plusBadge, state == BoosterVisualState.Empty);
        SetActive(lockBadge, locked);
        SetActive(checkBadge, active && activeStyle == BoosterActiveStyle.Equipped);

        if (glow != null) glow.color = glowColor;
        SetActive(glow, active);
        SetActive(ring, active);
        SetActive(ringTrack, active && activeStyle == BoosterActiveStyle.Timer);
        if (ring != null) ring.fillAmount = activeStyle == BoosterActiveStyle.Timer ? timer : 1f;

        if (Application.isPlaying && pressEffect != null)
        {
            pressEffect.SetHeld(active && activeStyle != BoosterActiveStyle.Equipped);
        }
    }

    private static void SetActive(Component component, bool value)
    {
        if (component != null) SetActive(component.gameObject, value);
    }

    private static void SetActive(GameObject target, bool value)
    {
        if (target != null && target.activeSelf != value) target.SetActive(value);
    }
}
