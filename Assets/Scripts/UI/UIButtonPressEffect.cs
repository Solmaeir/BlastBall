using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 3D butonlarda basılınca yüzeyi tabana doğru indirir ve hafifçe karartır. onClick davranışına dokunmaz.
[DisallowMultipleComponent]
public class UIButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Basılınca aşağı inecek yüzey (ikon ve yazılar bunun altında olmalı).")]
    [SerializeField] private RectTransform face;
    [Tooltip("Yüzey dışında karartılacak ek grafikler (ör. buton tabanı).")]
    [SerializeField] private Graphic[] extraTintTargets;
    [SerializeField] private float pressDepth = 10f;
    [SerializeField] private Color pressedTint = new Color(0.8f, 0.8f, 0.88f, 1f);
    [SerializeField] private float fadeDuration = 0.06f;

    private Selectable selectable;
    private Graphic[] faceGraphics;
    private Vector2 faceRestPosition;
    private bool initialized;
    private bool pointerDown;
    private bool pointerInside;
    private bool held;

    public RectTransform Face => face;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (initialized || face == null) return;
        initialized = true;
        selectable = GetComponent<Selectable>();
        faceGraphics = face.GetComponentsInChildren<Graphic>(true);
        faceRestPosition = face.anchoredPosition;
    }

    private void OnDisable()
    {
        pointerDown = false;
        pointerInside = false;
        Refresh(true);
    }

    // Aktif güçlendirici gibi kalıcı "basılı" görünüm için.
    public void SetHeld(bool value)
    {
        held = value;
        Refresh(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !IsInteractable()) return;
        pointerDown = true;
        pointerInside = true;
        Refresh(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        Refresh(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        if (pointerDown) Refresh(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        if (pointerDown) Refresh(false);
    }

    private bool IsInteractable()
    {
        return selectable == null || selectable.IsInteractable();
    }

    private void Refresh(bool instant)
    {
        Init();
        if (face == null) return;

        bool pressedByPointer = pointerDown && pointerInside;
        face.anchoredPosition = faceRestPosition + ((pressedByPointer || held) ? Vector2.down * pressDepth : Vector2.zero);

        Color tint = pressedByPointer ? pressedTint : Color.white;
        float duration = instant ? 0f : fadeDuration;
        foreach (Graphic graphic in faceGraphics) Tint(graphic, tint, duration);
        if (extraTintTargets != null)
        {
            foreach (Graphic graphic in extraTintTargets) Tint(graphic, tint, duration);
        }
    }

    private static void Tint(Graphic graphic, Color tint, float duration)
    {
        if (graphic != null) graphic.CrossFadeColor(tint, duration, true, false);
    }
}
