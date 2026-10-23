using UnityEngine;

// Oyun içi duraklatma: pause butonu ayarlar panelini açar ve oyunu durdurur, panel kapanınca oyun devam eder.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private SettingsMenu settings;

    private bool pausedByMenu;

    public bool IsOpen => settings != null && settings.IsOpen;

    private void OnEnable()
    {
        settings.Closed += Resume;
    }

    private void OnDisable()
    {
        settings.Closed -= Resume;
        Resume();
    }

    public void OnPausePressed()
    {
        if (IsOpen) return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.IsPlaying) return;

        gameManager.PauseGame();
        pausedByMenu = true;
        settings.Open();
    }

    private void Resume()
    {
        if (!pausedByMenu) return;

        pausedByMenu = false;
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }
}
