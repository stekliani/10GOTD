using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private int[] _bossWaveIndices = { 9, 19, 29 };
    private bool _isEventFired = false;
    private class EnemyVariantRuntime
    {
        public EnemyStats prefab;
        public int        remaining;
        public float      timer;
        public float      interval;
        public WaveAffectedEnemyStats affectedStats;
    }

    private class WaveRuntime
    {
        public bool                      isActive;
        public bool                      isSpawningComplete;
        public bool                      isComplete;
        public int                       aliveEnemies;
        public int                       diamondsReward;
        public List<EnemyVariantRuntime> variants = new();
    }

    [SerializeField] private float      spawnRadius = 10f;
    [SerializeField] private WaveDataSO[] wavesArray;
    [Header("Wave Enemy Stat Scaling")]
    [SerializeField] private bool enableWaveStatScaling = true;
    [SerializeField, Min(1)] private int scalingStartsAtWave = 2;
    [SerializeField, Min(0f)] private float bonusPercentPerWave = 1f;
    [SerializeField, Min(0f)] private float maxBonusPercent = 100f;

    private Transform         _playerTransform;
    private PlayerStats _playerStats;
    private PlayerInventory _playerInventory;
    private PlayerLevels _playerLevels;
    private List<WaveRuntime> _waves = new();
    private int               _highestActiveIndex;


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
        foreach (WaveDataSO so in wavesArray)
        {
            WaveRuntime runtime = new()
            {
                isActive = false,
                isSpawningComplete = false,
                isComplete = false,
                aliveEnemies = 0,
                diamondsReward = so.diamondsReward
            };
            foreach (EnemySpawnConfig cfg in so.variants)
            {
                runtime.variants.Add(new EnemyVariantRuntime
                {
                    prefab    = cfg.prefab,
                    remaining = cfg.spawnCount,
                    interval  = cfg.spawnInterval,
                    timer     = 0f,
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
        if( _highestActiveIndex == _waves.Count - 1 
            && AllEnemiesFinishedInWave(_waves[_highestActiveIndex]) 
            && EnemyStats.AliveEnemiesCount == 0)
        {
            return true;
        }
        return false;
    }

    private void HandleLastWave()
    {

        if(IsAllWavesFinished() && !_isEventFired)
        {
            EventsManager.Instance.FireOnLastEnemyDeathEvent();
            _isEventFired = true;
        }
    }

    public void TrySkipWave()
    {
        int nextIndex = _highestActiveIndex + 1;

        if (nextIndex >= _waves.Count) return;
        if (_bossWaveIndices.Contains(nextIndex)) return;

        // just activate next wave current wave keeps spawning untouched
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

            // Reward completion now means: wave finished spawning AND all enemies from it are dead.
            if (wave.isSpawningComplete && !wave.isComplete && wave.aliveEnemies <= 0)
            {
                wave.isComplete = true;
            }
        }
    }

    /// <summary>
    /// Returns number of waves that have completed spawning all enemies.
    /// </summary>
    public int GetCompletedWavesCount()
    {
        int count = 0;
        foreach (WaveRuntime wave in _waves)
        {
            if (wave.isComplete) count++;
        }
        return count;
    }

    /// <summary>
    /// Returns the total diamond reward for all completed waves
    /// at the moment this function is called.
    /// </summary>
    public int GetCompletedWavesDiamondReward()
    {
        int sum = 0;
        foreach (WaveRuntime wave in _waves)
        {
            if (wave.isComplete)
            {
                sum += wave.diamondsReward;
            }
        }
        return sum;
    }

    private void SpawnOnEdge(EnemyStats prefab, int waveIndex, WaveAffectedEnemyStats affectedStats)
    {
        Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 spawnPos = _playerTransform.position + (Vector3)(dir * GetSpawnRadius());

        GameObject go = MainPoolManager.Instance.Get(prefab);
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
}
