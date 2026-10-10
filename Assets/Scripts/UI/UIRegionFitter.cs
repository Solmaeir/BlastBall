using UnityEngine;

// Bir içerik grubunu bu RectTransform'un alanına (isteğe bağlı olarak iki sınır arasında kalan kısmına)
// sığdırır ve ortalar. İçerik sınırları editörde çocuklardan otomatik ölçülür; yeni eklenen öğeler de hesaba katılır.
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UIRegionFitter : MonoBehaviour
{
    [SerializeField] private RectTransform content;

    [Tooltip("Bölgenin üst sınırı bu objenin alt kenarı olur (ör. üst bar). Boşsa bölgenin kendi üstü.")]
    [SerializeField] private RectTransform topBound;
    [Tooltip("Bölgenin alt sınırı bu objenin üst kenarı olur (ör. alt bar). Boşsa bölgenin kendi altı.")]
    [SerializeField] private RectTransform bottomBound;

    [SerializeField] private Vector2 padding = new Vector2(0f, 12f);
    [Tooltip("İçerik tasarım boyutundan en fazla bu kadar büyütülür.")]
    [SerializeField, Min(0.1f)] private float maxScale = 1f;

    [Tooltip("Editörde içerik sınırlarını çocuklardan otomatik ölçer. Kapalıysa aşağıdaki değerler elle girilir.")]
    [SerializeField] private bool autoMeasureInEditor = true;
    [SerializeField] private Vector2 contentSize;
    [SerializeField] private Vector2 contentCenter;

    private RectTransform self;
    private readonly Vector3[] corners = new Vector3[4];

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && autoMeasureInEditor) Measure();
#endif
        Fit();
    }

    [ContextMenu("İçerik sınırlarını şimdi ölç")]
    private void Measure()
    {
        if (content == null) return;

        Vector3 scale = content.localScale;
        content.localScale = Vector3.one;
        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(content);
        content.localScale = scale;

        Vector2 size = b.size;
        Vector2 center = b.center;
        if ((size - contentSize).sqrMagnitude > 0.01f || (center - contentCenter).sqrMagnitude > 0.01f)
        {
            contentSize = size;
            contentCenter = center;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    private void Fit()
    {
        if (content == null || contentSize.x <= 0f || contentSize.y <= 0f) return;
        if (self == null) self = (RectTransform)transform;

        Rect r = self.rect;
        float top = r.yMax;
        float bottom = r.yMin;
        if (topBound != null) top = Mathf.Min(top, Edge(topBound, false));
        if (bottomBound != null) bottom = Mathf.Max(bottom, Edge(bottomBound, true));
        top -= padding.y;
        bottom += padding.y;

        float height = top - bottom;
        float width = r.width - 2f * padding.x;
        if (height <= 1f || width <= 1f) return;

        float s = Mathf.Min(maxScale, height / contentSize.y, width / contentSize.x);
        Vector2 regionCenter = new Vector2(r.center.x, (top + bottom) * 0.5f);
        Vector3 position = regionCenter - contentCenter * s;
        position.z = content.localPosition.z;

        if (!Mathf.Approximately(content.localScale.x, s)) content.localScale = new Vector3(s, s, 1f);
        if ((content.localPosition - position).sqrMagnitude > 0.0001f) content.localPosition = position;
    }

    // Sınır objesinin alt (useTop=false) veya üst (useTop=true) kenarının bu objenin yerel uzayındaki y'si.
    private float Edge(RectTransform bound, bool useTop)
    {
        bound.GetWorldCorners(corners);
        float min = float.MaxValue, max = float.MinValue;
        foreach (Vector3 corner in corners)
        {
            float y = self.InverseTransformPoint(corner).y;
            min = Mathf.Min(min, y);
            max = Mathf.Max(max, y);
        }
        return useTop ? max : min;
    }
}
