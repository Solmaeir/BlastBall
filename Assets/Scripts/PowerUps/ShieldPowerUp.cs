using UnityEngine;

// Pasif güçlendirici (bir kez). Butona basınca kalkan kurulur ve Kara Delikler kapanır (SpecialBall.CounterAll);
// oyun bitecekken (üst çizgiye temas süresi dolunca) game over yerine üst çizgi yakınındaki toplar temizlenir
// ve oyun devam eder. Aynı anda tek kalkan olabilir.
[CreateAssetMenu(menuName = "BlastBall/Power-Ups/Shield", fileName = "ShieldPowerUp")]
public class ShieldPowerUp : PowerUpDefinition
{
    [Header("Kalkan")]
    [Tooltip("Temizlenecek bölgenin yüksekliği, top çapının katı olarak (üst çizgiden aşağı).")]
    [Min(1f)] public float clearDepthInBalls = 3.5f;
    public ShieldLine linePrefab;
    public AudioClip breakSound;
    public HapticType breakHaptic = HapticType.Heavy;
    [Min(0f)] public float shakeStrength = 0.08f;
    [Min(0.05f)] public float shakeDuration = 0.2f;

    public override PowerUpRuntime CreateRuntime(PowerUpSlot slot) => new Runtime(this, slot);

    private sealed class Runtime : PowerUpRuntime
    {
        private readonly ShieldPowerUp definition;
        private ShieldLine line;

        public Runtime(ShieldPowerUp definition, PowerUpSlot slot) : base(slot)
        {
            this.definition = definition;
        }

        public override void Press()
        {
            if (Slot.IsActive) return;

            if (line == null) line = Object.Instantiate(definition.linePrefab);
            line.Show(Manager.Field, Manager.TopFrame.bounds.min.y);

            Slot.Consume();
            SpecialBall.CounterAll(definition);
            Slot.SetActive(true);
            Manager.PlaySound(definition.activateSound);
            Haptics.Play(definition.activateHaptic);
        }

        public override bool TryAbsorbGameOver()
        {
            Bounds field = Manager.Field;
            float top = Manager.TopFrame.bounds.min.y;
            float depth = definition.clearDepthInBalls * Manager.BallDiameter;

            Vector2 center = new Vector2(field.center.x, top - depth * 0.5f);
            Vector2 size = new Vector2(field.size.x, depth);
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, size, 0f))
            {
                if (hit.TryGetComponent(out BallMatching _) && !SpecialBall.Resists(hit, definition)) PoolManager.Despawn(hit.gameObject);
            }

            line.Break();
            Manager.Shake(definition.shakeStrength, definition.shakeDuration);
            Manager.PlaySound(definition.breakSound);
            Haptics.Play(definition.breakHaptic);
            Slot.SetActive(false);
            return true;
        }

        public override void Dispose()
        {
            if (line != null) Object.Destroy(line.gameObject);
        }
    }
}
