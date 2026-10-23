// Ses efektlerinin ve müziklerin kimlikleri. AudioManager prefab'ında her kimlik için bir giriş bulunur.
// Yeni kimlik eklerken mevcut sıra bozulmasın diye listenin SONUNA eklenir (Unity enum'u sayı olarak kaydeder).
public enum SfxId
{
    None = 0,
    ButtonClick,
    PlayPress,
    PanelOpen,
    PanelClose,
    GameStart,
    GameOver,
    LevelUp,
    BallHit,
    BallWall,
    BallGrab,
    BallThrow,
    BallPop,
    PowerUpDenied,
    PowerUpUnlock
}

public enum MusicId
{
    None = 0,
    Loading,
    Menu,
    Game,
    GameOver
}
