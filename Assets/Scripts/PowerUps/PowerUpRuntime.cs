// Bir güçlendiricinin oyun içi durumu ve davranışı. Veri paketi (ScriptableObject) değişmez, durum burada tutulur.
public abstract class PowerUpRuntime
{
    protected PowerUpSlot Slot { get; }
    protected PowerUpManager Manager => Slot.Manager;

    protected PowerUpRuntime(PowerUpSlot slot)
    {
        Slot = slot;
    }

    // Butona basıldı. Yönetici, kilit ve hak kontrolünü zaten yapmıştır (Count > 0).
    public abstract void Press();

    // Slot aktifken her karede çağrılır.
    public virtual void Tick() { }

    public virtual bool BlocksSpawning => false;
    public virtual bool PausesGameOver => false;
    public virtual bool LocksBallInput => false;

    // Oyun bitecekken çağrılır; true dönerse oyun bitmez.
    public virtual bool TryAbsorbGameOver() => false;

    public virtual void Dispose() { }
}
