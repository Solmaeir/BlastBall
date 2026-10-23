using UnityEngine;

// Bir düşman top türünün veri paketi: ne zaman açılır, ne sıklıkla gelir, hangi güçlendirici onu yok eder.
// Yeni tür eklemek: SpecialBall'dan türeyen bir davranış + prefab + bu asset (Create > BlastBall > Special Ball),
// ardından sahnedeki SpecialBallDirector listesine eklemek.
[CreateAssetMenu(menuName = "BlastBall/Special Ball", fileName = "SpecialBall")]
public class SpecialBallDefinition : ScriptableObject
{
    [Header("Kimlik")]
    public string id;
    [Tooltip("Localization anahtarları: tanıtım penceresindeki ad ve açıklama.")]
    public string nameKey;
    public string descriptionKey;
    public Sprite icon;
    public GameObject prefab;

    [Header("Ortaya Çıkış")]
    [Tooltip("Bu seviyeye gelince tür açılır ve ilk topu hemen düşer.")]
    [Min(1)] public int unlockLevel = 10;
    [Tooltip("Açıldıktan sonra her normal atışın bu türe dönüşme olasılığı.")]
    [Range(0f, 1f)] public float spawnChance = 0.04f;
    [Tooltip("Sahnede aynı anda en fazla kaç tane olabilir.")]
    [Min(1)] public int maxAlive = 1;
    [Tooltip("Aynı türden iki top arasındaki en kısa süre (oynanan saniye).")]
    [Min(0f)] public float minInterval = 25f;

    [Header("Karşı Güçlendirici")]
    [Tooltip("Bu topu yalnızca bu güçlendirici yok eder; diğerleri ona işlemez.")]
    public PowerUpDefinition counter;
    [Tooltip("Tür açıldığında oyuncuya verilen karşı güçlendirici hakkı.")]
    [Min(0)] public int counterGrantOnUnlock = 1;

    [Header("Yok Olma Efekti")]
    [Tooltip("BlastEffect içeren prefab (isteğe bağlı).")]
    public BlastEffect neutralizeEffect;

    public PopupContent Intro => new PopupContent
    {
        headerKey = "new_ball",
        titleKey = nameKey,
        bodyKey = descriptionKey,
        image = icon
    };
}
