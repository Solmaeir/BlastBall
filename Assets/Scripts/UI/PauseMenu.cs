using UnityEngine;

// Oyun içi duraklatma: pause butonu ayar panelini açar ve oyunu durdurur, panel kapanınca oyun devam eder.
// Ayarlar paneli (ses, dil, bilgi) Menu sahnesindekiyle aynı SettingsMenu bileşenini kullanır.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    private bool pausedByMenu;

    public bool IsOpen => panel != null && panel.activeSelf;

    public void OnPausePressed()
    {
        if (panel == null || IsOpen) return;

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.IsPlaying) return;

        gameManager.PauseGame();
        pausedByMenu = true;
        panel.SetActive(true);
    }

    public void OnClosePressed()
    {
        if (panel != null) panel.SetActive(false);
        Resume();
    }

    private void Update()
    {
        // Escape / Android geri tuşu: açıksa kapatır.
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) OnClosePressed();
    }

    private void OnDisable()
    {
        Resume();
    }

    private void Resume()
    {
        if (!pausedByMenu) return;

        pausedByMenu = false;
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }
}
