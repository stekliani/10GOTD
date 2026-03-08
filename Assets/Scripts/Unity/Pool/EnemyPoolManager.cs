using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    [System.Serializable]
    private class Pool
    {
        public EnemyStats prefab;
        public int initialSize = 32;

        [HideInInspector] public Queue<EnemyStats> objects;
    }

    [SerializeField] private List<Pool> pools = new();

    private Dictionary<EnemyStats, Pool> lookup;

    public static EnemyPoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        lookup = new Dictionary<EnemyStats, Pool>();

        foreach (var pool in pools)
        {
            pool.objects = new Queue<EnemyStats>();

            for (int i = 0; i < pool.initialSize; i++)
                Create(pool);

            lookup.Add(pool.prefab, pool);
        }
    }

    private EnemyStats Create(Pool pool)
    {
        EnemyStats p = Instantiate(pool.prefab, transform);
        p.gameObject.SetActive(false);
        p.SetPoolManager(this, pool.prefab);
        pool.objects.Enqueue(p);

        return p;
    }

    public EnemyStats Get(EnemyStats prefab)
    {
        if (!lookup.TryGetValue(prefab, out var pool))
        {
            Debug.LogError($"No pool registered for {prefab.name}");
            return null;
        }

        if (pool.objects.Count == 0)
            Create(pool);

        EnemyStats p = pool.objects.Dequeue();
        p.gameObject.SetActive(true);
        return p;
    }

    public void Return(EnemyStats enemy, EnemyStats prefab)
    {
        enemy.gameObject.SetActive(false);
        lookup[prefab].objects.Enqueue(enemy);
    }
}