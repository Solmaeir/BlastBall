using UnityEngine;

// Splash sahnesindeki tek component. Oyun açılır açılmaz loading ekranını gösterir
// ve ana menüyü arka planda yükler.
public class SplashBootstrap : MonoBehaviour
{
    [SerializeField] private string firstSceneName = SceneLoader.MenuScene;
    [SerializeField] private float minDuration = 2.5f;

    private void Start()
    {
        SceneLoader.LoadFromSplash(firstSceneName, minDuration);
    }
}
