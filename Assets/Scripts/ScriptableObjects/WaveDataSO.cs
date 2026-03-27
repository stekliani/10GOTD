using System;
using UnityEngine;

[Serializable]
public class EnemySpawnConfig
{
    public EnemyStats prefab;
    public WaveAffectedEnemyStats affectedStats;

    [Header("Balance")]
    [Tooltip("Relative threat of this enemy type (1 = trash, 2 = tanky, 3 = elite, etc).")]
    public float weight = 1f;
}

[CreateAssetMenu(menuName = "WaveData")]
public class WaveDataSO : ScriptableObject
{
    [Header("Enemy Variants in this Wave")]
    public EnemySpawnConfig[] variants;
    public EnemyStats bossPrefab;

    [Header("Wave Reward")]
    public int diamondsReward;
}
