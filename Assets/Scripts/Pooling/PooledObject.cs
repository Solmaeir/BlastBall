using UnityEngine;

// PoolManager tarafından oluşturulan her instance'a otomatik eklenir.
public class PooledObject : MonoBehaviour
{
    internal PoolManager Manager;
    internal GameObject Prefab;
    internal bool InPool;

    private IPoolable[] poolables;

    internal void Cache()
    {
        poolables = GetComponents<IPoolable>();
    }

    internal void NotifySpawned()
    {
        foreach (IPoolable poolable in poolables) poolable.OnSpawnedFromPool();
    }

    internal void NotifyReturned()
    {
        foreach (IPoolable poolable in poolables) poolable.OnReturnedToPool();
    }

    public void Release()
    {
        if (Manager != null) Manager.Return(this);
        else Destroy(gameObject);
    }
}
