using System.Collections.Generic;
using UnityEngine;

// Topun sürüklenme ve fırlatılma fiziği. Girişi BallInputController iletir.
[RequireComponent(typeof(Rigidbody2D))]
public class BallDragControl : MonoBehaviour, IPoolable
{
    [Header("Sürükleme Ayarları")]
    public float dragSpeed = 20f;

    [Header("Dengeli Fırlatma Ayarları")]
    public float throwSpeedMultiplier = 1.0f;
    public float minThrowSpeed = 1.5f;
    public float maxThrowSpeed = 15f;

    private const float HistoryTimeWindow = 0.1f;

    private struct PositionPoint
    {
        public Vector2 position;
        public float time;
    }

    private readonly Queue<PositionPoint> positionHistory = new Queue<PositionPoint>();
    private Rigidbody2D rb;
    private BallMatching matching;
    private bool isDragging;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        matching = GetComponent<BallMatching>();
    }

    public void OnSpawnedFromPool()
    {
        ResetDragState();
    }

    public void OnReturnedToPool()
    {
        ResetDragState();
    }

    private void ResetDragState()
    {
        isDragging = false;
        positionHistory.Clear();
    }

    public void BeginDrag(Vector2 worldPosition)
    {
        isDragging = true;
        positionHistory.Clear();
        AddPositionPoint(worldPosition);
    }

    public void DragTo(Vector2 worldPosition)
    {
        if (!isDragging) return;

        rb.linearVelocity = (worldPosition - rb.position) * dragSpeed;
        AddPositionPoint(worldPosition);
    }

    public void EndDrag(Vector2 worldPosition)
    {
        if (!isDragging) return;
        isDragging = false;

        AddPositionPoint(worldPosition);
        Vector2 velocity = CalculateThrowVelocity();
        rb.linearVelocity = velocity.magnitude >= minThrowSpeed
            ? Vector2.ClampMagnitude(velocity * throwSpeedMultiplier, maxThrowSpeed)
            : Vector2.zero;

        // Top bırakıldığında bağlı aynı renkteki topları kontrol et ve patlat
        if (matching != null) matching.CheckMatches();
    }

    private void AddPositionPoint(Vector2 position)
    {
        float now = Time.time;
        positionHistory.Enqueue(new PositionPoint { position = position, time = now });

        while (positionHistory.Count > 0 && now - positionHistory.Peek().time > HistoryTimeWindow)
        {
            positionHistory.Dequeue();
        }
    }

    private Vector2 CalculateThrowVelocity()
    {
        if (positionHistory.Count < 2) return Vector2.zero;

        PositionPoint oldest = positionHistory.Peek();
        PositionPoint newest = oldest;
        foreach (PositionPoint point in positionHistory) newest = point;

        float timeDelta = newest.time - oldest.time;
        if (timeDelta <= 0.0001f) return Vector2.zero;

        return (newest.position - oldest.position) / timeDelta;
    }
}
