using UnityEngine;

// Toplar çarptığında kendiliğinden ses çıkarır: top-top için bilardo tıkırtısı, top-duvar için yumuşak tok ses.
// Ses seviyesi çarpma hızına göre değişir; çok yavaş temaslar (yığındaki titreşim) ses çıkarmaz.
[RequireComponent(typeof(Rigidbody2D))]
public class BallCollisionSound : MonoBehaviour
{
    [Tooltip("Bu hızın altındaki çarpmalar sessizdir.")]
    [SerializeField, Min(0f)] private float minSpeed = 0.5f;
    [Tooltip("Bu hızdaki çarpma tam ses seviyesinde çalar.")]
    [SerializeField, Min(0.1f)] private float maxSpeed = 6f;
    [Tooltip("En yavaş duyulur çarpmanın ses seviyesi (0-1).")]
    [SerializeField, Range(0f, 1f)] private float minVolume = 0.2f;
    [Tooltip("Sağ-sol dengesi için: bu x uzaklığı tam sağ/sol demektir.")]
    [SerializeField, Min(0.1f)] private float panWidth = 1.5f;
    [Range(0f, 1f)] [SerializeField] private float panAmount = 0.5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float speed = collision.relativeVelocity.magnitude;
        if (speed < minSpeed) return;

        // Eş zamanlı top sesi sınırı doluysa (top-top ve top-duvar aynı bütçeyi paylaşır) hesap yapmadan çık.
        if (AudioManager.IsSaturated(SfxId.BallHit)) return;

        bool ballHit = collision.collider.TryGetComponent(out BallCollisionSound other);

        // İki top birbirine çarpınca iki tarafta da olay gelir; sesi yalnızca biri çalar.
        if (ballHit && GetInstanceID() < other.GetInstanceID()) return;

        float volume = Mathf.Lerp(minVolume, 1f, Mathf.InverseLerp(minSpeed, maxSpeed, speed));
        float pan = Mathf.Clamp(transform.position.x / panWidth, -1f, 1f) * panAmount;
        AudioManager.PlaySfx(ballHit ? SfxId.BallHit : SfxId.BallWall, volume, 1f, pan);
    }
}
