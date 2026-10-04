using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Kalıcı (DontDestroyOnLoad) loading ekranı. Arayüz kodla oluşturulur, sahneye veya prefab'a bağımlı değildir.
// Tüm zamanlamalar unscaled: GameOver'da Time.timeScale = 0 iken de çalışır.
public class LoadingScreen : MonoBehaviour
{
    private const string LogoResource = "Loading/BlastBallLogo";
    private const string BackgroundResource = "Loading/LoadingBackground";

    private const float FadeInDuration = 0.25f;
    private const float FadeOutDuration = 0.35f;
    private const int SettleFrames = 3;       // yeni sahnenin Awake/Start/ilk kare işleri bitsin
    private const float SettleSeconds = 0.15f;

    // Oyundaki toplarla aynı gezegenler: kırmızı, mavi, sarı, mor.
    private static readonly string[] PlanetResources =
    {
        "Loading/Planet_Venus",
        "Loading/Planet_Earth",
        "Loading/Planet_Sun",
        "Loading/Planet_Exoplanet",
    };

    private static readonly Color BarFillColor = new Color(1f, 0.82f, 0.25f);
    private static readonly Color BarTrackColor = new Color(0f, 0f, 0f, 0.35f);

    public static LoadingScreen Instance { get; private set; }

    public bool IsBusy { get; private set; }

    private Canvas canvas;
    private CanvasGroup group;
    private RawImage background;
    private RectTransform logo;
    private TMP_Text label;
    private RectTransform barFill;
    private readonly RectTransform[] balls = new RectTransform[PlanetResources.Length];

    private Sprite circleSprite;
    private float progress;
    private float shownProgress;
    private Vector2Int lastScreenSize;

    public static LoadingScreen GetOrCreate()
    {
        if (Instance != null) return Instance;

        GameObject go = new GameObject("LoadingScreen");
        DontDestroyOnLoad(go);
        return go.AddComponent<LoadingScreen>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Build();
        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Begin(string sceneName, float minDuration, bool startVisible)
    {
        if (IsBusy) return;
        StartCoroutine(Run(sceneName, minDuration, startVisible));
    }

    private IEnumerator Run(string sceneName, float minDuration, bool startVisible)
    {
        IsBusy = true;
        progress = 0f;
        shownProgress = 0f;
        RefreshLabel(0f);

        SetVisible(true);
        group.alpha = startVisible ? 1f : 0f;

        if (!startVisible)
        {
            yield return Fade(0f, 1f, FadeInDuration);
        }

        // Fade-in sırasında ekran donmasın diye bir kare bekle, sonra yüklemeyi başlat.
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float elapsed = 0f;
        while (op.progress < 0.9f || elapsed < minDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float loaded = Mathf.Clamp01(op.progress / 0.9f);
            float timed = Mathf.Clamp01(elapsed / minDuration);
            progress = Mathf.Min(loaded, timed) * 0.95f;
            yield return null;
        }

        // Yükleme bitti: sahneyi etkinleştir. Ağır Awake işleri bu karede olur, ekran tam opak.
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        progress = 1f;

        for (int i = 0; i < SettleFrames; i++) yield return null;
        yield return new WaitForSecondsRealtime(SettleSeconds);

        yield return Fade(1f, 0f, FadeOutDuration);

        SetVisible(false);
        IsBusy = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        group.alpha = to;
    }

    private void SetVisible(bool visible)
    {
        canvas.gameObject.SetActive(visible);
        if (visible) FitBackground();
    }

    private void Update()
    {
        if (!IsBusy) return;

        float t = Time.unscaledTime;

        shownProgress = Mathf.MoveTowards(shownProgress, progress, Time.unscaledDeltaTime * 0.8f);
        barFill.anchorMax = new Vector2(Mathf.Max(shownProgress, 0.001f), 1f);
        RefreshLabel(t);

        logo.localScale = Vector3.one * (1f + 0.025f * Mathf.Sin(t * 2.2f));

        for (int i = 0; i < balls.Length; i++)
        {
            float bounce = Mathf.Abs(Mathf.Sin(t * 4.5f - i * 0.6f));
            balls[i].anchoredPosition = new Vector2(balls[i].anchoredPosition.x, bounce * 34f);
            balls[i].localRotation = Quaternion.Euler(0f, 0f, t * (i % 2 == 0 ? -45f : 45f));
        }

        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y) FitBackground();
    }

    private void RefreshLabel(float t)
    {
        int dots = (int)(t * 2.5f) % 4;
        string visible = new string('.', dots);
        string hidden = new string('.', 3 - dots);
        label.text = Localization.Get("loading") + visible + "<alpha=#00>" + hidden;
    }

