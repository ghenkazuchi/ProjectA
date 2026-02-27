using System.Collections.Generic;
using UnityEngine;

public class MapTopologyAnalyzer
{
    public HashSet<Vector2Int> AllPathTiles { get; private set; }
    public Dictionary<Vector2Int, int> DepthMap { get; private set; }
    public List<Vector2Int> DeadEnds { get; private set; } = new List<Vector2Int>();
    public List<Vector2Int> Junctions { get; private set; } = new List<Vector2Int>();
    public List<Vector2Int> Corridors { get; private set; } = new List<Vector2Int>();
    public Vector2Int MapSizeInt { get; private set; }

    public int MaxDepth { get; private set; } = 0;

    public MapTopologyAnalyzer(HashSet<Vector2Int> pathTiles, Vector2Int spawn, Vector2Int mapSizeInt, int safeRing)
    {
        this.AllPathTiles = pathTiles;
        this.MapSizeInt = mapSizeInt;
        ComputeTopology(spawn, safeRing);
    }

    private void ComputeTopology(Vector2Int spawn, int safeRing)
    {
        DepthMap = ComputeDepth(spawn);

        MaxDepth = 0;
        foreach (var kv in DepthMap)
            if (kv.Value > MaxDepth) MaxDepth = kv.Value;

        foreach (var p in AllPathTiles)
        {
            if (p == spawn) continue;

            int d = DepthMap.TryGetValue(p, out var dd) ? dd : int.MaxValue;
            if (d <= safeRing) continue;

            int deg = Degree4(p);
            if (deg <= 1) DeadEnds.Add(p);
            else if (deg >= 3) Junctions.Add(p);
            else Corridors.Add(p);
        }
    }

    private Dictionary<Vector2Int, int> ComputeDepth(Vector2Int start)
    {
        var dist = new Dictionary<Vector2Int, int>(AllPathTiles.Count);
        var q = new Queue<Vector2Int>();

        dist[start] = 0;
        q.Enqueue(start);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int cd = dist[cur];

            foreach (var d in dirs)
            {
                var nb = cur + d;
                if (!AllPathTiles.Contains(nb) || dist.ContainsKey(nb)) continue;
                dist[nb] = cd + 1;
                q.Enqueue(nb);
            }
        }

        return dist;
    }

    public int Degree4(Vector2Int p)
    {
        int d = 0;
        if (AllPathTiles.Contains(p + Vector2Int.up)) d++;
        if (AllPathTiles.Contains(p + Vector2Int.down)) d++;
        if (AllPathTiles.Contains(p + Vector2Int.left)) d++;
        if (AllPathTiles.Contains(p + Vector2Int.right)) d++;
        return d;
    }
}
