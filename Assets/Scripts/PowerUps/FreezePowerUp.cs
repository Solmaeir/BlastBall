using UnityEngine;

// Süreli güçlendirici. Aktifken yeni top gelmez ve game over sayacı durur.
// Kullanılınca Yanan Topları söndürür (SpecialBall.CounterAll).
// Aktifken tekrar basılırsa bir hak harcanır ve süre baştan başlar (üst üste binmez).
[CreateAssetMenu(menuName = "BlastBall/Power-Ups/Freeze", fileName = "FreezePowerUp")]
public class FreezePowerUp : PowerUpDefinition
{
    [Header("Dondur")]
    [Min(1f)] public float duration = 6f;
    [Tooltip("Bitmeden bu kadar saniye önce ekran yanıp sönerek uyarır.")]
    [Min(0f)] public float warningSeconds = 1.5f;
    [Tooltip("Buz kenarlığı ve mavi filtre içeren ScreenOverlay prefab'ı.")]
    public ScreenOverlay overlayPrefab;
    [Tooltip("Dondurma bittiğinde çalan ses (isteğe bağlı).")]
    public AudioClip endSound;

    public override PowerUpRuntime CreateRuntime(PowerUpSlot slot) => new Runtime(this, slot);

    private sealed class Runtime : PowerUpRuntime
    {
        private readonly FreezePowerUp definition;
        private ScreenOverlay overlay;
        private float remaining;

        public Runtime(FreezePowerUp definition, PowerUpSlot slot) : base(slot)
        {
            this.definition = definition;
        }

        public override bool BlocksSpawning => true;
        public override bool PausesGameOver => true;

        public override void Press()
        {
            Slot.Consume();
            SpecialBall.CounterAll(definition);
            remaining = definition.duration;
            Slot.SetProgress(1f);

            if (Slot.IsActive) return;

            if (overlay == null) overlay = Manager.CreateOverlay(definition.overlayPrefab);
            overlay.Show();
            Slot.SetActive(true);
            Manager.PlaySound(definition.activateSound);
            Haptics.Play(definition.activateHaptic);
        }

        public override void Tick()
        {
            remaining -= Time.deltaTime;
            Slot.SetProgress(remaining / definition.duration);
            overlay.SetWarning(remaining <= definition.warningSeconds);

            if (remaining > 0f) return;

            overlay.Hide();
            Slot.SetActive(false);
            Manager.PlaySound(definition.endSound);
        }
    }
}
