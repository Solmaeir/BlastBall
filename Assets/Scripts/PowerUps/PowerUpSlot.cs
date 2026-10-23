// Bir güçlendirici için çalışma anı kaydı: hak sayısı, durum ve butonu.
public class PowerUpSlot
{
    public PowerUpManager Manager { get; }
    public PowerUpDefinition Definition { get; }
    public BoosterButtonView View { get; }
    public PowerUpRuntime Runtime { get; internal set; }

    public int Count { get; private set; }
    public bool IsUnlocked { get; internal set; }
    public bool IsActive { get; private set; }

    internal PowerUpSlot(PowerUpManager manager, PowerUpDefinition definition, BoosterButtonView view)
    {
        Manager = manager;
        Definition = definition;
        View = view;
        Count = definition.startingCount;
    }

    public void Add(int amount)
    {
        Count += amount;
        Refresh();
    }

    public void Consume()
    {
        if (Count <= 0) return;
        Count--;
        Refresh();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        Refresh();
    }

    // 0..1; süreli güçlendiricilerde kalan süre halkası.
    public void SetProgress(float normalized)
    {
        View.SetTimer(normalized);
    }

    internal void Refresh()
    {
        View.SetCount(Count);
        View.SetState(!IsUnlocked ? BoosterVisualState.Locked
                    : IsActive ? BoosterVisualState.Active
                    : Count > 0 ? BoosterVisualState.Ready
                    : BoosterVisualState.Empty);
    }
}
