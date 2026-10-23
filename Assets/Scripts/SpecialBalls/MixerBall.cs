using UnityEngine;

// 10. seviye düşmanı: Karıştırıcı. Yerçekimsizdir; topların biriktiği alt kısımda hızlı ve kıvrak yönlerde yüzer.
// Değdiği her topu girdap yönünde savurur (dışa + teğet itiş), böylece yığın sürekli dağılır.
// Alanın kenarına yaklaşınca merkeze döner. Yalnızca Bomba ile yok olur.
public class MixerBall : SpecialBall
{
    [Header("Hareket")]
    [SerializeField, Min(0f)] private float speed = 3f;
    [SerializeField, Min(0f)] private float acceleration = 10f;
    [Tooltip("Yön değiştirme hızı. Büyüdükçe daha sık ve keskin döner.")]
    [SerializeField, Min(0f)] private float wanderRate = 0.9f;
    [Tooltip("Alanın bu oranının dışına çıkınca merkeze yönelir.")]
    [SerializeField, Range(0.3f, 1f)] private float roamArea = 0.75f;
    [Tooltip("Tercih ettiği yükseklik, alan yüksekliğinin oranı olarak (0 = taban). Toplar altta biriktiği için aşağıda gezer.")]
    [SerializeField, Range(0f, 1f)] private float preferredHeight = 0.25f;
    [Tooltip("Tercih ettiği yüksekliğe ne kadar güçlü döner.")]
    [SerializeField, Min(0f)] private float heightPull = 1.2f;

    [Header("Savurma")]
    [Tooltip("Değdiği topa kazandırdığı hız (birim/sn); topun kütlesinden bağımsızdır.")]
    [SerializeField, Min(0f)] private float kickSpeed = 4.5f;
    [Tooltip("İtişin teğet (girdap) payı. 0 = düz dışa, 1 = dışa ve yana eşit.")]
    [SerializeField, Range(0f, 2f)] private float swirl = 0.8f;
    [Tooltip("Temas sürdükçe aynı topların tekrar savrulma aralığı (saniye).")]
    [SerializeField, Min(0.02f)] private float kickInterval = 0.15f;

    [Header("Görsel")]
    [SerializeField] private Transform swirlVisual;
    [SerializeField] private float spinSpeed = -220f;

    private float seed;
    private float nextKickTime;
    private bool kickThisStep;

    protected override void OnEnable()
    {
        base.OnEnable();
        seed = Random.value * 100f;
        nextKickTime = 0f;
    }

    private void FixedUpdate()
    {
        // Çarpışma geri çağrıları bu fizik adımından sonra gelir; savurma penceresi adım başında açılır.
        kickThisStep = Time.time >= nextKickTime;
        if (kickThisStep) nextKickTime = Time.time + kickInterval;

        Steer();
    }

    private void Steer()
    {
        // Perlin gürültüsü yönü kesintisiz ve doğal biçimde döndürür (Time.time duraklamada durur).
        float angle = Mathf.PerlinNoise(seed, Time.time * wanderRate) * Mathf.PI * 4f;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        PowerUpManager field = PowerUpManager.Instance;
        if (field != null)
        {
            Bounds bounds = field.Field;

            // Dikeyde tercih edilen yüksekliğe çekilir; yatayda serbest kalır.
            float homeY = bounds.min.y + bounds.size.y * preferredHeight;
            direction.y += (homeY - Body.position.y) / bounds.extents.y * heightPull;
            direction.Normalize();

            Vector2 toCenter = (Vector2)bounds.center - Body.position;
            Vector2 limit = bounds.extents * roamArea;
            if (Mathf.Abs(toCenter.x) > limit.x || Mathf.Abs(toCenter.y) > limit.y)
            {
                direction = (direction + toCenter.normalized * 2f).normalized;
            }
        }

        Body.linearVelocity = Vector2.MoveTowards(Body.linearVelocity, direction * speed, acceleration * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision) => Kick(collision.rigidbody);

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (kickThisStep) Kick(collision.rigidbody);
    }

    private void Kick(Rigidbody2D other)
    {
        if (other == null || other.bodyType != RigidbodyType2D.Dynamic) return;
        if (!other.TryGetComponent(out BallMatching _) || other.TryGetComponent(out SpecialBall _)) return;

        Vector2 outward = other.position - Body.position;
        if (outward.sqrMagnitude < 0.0001f) return;
        outward.Normalize();

        // Görsel girdabın döndüğü yönde (saat yönü) teğet bileşen eklenir.
        Vector2 tangent = spinSpeed < 0f ? new Vector2(outward.y, -outward.x) : new Vector2(-outward.y, outward.x);
        Vector2 direction = (outward + tangent * swirl).normalized;
        other.AddForce(direction * (kickSpeed * other.mass), ForceMode2D.Impulse);
    }

    private void Update()
    {
        if (swirlVisual != null) swirlVisual.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }
}
