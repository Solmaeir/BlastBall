using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private string gameSceneName = "SampleScene";

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettingsPressed()
    {
        settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsPressed()
    {
        settingsPanel.SetActive(false);
    }
}