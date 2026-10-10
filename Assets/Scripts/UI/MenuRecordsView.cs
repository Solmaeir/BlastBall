using System.Globalization;
using TMPro;
using UnityEngine;

// Ana menüde cihazda kayıtlı en yüksek seviye ve skoru gösterir.
public class MenuRecordsView : MonoBehaviour
{
    [SerializeField] private TMP_Text bestLevelText;
    [SerializeField] private TMP_Text bestScoreText;

    private static readonly CultureInfo Turkish = new CultureInfo("tr-TR");

    private void OnEnable()
    {
        PlayerProgress.Changed += Refresh;
        Localization.LanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        PlayerProgress.Changed -= Refresh;
        Localization.LanguageChanged -= Refresh;
    }

    private void Refresh()
    {
        CultureInfo culture = Localization.Current == Language.Turkish ? Turkish : CultureInfo.InvariantCulture;
        if (bestLevelText != null) bestLevelText.text = PlayerProgress.BestLevel.ToString();
        if (bestScoreText != null) bestScoreText.text = PlayerProgress.BestScore.ToString("N0", culture);
    }
}
