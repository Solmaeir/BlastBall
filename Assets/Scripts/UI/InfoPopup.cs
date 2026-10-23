using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Bir bilgi penceresinin içeriği. Metinler Localization anahtarıdır; boş başlık/görsel gizlenir.
[Serializable]
public struct PopupContent
{
    public string headerKey;
    public string titleKey;
    public string bodyKey;
    public Sprite image;
}

// Tek kartlık bilgi penceresi (prefab: Assets/UI/InfoPopup/Prefabs/InfoPopup.prefab). İçerik veriyle gelir;
// "Nasıl Oynanır" ve yeni düşman top tanıtımı aynı prefab'ı kullanır. Oyun sırasında açılırsa oyunu duraklatır.
// Açıkken yeni içerik gelirse sıraya girer, "Tamam"dan sonra gösterilir.
public class InfoPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text header;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text body;
    [SerializeField] private Image image;
    [SerializeField] private Button okButton;
    [SerializeField, Min(1f)] private float fadeSpeed = 9f;

    private static int openCount;
    private static int lastClosedFrame = -1;

    private readonly Queue<PopupContent> queue = new Queue<PopupContent>();
    private CanvasGroup group;
    private bool pausedGame;

    // Açık bir pencere (veya bu karede kapanan) varsa geri tuşu başka panellere iletilmez.
    public static bool BlocksBackButton => openCount > 0 || lastClosedFrame == Time.frameCount;

    public event Action Closed;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        okButton.onClick.AddListener(Next);
    }

    public void Show(PopupContent content)
    {
        if (gameObject.activeSelf)
        {
            queue.Enqueue(content);
            return;
        }

        Apply(content);
        gameObject.SetActive(true);

        GameManager game = GameManager.Instance;
        if (game != null && game.IsPlaying)
        {
            game.PauseGame();
            pausedGame = true;
        }
    }

    private void OnEnable()
    {
        openCount++;
        group.alpha = 0f;
    }

    private void OnDisable()
    {
        openCount--;
        lastClosedFrame = Time.frameCount;
    }

    private void Update()
    {
        if (group.alpha < 1f) group.alpha = Mathf.MoveTowards(group.alpha, 1f, Time.unscaledDeltaTime * fadeSpeed);
        if (BackButton.WasPressed()) Next();
    }

    private void Next()
    {
        if (queue.Count > 0)
        {
            Apply(queue.Dequeue());
            group.alpha = 0f;
            return;
        }

        gameObject.SetActive(false);
        if (pausedGame)
        {
            pausedGame = false;
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
        }
        Closed?.Invoke();
    }

    private void Apply(PopupContent content)
    {
        SetText(header, content.headerKey);
        SetText(title, content.titleKey);
        SetText(body, content.bodyKey);

        image.sprite = content.image;
        image.gameObject.SetActive(content.image != null);
    }

    private static void SetText(TMP_Text label, string key)
    {
        bool visible = !string.IsNullOrEmpty(key);
        label.gameObject.SetActive(visible);
        if (visible) label.text = Localization.Get(key);
    }
}
