using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class EnemyMovementSystemTests
{
    // ---------------- HELPERS ----------------

    private Collider2D CreateCollider(Vector2 position)
    {
        GameObject go = new GameObject("TestCollider");
        go.transform.position = position;
        return go.AddComponent<BoxCollider2D>();
    }

    // ---------------- TESTS ----------------

    [Test]
    public void CalculateAvoidance_NullHits_ReturnsZero()
    {
        Vector2 result =
            EnemyMovementSystem.CalculateAvoidance(Vector2.zero, 2f, null);

        Assert.AreEqual(Vector2.zero, result);
    }

    [Test]
    public void CalculateAvoidance_EmptyHits_ReturnsZero()
    {
        Vector2 result =
            EnemyMovementSystem.CalculateAvoidance(Vector2.zero, 2f, new List<Collider2D>());

        Assert.AreEqual(Vector2.zero, result);
    }

    [Test]
    public void CalculateAvoidance_SingleObstacle_PushesAway()
    {
        Collider2D col = CreateCollider(Vector2.right);

        Vector2 result =
            EnemyMovementSystem.CalculateAvoidance(Vector2.zero, 5f, new[] { col });

        Assert.Less(result.x, 0f); // should push left
    }

    [Test]
    public void CalculateAvoidance_TwoOppositeObstacles_CancelsOut()
    {
        Collider2D left = CreateCollider(Vector2.left);
        Collider2D right = CreateCollider(Vector2.right);

        Vector2 result =
            EnemyMovementSystem.CalculateAvoidance(Vector2.zero, 5f, new[] { left, right });

        Assert.AreEqual(Vector2.zero, result);
    }

    [Test]
    public void CalculateAvoidance_CloserObstacle_StrongerInfluence()
    {
        Collider2D close = CreateCollider(new Vector2(0.5f, 0));
        Collider2D far = CreateCollider(new Vector2(3f, 0));

        Vector2 result =
            EnemyMovementSystem.CalculateAvoidance(Vector2.zero, 5f, new[] { close, far });

        Assert.Less(result.x, 0f); // pushed left
    }
}