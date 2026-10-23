using System;
using UnityEngine;
using UnityEngine.UI;

// Ayarlar paneli (prefab: Assets/UI/SettingsPanel/Prefabs/SettingsPanel.prefab). Menü ve oyun sahnesi aynı prefab'ı kullanır.
// Sayfalar: ana ayarlar, dil, bilgi. Tercihler GameSettings'e yazılır; sesi ve titreşimi AudioManager / Haptics uygular.
public class SettingsMenu : MonoBehaviour
{
    [Header("Sayfalar")]
    [SerializeField] private GameObject mainPage;
    [SerializeField] private GameObject languagePage;
    [SerializeField] private GameObject infoPage;

    [Header("Ana Sayfa")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private SwitchToggle soundToggle;
    [SerializeField] private SwitchToggle hapticsToggle;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button languageButton;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button closeButton;
    [Tooltip("Panelin altındaki 'Ana Menü' butonu. Yalnızca oyun sahnesinde gösterilir.")]
    [SerializeField] private GameObject mainMenuButtonObject;
    [SerializeField] private Button mainMenuButton;

    [Header("Dil Sayfası")]
    [SerializeField] private Button turkishButton;
    [SerializeField] private Button englishButton;
    [SerializeField] private Button languageBackButton;

    [Header("Bilgi Sayfası")]
    [SerializeField] private Button infoBackButton;

    [Header("Davranış")]
    [Tooltip("Açıksa 'Ana Menü' butonu gösterilir (oyun sahnesi için).")]
    [SerializeField] private bool showMainMenuButton;
    [SerializeField, Min(1f)] private float fadeSpeed = 9f;

    private CanvasGroup group;
    private bool initialized;

    public event Action Closed;
    public event Action MainMenuPressed;
    public event Action HowToPlayPressed;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;
        initialized = true;

        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();

        musicSlider.onValueChanged.AddListener(value => GameSettings.MusicVolume = value);
        soundToggle.ValueChanged += on => GameSettings.SoundEnabled = on;
        hapticsToggle.ValueChanged += OnHapticsToggled;

        howToPlayButton.onClick.AddListener(() => HowToPlayPressed?.Invoke());
        languageButton.onClick.AddListener(() => ShowPage(languagePage));
        infoButton.onClick.AddListener(() => ShowPage(infoPage));
        languageBackButton.onClick.AddListener(() => ShowPage(mainPage));
        infoBackButton.onClick.AddListener(() => ShowPage(mainPage));
        closeButton.onClick.AddListener(Close);
        mainMenuButton.onClick.AddListener(() => MainMenuPressed?.Invoke());
        turkishButton.onClick.AddListener(() => Localization.Current = Language.Turkish);
        englishButton.onClick.AddListener(() => Localization.Current = Language.English);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Initialize();

        musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
        soundToggle.SetWithoutNotify(GameSettings.SoundEnabled);
        hapticsToggle.SetWithoutNotify(GameSettings.HapticsEnabled);
        mainMenuButtonObject.SetActive(showMainMenuButton);
        ShowPage(mainPage);

        group.alpha = 0f;
    }

    private void OnDisable()
    {
        GameSettings.Save();
        Closed?.Invoke();
    }

    private void Update()
    {
        if (group.alpha < 1f) group.alpha = Mathf.MoveTowards(group.alpha, 1f, Time.unscaledDeltaTime * fadeSpeed);

        // Escape / Android geri tuşu: alt sayfadaysa ana sayfaya, ana sayfadaysa paneli kapatır.
        if (InfoPopup.BlocksBackButton || !BackButton.WasPressed()) return;

        if (mainPage.activeSelf) Close();
        else ShowPage(mainPage);
    }

    private void ShowPage(GameObject page)
    {
        mainPage.SetActive(page == mainPage);
        languagePage.SetActive(page == languagePage);
        infoPage.SetActive(page == infoPage);
    }

    private static void OnHapticsToggled(bool on)
    {
        GameSettings.HapticsEnabled = on;
        if (on) Haptics.Play(HapticType.Medium);
    }
}
