using System;

[Flags]
public enum WaveAffectedEnemyStats
{
    None       = 0,
    Health     = 1 << 0,
    Damage     = 1 << 1,
    MoveSpeed  = 1 << 2,
    XpReward   = 1 << 3,
    CoinReward = 1 << 4
}
