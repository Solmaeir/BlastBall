using UnityEngine;
using UnityEngine.UI;

// Butona basılınca ses çalar. Bileşeni olmayan butonlara AudioManager sahne açılırken varsayılan tık sesiyle ekler;
// farklı ses için butona elle ekleyip "Sound" alanını değiştirmek yeterlidir (None = sessiz).
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    [SerializeField] private SfxId sound = SfxId.ButtonClick;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }

    private void Play()
    {
        AudioManager.PlaySfx(sound);
    }
}
