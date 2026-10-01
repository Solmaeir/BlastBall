using UnityEngine;
using UnityEngine.UI;

// Arka plan görselini bozmadan (aspect korunarak) tüm ekranı/kamera alanını kaplayacak şekilde ölçekler.
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class BackgroundFit : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image image;
    private Vector2 lastParentSize;
    private Sprite lastSprite;

    private void OnEnable()
    {
        Fit();
    }

    private void LateUpdate()
    {
        Fit();
    }

    private void Fit()
    {
        if (rectTransform == null)
        {
            rectTransform = (RectTransform)transform;
            image = GetComponent<Image>();
        }

        RectTransform parent = rectTransform.parent as RectTransform;
        if (parent == null || image.sprite == null)
        {
            return;
        }

        Vector2 parentSize = parent.rect.size;
        if (parentSize == lastParentSize && image.sprite == lastSprite)
        {
            return;
        }

        if (parentSize.x <= 0f || parentSize.y <= 0f)
        {
            return;
        }

        lastParentSize = parentSize;
        lastSprite = image.sprite;

        Rect spriteRect = image.sprite.rect;
        float spriteAspect = spriteRect.width / spriteRect.height;
        float parentAspect = parentSize.x / parentSize.y;

        Vector2 size = parentAspect > spriteAspect
            ? new Vector2(parentSize.x, parentSize.x / spriteAspect)
            : new Vector2(parentSize.y * spriteAspect, parentSize.y);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;
    }
}
