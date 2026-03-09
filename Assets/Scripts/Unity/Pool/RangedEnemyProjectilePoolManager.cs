using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyProjectilePoolManager : MonoBehaviour
{
    [System.Serializable]
    private class Pool
    {
        public RangedEnemyProjectile prefab;
        public int initialSize = 32;
        [HideInInspector] public Queue<RangedEnemyProjectile> objects;
    }

    [SerializeField] private List<Pool> pools = new();

    private Dictionary<RangedEnemyProjectile, Pool> lookup;

    public static RangedEnemyProjectilePoolManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        lookup = new Dictionary<RangedEnemyProjectile, Pool>();

        foreach (var pool in pools)
        {
            pool.objects = new Queue<RangedEnemyProjectile>();

            for (int i = 0; i < pool.initialSize; i++)
                Create(pool);

            lookup.Add(pool.prefab, pool);
        }
    }

    private RangedEnemyProjectile Create(Pool pool)
    {
        RangedEnemyProjectile p = Instantiate(pool.prefab, transform);
        p.gameObject.SetActive(false);
        p.SetPoolManager(this, pool.prefab);
        pool.objects.Enqueue(p);
        return p;
    }

    public RangedEnemyProjectile Get(RangedEnemyProjectile prefab, EnemyStats enemy)
    {
        if (!lookup.TryGetValue(prefab, out var pool))
        {
            Debug.LogError($"No pool registered for {prefab.name}");
            return null;
        }

        if (pool.objects.Count == 0)
            Create(pool);

        RangedEnemyProjectile p = pool.objects.Dequeue();
        p.gameObject.transform.position = enemy.transform.position;
        p.gameObject.SetActive(true);
        return p;
    }

    public void Return(RangedEnemyProjectile projectile, RangedEnemyProjectile prefab)
    {
        projectile.gameObject.SetActive(false);
        lookup[prefab].objects.Enqueue(projectile);
        prefab.transform.position = transform.position;
    }
}