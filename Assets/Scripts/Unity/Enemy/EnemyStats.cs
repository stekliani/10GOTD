using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyStats : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float xpReward;
    [SerializeField] private int coinReward;

    private float _currentHealth;
    private PlayerLevels _playerLevels;
    private PlayerInventory _playerInventory;
    private EnemyMovement _enemyMovement;
    private EnemyAnimationController _enemyAnimationController;
    private EnemyPoolManager _manager;
    private EnemyStats _originalPrefab;
    private int _contactCount; // supports multiple colliders safely
    private CancellationTokenSource _cts;
    private Collider2D[] _colliders;
    private bool isAlive = false;

    public static readonly List<EnemyStats> ActiveEnemies = new();
    public static int AliveEnemiesCount;
    private void OnEnable()
    {
        EnableColliders();
        isAlive = true;

        _cts = new CancellationTokenSource();

        // CACHE FIRST
        _enemyAnimationController ??= GetComponent<EnemyAnimationController>();
        _enemyMovement ??= GetComponent<EnemyMovement>();

        EnsureReferences();

        ActiveEnemies.Add(this);

        ResetState();

        _currentHealth = maxHealth;

        _playerLevels ??= FindObjectOfType<PlayerLevels>();
        _playerInventory ??= FindObjectOfType<PlayerInventory>();

        AliveEnemiesCount++;
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        ActiveEnemies.Remove(this);

        AliveEnemiesCount--;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0) return;

        _currentHealth -= damage;

        if (_currentHealth <= 0)
            Die();
    }

    public void ApplySlow(float amount) => _enemyMovement?.SetSpeed(-amount);
    public void RemoveSlow(float amount) => _enemyMovement?.SetSpeed(amount);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable player))
        {
            _contactCount++;
            _enemyAnimationController.ChangeAnimation(EnemyAnimations.atacking);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable player))
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
        if (collision.TryGetComponent(out IDamageable player))
        {
            player.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }

    private void Die()
    {
        DisableColliders();
        isAlive = false;
        _playerLevels?.AddXp(xpReward);
        _playerInventory?.AddCoins(coinReward);
        _enemyAnimationController.ChangeAnimation(EnemyAnimations.dying);
        ReturnToPool(GetDeathAnimationTime());
    }
    private async void ReturnToPool(float deathAnimationTime)
    {
        try
        {
            await Task.Delay(
                (int)(deathAnimationTime * 1000),
                _cts.Token);

            if (_manager != null)
                _manager.Return(this, _originalPrefab);
        }
        catch (TaskCanceledException)
        {
            // object disabled or reused — ignore
        }
    }

    private void ResetState()
    {
        _currentHealth = maxHealth;
        _contactCount = 0;

        _enemyAnimationController.ChangeAnimation(EnemyAnimations.walking);

        // optional but recommended
        StopAllCoroutines();
    }

    private void EnsureReferences()
    {
        if (_playerLevels == null)
            _playerLevels = FindObjectOfType<PlayerLevels>();

        if (_playerInventory == null)
            _playerInventory = FindObjectOfType<PlayerInventory>();

        if (_enemyMovement == null)
            _enemyMovement = GetComponent<EnemyMovement>();
    }

    public void SetPoolManager(EnemyPoolManager manager, EnemyStats originalPrefab)
    {
        _manager = manager;
        _originalPrefab = originalPrefab;
    }

    private float GetDeathAnimationTime()
    {
        float deathAnimationTime = _enemyAnimationController.GetCurrentAnimationDuration();

        return deathAnimationTime;
    }

    private void EnableColliders()
    {
        _colliders = GetComponentsInChildren<Collider2D>();

        foreach(Collider2D collider in _colliders)
        {
            collider.enabled = true;
        }
    }

    private void DisableColliders()
    {
        _colliders = GetComponentsInChildren<Collider2D>();

        foreach(Collider2D collider in _colliders)
        {
            collider.enabled = false;
        }
    }

    public bool CheckIfAlive()
    {
        return isAlive;
    }
    public static class AwaitExtensions
    {
        public static Task WaitForSeconds(float seconds)
        {
            return Task.Delay((int)(seconds * 1000));
        }
    }
}