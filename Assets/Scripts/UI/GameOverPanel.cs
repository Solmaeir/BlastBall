using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// GameManager GameOver durumuna geçince UIPanel'i açar: skor, en yüksek skor, ikon butonlar ve menüye dönüş.
// Sahneye kurulum: oyun sahnesi açıkken Tools > BlastBall > Add GameOver Panel To Scene.
// Bu obje aktif kalmalı; açılıp kapanan kısım child'daki UIPanel'dir.
public class GameOverPanel : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private UIPanel panel;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Button continueWithGemsButton;
    [SerializeField] private Button restartButton;

    [Header("Olaylar")]
    [Tooltip("Reklam izle butonu. Reklam sistemi eklenince buraya bağlanacak.")]
    public UnityEvent WatchAdPressed;
    [Tooltip("Elmasla kaldığın yerden devam et butonu. Elmas sistemi eklenince buraya bağlanacak.")]
    public UnityEvent ContinueWithGemsPressed;

    private GameManager gameManager;

    private void Awake()
    {
        if (watchAdButton != null) watchAdButton.onClick.AddListener(() => WatchAdPressed?.Invoke());
        if (continueWithGemsButton != null) continueWithGemsButton.onClick.AddListener(() => ContinueWithGemsPressed?.Invoke());
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
    }

    private void Start()
    {
        // GameManager.Instance Awake'te atanıyor, bu yüzden abonelik Start'ta.
        gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.StateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (gameManager != null) gameManager.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.GameOver) Show();
    }

    private void Show()
    {
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.score : 0;
        int level = ScoreManager.Instance != null ? ScoreManager.Instance.currentLevel : 1;
        PlayerProgress.Submit(score, level);
        int bestScore = PlayerProgress.BestScore;

        if (bestScoreText != null) bestScoreText.text = $"{Localization.Get("best_score")}: {bestScore}";

        panel.Configure(title: "game_over",
                        message: $"{Localization.Get("score")}: {score}",
                        confirmText: "main_menu", confirmAction: GoToMenu)
             .Show();
    }

    private void Restart()
    {
        if (SceneLoader.IsLoading) return;

        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
        SceneLoader.Load(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        if (SceneLoader.IsLoading) return;

        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
        SceneLoader.Load(SceneLoader.MenuScene);
    }
}
