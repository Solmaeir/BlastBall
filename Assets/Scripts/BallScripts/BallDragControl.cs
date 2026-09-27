using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallDragControl : MonoBehaviour
{
    [Header("Sürükleme Ayarları")]
    public float dragSpeed = 20f;

    [Header("Dengeli Fırlatma Ayarları")]
    public float throwSpeedMultiplier = 1.0f;
    public float minThrowSpeed = 1.5f;
    public float maxThrowSpeed = 15f;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private bool isDragging = false;

    private Queue<PositionPoint> positionHistory = new Queue<PositionPoint>();
    private const float historyTimeWindow = 0.1f;

    private struct PositionPoint
    {
        public Vector2 position;
        public float time;

        public PositionPoint(Vector2 pos, float t)
        {
            position = pos;
            time = t;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        isDragging = true;
        positionHistory.Clear();
        AddPositionPoint(mainCamera.ScreenToWorldPoint(Input.mousePosition));
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector2 currentMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 targetDirection = currentMousePos - rb.position;
        rb.linearVelocity = targetDirection * dragSpeed;

        AddPositionPoint(currentMousePos);
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        Vector2 currentMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        AddPositionPoint(currentMousePos);

        Vector2 calculatedVelocity = CalculateThrowVelocity();

        if (calculatedVelocity.magnitude >= minThrowSpeed)
        {
            Vector2 finalVelocity = Vector2.ClampMagnitude(calculatedVelocity * throwSpeedMultiplier, maxThrowSpeed);
            rb.linearVelocity = finalVelocity;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        // KULLANICI ETKİLEŞİMİ: Top bırakıldığında veya tıklandığında bağlı topları kontrol et ve patlat
        BallMatching matchingScript = GetComponent<BallMatching>();
        if (matchingScript != null)
        {
            matchingScript.CheckMatches();
        }
    }

    private void AddPositionPoint(Vector2 pos)
    {
        float currentTime = Time.time;
        positionHistory.Enqueue(new PositionPoint(pos, currentTime));

        while (positionHistory.Count > 0 && (currentTime - positionHistory.Peek().time) > historyTimeWindow)
        {
            positionHistory.Dequeue();
        }
    }

    private Vector2 CalculateThrowVelocity()
    {
        if (positionHistory.Count < 2) return Vector2.zero;

        PositionPoint oldest = positionHistory.Peek();
        PositionPoint newest = oldest;

        foreach (var point in positionHistory)
        {
            newest = point;
        }

        float timeDelta = newest.time - oldest.time;
        if (timeDelta <= 0.0001f) return Vector2.zero;

        return (newest.position - oldest.position) / timeDelta;
    }
}