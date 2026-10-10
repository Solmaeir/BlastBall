using TMPro;
using UnityEngine;

// Oynanan süreyi dk:sn olarak gösterir. Metin yalnızca saniye değişince güncellenir.
public class GameTimerView : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private readonly char[] buffer = new char[6];
    private int shownSeconds = -1;

    private void Update()
    {
        if (label == null || GameManager.Instance == null) return;

        int seconds = Mathf.FloorToInt(GameManager.Instance.PlayTime);
        if (seconds == shownSeconds) return;

        shownSeconds = seconds;
        int minutes = Mathf.Min(seconds / 60, 999);
        int length = 0;
        if (minutes >= 100) buffer[length++] = (char)('0' + minutes / 100);
        buffer[length++] = (char)('0' + minutes / 10 % 10);
        buffer[length++] = (char)('0' + minutes % 10);
        buffer[length++] = ':';
        buffer[length++] = (char)('0' + seconds % 60 / 10);
        buffer[length++] = (char)('0' + seconds % 10);
        label.SetText(buffer, 0, length);
    }
}
