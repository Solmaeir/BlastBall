using System.Collections;
using UnityEngine;

// Oyun sahnesindeki olaylara geri bildirim (ses + titreşim) bağlar. Oyun kodu bu sınıfı bilmez;
// yalnızca kendi olaylarını (StateChanged, LevelChanged, GroupPopped) yayınlar.
public class GameFeedbackEvents : MonoBehaviour
{
    private const int BigPopSize = 10;

    private GameManager gameManager;
    private ScoreManager scoreManager;

    private IEnumerator Start()
    {
        gameManager = GameManager.Instance;
        scoreManager = ScoreManager.Instance;
        if (gameManager != null) gameManager.StateChanged += OnStateChanged;
        if (scoreManager != null) scoreManager.LevelChanged += OnLevelChanged;
        BallMatching.GroupPopped += OnGroupPopped;

        // Başlangıç sesi, loading ekranı kapandıktan sonra çalsın.
        while (SceneLoader.IsLoading) yield return null;
        AudioManager.PlaySfx(SfxId.GameStart);
    }

    private void OnDestroy()
    {
        if (gameManager != null) gameManager.StateChanged -= OnStateChanged;
        if (scoreManager != null) scoreManager.LevelChanged -= OnLevelChanged;
        BallMatching.GroupPopped -= OnGroupPopped;
    }

    private void OnStateChanged(GameState state)
    {
        if (state != GameState.GameOver) return;

        AudioManager.PlaySfx(SfxId.GameOver);
        AudioManager.PlayMusicTrack(MusicId.GameOver);
        Haptics.Play(HapticType.Heavy);
    }

    private void OnLevelChanged(int level)
    {
        if (level <= 1) return;

        AudioManager.PlaySfx(SfxId.LevelUp);
        Haptics.Play(HapticType.Success);
    }

    // Grup büyüdükçe patlama sesi biraz tizleşir ve titreşim güçlenir.
    private void OnGroupPopped(int count)
    {
        AudioManager.PlaySfx(SfxId.BallPop, 1f, Mathf.Lerp(1f, 1.3f, Mathf.InverseLerp(6f, 14f, count)));
        Haptics.Play(count >= BigPopSize ? HapticType.Heavy : HapticType.Medium);
    }
}
