using UnityEngine;
using UnityEngine.UI;

// Basılınca bir bağlantıyı açar. Url boş bırakılırsa (henüz bağlanmadıysa) hiçbir şey yapmaz.
[RequireComponent(typeof(Button))]
public class OpenUrlButton : MonoBehaviour
{
    [SerializeField] private string url;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Open);
    }

    private void Open()
    {
        if (!string.IsNullOrWhiteSpace(url)) Application.OpenURL(url);
    }
}
