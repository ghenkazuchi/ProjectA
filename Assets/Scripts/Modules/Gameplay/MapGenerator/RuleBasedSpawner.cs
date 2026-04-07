using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuleBasedSpawner
{
    private VoronoiPathGenerator generator;
    private MapTopologyAnalyzer topology;
    private Dictionary<Vector2Int, GameObject> spawnedObjects;
    private Dictionary<InteracableGroup, List<SpawnableObject>> byGroup;

    // Per-type tracking for spawn limits
    private Dictionary<SpawnableObject, int> spawnCounts = new Dictionary<SpawnableObject, int>();
    private Dictionary<SpawnableObject, List<Vector2Int>> spawnPositions = new Dictionary<SpawnableObject, List<Vector2Int>>();

    public RuleBasedSpawner(VoronoiPathGenerator gen, MapTopologyAnalyzer topo, Dictionary<Vector2Int, GameObject> spawned, Dictionary<InteracableGroup, List<SpawnableObject>> groups)
    {
        generator = gen;
        topology = topo;
        spawnedObjects = spawned;
        byGroup = groups;
    }

    public int itemsSpawnedThisFrame = 0;
    public int batchLimit = 15;

    public int ExecuteRule(SpawnRule rule, int targetCount)
    {
        if (rule == null || rule.matchTypes == null || rule.matchTypes.Count == 0 || targetCount <= 0) 
            return 0;

        // 1. Gather all candidates matching the Tile Mask
        List<Vector2Int> candidates = GatherCandidates(rule.tileMask);
        if (candidates.Count == 0) return 0;

        // 2. Filter by Depth and minimum distance from spawn
        Vector2Int spawnPos = generator.GetPlayerSpawnPosition();
        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            Vector2Int p = candidates[i];
            if (p == spawnPos) { candidates.RemoveAt(i); continue; }
            
            if (!topology.DepthMap.TryGetValue(p, out int d)) d = int.MaxValue;
            if (d < rule.minDepth || d > rule.maxDepth || d <= Mathf.Max(generator.safeRing, rule.minDistanceFromSpawn))
            {
                candidates.RemoveAt(i);
            }
        }

        if (candidates.Count == 0) return 0;

        // 3. Sort or Shuffle based on Strategy
        if (rule.strategy == PlacementStrategy.DeepestFirst)
        {
            candidates.Sort((a, b) => 
            {
                int dA = topology.DepthMap.TryGetValue(a, out int da) ? da : 0;
                int dB = topology.DepthMap.TryGetValue(b, out int db) ? db : 0;
                return dB.CompareTo(dA); // Highest depth first
            });
        }
        else
        {
            ShuffleList(candidates);
        }

        // 4. Place Objects
        int spawnedCount = 0;
        foreach (var p in candidates)
        {
            if (spawnedCount >= targetCount) break;

            if (TryPlaceObject(rule, p))
            {
                spawnedCount++;
            }
        }

        return spawnedCount;
    }

    public IEnumerator ExecuteRuleCoroutine(SpawnRule rule, int targetCount)
    {
        if (rule == null || rule.matchTypes == null || rule.matchTypes.Count == 0 || targetCount <= 0) 
            yield break;

        // 1. Gather all candidates matching the Tile Mask
        List<Vector2Int> candidates = GatherCandidates(rule.tileMask);
        if (candidates.Count == 0) yield break;

        // 2. Filter by Depth and minimum distance from spawn
        Vector2Int spawnPos = generator.GetPlayerSpawnPosition();
        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            Vector2Int p = candidates[i];
            if (p == spawnPos) { candidates.RemoveAt(i); continue; }
            
            if (!topology.DepthMap.TryGetValue(p, out int d)) d = int.MaxValue;
            if (d < rule.minDepth || d > rule.maxDepth || d <= Mathf.Max(generator.safeRing, rule.minDistanceFromSpawn))
            {
                candidates.RemoveAt(i);
            }
        }

        if (candidates.Count == 0) yield break;

        // 3. Sort or Shuffle based on Strategy
        if (rule.strategy == PlacementStrategy.DeepestFirst)
        {
            candidates.Sort((a, b) => 
            {
                int dA = topology.DepthMap.TryGetValue(a, out int da) ? da : 0;
                int dB = topology.DepthMap.TryGetValue(b, out int db) ? db : 0;
                return dB.CompareTo(dA); // Highest depth first
            });
        }
        else
        {
            ShuffleList(candidates);
        }

        // 4. Place Objects
        int spawnedCount = 0;
        foreach (var p in candidates)
        {
            if (spawnedCount >= targetCount) break;

            if (TryPlaceObject(rule, p))
            {
                spawnedCount++;
                itemsSpawnedThisFrame++;
                if (itemsSpawnedThisFrame >= batchLimit)
                {
                    itemsSpawnedThisFrame = 0;
                    yield return null;
                }
            }
        }
    }

    public void SpawnScatter(int targetCount, List<SpawnableObject> pool)
    {
        if (targetCount <= 0 || pool == null || pool.Count == 0) return;

        List<Vector2Int> candidates = new List<Vector2Int>(topology.AllPathTiles);
        ShuffleList(candidates);

        Vector2Int spawnPos = generator.GetPlayerSpawnPosition();
        
        int spawnedCount = 0;

        foreach (var p in candidates)
        {
            if (spawnedCount >= targetCount) break;
            if (p == spawnPos || spawnedObjects.ContainsKey(p)) continue;
            
            int d = topology.DepthMap.TryGetValue(p, out var dd) ? dd : int.MaxValue;
            if (d <= generator.safeRing) continue;

            if (!FarEnough(p, generator.minSpacing)) continue;

            // Pick weighted random fallback object
            var so = WeightedPick(pool);
            if (so == null || so.prefab == null) continue;

            if (TryPlaceExplicit(so, p))
            {
                spawnedCount++;
            }
        }
    }

    public IEnumerator SpawnScatterCoroutine(int targetCount, List<SpawnableObject> pool)
    {
        if (targetCount <= 0 || pool == null || pool.Count == 0) yield break;

        List<Vector2Int> candidates = new List<Vector2Int>(topology.AllPathTiles);
        ShuffleList(candidates);

        Vector2Int spawnPos = generator.GetPlayerSpawnPosition();
        
        int spawnedCount = 0;

        foreach (var p in candidates)
        {
            if (spawnedCount >= targetCount) break;
            if (p == spawnPos || spawnedObjects.ContainsKey(p)) continue;
            
            int d = topology.DepthMap.TryGetValue(p, out var dd) ? dd : int.MaxValue;
            if (d <= generator.safeRing) continue;

            if (!FarEnough(p, generator.minSpacing)) continue;

            // Pick weighted random fallback object
            var so = WeightedPick(pool);
            if (so == null || so.prefab == null) continue;

            if (TryPlaceExplicit(so, p))
            {
                spawnedCount++;
                itemsSpawnedThisFrame++;
                if (itemsSpawnedThisFrame >= batchLimit)
                {
                    itemsSpawnedThisFrame = 0;
                    yield return null;
                }
            }
        }
    }

    private List<Vector2Int> GatherCandidates(SpawnTileMask mask)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        if ((mask & SpawnTileMask.DeadEnd) != 0) result.AddRange(topology.DeadEnds);
        if ((mask & SpawnTileMask.Junction) != 0) result.AddRange(topology.Junctions);
        if ((mask & SpawnTileMask.Corridor) != 0) result.AddRange(topology.Corridors);
        return result;
    }

    private bool TryPlaceObject(SpawnRule rule, Vector2Int position)
    {
        if (spawnedObjects.ContainsKey(position)) return false;

        // Check spacing
        int spacing = Mathf.Max(generator.minSpacing, rule.minSpacing);
        if (!FarEnough(position, spacing)) return false;

        var so = PickFromRuleGroups(rule);
        if (so == null) return false;

        return TryPlaceExplicit(so, position);
    }

    private static int portalCounter = 0;

    private bool TryPlaceExplicit(SpawnableObject so, Vector2Int position)
    {
        if (so == null || so.prefab == null) return false;

        // --- Per-type spawn limit checks ---
        // 1. Check maxPerMap
        if (so.maxPerMap > 0)
        {
            int currentCount = spawnCounts.TryGetValue(so, out int c) ? c : 0;
            if (currentCount >= so.maxPerMap) return false;
        }

        // 2. Check minSelfSpacing (distance to other copies of the SAME object)
        if (so.minSelfSpacing > 0 && spawnPositions.TryGetValue(so, out var positions))
        {
            foreach (var existing in positions)
            {
                int manhattan = Mathf.Abs(position.x - existing.x) + Mathf.Abs(position.y - existing.y);
                if (manhattan < so.minSelfSpacing) return false;
            }
        }

        // --- Place the object ---
        Vector3 pos = generator.GetWorldPosition(position);
        GameObject inst = Object.Instantiate(so.prefab, pos, Quaternion.identity);

        var inter = inst.GetComponent<Interacable>();
        if (inter != null) inter.spawnableData = so;

        // Initialize portal with grid position and auto-generated name
        var portal = inst.GetComponent<PortalInteracableObject>();
        if (portal != null)
        {
            portalCounter++;
            portal.Initialize(position, portalCounter);
        }

        // --- Update per-type tracking ---
        if (!spawnCounts.ContainsKey(so)) spawnCounts[so] = 0;
        spawnCounts[so]++;

        if (!spawnPositions.ContainsKey(so)) spawnPositions[so] = new List<Vector2Int>();
        spawnPositions[so].Add(position);

        spawnedObjects[position] = inst;
        return true;
    }

    private bool FarEnough(Vector2Int p, int minManhattan)
    {
        foreach (var kv in spawnedObjects)
        {
            var q = kv.Key;
            int man = Mathf.Abs(p.x - q.x) + Mathf.Abs(p.y - q.y);
            if (man < minManhattan) return false;
        }
        return true;
    }

    private SpawnableObject PickFromRuleGroups(SpawnRule rule)
    {
        // Build a flat weighted pool from all matching groups
        var weightedPool = new List<SpawnableObject>();
        for (int i = 0; i < rule.matchTypes.Count; i++)
        {
            var g = rule.matchTypes[i];
            if (byGroup.TryGetValue(g, out var pool))
                weightedPool.AddRange(pool);
        }
        return WeightedPick(weightedPool);
    }

    private SpawnableObject WeightedPick(List<SpawnableObject> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        int totalWeight = 0;
        foreach (var so in pool)
        {
            if (so != null) totalWeight += so.spawnWeight;
        }
        if (totalWeight <= 0) return pool[Random.Range(0, pool.Count)];

        int roll = Random.Range(0, totalWeight);
        foreach (var so in pool)
        {
            if (so == null) continue;
            roll -= so.spawnWeight;
            if (roll < 0) return so;
        }
        return pool[pool.Count - 1];
    }

    private void ShuffleList<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

