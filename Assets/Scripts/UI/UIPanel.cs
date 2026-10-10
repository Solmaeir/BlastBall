using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Genel amaçlı panel: başlık, mesaj, özel içerik alanı, onay/iptal butonları ve kapatma yolları.
// Prefab: Assets/Prefabs/UIPanel.prefab (Tools > BlastBall > Create UIPanel Prefab ile üretilir).
//
// Kullanım:
//   panel.Configure("Oyun Bitti", "Skor: 120", "Tekrar Oyna", Restart, "Menü", ToMenu).Show();
// Özel içerik için Content altına kendi objelerini ekle.
public class UIPanel : MonoBehaviour
{
    private static readonly List<UIPanel> OpenPanels = new List<UIPanel>();

    [Header("Referanslar")]
    [SerializeField] private RectTransform window;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private RectTransform content;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backdropButton;
    [SerializeField] private GameObject buttonRow;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text confirmLabel;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text cancelLabel;

    [Header("Davranış")]
    [Tooltip("Panel açıkken oyunu durdurur, kapanınca (biz durdurduysak) devam ettirir.")]
    [SerializeField] private bool pauseGameWhileOpen;
    [SerializeField] private bool showCloseButton = true;
    [SerializeField] private bool closeOnBackdrop = true;
    [Tooltip("Escape / Android geri tuşu. Birden fazla panel açıksa sadece en üsttekini kapatır.")]
    [SerializeField] private bool closeOnBackKey = true;
    [Tooltip("Onay butonuna basınca panel otomatik kapansın.")]
    [SerializeField] private bool closeOnConfirm = true;

    [Header("Animasyon")]
    [Min(0f)] [SerializeField] private float animationDuration = 0.18f;
    [Range(0.5f, 1f)] [SerializeField] private float startScale = 0.85f;

    [Header("Olaylar")]
    public UnityEvent Opened;
    public UnityEvent Closed;

    private CanvasGroup canvasGroup;
    private Coroutine animationRoutine;
    private Action onConfirm;
    private Action onCancel;
    private bool initialized;
    private bool pausedByThisPanel;

    public bool IsOpen { get; private set; }
    public RectTransform Content => content;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;
        initialized = true;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (closeButton != null) closeButton.onClick.AddListener(Dismiss);
        if (backdropButton != null) backdropButton.onClick.AddListener(OnBackdropPressed);
        if (confirmButton != null) confirmButton.onClick.AddListener(Confirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(Dismiss);

        RefreshButtons();
    }

    private void Update()
    {
        if (!closeOnBackKey || !IsOpen) return;
        if (OpenPanels.Count == 0 || OpenPanels[OpenPanels.Count - 1] != this) return;

        if (BackButton.WasPressed()) Dismiss();
    }

    private void OnDisable()
    {
        // Dışarıdan SetActive(false) yapılsa bile oyun durmuş halde kalmasın.
        if (IsOpen) FinishClose(false);
    }

    // ---- Yapılandırma ----

    // null verilen alan gizlenir. Metinler Localization anahtarı ya da düz metin olabilir.
    public UIPanel Configure(string title = null, string message = null,
                             string confirmText = null, Action confirmAction = null,
                             string cancelText = null, Action cancelAction = null)
    {
        Initialize();

        SetLabel(titleText, title);
        SetLabel(messageText, message);
        SetLabel(confirmLabel, confirmText);
        SetLabel(cancelLabel, cancelText);

        onConfirm = confirmAction;
        onCancel = cancelAction;

        RefreshButtons();
        return this;
    }

    public UIPanel SetTitle(string title)
    {
        SetLabel(titleText, title);
        return this;
    }

    public UIPanel SetMessage(string message)
    {
        SetLabel(messageText, message);
        return this;
    }

    private static void SetLabel(TMP_Text label, string text)
    {
        if (label == null) return;

        bool hasText = !string.IsNullOrEmpty(text);
        // Boş alan layout'ta yer kaplamasın diye label'ın objesini kapatıyoruz.
        label.gameObject.SetActive(hasText);
        if (hasText) label.text = Localization.Get(text);
    }

    private void RefreshButtons()
    {
        bool hasConfirm = confirmLabel != null && confirmLabel.gameObject.activeSelf;
        bool hasCancel = cancelLabel != null && cancelLabel.gameObject.activeSelf;

        // Label gizliyse butonun kendisi de gizli olmalı.
        if (confirmButton != null) confirmButton.gameObject.SetActive(hasConfirm);
        if (cancelButton != null) cancelButton.gameObject.SetActive(hasCancel);
        if (buttonRow != null) buttonRow.SetActive(hasConfirm || hasCancel);
        if (closeButton != null) closeButton.gameObject.SetActive(showCloseButton);
    }

    // ---- Aç / Kapat ----

    public void Show()
    {
        Initialize();

        if (IsOpen)
        {
            // Kapanma animasyonu sürerken tekrar açılırsa kaldığı yerden geri açılır.
            if (!canvasGroup.interactable)
            {
                canvasGroup.interactable = true;
                PlayAnimation(true, null);
            }
            return;
        }

        IsOpen = true;
        OpenPanels.Add(this);

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        if (pauseGameWhileOpen && GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            GameManager.Instance.PauseGame();
            pausedByThisPanel = true;
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        Apply(0f);
        PlayAnimation(true, null);
        Opened?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen) return;

        // Animasyon boyunca tıklamalar engellenir, bitince obje kapanır.
        canvasGroup.interactable = false;
        PlayAnimation(false, () => FinishClose(true));
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Show();
    }

    // X, arka plan, geri tuşu ve iptal butonu: iptal callback'ini çağırıp kapatır.
    public void Dismiss()
    {
        if (!IsOpen) return;

        Action callback = onCancel;
        Close();
        callback?.Invoke();
    }

    private void Confirm()
    {
        if (!IsOpen) return;

        Action callback = onConfirm;
        if (closeOnConfirm) Close();
        callback?.Invoke();
    }

    private void OnBackdropPressed()
    {
        if (closeOnBackdrop) Dismiss();
    }

    private void FinishClose(bool deactivate)
    {
        if (!IsOpen) return;

        IsOpen = false;
        OpenPanels.Remove(this);

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        if (pausedByThisPanel)
        {
            pausedByThisPanel = false;
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
        }

        if (deactivate) gameObject.SetActive(false);
        Closed?.Invoke();
    }

    // ---- Animasyon (oyun durmuş olabilir, bu yüzden unscaled time) ----

    private void PlayAnimation(bool opening, Action onDone)
    {
        if (animationRoutine != null) StopCoroutine(animationRoutine);

        if (animationDuration <= 0f || !isActiveAndEnabled)
        {
            Apply(opening ? 1f : 0f);
            onDone?.Invoke();
            return;
        }

        animationRoutine = StartCoroutine(Animate(opening, onDone));
    }

    private IEnumerator Animate(bool opening, Action onDone)
    {
        float from = canvasGroup.alpha;
        float to = opening ? 1f : 0f;

        for (float t = 0f; t < animationDuration; t += Time.unscaledDeltaTime)
        {
            float eased = 1f - Mathf.Pow(1f - t / animationDuration, 3f);
            Apply(Mathf.Lerp(from, to, eased));
            yield return null;
        }

        Apply(to);
        animationRoutine = null;
        onDone?.Invoke();
    }

    private void Apply(float progress)
    {
        canvasGroup.alpha = progress;
        if (window != null) window.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, progress);
    }
}
