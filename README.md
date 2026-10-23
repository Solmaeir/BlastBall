# BlastBall

Dikey ekranlı, mobil odaklı bir 2D bulmaca-arcade oyunu. Üst köşelerdeki iki spawner renkli gezegen toplarını bir kutunun içine düşürür; oyuncu topları sürükleyerek aynı renkten gruplar oluşturur ve patlatır. Kutu dolup toplar tavana dayandığında oyun biter.


<img width="230" height="512" alt="image" src="https://github.com/user-attachments/assets/7670b24a-44b7-4e2e-9d36-39d8c6d15945" /> <img width="230" height="512" alt="image" src="https://github.com/user-attachments/assets/4834a89b-d80b-4b83-bbbd-d691822d6c9c" /> 


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
- Her 5 seviyede bir güçlendirici hakkı kazanılır; 10, 20 ve 30. seviyelerde yeni düşman top türleri açılır (aşağıda).

### Toplar
- Dört çeşit: kırmızı, mavi, mor, sarı.
- Eşleşme topun etiketiyle (tag) yapılır; komşu algılama yarıçapı topun kendi boyutuna oranlıdır.
- `BallMatching` iki kural bayrağı taşır: `Matchable` (kapalıysa eşleşmeye girmez; yanmış ve düşman toplar) ve `Counts For Game Over` (kapalıysa üst çerçeveye değmesi oyunu bitirmez).
- Toplar `Discrete` çarpışma algılama ve interpolation kullanır; kutu duvarları fırlatmaların içinden geçmemesi için kalın collider'lıdır.

### Skor ve seviye
- Patlayan her top 10 puan verir.
- Seviye, toplam skor eşiğine göre atlanır. Seviye n'i geçmek için gereken toplam skor `35 × (5 + 6 + … + (4+n))` formülüyle hesaplanır (175, 385, 630, …). Tek bir büyük patlatma birden fazla seviye atlatabilir.
- HUD'da seviye rozeti, ilerleme çubuğu, hedef skor ve oynanan süre gösterilir.

### Zorluk
- `DifficultyDirector`, oynanan süre ve seviye ilerlemesinden 0 ile 1 arasında bir zorluk değeri üretir. Eğri yumuşaktır ve üst sınıra yaklaşarak durur.
- Başlangıç atış aralığı 1,2 sn'dir (her spawner için). Zorluk arttıkça aralık başlangıcın %50'sine kadar kısalır ve fırlatma gücü artar (başlangıcın 1,3 katına kadar).
- Zorluk yalnızca süreyle 100 sn'de, yalnızca seviyeyle 4 seviyede yarıya ulaşır.
- Duraklatma ve oyun sonu süreye sayılmaz.
- Ayarlar `DifficultyDirector` bileşeninde Inspector'dan değiştirilebilir.

### Güçlendiriciler
Her oyun başında her güçlendiriciden birer hak verilir. Butonlar seviye atlayınca açılır; açılmamış butonlar kilit rozetiyle gri görünür.

| Güçlendirici | Tür | Açılış | Etki |
|---|---|---|---|
| Dondur | Süreli (6 sn) | Seviye 2 | Süre boyunca yeni top gelmez ve game over sayacı durur. Aktifken tekrar basılırsa bir hak harcanır ve süre baştan başlar. Bitmeden 1,5 sn önce ekran yanıp sönerek uyarır. |
| Bomba | Anlık | Seviye 3 | Butona basınca hedefleme moduna geçilir ve oyun durur. Havuz içinde bir noktaya dokunup bırakınca çevresindeki (yarıçap 2 top boyu) tüm toplar renge bakmadan patlar, normal puan verir. Butona tekrar basmak veya havuz dışına dokunmak hak harcamaz. |
| Kalkan | Pasif (bir kez) | Seviye 6 | Butona basınca kurulur ve havuzun üstünde bir çizgi görünür. Oyun bitecekken game over yerine üst çizgi yakınındaki toplar temizlenir, oyun devam eder ve kalkan harcanır. Aynı anda tek kalkan olabilir. |

