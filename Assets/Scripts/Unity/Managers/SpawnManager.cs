using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Boss Waves")]
    [Tooltip("Boss every N waves (10 = wave 10, 20, 30, ...)")]
    [SerializeField] private int bossEveryNWaves = 10;

    [Header("Wave Budget")]
    [SerializeField] private float baseWaveBudget = 20f;
    [SerializeField] private float budgetGrowthPerWave = 0.06f;

    [Header("Wave Duration")]
    [SerializeField] private float baseWaveDuration = 30f;
    [SerializeField] private float durationGrowthPerWave = 2f;
    [SerializeField] private float minSpawnInterval = 0.4f;
    [SerializeField] private float maxSpawnInterval = 3.0f;

    private bool _isEventFired = false;

    private class EnemyVariantRuntime
    {
        public EnemyStats prefab;
        public int remaining;
        public float timer;
        public float interval;
        public WaveAffectedEnemyStats affectedStats;
    }

    private class WaveRuntime
    {
        public bool isActive;
        public bool isSpawningComplete;
        public bool isComplete;
        public int aliveEnemies;
        public int diamondsReward;
        public bool bossSpawned;
        public EnemyStats bossPrefab;
        public List<EnemyVariantRuntime> variants = new();
    }

    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private WaveDataSO[] wavesArray;

    [Header("Wave Enemy Stat Scaling")]
    [SerializeField] private bool enableWaveStatScaling = true;
    [SerializeField, Min(1)] private int scalingStartsAtWave = 2;
    [SerializeField, Min(0f)] private float bonusPercentPerWave = 1f;
    [SerializeField, Min(0f)] private float maxBonusPercent = 100f;

    private Transform _playerTransform;
    private PlayerStats _playerStats;
    private PlayerInventory _playerInventory;
    private PlayerLevels _playerLevels;
    private List<WaveRuntime> _waves = new();
    private int _highestActiveIndex;

    private void Start()
    {
        _playerStats = FindObjectOfType<PlayerStats>();
        if (_playerStats != null) _playerTransform = _playerStats.transform;

        _playerInventory = FindObjectOfType<PlayerInventory>();
        _playerLevels = FindObjectOfType<PlayerLevels>();

        BuildWaves();
        if (_waves.Count > 0) _waves[0].isActive = true;
    }

    private void Update()
    {
        HandeWaves();
        HandleLastWave();
    }

    private void BuildWaves()
    {
        _waves.Clear();

        for (int i = 0; i < wavesArray.Length; i++)
        {
            WaveDataSO so = wavesArray[i];
            int waveNumber = i + 1;

            WaveRuntime runtime = new()
            {
                isActive = false,
                isSpawningComplete = false,
                isComplete = false,
                aliveEnemies = 0,
                diamondsReward = so.diamondsReward,
                bossSpawned = false,
                bossPrefab = so.bossPrefab   // null = no boss for this wave
            };

            if (so.variants == null || so.variants.Length == 0)
            {
                _waves.Add(runtime);
                continue;
            }

            float difficultyMultiplier = Mathf.Pow(1f + budgetGrowthPerWave, waveNumber - 1);
            float budget = baseWaveBudget * difficultyMultiplier;

            bool isBossWave = bossEveryNWaves > 0 && (waveNumber % bossEveryNWaves) == 0;
            if (isBossWave) budget *= 2f;

            float totalWeight = 0f;
            foreach (EnemySpawnConfig cfg in so.variants)
            {
                if (cfg.prefab == null) continue;
                totalWeight += Mathf.Max(0.01f, cfg.weight);
            }

            if (totalWeight <= 0f)
            {
                _waves.Add(runtime);
                continue;
            }

            float waveDuration = baseWaveDuration + durationGrowthPerWave * (waveNumber - 1);
            if (isBossWave) waveDuration *= 1.2f;

            var perVariantCounts = new List<int>();
            int totalCount = 0;

            foreach (EnemySpawnConfig cfg in so.variants)
            {
                if (cfg.prefab == null)
                {
                    perVariantCounts.Add(0);
                    continue;
                }

                float share = Mathf.Max(0.01f, cfg.weight) / totalWeight;
                float variantBudget = budget * share;
                int count = Mathf.Max(1, Mathf.RoundToInt(variantBudget));

                perVariantCounts.Add(count);
                totalCount += count;
            }

            if (totalCount <= 0) totalCount = 1;

            float interval = Mathf.Clamp(
                waveDuration / totalCount,
                minSpawnInterval,
                maxSpawnInterval
            );

            for (int v = 0; v < so.variants.Length; v++)
            {
                EnemySpawnConfig cfg = so.variants[v];
                if (cfg.prefab == null) continue;

                runtime.variants.Add(new EnemyVariantRuntime
                {
                    prefab = cfg.prefab,
                    remaining = perVariantCounts[v],
                    interval = interval,
                    timer = 0f,
                    affectedStats = cfg.affectedStats
                });
            }

            _waves.Add(runtime);
        }
    }

    private void ActivateNextWave()
    {
        if (_highestActiveIndex + 1 >= _waves.Count) return;
        _highestActiveIndex++;
        _waves[_highestActiveIndex].isActive = true;
    }

    private bool AllEnemiesFinishedInWave(WaveRuntime wave)
    {
        foreach (EnemyVariantRuntime v in wave.variants)
            if (v.remaining > 0) return false;
        return true;
    }

    private bool IsAllWavesFinished()
    {
        return _highestActiveIndex == _waves.Count - 1
            && AllEnemiesFinishedInWave(_waves[_highestActiveIndex])
            && EnemyStats.AliveEnemiesCount == 0;
    }

    private void HandleLastWave()
    {
        if (IsAllWavesFinished() && !_isEventFired)
        {
            EventsManager.Instance.FireOnLastEnemyDeathEvent();
            _isEventFired = true;
        }
    }

    public void TrySkipWave()
    {
        int nextIndex = _highestActiveIndex + 1;
        if (nextIndex >= _waves.Count) return;

        int nextWaveNumber = nextIndex + 1;
        if (bossEveryNWaves > 0 && (nextWaveNumber % bossEveryNWaves) == 0)
            return;

        _highestActiveIndex++;
        _waves[_highestActiveIndex].isActive = true;
    }

    private void HandeWaves()
    {
        if (_playerTransform == null) return;

        for (int waveIndex = 0; waveIndex < _waves.Count; waveIndex++)
        {
            WaveRuntime wave = _waves[waveIndex];
            if (!wave.isActive) continue;

            // Spawn boss once if this wave has a bossPrefab configured
            if (!wave.bossSpawned && wave.bossPrefab != null)
            {
                SpawnOnEdge(wave.bossPrefab, waveIndex, default);
                wave.bossSpawned = true;
            }

            foreach (EnemyVariantRuntime variant in wave.variants)
            {
                if (variant.remaining <= 0) continue;
                variant.timer -= Time.deltaTime;
                if (variant.timer <= 0f)
                {
                    SpawnOnEdge(variant.prefab, waveIndex, variant.affectedStats);
                    variant.remaining--;
                    variant.timer = variant.interval;
                }
            }

            if (!wave.isSpawningComplete && AllEnemiesFinishedInWave(wave))
            {
                wave.isSpawningComplete = true;

                if (wave == _waves[_highestActiveIndex])
                    ActivateNextWave();
            }

            if (wave.isSpawningComplete && !wave.isComplete && wave.aliveEnemies <= 0)
            {
                wave.isComplete = true;
            }
        }
    }

    public int GetCompletedWavesCount()
    {
        int count = 0;
        foreach (WaveRuntime wave in _waves)
            if (wave.isComplete) count++;
        return count;
    }

    public int GetCompletedWavesDiamondReward()
    {
        int sum = 0;
        foreach (WaveRuntime wave in _waves)
            if (wave.isComplete) sum += wave.diamondsReward;
        return sum;
    }

    private void SpawnOnEdge(EnemyStats prefab, int waveIndex, WaveAffectedEnemyStats affectedStats)
    {
        Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 spawnPos = _playerTransform.position + (Vector3)(dir * GetSpawnRadius());

        GameObject go = MainPoolManager.Instance.Get(prefab);
        if (go == null) return;

        EnemyStats enemy = go.GetComponent<EnemyStats>();
        int waveNumber = waveIndex + 1;
        float waveMultiplier = GetWaveStatMultiplier(waveNumber);

        _waves[waveIndex].aliveEnemies++;
        enemy.SetOnDeathCallback(() => OnEnemyDiedFromWave(waveIndex));
        enemy.ConfigureWaveScaling(waveMultiplier, affectedStats);

        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetPlayerReferences(_playerLevels, _playerInventory);
        enemy.gameObject.SetActive(true);
    }

    private void OnEnemyDiedFromWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= _waves.Count) return;
        _waves[waveIndex].aliveEnemies = Mathf.Max(0, _waves[waveIndex].aliveEnemies - 1);
    }

    private float GetWaveStatMultiplier(int waveNumber)
    {
        if (!enableWaveStatScaling) return 1f;
        if (waveNumber < scalingStartsAtWave) return 1f;

        float bonusPercent = Mathf.Min(maxBonusPercent, waveNumber * bonusPercentPerWave);
        return 1f + (bonusPercent / 100f);
    }

    private float GetSpawnRadius()
    {
        spawnRadius = (_playerStats.Area * 2f) + 0.5f;
        return spawnRadius;
    }

    public int GetCurrentWaveIndex()
    {
        return _highestActiveIndex + 1;
    }
}