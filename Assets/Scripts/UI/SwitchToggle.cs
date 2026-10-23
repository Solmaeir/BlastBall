using System;
using UnityEngine;
using UnityEngine.UI;

// Açık/kapalı anahtarı: Toggle'ı sarar, düğmeyi kaydırır ve rayı açık/kapalı görseline çevirir.
[RequireComponent(typeof(Toggle))]
public class SwitchToggle : MonoBehaviour
{
    [SerializeField] private Image track;
    [SerializeField] private Sprite onTrack;
    [SerializeField] private Sprite offTrack;
    [SerializeField] private RectTransform knob;
    [Tooltip("Düğmenin kapalı ve açık konumu (rayın içinde, canvas birimi).")]
    [SerializeField] private Vector2 offPosition;
    [SerializeField] private Vector2 onPosition;
    [SerializeField, Min(1f)] private float speed = 14f;

    private Toggle toggle;
    private float shown;

    public bool IsOn => toggle.isOn;

    // Yalnızca oyuncu dokunuşuyla değişince tetiklenir (SetWithoutNotify tetiklemez).
    public event Action<bool> ValueChanged;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggled);
    }

    private void OnEnable()
    {
        shown = toggle.isOn ? 1f : 0f;
        Refresh();
    }

    public void SetWithoutNotify(bool on)
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        toggle.SetIsOnWithoutNotify(on);
        shown = on ? 1f : 0f;
        Refresh();
    }

    private void OnToggled(bool on)
    {
        AudioManager.PlaySfx(SfxId.ButtonClick);
        ValueChanged?.Invoke(on);
    }

    private void Update()
    {
        float target = toggle.isOn ? 1f : 0f;
        if (Mathf.Approximately(shown, target)) return;

        shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * speed);
        Refresh();
    }

    private void Refresh()
    {
        knob.anchoredPosition = Vector2.Lerp(offPosition, onPosition, shown);
        track.sprite = toggle.isOn ? onTrack : offTrack;
    }
}