- Görsel ve ses efektleri: buz kenarlığı ve mavi filtre (Dondur), hedefleme halkası, patlama efekti, ekran sarsıntısı ve patlama sesi (Bomba), kalkan çizgisi ve kırılma efekti (Kalkan).
- Hakkı biten buton "+" rozetiyle gösterilir; bu butona basılması `PowerUpManager.EmptyPressed` olayını tetikler (reklam / mağaza buraya bağlanacaktır).
- Dışarıdan hak vermek için `PowerUpManager.Grant(tanım, adet)` kullanılır.
- Seviye ödülü (`LevelRewardGiver`): her 5 seviyede bir (5, 10, 15…) açık güçlendiricilerden rastgele birine 1 hak verilir; buton parlar, ses ve titreşim çalar. Aralık ve adet Inspector'dan ayarlanır. Ödül tek bir noktadan (`Offer`) verildiği için web sürümünde reklam izleme şartı oraya bağlanacaktır.

### Düşman toplar
Belirli seviyelerde yeni bir düşman top türü açılır. Açıldığı an ilk topu bir sonraki atışta düşer, oyuncuya karşı güçlendiricisinden bir hak verilir ve tür ilk kez görülüyorsa tanıtım penceresi açılır (oyun duraklar). Sonrasında normal atışların arasına seyrek karışır. Düşman toplar renk eşleşmesine girmez ve sürüklenemez; yalnızca kendi karşı güçlendiricisiyle yok edilir.

| Top | Açılış | Davranış | Karşı güçlendirici |
|---|---|---|---|
| Karıştırıcı | Seviye 10 | Yerçekimsiz; topların biriktiği alt kısımda hızlı ve kıvrak yönlerde yüzer, değdiği her topu girdap yönünde savurarak yığını dağıtır. Üst çerçeveye değmesi oyunu bitirmez. | Bomba (patlama alanındaysa) |
| Yanan Top | Seviye 20 | Magma görünümlü, çevresinde yanıp sönen turuncu parıltı. Değdiği topları yakar; yanan toplar kömür plakalarıyla kaplanır, parıldar ve eşleşmeye giremez. | Dondur (kullanılınca söner, yanan toplar eski rengine döner) |
| Kara Delik | Seviye 30 | Alanın üst kısmında süzülür, çevresindeki topları mıknatıs gibi güçlü çekip havada toplar ve renkleri karıştırır. | Kalkan (kurulunca kapanır) |

- Her türün açılış seviyesi, atış başına gelme olasılığı (%3,5–5), aynı anda en fazla adedi (1) ve iki top arası en kısa süresi (30–45 sn) kendi `SpecialBallDefinition` asset'indedir. İki düşman top arasında ayrıca en az 10 sn bulunur.
- Davranış ayarları prefab'lardadır: Karıştırıcı'da hız, tercih edilen yükseklik ve savurma gücü (`Kick Speed`, `Swirl`, `Kick Interval`); Kara Delik'te çekim yarıçapı ve gücü (`Pull Acceleration`); Yanan Top'ta yakma aralığı.
- Güçlendiriciler düşman toplarla iki yoldan etkileşir: alan etkileri (Bomba patlaması, Kalkan temizliği) dayanıklı topları atlar (`SpecialBall.Resists`), kullanım anı etkileri (Dondur, Kalkan) ilgili tüm topları etkisizleştirir (`SpecialBall.CounterAll`).
- Yeni tür eklemek: `SpecialBall`'dan türeyen davranış + prefab + `SpecialBallDefinition` asset'i, ardından sahnedeki `SpecialBallDirector` listesine eklemek.
- Görseller `UI/SpecialBalls/generate_special_ball_sprites.py` ile üretilir (Voronoi magma deseni, parıltılar, girdaplar, tanıtım ikonları ve "Nasıl Oynanır" çizimi).

### Test modu
Oyun sahnesinde iki işaret vardır (varsayılan kapalı). Açılınca oyun 1. seviyede üç düşman topla ve tüm güçlendiriciler açık başlar; düşman topları hızlıca denemek için kullanılır. **Yayından önce kapalı olmalıdır:**
- `SpecialBallDirector` → `Unlock All At Start`
- `PowerUps` (`PowerUpManager`) → `Unlock All For Testing`

### Oyun sonu
- Bir top, üst çerçeveye kesintisiz 2 saniye temas ederse oyun biter. Kısa süreli çarpmalar ve zıplamalar sayılmaz.
- Oyun sonu paneli skoru ve en yüksek skoru gösterir; ana menüye dönüş sunar. Reklam izleme ve elmasla devam butonları arayüzde yer alır, işlevleri henüz bağlı değildir.
- Oyun her seferinde 1. seviyeden başlar.

