using UnityEngine;

public class RangedEnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 _moveDir; // fixed at spawn

    private void Start()
    {
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player == null) { Destroy(gameObject); return; }

        _moveDir = ((Vector2)player.transform.position - rb.position).normalized;
    }

    private void FixedUpdate()
    {
        rb.velocity = _moveDir * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IEnemyTarget target))
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}