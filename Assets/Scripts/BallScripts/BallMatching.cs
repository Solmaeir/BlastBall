using System.Collections.Generic;
using UnityEngine;

public class BallMatching : MonoBehaviour
{
    [Header("Eşleşme Algılama Ayarları")]
    [Tooltip("Komşu algılama yarıçapı, topun kendi yarıçapının katı olarak. Top büyüyüp küçülse de doğru çalışır.")]
    [Min(1f)] public float detectionRadiusMultiplier = 2f;

    private CircleCollider2D circle;

    private float DetectionRadius
    {
        get
        {
            if (circle == null) circle = GetComponent<CircleCollider2D>();
            Vector3 scale = transform.lossyScale;
            return circle.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)) * detectionRadiusMultiplier;
        }
    }

    // Kullanıcı etkileşimiyle (BallDragControl üzerinden) çağrılan ana metod
    public void CheckMatches()
    {
        List<BallMatching> connectedGroup = GetConnectedBalls();

        // 6 veya daha fazla top aynı Tag ile birleştiyse
        if (connectedGroup.Count >= 6)
        {
            // 1. Skor yöneticisine patlatılan top sayısını bildir
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPop(connectedGroup.Count);
            }

            // 2. Topları havuza geri gönder
            foreach (BallMatching ball in connectedGroup)
            {
                if (ball != null)
                {
                    PoolManager.Despawn(ball.gameObject);
                }
            }
        }
    }

    // BFS (Genişlik Öncelikli Arama) ile temas halindeki aynı Tag'li tüm topları bulur
    private List<BallMatching> GetConnectedBalls()
    {
        List<BallMatching> group = new List<BallMatching>();
        HashSet<BallMatching> visited = new HashSet<BallMatching>();
        Queue<BallMatching> searchQueue = new Queue<BallMatching>();

        searchQueue.Enqueue(this);
        visited.Add(this);

        while (searchQueue.Count > 0)
        {
            BallMatching currentBall = searchQueue.Dequeue();
            group.Add(currentBall);

            // Mevcut topun anlık fiziki temasındaki komşularını al
            List<BallMatching> neighbors = currentBall.GetDirectNeighbors();

            foreach (BallMatching neighbor in neighbors)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    searchQueue.Enqueue(neighbor);
                }
            }
        }

        return group;
    }

    // Physics2D kullanarak o an fiziksel olarak temas eden aynı Tag'deki topları bulur
    private List<BallMatching> GetDirectNeighbors()
    {
        List<BallMatching> neighbors = new List<BallMatching>();

        // Topun etrafındaki belirlenen yarıçaptaki tüm kolaydırları tara
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, DetectionRadius);

        foreach (Collider2D col in hitColliders)
        {
            // Kendisi hariç ve aynı Tag'e sahip olan objeleri ekle
            if (col.gameObject != gameObject && col.CompareTag(gameObject.tag))
            {
                BallMatching neighborBall = col.GetComponent<BallMatching>();
                if (neighborBall != null)
                {
                    neighbors.Add(neighborBall);
                }
            }
        }

        return neighbors;
    }

    // Unity Scene ekranında algılama yarıçapını yeşil çember olarak gösterir
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);
    }
}