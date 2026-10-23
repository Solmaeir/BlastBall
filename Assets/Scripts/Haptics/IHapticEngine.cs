public enum HapticType
{
    None = 0,
    Light,
    Medium,
    Heavy,
    Success
}

// Platforma özgü titreşim uygulaması. Haptics sınıfı çalışma anında uygun olanı seçer.
public interface IHapticEngine
{
    void Play(HapticType type);
}
