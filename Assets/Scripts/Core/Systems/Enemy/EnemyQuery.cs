using System.Collections.Generic;
using UnityEngine;

public static class EnemyQuery
{
    public static List<EnemyStats> GetAliveEnemiesInRange(Vector2 origin, float range)
    {
        List<EnemyStats> result = new();
        float rangeSqr = range * range;

        foreach (EnemyStats e in EnemyStats.ActiveEnemies)
        {
            if (e == null || !e.CheckIfAlive())
                continue;

            Vector2 diff = (Vector2)e.transform.position - origin;
            if (diff.sqrMagnitude <= rangeSqr)
                result.Add(e);
        }

        return result;
    }

    public static EnemyStats GetClosestEnemy(Vector2 origin, IEnumerable<EnemyStats> enemies)
    {
        EnemyStats closest = null;
        float minDist = float.MaxValue;

        foreach (EnemyStats e in enemies)
        {
            if (e == null) continue;

            float d = Vector2.Distance(origin, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = e;
            }
        }

        return closest;
    }

    // Convenience: closest alive enemy within a range
    public static EnemyStats GetClosestAliveEnemyInRange(Vector2 origin, float range)
    {
        var enemies = GetAliveEnemiesInRange(origin, range);
        return GetClosestEnemy(origin, enemies);
    }
}