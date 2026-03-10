using System.Collections.Generic;
using UnityEngine;

public class MainPoolManager : MonoBehaviour
{
    [System.Serializable]
    private class Pool
    {
        public MonoBehaviour prefab;        // Serializable, drag any MB in Inspector
        public int initialSize = 32;
        [HideInInspector] public Queue<IPoolable> objects;
    }

    [SerializeField] private List<Pool> pools = new();
    private Dictionary<MonoBehaviour, Pool> lookup = new();

    public static MainPoolManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        foreach (var pool in pools)
        {
            pool.objects = new Queue<IPoolable>();
            for (int i = 0; i < pool.initialSize; i++)
                CreateOne(pool);
            lookup[pool.prefab] = pool;
        }
    }

    private IPoolable CreateOne(Pool pool)
    {
        var go = Instantiate(pool.prefab.gameObject, transform);
        go.SetActive(false);
        var poolable = go.GetComponent<IPoolable>();
        poolable.Init(this, pool.prefab);   // pass MB as the key
        pool.objects.Enqueue(poolable);
        return poolable;
    }

    public GameObject Get(MonoBehaviour prefabKey)
    {
        if (!lookup.TryGetValue(prefabKey, out var pool))
        {
            Debug.LogError($"No pool for {prefabKey.name}");
            return null;
        }

        if (pool.objects.Count == 0)
            CreateOne(pool);

        var poolable = pool.objects.Dequeue();
        return poolable.gameObject;
    }

    public void Return(IPoolable poolable)
    {
        poolable.gameObject.SetActive(false);
        poolable.gameObject.transform.SetParent(transform);
        lookup[poolable.PrefabKey].objects.Enqueue(poolable);
    }
}

public interface IPoolable
{
    GameObject gameObject { get; }          // MonoBehaviours have this already
    MonoBehaviour PrefabKey { get; }        // which pool this belongs to
    void Init(MainPoolManager manager, MonoBehaviour prefabKey);
}