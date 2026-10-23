using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private SettingsMenu settings;
    [SerializeField] private string gameSceneName = "SampleScene";

    public void OnPlayPressed()
    {
        SceneLoader.Load(gameSceneName);
    }

    public void OnSettingsPressed()
    {
        settings.Open();
    }
}
