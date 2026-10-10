# BlastBall

Dikey ekranlı, mobil odaklı bir 2D bulmaca-arcade oyunu. Üst köşelerdeki iki spawner renkli gezegen toplarını bir kutunun içine düşürür; oyuncu topları sürükleyerek aynı renkten gruplar oluşturur ve patlatır. Kutu dolup toplar tavana dayandığında oyun biter.
<img width="460" height="1024" alt="image" src="https://github.com/user-attachments/assets/7670b24a-44b7-4e2e-9d36-39d8c6d15945" /> <img width="460" height="1024" alt="image" src="https://github.com/user-attachments/assets/4834a89b-d80b-4b83-bbbd-d691822d6c9c" /> 


## Teknik Bilgiler

| | |
|---|---|
| Motor | Unity 6 (6000.0.79f1) |
| Render | Universal Render Pipeline, 2D Renderer |
| Giriş | Input System paketi (yalnızca yeni sistem) |
| UI | uGUI + TextMesh Pro |
| Hedef | Android / iOS (dikey), 60 FPS |

## Sahneler

| Sahne | Görev |
|---|---|
| `Splash` | Açılış ve yükleme ekranı; build'in ilk sahnesi |
| `Menu` | Ana menü |
| `SampleScene` | Oyun sahnesi |

## Oyun Mekanikleri

### Temel döngü
- Sahnenin iki üst köşesindeki spawner'lar belirli aralıklarla top fırlatır. Toplar yerçekimiyle kutuya düşer.
- Oyuncu topu parmakla tutup sürükler ve bırakır; bırakma hızı fırlatma hızını belirler (alt ve üst sınırlı).
- Bırakılan topa bağlı, aynı renkteki toplar (temas zinciri) 6 veya daha fazlaysa tümü patlar.
- Patlayan toplar silinmez, havuza geri döner.

### Toplar
- Dört çeşit: kırmızı, mavi, mor, sarı.
- Eşleşme topun etiketiyle (tag) yapılır; komşu algılama yarıçapı topun kendi boyutuna oranlıdır.
- Toplar `Discrete` çarpışma algılama ve interpolation kullanır; kutu duvarları fırlatmaların içinden geçmemesi için kalın collider'lıdır.

### Skor ve seviye
- Patlayan her top 10 puan verir.
- Seviye, toplam skor eşiğine göre atlanır. Seviye n'i geçmek için gereken toplam skor `35 × (5 + 6 + … + (4+n))` formülüyle hesaplanır (175, 385, 630, …). Tek bir büyük patlatma birden fazla seviye atlatabilir.
- HUD'da seviye rozeti, ilerleme çubuğu, hedef skor ve oynanan süre gösterilir.

