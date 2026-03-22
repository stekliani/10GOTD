using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : AnimationSubject,IPoolable
{
    [Header("Spiral Homing")]
    [SerializeField] private float spiralStrength = 2f;
    [SerializeField] private float spiralDuration = 0.6f;
    [SerializeField] private bool randomSpiralDirection = true;

    [Header("Hit SFX")]
    [SerializeField] private SoundActions hitSFX;

    [Header("Hit Visual")]
    [SerializeField] private AnimationActions hitVisual;

    private float _damage;
    private float _speed;
    private bool _isHoming;
    private EnemyStats _target;
    private float _spiralTimer;
    private float _spiralSign;
    private float _lifeTimer;

    private bool _hasDamagedEnemy;
    private bool _hasRotateVisual;

    private Rigidbody2D _rb;
    private ProjectileWeapon _parent;


    private MainPoolManager _pool;
    public MonoBehaviour PrefabKey { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _hasRotateVisual = TryGetComponent(out RotateVisual rotate);
    }

    public void Initialize(
        Vector2 direction,
        IPlayerStats player,
        WeaponDataSO weaponData,
        bool homing = false,
        EnemyStats target = null,
        ProjectileWeapon parent = null)
    {
        _hasDamagedEnemy = false;

        float damageMultiplier = 1f + player.DamageBoost / 100f;
        _damage = weaponData.Damage * damageMultiplier;

        float speedMultiplier = 1f + player.ProjectileSpeed / 100f;
        _speed = weaponData.Speed * speedMultiplier;

        _isHoming = homing;
        _target = target;
        _parent = parent;

        _spiralTimer = spiralDuration;
        _spiralSign = (randomSpiralDirection && Random.value > 0.5f) ? -1f : 1f;

        _rb.velocity = direction.normalized * _speed;

        _lifeTimer = weaponData.Lifetime;
    }

    private void Update()
    {
        _lifeTimer -= Time.deltaTime;

        if (_lifeTimer <= 0f)
            Despawn();
    }

    private void FixedUpdate()
    {
        if (!_isHoming) return;

        // If we somehow lost our weapon, just despawn safely
        if (_parent == null)
        {
            Despawn();
            return;
        }

        // No target yet OR target died -> try to reacquire
        if (_target == null || _target.CheckIfAlive() == false)
        {
            var closest = _parent.GetClosestEnemyForProjectile();

            if (closest != null)
            {
                _target = closest; // EnemyStats
                                   // continue, we will move towards new target this same frame
            }
            else
            {
                Despawn();
                return;
            }
        }

        // From here _target is guaranteed non-null and alive
        Vector2 toTarget = ((Vector2)_target.transform.position - _rb.position).normalized;

        if (_spiralTimer > 0f)
        {
            _spiralTimer -= Time.fixedDeltaTime;
            float pct = _spiralTimer / spiralDuration;

            Vector2 perp = new Vector2(-toTarget.y, toTarget.x) * _spiralSign;
            Vector2 spiralDir = toTarget + perp * spiralStrength * pct;

            _rb.velocity = spiralDir.normalized * _speed;
        }
        else
        {
            _rb.velocity = toTarget * _speed;
        }
    }

    public RotateVisual GetRotateVisual()
    {
        if (_hasRotateVisual)
        {
            return GetComponent<RotateVisual>();
        }
        else
        {
            return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasDamagedEnemy) return;

        EnemyStats enemy = collision.GetComponentInParent<EnemyStats>();
        if (enemy == null) return;

        if (!enemy.TryTakeDamage(_damage))
            return;

        _hasDamagedEnemy = true;

        int layer = enemy.gameObject.layer;

        if (hitVisual != AnimationActions.none)
        {
            if (layer == LayerMask.NameToLayer("GroundEnemy"))
                NotifyObservers(AnimationActions.PlayGroundExplosion, enemy.transform.position);
            else if (layer == LayerMask.NameToLayer("FlyingEnemy"))
                NotifyObservers(AnimationActions.PlayMidAirExplosion, enemy.transform.position);
        }


        SoundEventBus.Raise(hitSFX);

        Despawn();
    }

    private void Despawn()
    {
        _rb.velocity = Vector2.zero;
        _pool.Return(this);
    }

    public void Init(MainPoolManager manager, MonoBehaviour prefabKey)
    {
        _pool = manager;
        PrefabKey = prefabKey;
    }
}