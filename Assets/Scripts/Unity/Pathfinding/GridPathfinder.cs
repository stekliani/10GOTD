using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure A* on a 2-D integer grid. No MonoBehaviour fully unit-testable.
/// </summary>
public static class GridPathfinder
{
    public const float CellSize = 1f; // world units per cell

    // coordinate helpers

    public static Vector2Int WorldToCell(Vector2 world)
        => new Vector2Int(
            Mathf.RoundToInt(world.x / CellSize),
            Mathf.RoundToInt(world.y / CellSize));

    public static Vector2 CellToWorld(Vector2Int cell)
        => new Vector2(cell.x * CellSize, cell.y * CellSize);

    // A*

    /// <summary>
    /// Returns a path (start ? goal) as world-space waypoints,
    /// or an empty list when no path exists.
    /// isWalkable(cell) must return true for passable cells.
    /// </summary>
    public static List<Vector2> FindPath(
        Vector2 startWorld,
        Vector2 goalWorld,
        System.Func<Vector2Int, bool> isWalkable,
        int maxSearchCells = 512)
    {
        Vector2Int start = WorldToCell(startWorld);
        Vector2Int goal = WorldToCell(goalWorld);

        if (start == goal)
            return new List<Vector2>();

        var openSet = new SortedList<float, Vector2Int>(new DuplicateKeyComparer());
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();

        gScore[start] = 0f;
        openSet.Add(Heuristic(start, goal), start);

        int iterations = 0;

        while (openSet.Count > 0 && iterations++ < maxSearchCells)
        {
            Vector2Int current = openSet.Values[0];
            openSet.RemoveAt(0);

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (Vector2Int neighbour in Neighbours(current))
            {
                if (!isWalkable(neighbour))
                    continue;

                float tentative = gScore.GetValueOrDefault(current, float.MaxValue) + 1f;

                if (tentative < gScore.GetValueOrDefault(neighbour, float.MaxValue))
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentative;
                    float f = tentative + Heuristic(neighbour, goal);
                    openSet.Add(f, neighbour);
                }
            }
        }

        return new List<Vector2>(); // no path found
    }

    // internals

    private static float Heuristic(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance

    private static readonly Vector2Int[] Dirs =
    {
        //4 directional
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,

        //8 directional, comment this part for 4 directional only
         new Vector2Int( 1,  1),
         new Vector2Int(-1,  1),
         new Vector2Int( 1, -1),
         new Vector2Int(-1, -1),
    };

    private static IEnumerable<Vector2Int> Neighbours(Vector2Int cell)
    {
        foreach (var d in Dirs)
            yield return cell + d;
    }

    private static List<Vector2> ReconstructPath(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();

        // Convert to world positions; skip index 0 (the enemy's own cell)
        var worldPath = new List<Vector2>(path.Count);
        for (int i = 1; i < path.Count; i++)
            worldPath.Add(CellToWorld(path[i]));

        return worldPath;
    }

    // SortedList requires unique keys; this comparer breaks ties deterministically
    private class DuplicateKeyComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            int result = x.CompareTo(y);
            return result == 0 ? 1 : result; // never return 0 ? allow duplicates
        }
    }
}