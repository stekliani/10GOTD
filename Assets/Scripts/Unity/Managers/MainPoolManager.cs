using System.Collections;
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
    private List<IPoolable> _activeObjectsList = new();
    public static MainPoolManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public IEnumerator InitializePoolsAsync(System.Action<float> onProgress)
    {
        lookup.Clear();

        int total = 0;
        foreach (var pool in pools)
            total += pool.initialSize;

        int created = 0;

        foreach (var pool in pools)
        {
            pool.objects = new Queue<IPoolable>();

            for (int i = 0; i < pool.initialSize; i++)
            {
                CreateOne(pool);
                created++;

                onProgress?.Invoke((float)created / total);

                // Spread work across frames
                if (created % 5 == 0)
                    yield return null;
            }

            lookup[pool.prefab] = pool;
        }

        onProgress?.Invoke(1f);
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
        _activeObjectsList.Add(poolable);

        return poolable.gameObject;
    }

    public void Return(IPoolable poolable)
    {
        if (poolable == null) return;

        // IPoolable is an interface; if the underlying Unity object was destroyed,
        // Unity's custom null checks won't run unless we cast back to MonoBehaviour.
        var mb = poolable as MonoBehaviour;
        if (!mb) return; // destroyed or invalid

        var go = mb.gameObject;
        if (!go) return; // extra safety

        go.SetActive(false);
        go.transform.SetParent(transform);

        _activeObjectsList.Remove(poolable);

        var key = poolable.PrefabKey;
        if (key == null) return;
        if (!lookup.TryGetValue(key, out var pool)) return;
        pool.objects.Enqueue(poolable);
    }

    public void ReturnAllActiveObjects()
    {
        for (int i = _activeObjectsList.Count - 1; i >= 0; i--)
        {
            Return(_activeObjectsList[i]);
        }

        _activeObjectsList.Clear();
    }
}

public interface IPoolable
{
    GameObject gameObject { get; }          // MonoBehaviours have this already
    MonoBehaviour PrefabKey { get; }        // which pool this belongs to
    void Init(MainPoolManager manager, MonoBehaviour prefabKey);
}