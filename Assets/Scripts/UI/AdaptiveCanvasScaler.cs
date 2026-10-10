using UnityEngine;
using UnityEngine.UI;

// Tasarımın güvenli alan kutusunu (referenceSafeSize) her cihazda güvenli alana sığacak şekilde ölçekler.
// Hangisi daha dar ise (genişlik ya da yükseklik) ona göre ölçeklediği için telefon ve tablette taşma olmaz;
// fazla alan kenarlara/bölgelere dağılır. CanvasScaler'ın kendi modunun yerine geçer.
[ExecuteAlways]
[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(CanvasScaler))]
public class AdaptiveCanvasScaler : MonoBehaviour
{
    [Tooltip("Tasarımın güvenli alanının canvas birimindeki boyutu. İçerik bu kutuya göre yerleştirilmelidir.")]
    [SerializeField] private Vector2 referenceSafeSize = new Vector2(1080f, 2113f);

    private CanvasScaler scaler;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void OnEnable()
    {
        scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        Apply();
    }

    private void Update()
    {
        if (Screen.safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            Apply();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        lastScreenSize = Vector2Int.zero;
    }
#endif

    private void Apply()
    {
        Rect safe = Screen.safeArea;
        if (safe.width <= 0f || safe.height <= 0f || referenceSafeSize.x <= 0f || referenceSafeSize.y <= 0f) return;

        lastSafeArea = safe;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        float scale = Mathf.Min(safe.width / referenceSafeSize.x, safe.height / referenceSafeSize.y);
        if (!Mathf.Approximately(scaler.scaleFactor, scale)) scaler.scaleFactor = scale;
    }
}