    // Arka plan görselini bozmadan tüm ekranı kaplar (cover).
    private void FitBackground()
    {
        if (Screen.width <= 0 || Screen.height <= 0 || background == null || background.texture == null) return;

        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        float screenAspect = (float)Screen.width / Screen.height;
        float texAspect = (float)background.texture.width / background.texture.height;

        Rect uv = new Rect(0f, 0f, 1f, 1f);
        if (screenAspect > texAspect)
        {
            uv.height = texAspect / screenAspect;
            uv.y = (1f - uv.height) * 0.5f;
        }
        else
        {
            uv.width = screenAspect / texAspect;
            uv.x = (1f - uv.width) * 0.5f;
        }

        background.uvRect = uv;
    }

    // ---------------------------------------------------------------- UI oluşturma

    private void Build()
    {
        circleSprite = CreateCircleSprite();

        GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform));
        canvasGo.transform.SetParent(transform, false);

        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
        group = canvasGo.AddComponent<CanvasGroup>();

        RectTransform root = (RectTransform)canvasGo.transform;

        background = CreateRawImage("Background", root, Resources.Load<Texture2D>(BackgroundResource));
        Stretch(background.rectTransform);
        background.raycastTarget = true; // yükleme sırasında arkadaki butonlara tıklanmasın

        BuildLogo(root);
        BuildBalls(root);
        BuildLabel(root);
        BuildProgressBar(root);
    }

    private void BuildLogo(RectTransform root)
    {
        RectTransform area = CreateRect("LogoArea", root);
        area.anchorMin = new Vector2(0.08f, 0.32f);
        area.anchorMax = new Vector2(0.92f, 0.92f);
        area.offsetMin = area.offsetMax = Vector2.zero;

        RawImage image = CreateRawImage("Logo", area, Resources.Load<Texture2D>(LogoResource));
        image.raycastTarget = false;
        Stretch(image.rectTransform);

        AspectRatioFitter fitter = image.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = image.texture != null ? (float)image.texture.width / image.texture.height : 1f;

        logo = image.rectTransform;
    }

    private void BuildBalls(RectTransform root)
    {
        const float size = 84f;
        const float spacing = 120f;

        RectTransform row = CreateRect("Balls", root);
        row.anchorMin = row.anchorMax = new Vector2(0.5f, 0.255f);
        row.sizeDelta = Vector2.zero;

        for (int i = 0; i < balls.Length; i++)
        {
            RawImage ball = CreateRawImage("Planet" + i, row, Resources.Load<Texture2D>(PlanetResources[i]));
            ball.raycastTarget = false;

            RectTransform rt = ball.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2((i - (balls.Length - 1) * 0.5f) * spacing, 0f);
            balls[i] = rt;
        }
    }

    private void BuildLabel(RectTransform root)
    {
        RectTransform rt = CreateRect("Label", root);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.2f);
        rt.sizeDelta = new Vector2(700f, 90f);

        TextMeshProUGUI text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 56f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        label = text;
    }

    private void BuildProgressBar(RectTransform root)
    {
        Image track = CreateImage("BarTrack", root, circleSprite, BarTrackColor);
        track.type = Image.Type.Sliced;
        track.raycastTarget = false;

        RectTransform trackRt = track.rectTransform;
        trackRt.anchorMin = trackRt.anchorMax = new Vector2(0.5f, 0.14f);
        trackRt.sizeDelta = new Vector2(640f, 34f);

        RectTransform inner = CreateRect("BarInner", trackRt);
        Stretch(inner);
        inner.offsetMin = new Vector2(5f, 5f);
        inner.offsetMax = new Vector2(-5f, -5f);

        Image fill = CreateImage("BarFill", inner, circleSprite, BarFillColor);
        fill.type = Image.Type.Sliced;
        fill.raycastTarget = false;

        barFill = fill.rectTransform;
        barFill.anchorMin = Vector2.zero;
        barFill.anchorMax = new Vector2(0.001f, 1f);
        barFill.offsetMin = barFill.offsetMax = Vector2.zero;
    }

    private static RectTransform CreateRect(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static RawImage CreateRawImage(string name, RectTransform parent, Texture texture)
    {
        RawImage image = CreateRect(name, parent).gameObject.AddComponent<RawImage>();
        image.texture = texture;
        return image;
    }

    private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Color color)
    {
        Image image = CreateRect(name, parent).gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // 64x64 yumuşak kenarlı daire. 9-slice (border 31) ile hap/pill şekli de verir.
    private static Sprite CreateCircleSprite()
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        float center = (size - 1) * 0.5f;
        float radius = size * 0.5f;
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                byte a = (byte)(Mathf.Clamp01(radius - dist) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(31, 31, 31, 31));
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}