### Nasıl Oynanır
- Oyuncunun ilk girişinde ana menüde kendiliğinden açılır; sonra ayarlar panelindeki "Nasıl Oynanır" butonundan açılır. "6 veya daha fazla aynı renk" kuralı görsel ve metinle öne çıkarılır.
- Pencere (`InfoPopup` prefab'ı) veriyle doldurulur; düşman top tanıtımları da aynı prefab'ı kullanır.

### Kayıt
- En yüksek seviye ve en yüksek skor cihazda yerel olarak saklanır (`PlayerPrefs`) ve ana menüde gösterilir.
- Rekor kırıldığı anda kaydedilir.

### Menü ve ayarlar
- Ana menü: oynat butonu, can ve coin göstergesi, rekor kartı, güçlendirici envanteri, ödül/çark/mağaza/sıralama butonları. Can, coin, envanter, ödül, çark, mağaza ve sıralama şimdilik görsel yer tutucudur.
- Ayarlar (`SettingsPanel` prefab'ı; ana menü ve oyun içi duraklatma aynı prefab'ı kullanır): müzik sesi kaydırıcısı, tüm sesleri kapatan anahtar, titreşim anahtarı, "Nasıl Oynanır", dil (Türkçe / İngilizce) ve bilgi sayfası (pakuegames, Instagram ve X butonları, iletişim e-postası). Sosyal medya bağlantıları `OpenUrlButton` üzerinde boş bırakılmıştır. Değerler `GameSettings` ile `PlayerPrefs`'te saklanır.
- Oyun içi duraklatma menüsü; ana menüye dönüş seçeneği.
- Android geri tuşu ve Escape açık panelleri kapatır.

## Teknik Yapı

| Klasör | İçerik |
|---|---|
| `Scripts/Core` | `GameManager` (durum, oynanan süre), `GameOverDetector`, `DifficultyDirector`, kare hızı kurulumu |
| `Scripts/BallScripts` | Top girişi, sürükleme/fırlatma, eşleşme, spawner, skor ve seviye |
| `Scripts/PowerUps` | Güçlendirici sistemi: `PowerUpManager`, `PowerUpDefinition` (veri paketi), `PowerUpRuntime`, üç güçlendirici türü ve efekt bileşenleri |
| `Scripts/Audio` | Ses sistemi: `AudioManager`, buton sesi, top çarpma sesi, oyun olayı sesleri |
| `Scripts/SpecialBalls` | Düşman toplar: `SpecialBall` (taban), üç davranış, `BallBurn` (yanık durumu), `SpecialBallDefinition`, `SpecialBallDirector` |
| `Scripts/Pooling` | Genel amaçlı nesne havuzu (`PoolManager`, `PooledObject`, `IPoolable`) |
| `Scripts/Save` | Yerel kayıt (`PlayerProgress`) |
| `Scripts/Settings` | Yerelleştirme, oyuncu tercihleri (`GameSettings`) ve ayarlar menüsü |
| `Scripts/Loading` | Splash ve sahne yükleme |
| `Scripts/Haptics` | Titreşim servisi ve platform motorları |
| `Scripts/UI` | Paneller, bilgi penceresi (`InfoPopup`, `HowToPlay`), HUD görünümleri, ölçekleme ve güvenli alan bileşenleri |
| `UI/GameHUD`, `UI/MainMenu`, `UI/PowerUps`, `UI/SettingsPanel`, `UI/InfoPopup`, `UI/SpecialBalls`, `UI/GameOver`, `UI/Shared` | Arayüz ve top görselleri (çoğu yanındaki Python betiğiyle üretilir), fontlar ve prefab'lar |
| `ScriptableObjects/PowerUps` | Güçlendirici veri paketleri |
| `ScriptableObjects/SpecialBalls` | Düşman top veri paketleri |
| `Resources/Audio` | `AudioManager` prefab'ı, `SFX/` ses efektleri ve `Music/` müzikleri |
| `Tools/Audio` | Ses efektlerini üreten Python betikleri |
| `Prefabs`, `Prefabs/SpecialBalls` | Toplar, spawner'lar ve düşman toplar |
| `Settings` | URP ayarları, Input System eylemleri (`InputSystem_Actions`), varsayılan Volume profili |
| `Plugins/iOS` | iOS titreşim eklentisi |
| `Editor` | Prefab üreten editör araçları ve ses içe aktarma ayarları |

`UI/Shared` birden fazla ekranın kullandığı ortak içeriği tutar: arka plan, gezegen top görselleri, panel sprite'ları ve Baloo2 fontunun kaynak dosyası. Kullanılmayan üçüncü parti paketler projeden çıkarılmıştır; yalnızca kullanılan görseller `UI/Shared` altına taşınmıştır.

### Nesne havuzu
- Her top çeşidi için ayrı bir havuz bulunur: `PoolManager > <Prefab> Pool > toplar`.
- Toplar oyun sırasında oluşturulup yok edilmez; aktif/pasif yapılarak yeniden kullanılır. Havuzlar oyun başında önceden doldurulur.
- Yeni bir top çeşidi için prefab'ı `PoolManager`'ın giriş listesine ve spawner'ların prefab dizisine eklemek yeterlidir.
- Düşman toplar (birer adet) ve patlama efekti (iki adet) de havuzda önceden hazırlanır; ilk kullanımda takılma olmaz. Listede olmayan prefab'lar ilk kullanımda otomatik havuzlanır.

### Güçlendirici sistemi
- Tüm mantık `PowerUpManager` içinde toplanır: hak sayıları, seviye kilidi, buton etkileşimi ve diğer sistemlerle köprü.
- Her güçlendirici bir `ScriptableObject` veri paketidir (`PowerUpDefinition`): kimlik, etiket, açılış seviyesi, başlangıç hakkı, buton görselleri, ses ve türe özel ayarlar (süre, yarıçap, temizleme derinliği). Oyun içi durum ayrı bir `PowerUpRuntime` nesnesinde tutulur; asset'ler oyun sırasında değişmez.
- Diğer sistemlerle bağlantı: `BallSpawner` atışı `SpawningBlocked` ile durdurur, `GameOverDetector` `GameOverPaused` ile bekler ve oyun bitmeden önce `TryAbsorbGameOver` sorar, `BallInputController` `InputLocked` iken top seçmez.
- Yeni güçlendirici eklemek:
  - Mevcut türde (ör. daha uzun süreli bir Dondur, daha büyük yarıçaplı bir Bomba): `Create > BlastBall > Power-Ups` ile yeni asset oluşturup değerleri girmek ve `PowerUpManager` listesine eklemek yeterlidir. Listede buton atanmazsa buton prefab'dan otomatik oluşturulur ve satıra yerleşir.
  - Yeni davranış: `PowerUpDefinition` ve `PowerUpRuntime` alt sınıfları yazılır.

### Ses sistemi
- `AudioManager` tek seferlik (`DontDestroyOnLoad`) çalışır; oyun hangi sahneden başlarsa başlasın `Resources/Audio/AudioManager` prefab'ından kendiliğinden oluşur. Ses seviyesi/sessize alma ayarları `AudioListener.volume` üzerinden tüm sesi etkiler.
- Müzik: `Loading`, `Menu`, `Game` ve `GameOver` yerleri prefab üzerindeki `Music` listesinde bulunur. Her yer için klip, ses seviyesi, döngü ve geçiş süresi Inspector'dan ayarlanır. Hangi sahnede hangi müziğin çalacağı `Scene Music` listesindedir; müzikler çapraz geçişle (yeni müziğin `Fade Seconds` süresiyle) değişir. Yeni sahnenin müzik yeri boşsa önceki müzik kendi `Fade Seconds` süresince sönerek durur.
- Geçiş (fade) ile oyuncunun müzik sesi ayrıdır: ayarlardaki müzik kaydırıcısı anında uygulanır; sıfıra çekilince müzik susar ama durmaz, açılınca kaldığı yerden duyulur.
- Ses efektleri: `Sfx` listesindeki her kimlik (`SfxId`) için birden fazla klip (rastgele seçilir), ses seviyesi, perde değişimi ve en kısa tekrar aralığı ayarlanır.
- Otomatik sesler:
  - Toplar çarpınca (`BallCollisionSound`): top-top için bilardo tıkırtısı, top-duvar için yumuşak tok ses. Ses seviyesi çarpma hızına, sağ-sol dengesi top konumuna göre değişir; çok yavaş temaslar sessizdir.
  - Butonlar: sahne açılırken `ButtonSound` olmayan her butona varsayılan tık sesi eklenir. Farklı ses için butona `ButtonSound` ekleyip `Sound` alanı seçilir (`None` = sessiz).
  - Oyun olayları (`GameFeedbackEvents`): başlangıç, seviye atlama, oyun sonu, top grubu patlaması (ses + titreşim).
  - Ses kalabalığı sınırı: `Sfx` girişindeki `Max Voices` aynı anda çalabilecek ses sayısını sınırlar. Top-top ve top-duvar sesleri ortak bir bütçeyi paylaşır (`Shared Limit With`, en fazla 3); dolunca yeni çağrılar hiç başlatılmaz.
  - Top tutma, fırlatma ve patlatma; güçlendirici açma/kullanma/reddedilme sesleri.
- Yeni ses kimliği eklemek: `SfxId` listesinin sonuna ekleyip prefab'daki `Sfx` listesine bir giriş eklemek yeterlidir.
- Klipleri `Resources/Audio/SFX` (kısa sesler) veya `Resources/Audio/Music` (müzik) klasörüne koymak, içe aktarma ayarlarını otomatik yapar (SFX: mono + ADPCM, Music: akış + Vorbis).

### Titreşim
- `Haptics.Play(HapticType)` tek giriş noktasıdır; ayarlardaki titreşim anahtarına uyar ve çok sık çağrıları sınırlar.
- Platform motorları: Android'de `Vibrator` (`VibrationEffect`), iOS'ta `Plugins/iOS/BlastBallHaptics.mm` (UIKit geri bildirim üreteçleri), editörde işlem yapmaz.
- Güçlendiriciler titreşim türünü `PowerUpDefinition` üzerinden alır; top patlaması, seviye atlama ve oyun sonu `GameFeedbackEvents` ile tetiklenir.

### Giriş
- `BallInputController` dokunmatik, fare ve kalemi tek yoldan okur. Basılan noktaya en yakın top seçilir; UI üzerine basılırsa veya oyun duraklatılmışsa seçim yapılmaz.

### Ekran uyumu
- `AdaptiveCanvasScaler`, tasarımın güvenli alan kutusunu her cihazda güvenli alana sığdıracak şekilde canvas ölçeğini belirler.
- `SafeArea` çentik ve köşe boşluklarını hesaba katar; `UIRegionFitter` içeriği iki çubuk arasındaki alana sığdırıp ortalar.
- `PlayfieldCameraFit` kamerayı, oyun kutusu üst ve alt panel arasına sığacak şekilde ayarlar.
- Yeni UI öğeleri uygun kenara çapalanıp `SafeArea` altına konduğunda bu sisteme otomatik dahil olur.

### Yerelleştirme
- Metinler `Localization` sözlüğünde anahtar-değer olarak tutulur; `LocalizedText` bileşeni ilgili metni dil değişince günceller.

### Yazı tipi
- Arayüzün ana fontu `UI/GameHUD/Fonts/Baloo2-ExtraBold SDF` (statik atlas); kaynak TTF `UI/Shared/Fonts` altındadır.
- Font asset'indeki kerning (harf çifti) tablosu hatalı ölçekle içe aktarıldığı için harfler iç içe giriyordu; tablo temizlenmiştir. Atlas Font Asset Creator ile yeniden üretilirse kerning tablosunun tekrar temizlenmesi gerekir.

## Planlanan Özellikler

- **Güçlendirici kaynakları:** Reklam (web sürümünde seviye ödülü reklam izlenince verilecek; bağlantı noktası `LevelRewardGiver.Offer`) ve mağaza (başlangıç hakkını artırma, güçlendirici seviye yükseltme). Hak ekleme noktaları hazırdır (`Grant`, `EmptyPressed`).
- **Boost barı:** Patlatılan top sayısına göre dolan bar; güçlendirici hakkı kazandırması planlanmaktadır.
- **Yeni düşman toplar:** 30. seviyeden sonrası için yeni türler.
- **Ödül, çark, mağaza ve sıralama:** Menüdeki butonlar yer tutucudur.
- **Can ve coin sistemi:** Menü göstergeleri şimdilik sabit değerlidir.
- **Reklam ve elmasla devam:** Oyun sonu panelindeki butonların işlevi.
- **Menüdeki sabit bilgiler:** Güçlendirici envanteri gerçek verilere bağlanacaktır.
