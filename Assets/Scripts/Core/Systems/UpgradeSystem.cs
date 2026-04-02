using System;
using System.Collections.Generic;

public static class UpgradeSystem
{
    /// <summary>
    /// Returns up to 'amount' random deduplicated weapons from the eligible pool.
    /// </summary>
    public static List<IWeapon> GetRandomUpgrades(List<IWeapon> eligiblePool, int amount, Random rng)
    {
        List<IWeapon> pool   = new(eligiblePool);
        List<IWeapon> result = new();

        amount = amount < pool.Count ? amount : pool.Count;

        for (int i = 0; i < amount; i++)
        {
            int index = rng.Next(pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    /// <summary>
    /// Returns only weapons that still have upgrade levels remaining.
    /// </summary>
    public static List<IWeapon> FilterEligible(List<IWeapon> allWeapons)
    {
        List<IWeapon> eligible = new();

        foreach (IWeapon w in allWeapons)
        {
            if (w == null) continue;
            if (w.GetLevel() < w.GetMaxLevel())
                eligible.Add(w);
        }

        return eligible;
    }
}
