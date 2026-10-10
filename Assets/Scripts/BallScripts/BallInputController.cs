using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Topların tek giriş noktası (Input System). Dokunmatik, fare ve kalem Pointer.current üzerinden aynı yoldan okunur.
// Basıldığı anda parmağa en yakın top seçilir; bırakılana kadar sürükleme o topa iletilir.
public class BallInputController : MonoBehaviour
{
    [Tooltip("Parmağın topu yakalama toleransı (dünya birimi). Küçük ekranlarda dokunmayı kolaylaştırır.")]
    [SerializeField, Min(0f)] private float touchRadius = 0.12f;

    private readonly Collider2D[] hits = new Collider2D[8];
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    private ContactFilter2D filter;
    private PointerEventData uiPointer;
    private Camera cam;
    private BallDragControl activeBall;

    private void Awake()
    {
        cam = Camera.main;
        filter = new ContactFilter2D().NoFilter();
    }

    private void Update()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 screen = pointer.position.ReadValue();

        if (activeBall != null)
        {
            if (!activeBall.isActiveAndEnabled)
            {
                activeBall = null;
                return;
            }

            Vector2 world = ToWorld(screen);
            if (pointer.press.isPressed)
            {
                activeBall.DragTo(world);
            }
            else
            {
                activeBall.EndDrag(world);
                activeBall = null;
            }
            return;
        }

        if (pointer.press.wasPressedThisFrame && CanPlay() && !IsOverUI(screen))
        {
            Vector2 world = ToWorld(screen);
            activeBall = FindBall(world);
            if (activeBall != null) activeBall.BeginDrag(world);
        }
    }

    private Vector2 ToWorld(Vector2 screen)
    {
        return cam.ScreenToWorldPoint(screen);
    }

    private BallDragControl FindBall(Vector2 world)
    {
        int count = Physics2D.OverlapCircle(world, touchRadius, filter, hits);
        BallDragControl best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (!hits[i].TryGetComponent(out BallDragControl ball)) continue;

            float distance = ((Vector2)hits[i].transform.position - world).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = ball;
            }
        }
        return best;
    }

    private static bool CanPlay()
    {
        return GameManager.Instance == null || GameManager.Instance.IsPlaying;
    }

    // UI üzerine basıldıysa (butonlar, paneller) top seçilmez.
    private bool IsOverUI(Vector2 screen)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        if (uiPointer == null) uiPointer = new PointerEventData(eventSystem);
        uiPointer.position = screen;
        uiHits.Clear();
        eventSystem.RaycastAll(uiPointer, uiHits);
        return uiHits.Count > 0;
    }
}
