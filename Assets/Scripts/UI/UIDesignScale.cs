using UnityEngine;

// Belirli bir tasarım boyutunda (referenceSize) hazırlanmış içeriği, bulunduğu alana (ebeveyn) orantılı sığdırır.
// Aynı prefab'ın farklı canvas birimlerine sahip sahnelerde (menü / oyun) aynı fiziksel boyutta görünmesini sağlar.
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UIDesignScale : MonoBehaviour
{
    [SerializeField] private Vector2 referenceSize = new Vector2(1080f, 2113f);

    private RectTransform self;

    private void OnEnable()
    {
        Apply();
    }

    private void LateUpdate()
    {
        Apply();
    }

    private void Apply()
    {
        if (self == null) self = (RectTransform)transform;
        var parent = self.parent as RectTransform;
        if (parent == null) return;

        Vector2 available = parent.rect.size;
        if (available.x <= 0f || available.y <= 0f) return;

        float scale = Mathf.Min(available.x / referenceSize.x, available.y / referenceSize.y);
        self.sizeDelta = referenceSize;
        if (!Mathf.Approximately(self.localScale.x, scale)) self.localScale = new Vector3(scale, scale, 1f);
    }
}
