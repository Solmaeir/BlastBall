using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BoosterVisualState
{
    Ready,
    Active,
    Empty
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

    [Header("Aktif Durum")]
    [SerializeField] private Image glow;
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField] private Image ring;
    [SerializeField] private Image ringTrack;
    [SerializeField] private UIButtonPressEffect pressEffect;

    public BoosterVisualState State => state;
    public int Count => count;

    public void SetState(BoosterVisualState newState)
    {
        state = newState;
        Apply();
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
        bool empty = state == BoosterVisualState.Empty;
        bool active = state == BoosterVisualState.Active;

        if (face != null) face.sprite = empty ? faceEmptySprite : faceSprite;
        if (baseImage != null) baseImage.sprite = empty ? baseEmptySprite : baseSprite;
        if (icon != null) icon.sprite = empty ? iconEmptySprite : iconSprite;

        if (countText != null) countText.text = count.ToString();
        SetActive(countBadge, state == BoosterVisualState.Ready && count > 0);
        SetActive(plusBadge, empty);
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
