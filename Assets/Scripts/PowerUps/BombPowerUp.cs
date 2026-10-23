using UnityEngine;
using UnityEngine.InputSystem;

// Anlık güçlendirici. Butona basınca hedefleme moduna geçilir (oyun durur); alan içinde bir noktaya dokunup
// bırakınca çevresindeki tüm toplar renge bakmadan patlar (Karıştırıcı dahil; diğer düşman toplar dayanıklıdır). Tekrar butona basmak iptal eder, hak harcanmaz.
[CreateAssetMenu(menuName = "BlastBall/Power-Ups/Bomb", fileName = "BombPowerUp")]
public class BombPowerUp : PowerUpDefinition
{
    [Header("Bomba")]
    [Tooltip("Patlama yarıçapı, top çapının katı olarak.")]
    [Min(0.5f)] public float radiusInBalls = 2f;
    public GameObject blastPrefab;
    [Tooltip("Hedefleme halkası; 1 birim çaplı sprite içeren prefab.")]
    public GameObject reticlePrefab;
    public AudioClip blastSound;
    public HapticType blastHaptic = HapticType.Heavy;
    [Min(0f)] public float shakeStrength = 0.12f;
    [Min(0.05f)] public float shakeDuration = 0.25f;

    public override PowerUpRuntime CreateRuntime(PowerUpSlot slot) => new Runtime(this, slot);

    private sealed class Runtime : PowerUpRuntime
    {
        private readonly BombPowerUp definition;
        private Transform reticle;
        private bool aiming;
        private float radius;

        public Runtime(BombPowerUp definition, PowerUpSlot slot) : base(slot)
        {
            this.definition = definition;
        }

        public override bool LocksBallInput => true;

        public override void Press()
        {
            if (Slot.IsActive) End();
            else Begin();
        }

        public override void Tick()
        {
            if (!Manager.IsGameRunning)
            {
                End();
                return;
            }

            Pointer pointer = Pointer.current;
            if (pointer == null) return;

            Vector2 screen = pointer.position.ReadValue();

            if (!aiming)
            {
                if (pointer.press.wasPressedThisFrame && !PointerOverUI.Check(screen)) aiming = true;
                else
                {
                    PlaceReticle(Manager.Field.center, 1f + 0.04f * Mathf.Sin(Time.unscaledTime * 6f));
                    return;
                }
            }

            Vector2 world = Manager.ScreenToWorld(screen);
            PlaceReticle(world, 1f);

            if (pointer.press.isPressed) return;

            aiming = false;
            if (Manager.FieldContains(world)) Detonate(world);
        }

        public override void Dispose()
        {
            if (reticle != null) Object.Destroy(reticle.gameObject);
        }

        private void Begin()
        {
            radius = definition.radiusInBalls * Manager.BallDiameter;
            aiming = false;

            if (reticle == null) reticle = Object.Instantiate(definition.reticlePrefab).transform;
            reticle.gameObject.SetActive(true);

            Slot.SetActive(true);
            Manager.SetWorldFrozen(true);
            Manager.PlaySound(definition.activateSound);
            Haptics.Play(definition.activateHaptic);
        }

        private void End()
        {
            aiming = false;
            if (reticle != null) reticle.gameObject.SetActive(false);

            Slot.SetActive(false);
            Manager.SetWorldFrozen(false);
        }

        private void PlaceReticle(Vector2 position, float pulse)
        {
            reticle.position = position;
            reticle.localScale = Vector3.one * (radius * 2f * pulse);
        }

        private void Detonate(Vector2 point)
        {
            int popped = 0;
            foreach (Collider2D hit in Physics2D.OverlapCircleAll(point, radius))
            {
                // Bombaya dayanıklı düşman toplar (Yanan Top, Kara Delik) etkilenmez.
                if (!hit.TryGetComponent(out BallMatching _) || SpecialBall.Resists(hit, definition)) continue;
                PoolManager.Despawn(hit.gameObject);
                popped++;
            }

            if (popped > 0 && ScoreManager.Instance != null) ScoreManager.Instance.AddPop(popped);

            GameObject blast = PoolManager.Spawn(definition.blastPrefab, point, Quaternion.identity);
            blast.GetComponent<BlastEffect>().Play(radius);

            Manager.Shake(definition.shakeStrength, definition.shakeDuration);
            Manager.PlaySound(definition.blastSound);
            Haptics.Play(definition.blastHaptic);

            Slot.Consume();
            End();
        }
    }
}
