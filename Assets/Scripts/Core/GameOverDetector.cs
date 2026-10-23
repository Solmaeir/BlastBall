using System.Collections.Generic;
using UnityEngine;

// Üst frame'e bir top kesintisiz biçimde holdSeconds boyunca temas ederse oyunu bitirir.
// Zıplama ve spawn anındaki anlık çarpmalar süre dolmadan temas kesildiği için sayılmaz.
// Süreler birikim olarak tutulur: dondurma gibi duraklatmalarda hiçbir şey ilerlemez, kalkan varsa oyun bitmez.
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
        public float held;
        public float missing;
    }

    private readonly Collider2D[] contacts = new Collider2D[32];
    private readonly Dictionary<Collider2D, ContactTime> touching = new Dictionary<Collider2D, ContactTime>();
    private readonly HashSet<Collider2D> present = new HashSet<Collider2D>();
    private readonly List<Collider2D> keys = new List<Collider2D>();

    private void FixedUpdate()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.IsPlaying || topFrame == null) return;

        PowerUpManager powerUps = PowerUpManager.Instance;
        if (powerUps != null && powerUps.GameOverPaused) return;

        float dt = Time.fixedDeltaTime;
        int count = topFrame.GetContacts(contacts);
        present.Clear();

        for (int i = 0; i < count; i++)
        {
            Collider2D other = contacts[i];
            if (other == null || !other.TryGetComponent(out BallMatching ball) || !ball.CountsForGameOver) continue;

            present.Add(other);
            touching.TryGetValue(other, out ContactTime contact);
            contact.held += dt;
            contact.missing = 0f;
            touching[other] = contact;

            if (contact.held < holdSeconds) continue;

            touching.Clear();
            if (powerUps != null && powerUps.TryAbsorbGameOver()) return;

            gameManager.GameOver();
            return;
        }

        keys.Clear();
        foreach (Collider2D key in touching.Keys) keys.Add(key);

        foreach (Collider2D key in keys)
        {
            if (key != null && present.Contains(key)) continue;

            ContactTime contact = touching[key];
            contact.missing += dt;

            if (key == null || contact.missing > contactLossTolerance) touching.Remove(key);
            else touching[key] = contact;
        }
    }
}
