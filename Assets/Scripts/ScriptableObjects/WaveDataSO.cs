using System;
using UnityEngine;

[Serializable]
public class EnemySpawnConfig
{
    public EnemyStats prefab;
    public int        spawnCount;
    public float      spawnInterval;
    public WaveAffectedEnemyStats affectedStats = WaveAffectedEnemyStats.Health;
}

[CreateAssetMenu(menuName = "WaveData")]
public class WaveDataSO : ScriptableObject
{
    [Header("Enemy Variants in this Wave")]
    public EnemySpawnConfig[] variants;

    [Header("Wave Reward")]
    public int diamondsReward;
}
