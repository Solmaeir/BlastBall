using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Paused,
    GameOver
}

// Taslak: oyun akışı netleşince burası genişletilecek.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Geçici Game Over Davranışı")]
    [Tooltip("Game over panelini ekleyince kapat.")]
    [SerializeField] private bool loadMenuOnGameOver = true;
    [SerializeField] private string menuSceneName = "Menu";

    public GameState State { get; private set; } = GameState.Playing;
    public bool IsPlaying => State == GameState.Playing;

    // Oynanan süre (saniye). Duraklatma ve oyun sonu sayılmaz.
    public float PlayTime { get; private set; }

    public event Action<GameState> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (IsPlaying) PlayTime += Time.deltaTime;
    }

    private void Start()
    {
        StartGame();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        SetState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (State == GameState.Playing) SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (State == GameState.Paused) SetState(GameState.Playing);
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;

        SetState(GameState.GameOver);

        if (loadMenuOnGameOver)
        {
            SceneLoader.Load(menuSceneName);
        }
    }

    private void SetState(GameState newState)
    {
        State = newState;
        Time.timeScale = newState == GameState.Playing ? 1f : 0f;
        StateChanged?.Invoke(newState);
    }
}
