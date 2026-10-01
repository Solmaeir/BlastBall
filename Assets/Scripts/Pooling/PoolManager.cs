using System.Collections.Generic;
using UnityEngine;

// Hiyerarşi: PoolManager > "<Prefab> Pool" > instance'lar.
// Instance'lar asla silinmez; aktif/pasif yapılarak yeniden kullanılır.
// PoolManager'ı (0,0,0) konumunda, rotasyonsuz ve scale 1 tut.
public class PoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public GameObject prefab;
        [Tooltip("Oyun başında önceden oluşturulacak instance sayısı.")]
        [Min(0)] public int prewarmCount = 10;
        [Tooltip("Sahneye elle konmuş, bu prefab'dan gelen objeler. Havuza dahil edilir; patlayınca silinmek yerine havuza döner.")]
        public List<Transform> sceneInstances = new List<Transform>();
    }

    private class Pool
    {
        public GameObject prefab;
        public Transform container;
        public readonly Stack<PooledObject> inactive = new Stack<PooledObject>();
    }

    public static PoolManager Instance { get; private set; }

    [Tooltip("Listeye istediğin kadar prefab eklenebilir. Listede olmayan prefab'lar ilk kullanımda otomatik havuzlanır.")]
    [SerializeField] private List<PoolEntry> entries = new List<PoolEntry>();

    private readonly Dictionary<GameObject, Pool> pools = new Dictionary<GameObject, Pool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (PoolEntry entry in entries)
        {
            if (entry.prefab == null) continue;

            Pool pool = GetOrCreatePool(entry.prefab);

            foreach (Transform sceneInstance in entry.sceneInstances)
            {
                if (sceneInstance != null) Adopt(pool, sceneInstance.gameObject);
            }

            Prewarm(pool, entry.prewarmCount);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Sahnede PoolManager yoksa normal Instantiate'e düşer.
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (Instance == null) return Instantiate(prefab, position, rotation);
        return Instance.GetFromPool(prefab, position, rotation);
    }

    // Havuza ait olmayan objeler Destroy edilir.
    public static void Despawn(GameObject instance)
    {
        if (instance.TryGetComponent(out PooledObject pooled)) pooled.Release();
        else Destroy(instance);
    }

    private GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        Pool pool = GetOrCreatePool(prefab);

        PooledObject obj = pool.inactive.Count > 0 ? pool.inactive.Pop() : CreateInstance(pool);

        obj.InPool = false;
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);

        if (obj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        obj.NotifySpawned();
        return obj.gameObject;
    }

    internal void Return(PooledObject obj)
    {
        if (obj.InPool) return;

        obj.InPool = true;
        obj.NotifyReturned();
        obj.gameObject.SetActive(false);
        pools[obj.Prefab].inactive.Push(obj);
    }

    private Pool GetOrCreatePool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Pool pool))
        {
            Transform container = new GameObject(prefab.name + " Pool").transform;
            container.SetParent(transform, false);

            pool = new Pool { prefab = prefab, container = container };
            pools.Add(prefab, pool);
        }

        return pool;
    }

    private void Prewarm(Pool pool, int count)
    {
        for (int i = 0; i < count; i++)
        {
            PooledObject obj = CreateInstance(pool);
            obj.InPool = true;
            obj.gameObject.SetActive(false);
            pool.inactive.Push(obj);
        }
    }

    private PooledObject CreateInstance(Pool pool)
    {
        GameObject go = Instantiate(pool.prefab, pool.container);
        return Register(pool, go);
    }

    private void Adopt(Pool pool, GameObject go)
    {
        go.transform.SetParent(pool.container, true);
        Register(pool, go).InPool = false;
    }

    private PooledObject Register(Pool pool, GameObject go)
    {
        PooledObject obj = go.GetComponent<PooledObject>();
        if (obj == null) obj = go.AddComponent<PooledObject>();

        obj.Manager = this;
        obj.Prefab = pool.prefab;
        obj.Cache();
        return obj;
    }
}
