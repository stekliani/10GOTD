using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyStats : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private float armor = 0f;
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float xpReward;
    [SerializeField] private int coinReward;
    [SerializeField] private int diamondsReward;

    [Header("Ranged Enemy Things")]
    [Tooltip("Leave Empty if this enemy is not ranged")]
    [SerializeField] private RangedEnemyProjectile rangedEnemyProjectilePrefab;
    [SerializeField] private float attackInterval;

    private float _currentAttackInterval;
    private float _currentHealth;
    private float _currentArmor;


    private PlayerLevels _playerLevels;
    private PlayerInventory _playerInventory;
    private EnemyMovement _enemyMovement;
    private EnemyAnimationController _enemyAnimationController;
    private MainPoolManager _poolManager;
    private int _contactCount;
    private CancellationTokenSource _cts;
    private Collider2D[] _colliders;
    private bool _isAlive = false;

    //base
    private float _baseMaxHealth;
    private float _baseArmor;
    private float _baseDamagePerSecond;
    private float _baseAttackInterval;
    private float _baseXpReward;
    private int _baseCoinReward;
    private int _baseDiamondsReward;


    //runtime
    private float _runtimeMaxHealth;
    private float _runtimeArmor;
    private float _runtimeDamagePerSecond;
    private float _runtimeAttackInterval;
    private float _runtimeXpReward;
    private int _runtimeCoinReward;
    private int _runtimeDiamondsReward;
    private float _runtimeProjectileDamageMultiplier = 1f;
    private float _waveMultiplier = 1f;
    private WaveAffectedEnemyStats _scaledStats;
    private System.Action _onDeathCallback;

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
        _baseMaxHealth = maxHealth;
        _baseArmor = armor;
        _baseDamagePerSecond = damagePerSecond;
        _baseAttackInterval = attackInterval;
        _baseXpReward = xpReward;
        _baseCoinReward = coinReward;
        _baseDiamondsReward = diamondsReward;
    }

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
        _isAlive = true;

        EnableColliders();
        ApplyRuntimeStatsFromBase();
        ApplyWaveScaling();
        ResetState();

        ActiveEnemies.Add(this);
        AliveEnemiesCount++;
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _onDeathCallback = null;

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
            _currentAttackInterval = _runtimeAttackInterval;
        }
    }

    private void FireProjectile()
    {
        if (_poolManager == null) return;
        if (_playerInventory == null) return;

        var go = _poolManager.Get(rangedEnemyProjectilePrefab);
        if (go == null) return;

        var proj = go.GetComponent<RangedEnemyProjectile>();
        proj.transform.position = transform.position;
        proj.gameObject.SetActive(true);
        proj.SetDamageMultiplier(_runtimeProjectileDamageMultiplier);
        proj.AcquireTarget(_playerInventory.gameObject);
    }

    public void TakeDamage(float damage, float piercing) => TryTakeDamage(damage, piercing);

    /// <summary>
    /// Returns false if the enemy was already dead or damage was invalid, so callers can pierce (e.g. another projectile killed it this frame).
    /// </summary>
    public bool TryTakeDamage(float damage, float piercing)
    {
        if (!_isAlive || damage <= 0) return false;

        // Convert to percentages
        float piercingMultiplier = 1 + piercing / 100f;
        float armorMultiplier = 1 + _currentArmor / 100f;

        // Calculate total damage
        float totalDamage = damage * (piercingMultiplier - armorMultiplier + 1);

        // Ensure damage is never negative
        totalDamage = Mathf.Max(0, totalDamage);

        // Apply damage
        _currentHealth -= totalDamage;

        if (_currentHealth <= 0)
            Die();

        return true;
    }

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
            player.TakeDamage(_runtimeDamagePerSecond * Time.deltaTime);
        }
    }

    private void Die()
    {
        _isAlive = false;
        DisableColliders();
        _onDeathCallback?.Invoke();
        _onDeathCallback = null;

        _playerLevels?.AddXp(_runtimeXpReward);
        _playerInventory?.AddCoins(_runtimeCoinReward);
        _enemyAnimationController.ChangeAnimation(EnemyAnimations.dying);

        GameManager.Instance.IncreaseDiamondsRewardFromMonsters(_runtimeDiamondsReward);
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
        _currentHealth = _runtimeMaxHealth;
        _currentArmor = _runtimeArmor;
        _contactCount = 0;
        StopAllCoroutines();
        _enemyAnimationController?.ResetToDefaults();
        _enemyAnimationController.ChangeAnimation(EnemyAnimations.walking);
    }

    public void SetPlayerReferences(PlayerLevels playerLevels, PlayerInventory playerInventory)
    {
        _playerLevels = playerLevels;
        _playerInventory = playerInventory;
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

    public void ConfigureWaveScaling(float multiplier, WaveAffectedEnemyStats affectedStats)
    {
        _waveMultiplier = Mathf.Max(0f, multiplier);
        _scaledStats = affectedStats;
    }

    public void SetOnDeathCallback(System.Action onDeathCallback)
    {
        _onDeathCallback = onDeathCallback;
    }

    private void ApplyRuntimeStatsFromBase()
    {
        _runtimeMaxHealth = _baseMaxHealth;
        _runtimeArmor = _baseArmor;
        _runtimeDamagePerSecond = _baseDamagePerSecond;
        _runtimeAttackInterval = _baseAttackInterval;
        _runtimeXpReward = _baseXpReward;
        _runtimeCoinReward = _baseCoinReward;
        _runtimeDiamondsReward = _baseDiamondsReward;
        _runtimeProjectileDamageMultiplier = 1f;
    }

    private void ApplyWaveScaling()
    {
        if ((_scaledStats & WaveAffectedEnemyStats.Health) != 0)
            _runtimeMaxHealth *= _waveMultiplier;

        if ((_scaledStats & WaveAffectedEnemyStats.Armor) != 0)
            _runtimeArmor *= _waveMultiplier;

        if ((_scaledStats & WaveAffectedEnemyStats.Damage) != 0)
        {
            _runtimeDamagePerSecond *= _waveMultiplier;
            _runtimeProjectileDamageMultiplier = _waveMultiplier;
        }

        if ((_scaledStats & WaveAffectedEnemyStats.XpReward) != 0)
            _runtimeXpReward *= _waveMultiplier;

        if ((_scaledStats & WaveAffectedEnemyStats.CoinReward) != 0)
            _runtimeCoinReward = Mathf.RoundToInt(_runtimeCoinReward * _waveMultiplier);
        if((_scaledStats & WaveAffectedEnemyStats.DiamondReward) != 0)
            _runtimeDiamondsReward = Mathf.RoundToInt(_runtimeDiamondsReward * _waveMultiplier);

        float speedMultiplier = (_scaledStats & WaveAffectedEnemyStats.MoveSpeed) != 0 ? _waveMultiplier : 1f;
        _enemyMovement?.SetWaveSpeedMultiplier(speedMultiplier);
    }

    public static class AwaitExtensions
    {
        public static Task WaitForSeconds(float seconds)
            => Task.Delay((int)(seconds * 1000));
    }
}