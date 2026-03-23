using UnityEngine;

public class RangedEnemyProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 _moveDir;
    private MainPoolManager _pool;
    private float _baseDamage;
    private float _runtimeDamage;
    public MonoBehaviour PrefabKey { get; private set; }

    private void Awake()
    {
        _baseDamage = damage;
        _runtimeDamage = _baseDamage;
    }

    private void OnEnable()
    {
        if (rb != null) rb.velocity = Vector2.zero;
        _runtimeDamage = _baseDamage;
    }

    private void FixedUpdate()
    {
        rb.velocity = _moveDir * speed;
    }

    public void AcquireTarget(GameObject target)
    {
        _moveDir = ((Vector2)target.transform.position - rb.position).normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IEnemyTarget target))
        {
            target.TakeDamage(_runtimeDamage);
            _pool.Return(this);
        }
    }

    public void Init(MainPoolManager manager, MonoBehaviour prefabKey)
    {
        _pool = manager;
        PrefabKey = prefabKey;
    }

    public void SetDamageMultiplier(float multiplier)
    {
        _runtimeDamage = _baseDamage * Mathf.Max(0f, multiplier);
    }
}