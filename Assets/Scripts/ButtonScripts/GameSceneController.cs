using UnityEngine;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Menu";
    [Tooltip("Ayarlar panelindeki 'Ana Menü' butonuna basılınca menüye dönülür.")]
    [SerializeField] private SettingsMenu settings;

    private void OnEnable()
    {
        if (settings != null) settings.MainMenuPressed += OnMainMenuPressed;
    }

    private void OnDisable()
    {
        if (settings != null) settings.MainMenuPressed -= OnMainMenuPressed;
    }

    public void OnMainMenuPressed()
    {
        // Skoru sıfırla; sahne yeniden yüklendiğinde de sıfırdan başlar
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneLoader.Load(menuSceneName);
    }
}
