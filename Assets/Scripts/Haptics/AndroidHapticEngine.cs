#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

// Android: Vibrator servisiyle kısa, şiddeti ayarlı titreşim (Handheld.Vibrate sabit ve uzundur).
// Handheld.Vibrate referansı derleme sırasında VIBRATE iznini manifest'e eklettirir; ayrıca eski cihazlar için yedektir.
public sealed class AndroidHapticEngine : IHapticEngine
{
    private readonly AndroidJavaObject vibrator;
    private readonly AndroidJavaClass effectClass;
    private readonly int sdk;
    private readonly bool hasAmplitudeControl;

    public AndroidHapticEngine()
    {
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                sdk = version.GetStatic<int>("SDK_INT");
            }

            if (sdk >= 26)
            {
                effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                hasAmplitudeControl = vibrator != null && vibrator.Call<bool>("hasAmplitudeControl");
            }
        }
        catch (AndroidJavaException)
        {
            vibrator = null;
        }
    }

    public void Play(HapticType type)
    {
        if (vibrator == null)
        {
            Handheld.Vibrate();
            return;
        }

        if (!vibrator.Call<bool>("hasVibrator")) return;

        if (sdk < 26)
        {
            vibrator.Call("vibrate", Duration(type));
            return;
        }

        using (AndroidJavaObject effect = CreateEffect(type))
        {
            vibrator.Call("vibrate", effect);
        }
    }

    private AndroidJavaObject CreateEffect(HapticType type)
    {
        if (type == HapticType.Success)
        {
            long[] timings = { 0, 18, 55, 28 };
            int[] amplitudes = hasAmplitudeControl ? new[] { 0, 120, 0, 200 } : new[] { 0, 255, 0, 255 };
            return effectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, amplitudes, -1);
        }

        return effectClass.CallStatic<AndroidJavaObject>("createOneShot", Duration(type), hasAmplitudeControl ? Amplitude(type) : -1);
    }

    private static long Duration(HapticType type)
    {
        switch (type)
        {
            case HapticType.Light: return 15;
            case HapticType.Medium: return 28;
            default: return 55;
        }
    }

    private static int Amplitude(HapticType type)
    {
        switch (type)
        {
            case HapticType.Light: return 70;
            case HapticType.Medium: return 150;
            default: return 255;
        }
    }
}
#endif
