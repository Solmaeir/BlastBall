using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Tools > BlastBall > Create UIPanel Prefab
// UIPanel prefab'ını Assets/Prefabs/UIPanel.prefab olarak üretir. Görsel yer tutucudur:
// sprite'ları ve renkleri prefab üzerinden değiştirebilirsin, mekanizma UIPanel.cs'tedir.
public static class UIPanelPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefabs/UIPanel.prefab";
    private const string SpriteFolder = "Assets/UI/Settings/";

    [MenuItem("Tools/BlastBall/Create UIPanel Prefab")]
    private static void Create()
    {
        if (File.Exists(PrefabPath) &&
            !EditorUtility.DisplayDialog("UIPanel", "UIPanel.prefab zaten var. Üzerine yazılsın mı?", "Üzerine yaz", "İptal"))
        {
            return;
        }

        GameObject root = Build();
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Debug.Log("UIPanel prefab'ı oluşturuldu: " + PrefabPath);
    }

    private static GameObject Build()
    {
        Sprite card = LoadSprite("ui_card");
        Sprite close = LoadSprite("ui_close");
        Sprite blue = LoadSprite("ui_pill_blue");
        Sprite pink = LoadSprite("ui_pill_pink");

        // Kök: tam ekran, kendi CanvasGroup'u ile animasyon ve tıklama engelleme yapar.
        RectTransform root = NewRect("UIPanel", null);
        Stretch(root);
        root.gameObject.AddComponent<CanvasGroup>();
        UIPanel panel = root.gameObject.AddComponent<UIPanel>();

        // Arka plan: karartır, dışarı tıklamayı yakalar.
        RectTransform backdrop = NewRect("Backdrop", root);
        Stretch(backdrop);
        AddImage(backdrop, null, new Color(0f, 0f, 0f, 0.65f));
        Button backdropButton = backdrop.gameObject.AddComponent<Button>();
        backdropButton.transition = Selectable.Transition.None;

        // Pencere: içeriğe göre dikey olarak kendini boyutlandırır.
        RectTransform window = NewRect("Window", root);
        window.anchorMin = new Vector2(0.08f, 0.5f);
        window.anchorMax = new Vector2(0.92f, 0.5f);
        window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = Vector2.zero;
        AddImage(window, card, new Color(0.18f, 0.2f, 0.32f, 1f));

        VerticalLayoutGroup windowLayout = window.gameObject.AddComponent<VerticalLayoutGroup>();
        windowLayout.padding = new RectOffset(60, 60, 70, 60);
        windowLayout.spacing = 30f;
        windowLayout.childAlignment = TextAnchor.UpperCenter;
        windowLayout.childControlWidth = true;
        windowLayout.childControlHeight = true;
        windowLayout.childForceExpandWidth = true;
        windowLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = window.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI title = AddText(NewRect("Title", window), "Başlık", 56f, FontStyles.Bold, Color.white);

        // İçerik alanı: mesaj burada, özel içerik de buraya eklenir.
        RectTransform content = NewRect("Content", window);
        VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 20f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        TextMeshProUGUI message = AddText(NewRect("Message", content), "Mesaj", 38f, FontStyles.Normal,
            new Color(0.9f, 0.92f, 1f, 1f));

        // Butonlar
        RectTransform buttonRow = NewRect("Buttons", window);
        HorizontalLayoutGroup rowLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 30f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = false;

        Button cancelButton = AddButton(buttonRow, "CancelButton", pink, new Color(0.9f, 0.35f, 0.5f), "İptal",
            out TextMeshProUGUI cancelLabel);
        Button confirmButton = AddButton(buttonRow, "ConfirmButton", blue, new Color(0.3f, 0.55f, 0.95f), "Tamam",
            out TextMeshProUGUI confirmLabel);

        // Kapatma (X): layout dışında, sağ üst köşede.
        RectTransform closeRect = NewRect("CloseButton", window);
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-20f, -20f);
        closeRect.sizeDelta = new Vector2(90f, 90f);
        closeRect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        AddImage(closeRect, close, Color.white);
        Button closeButton = closeRect.gameObject.AddComponent<Button>();

        // UIPanel'in serialize alanlarını bağla.
        SerializedObject so = new SerializedObject(panel);
        so.FindProperty("window").objectReferenceValue = window;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("messageText").objectReferenceValue = message;
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("closeButton").objectReferenceValue = closeButton;
        so.FindProperty("backdropButton").objectReferenceValue = backdropButton;
        so.FindProperty("buttonRow").objectReferenceValue = buttonRow.gameObject;
        so.FindProperty("confirmButton").objectReferenceValue = confirmButton;
        so.FindProperty("confirmLabel").objectReferenceValue = confirmLabel;
        so.FindProperty("cancelButton").objectReferenceValue = cancelButton;
        so.FindProperty("cancelLabel").objectReferenceValue = cancelLabel;
        so.ApplyModifiedPropertiesWithoutUndo();

        return root.gameObject;
    }

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");
    }

    internal static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    internal static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    internal static Image AddImage(RectTransform rect, Sprite sprite, Color color)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
        // Sprite varsa kendi rengi görünsün, yoksa yer tutucu renk kullan.
        image.color = sprite != null ? Color.white : color;
        return image;
    }

    internal static TextMeshProUGUI AddText(RectTransform rect, string text, float size, FontStyles style, Color color)
    {
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        return label;
    }

    private static Button AddButton(RectTransform parent, string name, Sprite sprite, Color fallback, string text,
                                    out TextMeshProUGUI label)
    {
        RectTransform rect = NewRect(name, parent);
        AddImage(rect, sprite, fallback);
        rect.gameObject.AddComponent<LayoutElement>().minHeight = 120f;
        Button button = rect.gameObject.AddComponent<Button>();

        RectTransform labelRect = NewRect("Label", rect);
        Stretch(labelRect);
        label = AddText(labelRect, text, 42f, FontStyles.Bold, Color.white);
        label.raycastTarget = false;
        return button;
    }
}
