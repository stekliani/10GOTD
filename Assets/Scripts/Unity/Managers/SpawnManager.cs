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
    }

    private class WaveRuntime
    {
        public bool                      isActive;
        public bool                      isComplete;
        public int                       diamondsReward;
        public List<EnemyVariantRuntime> variants = new();
    }

    [SerializeField] private float      spawnRadius = 10f;
    [SerializeField] private WaveDataSO[] wavesArray;

    private Transform         _playerTransform;
    private PlayerStats _playerStats;
    private List<WaveRuntime> _waves = new();
    private int               _highestActiveIndex;


    private void Start()
    {
        _playerStats = FindObjectOfType<PlayerStats>();
        if (_playerStats != null) _playerTransform = _playerStats.transform;



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
            WaveRuntime runtime = new() { isActive = false, isComplete = false, diamondsReward = 0 };
            foreach (EnemySpawnConfig cfg in so.variants)
            {
                runtime.variants.Add(new EnemyVariantRuntime
                {
                    prefab    = cfg.prefab,
                    remaining = cfg.spawnCount,
                    interval  = cfg.spawnInterval,
                    timer     = 0f
                });
                runtime.diamondsReward += cfg.diamonds;
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

        foreach (WaveRuntime wave in _waves)
        {
            if (!wave.isActive) continue;

            foreach (EnemyVariantRuntime variant in wave.variants)
            {
                if (variant.remaining <= 0) continue;
                variant.timer -= Time.deltaTime;
                if (variant.timer <= 0f)
                {
                    SpawnOnEdge(variant.prefab);
                    variant.remaining--;
                    variant.timer = variant.interval;
                }
            }

            if (AllEnemiesFinishedInWave(wave))
            {
                if (!wave.isComplete)
                {
                    wave.isComplete = true;
                }

                if (wave == _waves[_highestActiveIndex])
                    ActivateNextWave();
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

    private void SpawnOnEdge(EnemyStats prefab)
    {
        Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 spawnPos = _playerTransform.position + (Vector3)(dir * GetSpawnRadius());

        GameObject go = MainPoolManager.Instance.Get(prefab);
        EnemyStats enemy = go.GetComponent<EnemyStats>();

        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.identity;
        enemy.gameObject.SetActive(true);
    }

    private float GetSpawnRadius()
    {
        spawnRadius = _playerStats.Area + 0.5f;
        return spawnRadius;
    }
}
