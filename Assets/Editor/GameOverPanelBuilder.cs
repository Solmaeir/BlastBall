using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools > BlastBall > Add GameOver Panel To Scene
// Açık oyun sahnesine GameOver panelini kurar: UIPanel prefab'ından bir instance alır, içine
// en yüksek skor yazısını ve ikon butonları ekler, GameOverPanel'i bağlar ve GameManager'ın
// geçici "game over'da menüye dön" davranışını kapatır.
public static class GameOverPanelBuilder
{
    private const string PanelPrefabPath = "Assets/Prefabs/UIPanel.prefab";
    private const string IconFolder = "Assets/UI/GameOver/";
    // Sahnedeki sprite'lar 0'da; LoadingScreen short.MaxValue kullanıyor, onun altında kalır.
    private const int SortingOrder = 100;

    [MenuItem("Tools/BlastBall/Add GameOver Panel To Scene")]
    private static void Create()
    {
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        if (gameManager == null)
        {
            EditorUtility.DisplayDialog("GameOver Panel", "Açık sahnede GameManager yok. Önce oyun sahnesini aç.", "Tamam");
            return;
        }

        Canvas canvas = FindRootCanvas();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("GameOver Panel", "Açık sahnede Canvas yok.", "Tamam");
            return;
        }

        GameObject panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPrefabPath);
        if (panelPrefab == null)
        {
            EditorUtility.DisplayDialog("GameOver Panel",
                "UIPanel.prefab bulunamadı. Önce Tools > BlastBall > Create UIPanel Prefab çalıştır.", "Tamam");
            return;
        }

        GameOverPanel existing = Object.FindFirstObjectByType<GameOverPanel>(FindObjectsInactive.Include);
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("GameOver Panel", "Sahnede zaten GameOver paneli var. Yeniden kurulsun mu?",
                    "Yeniden kur", "İptal"))
            {
                return;
            }
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        GameObject root = Build(canvas.transform, panelPrefab);
        Undo.RegisterCreatedObjectUndo(root, "Add GameOver Panel");

        // Panel artık game over'ı karşılıyor; GameManager sahneyi kendisi değiştirmesin.
        SerializedObject managerSo = new SerializedObject(gameManager);
        managerSo.FindProperty("loadMenuOnGameOver").boolValue = false;
        managerSo.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        Debug.Log("GameOver paneli sahneye eklendi. Sahneyi kaydetmeyi unutma.");
    }

    private static Canvas FindRootCanvas()
    {
        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas.isRootCanvas) return canvas;
        }
        return null;
    }

    private static GameObject Build(Transform canvas, GameObject panelPrefab)
    {
        // Kök: hep aktif kalır ve GameManager'ı dinler. Graphic içermediği için tıklamaları engellemez.
        RectTransform root = UIPanelPrefabBuilder.NewRect("GameOverPanel", canvas);
        UIPanelPrefabBuilder.Stretch(root);
        root.SetAsLastSibling();

        // Ana Canvas "Screen Space - Camera" olduğu için toplar/frame'ler mesafeye göre panelin önüne geçiyor.
        // Kendi sorting order'ı olan alt Canvas, paneli tüm sprite'ların üstünde çizer.
        Canvas canvasOverride = root.gameObject.AddComponent<Canvas>();
        canvasOverride.overrideSorting = true;
        canvasOverride.sortingOrder = SortingOrder;
        root.gameObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = (GameObject)PrefabUtility.InstantiatePrefab(panelPrefab, root);
        panelObject.name = "Panel";
        UIPanelPrefabBuilder.Stretch((RectTransform)panelObject.transform);
        UIPanel panel = panelObject.GetComponent<UIPanel>();

        SerializedObject panelSo = new SerializedObject(panel);
        // Game over'da oyun zaten durmuş durumda; panel kapatılarak geçilemez, sadece butonlarla çıkılır.
        panelSo.FindProperty("pauseGameWhileOpen").boolValue = false;
        panelSo.FindProperty("showCloseButton").boolValue = false;
        panelSo.FindProperty("closeOnBackdrop").boolValue = false;
        panelSo.FindProperty("closeOnBackKey").boolValue = false;
        panelSo.FindProperty("closeOnConfirm").boolValue = false;
        panelSo.ApplyModifiedPropertiesWithoutUndo();

        RectTransform content = (RectTransform)panelSo.FindProperty("content").objectReferenceValue;
        TMP_Text message = (TMP_Text)panelSo.FindProperty("messageText").objectReferenceValue;

        // Mevcut oturum skoru panelin Message alanında gösterilir; öne çıksın diye büyütülür.
        message.fontSize = 60f;
        message.fontStyle = FontStyles.Bold;
        message.color = Color.white;

        TextMeshProUGUI bestScore = UIPanelPrefabBuilder.AddText(UIPanelPrefabBuilder.NewRect("BestScore", content),
            "En Yüksek Skor: 0", 38f, FontStyles.Normal, new Color(1f, 0.85f, 0.4f, 1f));

        RectTransform iconRow = UIPanelPrefabBuilder.NewRect("IconButtons", content);
        HorizontalLayoutGroup rowLayout = iconRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.padding = new RectOffset(0, 0, 20, 0);
        rowLayout.spacing = 30f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        Button watchAd = AddIconButton(iconRow, "WatchAdButton", "ui_icon_ad");
        Button continueWithGems = AddIconButton(iconRow, "ContinueWithGemsButton", "ui_icon_gem");
        Button restart = AddIconButton(iconRow, "RestartButton", "ui_icon_restart");

        // Başlangıçta gizli; GameOverPanel game over'da UIPanel.Show ile açar.
        panelObject.SetActive(false);

        GameOverPanel gameOver = root.gameObject.AddComponent<GameOverPanel>();
        SerializedObject so = new SerializedObject(gameOver);
        so.FindProperty("panel").objectReferenceValue = panel;
        so.FindProperty("bestScoreText").objectReferenceValue = bestScore;
        so.FindProperty("watchAdButton").objectReferenceValue = watchAd;
        so.FindProperty("continueWithGemsButton").objectReferenceValue = continueWithGems;
        so.FindProperty("restartButton").objectReferenceValue = restart;
        so.ApplyModifiedPropertiesWithoutUndo();

        return root.gameObject;
    }

    private static Button AddIconButton(RectTransform parent, string name, string iconName)
    {
        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconFolder + iconName + ".png");

        RectTransform rect = UIPanelPrefabBuilder.NewRect(name, parent);
        Image image = UIPanelPrefabBuilder.AddImage(rect, icon, new Color(0.3f, 0.55f, 0.95f));
        image.preserveAspect = true;

        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = 150f;
        layout.preferredHeight = 150f;

        return rect.gameObject.AddComponent<Button>();
    }
}
