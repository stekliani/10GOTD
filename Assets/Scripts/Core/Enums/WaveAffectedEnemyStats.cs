using System;

[Flags]
public enum WaveAffectedEnemyStats
{
    None = 0,
    Health = 1 << 0,
    Armor = 1 << 1,
    Damage = 1 << 2,
    MoveSpeed = 1 << 3,
    XpReward = 1 << 4,
    CoinReward = 1 << 5,
    DiamondReward = 1 << 6,
}