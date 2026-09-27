using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopKontrol : MonoBehaviour
{
    [Header("Fırlatma Ayarları")]
    public float gucCarpani = 8f;        // Topun fırlama gücü
    public float maksimumGuc = 40f;      // Maksimum fırlatma kuvveti sınırı

    private Rigidbody2D rb;
    private Vector2 baslangicNoktasi;
    private Vector2 bitisNoktasi;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnMouseDown()
    {
        baslangicNoktasi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void OnMouseUp()
    {
        bitisNoktasi = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Parmağın sürüklendiği yönü ve mesafeyi hesapla
        Vector2 yonVeMesafe = bitisNoktasi - baslangicNoktasi;

        // Uygulanacak kuvveti belirle ve sınırla
        Vector2 uygulananKuvvet = Vector2.ClampMagnitude(yonVeMesafe * gucCarpani, maksimumGuc);

        // 1. Topun o anki durgun durumunu sıfırla
        rb.linearVelocity = Vector2.zero;

        // 2. Kuvveti SADECE VE SADECE bu topa uygula
        rb.AddForce(uygulananKuvvet, ForceMode2D.Impulse);
    }
}