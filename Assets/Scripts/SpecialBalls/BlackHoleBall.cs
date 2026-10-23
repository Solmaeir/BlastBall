using UnityEngine;

// 30. seviye düşmanı: Kara Delik. Alanın üst yarısında süzülür ve çevresindeki topları kendine çeker;
// renkleri birbirine karıştırır ve yığını tavana doğru taşır. Yalnızca Kalkan kullanılınca kapanır.
public class BlackHoleBall : SpecialBall
{
    [Header("Süzülme")]
    [Tooltip("Durduğu yükseklik, alan yüksekliğinin oranı olarak (0 = taban, 1 = tavan).")]
    [SerializeField, Range(0f, 1f)] private float hoverHeight = 0.6f;
    [Tooltip("Yatayda gezindiği genişlik, alan genişliğinin oranı olarak.")]
    [SerializeField, Range(0f, 1f)] private float driftWidth = 0.55f;
    [SerializeField, Min(0f)] private float driftSpeed = 0.6f;

    [Header("Çekim")]
    [Tooltip("Çekim yarıçapı, top çapının katı olarak.")]
    [SerializeField, Min(0.5f)] private float pullRadiusInBalls = 4.5f;
    [Tooltip("Merkezdeki çekim ivmesi; kenara doğru sıfıra iner.")]
    [SerializeField, Min(0f)] private float pullAcceleration = 11f;

    [Header("Görsel")]
    [SerializeField] private Transform ring;
    [SerializeField] private float spinSpeed = 90f;

    private readonly Collider2D[] hits = new Collider2D[48];
    private ContactFilter2D filter;
    private float seed;

    protected override void Awake()
    {
        base.Awake();
        filter = new ContactFilter2D().NoFilter();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        seed = Random.value * 10f;
    }

    private void FixedUpdate()
    {
        Hover();
        Pull();
    }

    private void Hover()
    {
        PowerUpManager field = PowerUpManager.Instance;
        if (field == null) return;

        Bounds bounds = field.Field;
        Vector2 target = new Vector2(
            bounds.center.x + Mathf.Sin(Time.time * 0.35f + seed) * bounds.extents.x * driftWidth,
            bounds.min.y + bounds.size.y * hoverHeight);

        Vector2 desired = Vector2.ClampMagnitude((target - Body.position) * 1.5f, driftSpeed);
        Body.linearVelocity = Vector2.MoveTowards(Body.linearVelocity, desired, 3f * Time.fixedDeltaTime);
    }

    private void Pull()
    {
        float radius = pullRadiusInBalls * Diameter;
        int count = Physics2D.OverlapCircle(Body.position, radius, filter, hits);

        for (int i = 0; i < count; i++)
        {
            Rigidbody2D other = hits[i].attachedRigidbody;
            if (other == null || other == Body || other.bodyType != RigidbodyType2D.Dynamic) continue;
            if (!hits[i].TryGetComponent(out BallMatching _) || hits[i].TryGetComponent(out SpecialBall _)) continue;

            Vector2 offset = Body.position - other.position;
            float distance = offset.magnitude;
            if (distance < 0.001f) continue;

            float falloff = 1f - distance / radius;
            other.AddForce(offset / distance * (pullAcceleration * falloff * other.mass));
        }
    }

    private void Update()
    {
        if (ring != null) ring.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }
}
