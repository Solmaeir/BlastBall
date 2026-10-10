using UnityEngine.InputSystem;

// Masaüstünde Escape, Android'de geri tuşu (Input System ikisini de Keyboard.escapeKey olarak verir).
public static class BackButton
{
    public static bool WasPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
    }
}