### Zorluk
- `DifficultyDirector`, oynanan süre ve seviye ilerlemesinden 0 ile 1 arasında bir zorluk değeri üretir. Eğri yumuşaktır ve üst sınıra yaklaşarak durur.
- Zorluk arttıkça atış aralığı kısalır (başlangıcın %40'ına kadar) ve fırlatma gücü artar (başlangıcın 1,3 katına kadar).
- Duraklatma ve oyun sonu süreye sayılmaz.
- Ayarlar `DifficultyDirector` bileşeninde Inspector'dan değiştirilebilir.

### Oyun sonu
- Bir top, üst çerçeveye kesintisiz 2 saniye temas ederse oyun biter. Kısa süreli çarpmalar ve zıplamalar sayılmaz.
- Oyun sonu paneli skoru ve en yüksek skoru gösterir; ana menüye dönüş sunar. Reklam izleme ve elmasla devam butonları arayüzde yer alır, işlevleri henüz bağlı değildir.
- Oyun her seferinde 1. seviyeden başlar.

### Kayıt
- En yüksek seviye ve en yüksek skor cihazda yerel olarak saklanır (`PlayerPrefs`) ve ana menüde gösterilir.
- Rekor kırıldığı anda kaydedilir.

### Menü ve ayarlar
- Ana menü: oynat butonu, can ve coin göstergesi, rekor kartı, güçlendirici envanteri, ödül/çark/mağaza/sıralama butonları. Can, coin, envanter, ödül, çark, mağaza ve sıralama şimdilik görsel yer tutucudur.
- Ayarlar: ses seviyesi, sessize alma, dil (Türkçe / İngilizce), bilgi paneli.
- Oyun içi duraklatma menüsü; ana menüye dönüş seçeneği.
- Android geri tuşu ve Escape açık panelleri kapatır.

## Teknik Yapı

| Klasör | İçerik |
|---|---|
| `Scripts/Core` | `GameManager` (durum, oynanan süre), `GameOverDetector`, `DifficultyDirector`, kare hızı kurulumu |
| `Scripts/BallScripts` | Top girişi, sürükleme/fırlatma, eşleşme, spawner, skor ve seviye |
| `Scripts/Pooling` | Genel amaçlı nesne havuzu (`PoolManager`, `PooledObject`, `IPoolable`) |
| `Scripts/Save` | Yerel kayıt (`PlayerProgress`) |
| `Scripts/Settings` | Yerelleştirme ve ayarlar menüsü |
| `Scripts/Loading` | Splash ve sahne yükleme |
| `Scripts/UI` | Paneller, HUD görünümleri, ölçekleme ve güvenli alan bileşenleri |
| `UI/GameHUD`, `UI/MainMenu`, `UI/Settings`, `UI/GameOver` | Arayüz görselleri, fontlar ve prefab'lar |

### Nesne havuzu
- Her top çeşidi için ayrı bir havuz bulunur: `PoolManager > <Prefab> Pool > toplar`.
- Toplar oyun sırasında oluşturulup yok edilmez; aktif/pasif yapılarak yeniden kullanılır. Havuzlar oyun başında önceden doldurulur.
- Yeni bir top çeşidi için prefab'ı `PoolManager`'ın giriş listesine ve spawner'ların prefab dizisine eklemek yeterlidir.

### Giriş
- `BallInputController` dokunmatik, fare ve kalemi tek yoldan okur. Basılan noktaya en yakın top seçilir; UI üzerine basılırsa veya oyun duraklatılmışsa seçim yapılmaz.

### Ekran uyumu
- `AdaptiveCanvasScaler`, tasarımın güvenli alan kutusunu her cihazda güvenli alana sığdıracak şekilde canvas ölçeğini belirler.
- `SafeArea` çentik ve köşe boşluklarını hesaba katar; `UIRegionFitter` içeriği iki çubuk arasındaki alana sığdırıp ortalar.
- `PlayfieldCameraFit` kamerayı, oyun kutusu üst ve alt panel arasına sığacak şekilde ayarlar.
- Yeni UI öğeleri uygun kenara çapalanıp `SafeArea` altına konduğunda bu sisteme otomatik dahil olur.

### Yerelleştirme
- Metinler `Localization` sözlüğünde anahtar-değer olarak tutulur; `LocalizedText` bileşeni ilgili metni dil değişince günceller.

## Planlanan Özellikler

- **Güçlendiriciler:** Dondur (sahneyi yavaşlatır), Bomba (seçilen noktada patlatır), Kalkan (tavan temasında bir kez korur). Arayüz ve görsel durumlar (hazır, aktif, boş) hazırdır; oyun mekaniği henüz yoktur.
- **Yeni top çeşitleri:** Zamanla ve seviyeyle birlikte oyuna eklenecek yeni renkler ve türler.
- **Zaman ve seviyeyle zorlaşma:** Mevcut atış hızı ve güç artışına ek olarak yeni zorluk etkenleri.
- **Ödül, çark, mağaza ve sıralama:** Menüdeki butonlar yer tutucudur.
- **Can ve coin sistemi:** Menü göstergeleri şimdilik sabit değerlidir.
- **Reklam ve elmasla devam:** Oyun sonu panelindeki butonların işlevi.
- **Menüdeki sabit bilgiler:** Güçlendirici envanteri gerçek verilere bağlanacaktır.
