using UnityEngine;

public class RangedEnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private Rigidbody2D rb;
    private RangedEnemyProjectilePoolManager _manager;
    RangedEnemyProjectile _originalPrefab;

    private Vector2 _moveDir; // fixed at spawn

    private void Start()
    {
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player == null) { Destroy(gameObject); return; }

    }

    private void FixedUpdate()
    {
        rb.velocity = _moveDir * speed;
    }

    public void SetPoolManager(RangedEnemyProjectilePoolManager manager, RangedEnemyProjectile originalPrefab)
    {
        _manager = manager;
        _originalPrefab = originalPrefab;
    }

    public void AcquireTarget(GameObject target)
    {
        _moveDir = ((Vector2)target.transform.position - rb.position).normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IEnemyTarget target))
        {
            target.TakeDamage(damage);
            _manager.Return(this,_originalPrefab);
        }
    }
}