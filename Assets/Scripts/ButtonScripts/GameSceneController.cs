using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Menu";

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
