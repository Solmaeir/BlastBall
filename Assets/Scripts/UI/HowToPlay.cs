using UnityEngine;

// "Nasıl Oynanır" penceresini açar: oyuncunun ilk girişinde kendiliğinden, sonra ayarlardaki butondan.
// Menü ve oyun sahnesinde aynı bileşen kullanılır; ilk giriş kontrolü yalnızca menüde açıktır.
public class HowToPlay : MonoBehaviour
{
    private const string SeenKey = "HowToPlaySeen";

    [SerializeField] private InfoPopup popup;
    [SerializeField] private SettingsMenu settings;
    [SerializeField] private PopupContent content = new PopupContent { titleKey = "how_to_play", bodyKey = "how_to_body" };
    [Tooltip("Açıksa oyuncu pencereyi daha önce görmediyse sahne açılınca gösterilir.")]
    [SerializeField] private bool showOnFirstLaunch;

    private void OnEnable()
    {
        if (settings != null) settings.HowToPlayPressed += Show;
    }

    private void OnDisable()
    {
        if (settings != null) settings.HowToPlayPressed -= Show;
    }

    private void Start()
    {
        if (!showOnFirstLaunch || PlayerPrefs.GetInt(SeenKey, 0) == 1) return;

        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();
        Show();
    }

    public void Show()
    {
        popup.Show(content);
    }
}
