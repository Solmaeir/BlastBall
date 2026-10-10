using UnityEngine;

// Mobilde Unity varsayılan olarak 30 FPS'te çalışır. Oyun açılırken (sahneye obje gerektirmeden) 60 FPS hedeflenir.
public static class FrameRateSetup
{
    private const int TargetFrameRate = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;
    }
}
