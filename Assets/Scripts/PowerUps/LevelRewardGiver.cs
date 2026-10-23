using System;
using UnityEngine;

// Her N seviyede bir (5, 10, 15...) açık güçlendiricilerden rastgele birine hak verir; butonu parlar, ses ve titreşim çalar.
// Web sürümünde ödül reklam izlenince verilecek: o zaman yalnızca Offer, "reklamı göster, tamamlanınca Give" olarak değişir.
public class LevelRewardGiver : MonoBehaviour
{
    [Tooltip("Kaç seviyede bir ödül verilir.")]
    [SerializeField, Min(1)] private int everyLevels = 5;
    [Tooltip("Ödül olarak verilen hak sayısı.")]
    [SerializeField, Min(1)] private int amount = 1;

    // Ödül verildi (güçlendirici, adet). Arayüz bildirimi veya analitik buraya bağlanabilir.
    public event Action<PowerUpDefinition, int> Rewarded;

    private void Start()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.LevelChanged += OnLevelChanged;
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.LevelChanged -= OnLevelChanged;
    }

    private void OnLevelChanged(int level)
    {
        if (level <= 1 || level % everyLevels != 0) return;
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver) return;

        PowerUpSlot slot = PowerUpManager.Instance != null ? PowerUpManager.Instance.RandomUnlockedSlot() : null;
        if (slot != null) Offer(slot);
    }

    // Ödülün sunulduğu tek nokta. Reklamlı akışta burası reklamı açar ve yalnızca izlenirse Give çağrılır.
    private void Offer(PowerUpSlot slot)
    {
        Give(slot);
    }

    private void Give(PowerUpSlot slot)
    {
        slot.Add(amount);
        slot.View.PlayUnlock();
        AudioManager.PlaySfx(SfxId.PowerUpUnlock);
        Haptics.Play(HapticType.Light);
        Rewarded?.Invoke(slot.Definition, amount);
    }
}
