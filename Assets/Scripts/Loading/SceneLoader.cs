using UnityEngine;

// Sahne geçişleri için tek giriş noktası. Geçişi LoadingScreen üzerinden yapar:
// loading ekranı açılır, hedef sahne arkada yüklenir, sahne tam hazır olunca ekran kapanır.
public static class SceneLoader
{
    public const string MenuScene = "Menu";
    public const string GameScene = "SampleScene";

    private const float DefaultMinDuration = 1.2f;

    public static bool IsLoading => LoadingScreen.Instance != null && LoadingScreen.Instance.IsBusy;

    public static void Load(string sceneName, float minDuration = DefaultMinDuration)
    {
        LoadingScreen.GetOrCreate().Begin(sceneName, minDuration, startVisible: false);
    }

    // Açılış ekranı: loading ekranı ilk karede hazır görünür, fade-in yapılmaz.
    public static void LoadFromSplash(string sceneName, float minDuration)
    {
        LoadingScreen.GetOrCreate().Begin(sceneName, minDuration, startVisible: true);
    }
}
