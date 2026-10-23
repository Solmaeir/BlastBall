using UnityEngine;

// Bir güçlendiricinin veri paketi. Tür (anlık / süreli / pasif) alt sınıf tarafından belirlenir:
//   BombPowerUp   : anlık,  FreezePowerUp : süreli,  ShieldPowerUp : pasif (bir kez).
// Yeni güçlendirici eklemek: aynı türde ise yeni bir asset oluşturmak (Create > BlastBall > Power-Ups) ve
// PowerUpManager listesine eklemek; yeni davranış ise yeni bir alt sınıf + PowerUpRuntime yazmak.
public abstract class PowerUpDefinition : ScriptableObject
{
    [Header("Kimlik")]
    public string id;
    [Tooltip("Localization anahtarı (buton altındaki yazı).")]
    public string labelKey;

    [Header("Kurallar")]
    [Tooltip("Bu seviyeye ulaşılana kadar buton kilitli kalır.")]
    [Min(1)] public int unlockLevel = 1;
    [Tooltip("Her oyunun başında verilen hak sayısı.")]
    [Min(0)] public int startingCount = 1;

    [Header("Buton Görünümü")]
    public Sprite faceSprite;
    public Sprite baseSprite;
    public Sprite icon;
    public Sprite iconDisabled;
    public Color glowColor = Color.white;
    [Tooltip("Aktifken butonun gösterimi: Timer = süre halkası, Highlight = parlak çerçeve, Equipped = çerçeve + onay.")]
    public BoosterActiveStyle activeStyle = BoosterActiveStyle.Highlight;

    [Header("Ses ve Titreşim")]
    public AudioClip activateSound;
    public HapticType activateHaptic = HapticType.Light;

    public abstract PowerUpRuntime CreateRuntime(PowerUpSlot slot);
}
