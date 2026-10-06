using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;
    [Tooltip("İsteğe bağlı. {0} yerine çeviri yazılır, ör. \"{0} 12\" -> \"SEVİYE 12\". Boşsa sadece çeviri.")]
    [SerializeField] private string format;

    private TMP_Text label;

    private void OnEnable()
    {
        label = GetComponent<TMP_Text>();
        Localization.LanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        Localization.LanguageChanged -= Refresh;
    }

    private void Refresh()
    {
        string text = Localization.Get(key);
        label.text = string.IsNullOrEmpty(format) ? text : string.Format(format, text);
    }
}
