using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolManager : MonoBehaviour
{
    [System.Serializable]
    private class Pool
    {
        public Projectile prefab;
        public int initialSize = 32;

        [HideInInspector] public Queue<Projectile> objects;
    }

    [SerializeField] private List<Pool> pools = new();

    private Dictionary<Projectile, Pool> lookup;

    public static ProjectilePoolManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        lookup = new Dictionary<Projectile, Pool>();

        foreach (var pool in pools)
        {
            pool.objects = new Queue<Projectile>();

            for (int i = 0; i < pool.initialSize; i++)
                Create(pool);

            lookup.Add(pool.prefab, pool);
        }
    }

    private Projectile Create(Pool pool)
    {
        Projectile p = Instantiate(pool.prefab, transform);
        p.gameObject.SetActive(false);
        p.SetPoolManager(this, pool.prefab);
        pool.objects.Enqueue(p);
        return p;
    }

    public Projectile Get(Projectile prefab)
    {
        if (!lookup.TryGetValue(prefab, out var pool))
        {
            Debug.LogError($"No pool registered for {prefab.name}");
            return null;
        }

        if (pool.objects.Count == 0)
            Create(pool);

        Projectile p = pool.objects.Dequeue();
        p.gameObject.SetActive(true);
        return p;
    }

    public void Return(Projectile projectile, Projectile prefab, Transform weaponTransform)
    {
        projectile.gameObject.SetActive(false);
        lookup[prefab].objects.Enqueue(projectile);
        prefab.transform.position = weaponTransform.position;
    }
}