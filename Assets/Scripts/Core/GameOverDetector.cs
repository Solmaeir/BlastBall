using System.Collections.Generic;
using UnityEngine;

// Üst frame'e bir top kesintisiz biçimde holdSeconds boyunca temas ederse oyunu bitirir.
// Zıplama ve spawn anındaki anlık çarpmalar süre dolmadan temas kesildiği için sayılmaz.
public class GameOverDetector : MonoBehaviour
{
    [Tooltip("Havuzun üst sınırını oluşturan frame'in collider'ı.")]
    [SerializeField] private Collider2D topFrame;

    [Tooltip("Bir topun üst frame'e kesintisiz değmesi gereken süre (saniye).")]
    [Min(0.1f)] [SerializeField] private float holdSeconds = 2f;

    [Tooltip("Fizik titremesi yüzünden temasın çok kısa kesilmesini sayaç sıfırlamadan tolere eder.")]
    [Min(0f)] [SerializeField] private float contactLossTolerance = 0.25f;

    private struct ContactTime
    {
        public float start;
        public float lastSeen;
    }

    private readonly Collider2D[] contacts = new Collider2D[32];
    private readonly Dictionary<Collider2D, ContactTime> touching = new Dictionary<Collider2D, ContactTime>();
    private readonly List<Collider2D> expired = new List<Collider2D>();

    private void FixedUpdate()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.IsPlaying || topFrame == null) return;

        float now = Time.time;
        int count = topFrame.GetContacts(contacts);

        for (int i = 0; i < count; i++)
        {
            Collider2D other = contacts[i];
            if (other == null || !other.TryGetComponent(out BallMatching _)) continue;

            float start = touching.TryGetValue(other, out ContactTime previous) ? previous.start : now;
            touching[other] = new ContactTime { start = start, lastSeen = now };

            if (now - start >= holdSeconds)
            {
                gameManager.GameOver();
                return;
            }
        }

        expired.Clear();
        foreach (KeyValuePair<Collider2D, ContactTime> pair in touching)
        {
            if (pair.Key == null || now - pair.Value.lastSeen > contactLossTolerance)
            {
                expired.Add(pair.Key);
            }
        }

        foreach (Collider2D collider in expired)
        {
            touching.Remove(collider);
        }
    }
}
