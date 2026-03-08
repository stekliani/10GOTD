using System.Collections.Generic;
using UnityEngine;

public static class EnemyMovementSystem
{
    /// <summary>
    /// Unity-facing API (uses physics).
    /// </summary>
    public static Vector2 GetAvoidanceDirection(
        Vector2 origin,
        float radius,
        LayerMask obstacleLayer)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, obstacleLayer);

        return CalculateAvoidance(origin, radius, hits);
    }

    /// <summary>
    /// PURE LOGIC — testable without Unity physics.
    /// </summary>
    public static Vector2 CalculateAvoidance(
        Vector2 origin,
        float radius,
        IEnumerable<Collider2D> hits)
    {
        if (hits == null)
            return Vector2.zero;

        Vector2 avoidance = Vector2.zero;
        bool hasAny = false;

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            hasAny = true;

            Vector2 closest = hit.ClosestPoint(origin);
            Vector2 away = origin - closest;

            float dist = away.magnitude;
            if (dist <= 0.001f)
                continue;

            float weight = Mathf.Clamp01(1f - (dist / radius));
            avoidance += away.normalized * weight;
        }

        if (!hasAny || avoidance == Vector2.zero)
            return Vector2.zero;

        return avoidance.normalized;
    }
}