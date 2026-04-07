using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InteractableSpawnDirector
{
    private VoronoiPathGenerator generator;
    private MapTopologyAnalyzer topology;
    private Dictionary<Vector2Int, GameObject> spawnedObjects;
    private Dictionary<InteracableGroup, List<SpawnableObject>> byGroup;

    public InteractableSpawnDirector(VoronoiPathGenerator generator, MapTopologyAnalyzer topology, Dictionary<Vector2Int, GameObject> spawnedObjects)
    {
        this.generator = generator;
        this.topology = topology;
        this.spawnedObjects = spawnedObjects;

        byGroup = new Dictionary<InteracableGroup, List<SpawnableObject>>();
        foreach (var so in generator.spawnableObjects)
        {
            if (so == null || so.prefab == null) continue;

            if (!byGroup.TryGetValue(so.group, out var list))
            {
                list = new List<SpawnableObject>(8);
                byGroup[so.group] = list;
            }
            list.Add(so);
        }
    }

    public void SpawnAll()
    {
        if (byGroup.Count == 0) return;

        RuleBasedSpawner spawner = new RuleBasedSpawner(generator, topology, spawnedObjects, byGroup);

        HashSet<InteracableGroup> uniqueGroups = new HashSet<InteracableGroup>();

        if (generator.spawnRules != null && generator.spawnRules.Count > 0)
        {
            Debug.Log($"[MapGen] Executing {generator.spawnRules.Count} Unique Rules.");
            foreach (var rule in generator.spawnRules)
            {
                if (rule == null) continue;
                
                foreach(var g in rule.matchTypes) 
                    uniqueGroups.Add(g);

                spawner.ExecuteRule(rule, rule.count);
            }
        }

        // Collect all objects that don't belong to a unique rule
        List<SpawnableObject> scatterPool = new List<SpawnableObject>();
        foreach (var kv in byGroup)
        {
            if (!uniqueGroups.Contains(kv.Key))
            {
                scatterPool.AddRange(kv.Value);
            }
        }
        
        Debug.Log($"[MapGen] Created Scatter Pool. Unique groups excluded: {uniqueGroups.Count}. Scatter pool objects: {scatterPool.Count}");

        // Scatter non-unique generic objects based on the global density
        if (scatterPool.Count > 0)
        {
            int currentSpawned = spawnedObjects.Count;
            int scatterTarget = Mathf.Max(0, Mathf.RoundToInt(topology.AllPathTiles.Count * generator.globalSpawnDensity) - currentSpawned);
            Debug.Log($"[MapGen] Scattering generic objects. PathTiles: {topology.AllPathTiles.Count}, Target: {scatterTarget}");
            spawner.SpawnScatter(scatterTarget, scatterPool);
        }
        else
        {
            Debug.LogWarning("[MapGen] Scatter pool is EMPTY! Did you assign ALL of your Object Groups to SpawnRules in the Inspector?");
        }
    }

    public IEnumerator SpawnAllCoroutine()
    {
        if (byGroup.Count == 0) yield break;

        RuleBasedSpawner spawner = new RuleBasedSpawner(generator, topology, spawnedObjects, byGroup);

        HashSet<InteracableGroup> uniqueGroups = new HashSet<InteracableGroup>();

        if (generator.spawnRules != null && generator.spawnRules.Count > 0)
        {
            Debug.Log($"[MapGen] Executing {generator.spawnRules.Count} Unique Rules.");
            foreach (var rule in generator.spawnRules)
            {
                if (rule == null) continue;
                
                foreach(var g in rule.matchTypes) 
                    uniqueGroups.Add(g);

                yield return generator.StartCoroutine(spawner.ExecuteRuleCoroutine(rule, rule.count));
            }
        }

        // Collect all objects that don't belong to a unique rule
        List<SpawnableObject> scatterPool = new List<SpawnableObject>();
        foreach (var kv in byGroup)
        {
            if (!uniqueGroups.Contains(kv.Key))
            {
                scatterPool.AddRange(kv.Value);
            }
        }
        
        Debug.Log($"[MapGen] Created Scatter Pool. Unique groups excluded: {uniqueGroups.Count}. Scatter pool objects: {scatterPool.Count}");

        // Scatter non-unique generic objects based on the global density
        if (scatterPool.Count > 0)
        {
            int currentSpawned = spawnedObjects.Count;
            int scatterTarget = Mathf.Max(0, Mathf.RoundToInt(topology.AllPathTiles.Count * generator.globalSpawnDensity) - currentSpawned);
            Debug.Log($"[MapGen] Scattering generic objects. PathTiles: {topology.AllPathTiles.Count}, Target: {scatterTarget}");
            yield return generator.StartCoroutine(spawner.SpawnScatterCoroutine(scatterTarget, scatterPool));
        }
        else
        {
            Debug.LogWarning("[MapGen] Scatter pool is EMPTY! Did you assign ALL of your Object Groups to SpawnRules in the Inspector?");
        }
    }
}
