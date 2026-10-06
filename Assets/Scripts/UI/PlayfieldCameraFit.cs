using UnityEngine;

// Kamerayı, oyun alanı (dünya uzayındaki kutu) üst ve alt HUD panellerinin arasına sığacak şekilde ayarlar.
// Böylece her ekran oranında kutu panellerin altında kalmaz; fizik ve oyun alanı boyutu değişmez.
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class PlayfieldCameraFit : MonoBehaviour
{
    [SerializeField] private Renderer playfield;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [Tooltip("Kutu ile üst panel arasındaki boşluk (canvas birimi).")]
    [SerializeField] private float marginTop = 27f;
    [Tooltip("Kutu ile alt panel arasındaki boşluk (canvas birimi).")]
    [SerializeField] private float marginBottom = 30f;
    [Tooltip("Kutunun sağ ve sol ekran kenarına en az uzaklığı (canvas birimi).")]
    [SerializeField] private float minSideMargin = 24f;

    private Camera cam;
    private readonly Vector3[] corners = new Vector3[4];

    private void LateUpdate()
    {
        Fit();
    }

    private void Fit()
    {
        if (playfield == null || canvasRect == null || topBar == null || bottomBar == null) return;
        if (cam == null) cam = GetComponent<Camera>();

        Rect canvas = canvasRect.rect;
        if (canvas.height <= 0f || canvas.width <= 0f) return;

        float topEdge = EdgeFromCanvasBottom(topBar, false) - marginTop;
        float bottomEdge = EdgeFromCanvasBottom(bottomBar, true) + marginBottom;
        float available = (topEdge - bottomEdge) / canvas.height;
        if (available <= 0.05f) return;

        Bounds bounds = playfield.bounds;
        float sizeByHeight = bounds.size.y / (2f * available);
        float usableWidth = 1f - 2f * minSideMargin / canvas.width;
        float sizeByWidth = bounds.size.x / (2f * cam.aspect * Mathf.Max(0.1f, usableWidth));
        float size = Mathf.Max(sizeByHeight, sizeByWidth);

        float centerFraction = (topEdge + bottomEdge) * 0.5f / canvas.height;
        Vector3 position = transform.position;
        position.x = bounds.center.x;
        position.y = bounds.center.y - (centerFraction - 0.5f) * 2f * size;

        if (!Mathf.Approximately(cam.orthographicSize, size)) cam.orthographicSize = size;
        if ((transform.position - position).sqrMagnitude > 1e-8f) transform.position = position;
    }

    // Panelin alt (top=false) veya üst (top=true) kenarının canvas altından yüksekliği.
    private float EdgeFromCanvasBottom(RectTransform bar, bool top)
    {
        bar.GetWorldCorners(corners);
        float min = float.MaxValue, max = float.MinValue;
        foreach (Vector3 corner in corners)
        {
            float y = canvasRect.InverseTransformPoint(corner).y;
            min = Mathf.Min(min, y);
            max = Mathf.Max(max, y);
        }
        float local = top ? max : min;
        return local - canvasRect.rect.yMin;
    }
}
