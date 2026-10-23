using UnityEngine;

// Oyun kodunun titreşim için tek giriş noktası: Haptics.Play(HapticType.Medium).
// Oyuncu ayarlardan titreşimi kapattıysa hiçbir şey yapmaz. Çok sık çağrıları sınırlar.
public static class Haptics
{
    private const float MinInterval = 0.04f;

    private static IHapticEngine engine;
    private static float lastPlayed = -1f;

    // Test veya özel platformlar için uygulamayı değiştirmeye izin verir.
    public static void SetEngine(IHapticEngine newEngine)
    {
        engine = newEngine;
    }

    public static void Play(HapticType type)
    {
        if (type == HapticType.None || !GameSettings.HapticsEnabled) return;

        float now = Time.unscaledTime;
        if (now - lastPlayed < MinInterval) return;
        lastPlayed = now;

        if (engine == null) engine = CreateEngine();
        engine.Play(type);
    }

    private static IHapticEngine CreateEngine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return new AndroidHapticEngine();
#elif UNITY_IOS && !UNITY_EDITOR
        return new IosHapticEngine();
#else
        return new NullHapticEngine();
#endif
    }

    // Editör ve titreşimi olmayan platformlar için.
    private sealed class NullHapticEngine : IHapticEngine
    {
        public void Play(HapticType type) { }
    }
}
