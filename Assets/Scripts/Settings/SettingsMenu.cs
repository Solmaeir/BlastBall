using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private const string VolumeKey = "Volume";
    private const string MutedKey = "Muted";

    [Header("Paneller")]
    [SerializeField] private GameObject settingsContent;
    [SerializeField] private GameObject languagePanel;
    [SerializeField] private GameObject infoPanel;

    [Header("Ayarlar Menüsü")]
    [SerializeField] private Button infoButton;
    [SerializeField] private Button languageButton;
    [SerializeField] private Button muteButton;
    [SerializeField] private Image muteIcon;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    [SerializeField] private Slider volumeSlider;

    [Header("Dil Menüsü")]
    [SerializeField] private Button turkishButton;
    [SerializeField] private Button englishButton;
    [SerializeField] private Button languageBackButton;

    [Header("Bilgi Menüsü")]
    [SerializeField] private Button infoBackButton;

    private bool muted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedAudio()
    {
        bool savedMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        AudioListener.volume = savedMuted ? 0f : PlayerPrefs.GetFloat(VolumeKey, 1f);
    }

    private void Awake()
    {
        muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        volumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(VolumeKey, 1f));

        volumeSlider.onValueChanged.AddListener(SetVolume);
        muteButton.onClick.AddListener(ToggleMute);
        languageButton.onClick.AddListener(() => Show(languagePanel));
        infoButton.onClick.AddListener(() => Show(infoPanel));
        languageBackButton.onClick.AddListener(ShowSettings);
        infoBackButton.onClick.AddListener(ShowSettings);
        turkishButton.onClick.AddListener(() => Localization.Current = Language.Turkish);
        englishButton.onClick.AddListener(() => Localization.Current = Language.English);
    }

    private void OnEnable()
    {
        RefreshMuteIcon();
    }

    private void Start()
    {
        ShowSettings();
    }

    private void Show(GameObject panel)
    {
        settingsContent.SetActive(false);
        languagePanel.SetActive(panel == languagePanel);
        infoPanel.SetActive(panel == infoPanel);
    }

    private void ShowSettings()
    {
        settingsContent.SetActive(true);
        languagePanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    private void SetVolume(float value)
    {
        PlayerPrefs.SetFloat(VolumeKey, value);
        ApplyAudio();
    }

    private void ToggleMute()
    {
        muted = !muted;
        PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
        ApplyAudio();
        RefreshMuteIcon();
    }

    private void ApplyAudio()
    {
        AudioListener.volume = muted ? 0f : volumeSlider.value;
    }

    private void RefreshMuteIcon()
    {
        muteIcon.sprite = muted ? soundOffSprite : soundOnSprite;
    }
}
