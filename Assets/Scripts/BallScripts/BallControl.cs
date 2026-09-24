using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopKontrol : MonoBehaviour
{
    [Header("Fırlatma Ayarları")]
    public float gucCarpani = 3f; // İtekleme gücünü buradan ayarlayabilirsin
    public float maksimumGuc = 20f; // Topun çok hızlanmasını önlemek için sınır

    private Rigidbody2D rb;
    private Vector2 baslangicNoktasi;
    private Vector2 bitisNoktasi;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Ekranda topun üzerine dokunulduğunda çalışır
    void OnMouseDown()
    {
        baslangicNoktasi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    // Parmağı (veya tıklamayı) bıraktığında çalışır
    void OnMouseUp()
    {
        bitisNoktasi = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Parmağın sürüklendiği yönü ve mesafeyi hesapla
        Vector2 yonVeMesafe = bitisNoktasi - baslangicNoktasi;

        // Kuvveti sınırla (Çok uzun kaydırmalarda top uzaya uçmasın)
        Vector2 uygulananKuvvet = Vector2.ClampMagnitude(yonVeMesafe * gucCarpani, maksimumGuc);

        // Topa anlık fiziksel bir darbe (Impulse) uygula
        rb.AddForce(uygulananKuvvet, ForceMode2D.Impulse);
    }
}