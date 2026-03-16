using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyStats : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float xpReward;
    [SerializeField] private int coinReward;

    [Header("Ranged Enemy Things")]
    [Tooltip("Leave Empty if this enemy is not ranged")]
    [SerializeField] private RangedEnemyProjectile rangedEnemyProjectilePrefab;
    [SerializeField] private float attackInterval;

    private float _currentAttackInterval;
    private float _currentHealth;
    private PlayerLevels _playerLevels;
    private PlayerInventory _playerInventory;
    private EnemyMovement _enemyMovement;
    private EnemyAnimationController _enemyAnimationController;
    private MainPoolManager _poolManager;
    private int _contactCount;
    private CancellationTokenSource _cts;
    private Collider2D[] _colliders;
    private bool _isAlive = false;

    public MonoBehaviour PrefabKey { get; private set; }
    public static readonly List<EnemyStats> ActiveEnemies = new();
    public static int AliveEnemiesCount;

    public void Init(MainPoolManager manager, MonoBehaviour prefabKey)
    {
        _poolManager = manager;
        PrefabKey = prefabKey;
    }

    private void Awake()
    {
        _enemyAnimationController = GetComponent<EnemyAnimationController>();
        _enemyMovement = GetComponent<EnemyMovement>();
        _playerInventory = FindObjectOfType<PlayerInventory>();
    }

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
        _isAlive = true;

        EnableColliders();
        EnsureReferences();
        ResetState();

        _currentHealth = maxHealth;
        _currentAttackInterval = attackInterval;

        ActiveEnemies.Add(this);
        AliveEnemiesCount++;
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        ActiveEnemies.Remove(this);
        AliveEnemiesCount--;
    }

    private void Update()
    {
        if (!_isAlive) return;

        // only tick for ranged enemies
        if (rangedEnemyProjectilePrefab == null) return;

        _currentAttackInterval -= Time.deltaTime;
        if (_enemyMovement.CheckIfRangedEnemyCanAttack() && _currentAttackInterval <= 0)
        {
            FireProjectile();
            _currentAttackInterval = attackInterval;
        }
    }

    private void FireProjectile()
    {
        var go = MainPoolManager.Instance.Get(rangedEnemyProjectilePrefab);
        if (go == null) return;

        var proj = go.GetComponent<RangedEnemyProjectile>();
        proj.transform.position = transform.position;
        proj.gameObject.SetActive(true);
        proj.AcquireTarget(_playerInventory.gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (!_isAlive || damage <= 0) return;

        _currentHealth -= damage;

        if (_currentHealth <= 0)
            Die();
    }

    public void ApplySlow(float amount) => _enemyMovement?.SetSpeed(-amount);
    public void RemoveSlow(float amount) => _enemyMovement?.SetSpeed(amount);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IEnemyTarget player))
        {
            _contactCount++;
            _enemyAnimationController.ChangeAnimation(EnemyAnimations.atacking);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IEnemyTarget player))
        {
            _contactCount--;
            if (_contactCount <= 0)
            {
                _contactCount = 0;
                _enemyAnimationController.ChangeAnimation(EnemyAnimations.walking);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IEnemyTarget player))
        {
            player.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }

    private void Die()
    {
        _isAlive = false;
        DisableColliders();

        _playerLevels?.AddXp(xpReward);
        _playerInventory?.AddCoins(coinReward);
        _enemyAnimationController.ChangeAnimation(EnemyAnimations.dying);

        ReturnToPool(GetDeathAnimationTime());
    }

    private async void ReturnToPool(float deathAnimationTime)
    {
        try
        {
            await Task.Delay((int)(deathAnimationTime * 1000), _cts.Token);

            // Reset pose BEFORE disabling, otherwise pooled skeletal rigs can respawn in the final death pose.
            _enemyAnimationController?.ResetToDefaults();
            _poolManager?.Return(this);
        }
        catch (TaskCanceledException) { }
    }

    private void ResetState()
    {
        _currentHealth = maxHealth;
        _contactCount = 0;
        StopAllCoroutines();
        _enemyAnimationController?.ResetToDefaults();
        _enemyAnimationController.ChangeAnimation(EnemyAnimations.walking);
    }

    private void EnsureReferences()
    {
        _playerLevels ??= FindObjectOfType<PlayerLevels>();
        _playerInventory ??= FindObjectOfType<PlayerInventory>();
    }

    private float GetDeathAnimationTime()
        => _enemyAnimationController.GetCurrentAnimationDuration();

    private void EnableColliders()
    {
        _colliders ??= GetComponentsInChildren<Collider2D>();
        foreach (var col in _colliders) col.enabled = true;
    }

    private void DisableColliders()
    {
        _colliders ??= GetComponentsInChildren<Collider2D>();
        foreach (var col in _colliders) col.enabled = false;
    }

    public bool CheckIfAlive() => _isAlive;

    public static class AwaitExtensions
    {
        public static Task WaitForSeconds(float seconds)
            => Task.Delay((int)(seconds * 1000));
    }
}