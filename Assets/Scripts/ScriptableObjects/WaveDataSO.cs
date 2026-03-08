using System;
using UnityEngine;

[Serializable]
public class EnemySpawnConfig
{
    public EnemyStats prefab;
    public int        spawnCount;
    public float      spawnInterval;
}

[CreateAssetMenu(menuName = "WaveData")]
public class WaveDataSO : ScriptableObject
{
    [Header("Enemy Variants in this Wave")]
    public EnemySpawnConfig[] variants;
}
