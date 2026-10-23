#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;

// iOS: Taptic Engine. Native tarafı Assets/Plugins/iOS/BlastBallHaptics.mm içindedir.
public sealed class IosHapticEngine : IHapticEngine
{
    [DllImport("__Internal")]
    private static extern void BlastBall_Haptic(int type);

    public void Play(HapticType type)
    {
        BlastBall_Haptic((int)type);
    }
}
#endif
