using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VoronoiPathGenerator : MonoBehaviour
{
	public Vector2Int? playerSpawnPosition;
	public float poissonRadius = 3f;
	public Vector2 mapRegionSize = new Vector2(50, 30);
	public int tryNumber = 20;
	public Tilemap pathTilemap;
	public Tilemap regionTilemap;
	public Tile pathTile;
	public Tile regionTile;

	public List<SpawnableObject> spawnableObjects = new List<SpawnableObject>();
	public int numberOfSpawnable = 1000;
	private HashSet<Vector2Int> allPathTiles = new HashSet<Vector2Int>();
	private bool mapGenerated = false;
	private Dictionary<Vector2Int, GameObject> spawnedObjectInMap = new Dictionary<Vector2Int, GameObject>();
	public event System.Action OnMapGenerated;

	[Header("Auto Spawn Count")]
	public bool autoSpawnCount = true;
	[Range(0f, 0.5f)] public float spawnRatioOnPath = 0.12f;
	public int minSpawnCount = 20;
	public int maxSpawnCount = 400;

	[Header("Spawn Spacing")]
	public int safeRing = 3;
	public int minSpacing = 3;

	[Header("Spawn Rules")]
	public List<SpawnRule> spawnRules = new List<SpawnRule>();


	private void Awake()
	{
		GenerateMap();
	}

	[ContextMenu("Generate map")]
	public void GenerateMapInspector()
	{
		ClearSpawnableObjects();
		GenerateMap();
	}

	void GenerateMap()
	{
		ClearSpawnableObjects();
		List<Vector2> sitePoints = PoissonDisc.GeneratePoints(poissonRadius, mapRegionSize, tryNumber);

		Rect bounds = new Rect(0, 0, mapRegionSize.x, mapRegionSize.y);
		Voronoi voronoiDiagram = new Voronoi(sitePoints, bounds);
		List<Edge> edges = voronoiDiagram.Edges;
		allPathTiles.Clear();

		Vector2Int mapSizeInt = new Vector2Int(
			Mathf.CeilToInt(mapRegionSize.x),
			Mathf.CeilToInt(mapRegionSize.y)
		);

		int visibleEdges = 0;

		foreach (Edge e in edges)
		{
			if (!e.Visible())
				continue;

			Vector2 point1 = e.ClippedVertices[LR.LEFT];
			Vector2 point2 = e.ClippedVertices[LR.RIGHT];

			Vector2Int startPoint = new Vector2Int(Mathf.RoundToInt(point1.x), Mathf.RoundToInt(point1.y));
			Vector2Int endPoint = new Vector2Int(Mathf.RoundToInt(point2.x), Mathf.RoundToInt(point2.y));

			startPoint.x = Mathf.Clamp(startPoint.x, 0, mapSizeInt.x - 1);
			startPoint.y = Mathf.Clamp(startPoint.y, 0, mapSizeInt.y - 1);
			endPoint.x = Mathf.Clamp(endPoint.x, 0, mapSizeInt.x - 1);
			endPoint.y = Mathf.Clamp(endPoint.y, 0, mapSizeInt.y - 1);

			List<Vector2Int> currentPath = Pathfinding.FindPathAStar(startPoint, endPoint, mapSizeInt);
			if (currentPath == null)
				continue;

			visibleEdges++;
			foreach (var node in currentPath)
				allPathTiles.Add(node);
		}

		pathTilemap.ClearAllTiles();
		regionTilemap.ClearAllTiles();

		int pathTileCount = 0;
		int regionTileCount = 0;

		foreach (Vector2Int pathTilePos in allPathTiles)
		{
			Vector3Int tilePosition = new Vector3Int(pathTilePos.x, pathTilePos.y, 0);
			pathTilemap.SetTile(tilePosition, pathTile);
			pathTileCount++;
		}

		for (int x = 0; x < mapSizeInt.x; x++)
		{
			for (int y = 0; y < mapSizeInt.y; y++)
			{
				Vector2Int currentCell = new Vector2Int(x, y);
				if (!allPathTiles.Contains(currentCell))
				{
					Vector3Int tilePosition = new Vector3Int(x, y, 0);
					regionTilemap.SetTile(tilePosition, regionTile);
					regionTileCount++;
				}
			}
		}

		playerSpawnPosition = ChooseCenteredSpawn(mapSizeInt);

		if (autoSpawnCount)
		{
			int pathCount = allPathTiles.Count;
			int autoCount = Mathf.RoundToInt(pathCount * spawnRatioOnPath);
			numberOfSpawnable = Mathf.Clamp(autoCount, minSpawnCount, maxSpawnCount);
		}

		SpawnInteractableObjects();

		mapGenerated = true;
		OnMapGenerated?.Invoke();
	}

	public void ClearSpawnableObjects()
	{
		foreach (var entry in spawnedObjectInMap)
		{
			if (entry.Value != null)
			{
				Destroy(entry.Value);
			}
		}
		spawnedObjectInMap.Clear();
	}

	public bool IsMapGenerated()
	{
		return mapGenerated;
	}

	public bool IsPathTile(Vector2Int gridPos)
	{
		return allPathTiles.Contains(gridPos);
	}

	public Vector3 GetWorldPosition(Vector2Int gridPos)
	{
		return pathTilemap.CellToWorld(new Vector3Int(gridPos.x, gridPos.y, 0)) + pathTilemap.cellSize / 2;
	}

	private int Degree4(Vector2Int p)
	{
		int d = 0;
		if (allPathTiles.Contains(p + Vector2Int.up)) d++;
		if (allPathTiles.Contains(p + Vector2Int.down)) d++;
		if (allPathTiles.Contains(p + Vector2Int.left)) d++;
		if (allPathTiles.Contains(p + Vector2Int.right)) d++;
		return d;
	}

	private int DistToBorder(Vector2Int p, Vector2Int size)
	{
		int left = p.x;
		int right = size.x - 1 - p.x;
		int down = p.y;
		int up = size.y - 1 - p.y;
		return Mathf.Min(left, right, down, up);
	}

	private Vector2Int ChooseCenteredSpawn(Vector2Int mapSizeInt)
	{
		if (allPathTiles.Count == 0) return Vector2Int.zero;

		Vector2 center = new Vector2((mapSizeInt.x - 1) * 0.5f, (mapSizeInt.y - 1) * 0.5f);

		Vector2Int best = default;
		float bestScore = float.PositiveInfinity;

		foreach (var p in allPathTiles)
		{
			float dx = p.x - center.x;
			float dy = p.y - center.y;
			float dist2 = dx * dx + dy * dy;

			int deg = Degree4(p);
			int border = DistToBorder(p, mapSizeInt);

			float penalty = 0f;
			if (deg <= 1) penalty += 1000f;
			penalty += Mathf.Max(0, 4 - border) * 50f; 
			penalty += Mathf.Abs(deg - 2) * 10f; 

			float score = dist2 + penalty;
			if (score < bestScore)
			{
				bestScore = score;
				best = p;
			}
		}

		return best;
	}

	public Vector2Int GetPlayerSpawnPosition()
	{
		if (!playerSpawnPosition.HasValue)
		{
			Vector2Int mapSizeInt = new Vector2Int(
				Mathf.CeilToInt(mapRegionSize.x),
				Mathf.CeilToInt(mapRegionSize.y)
			);
			playerSpawnPosition = ChooseCenteredSpawn(mapSizeInt);
		}
		return playerSpawnPosition.Value;
	}

	private Dictionary<Vector2Int, int> ComputeDepth(Vector2Int start)
	{
		var dist = new Dictionary<Vector2Int, int>(allPathTiles.Count);
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
				if (!allPathTiles.Contains(nb) || dist.ContainsKey(nb)) continue;
				dist[nb] = cd + 1;
				q.Enqueue(nb);
			}
		}

		return dist;
	}

	private bool PrefabHas<T>(SpawnableObject so) where T : Component
	{
		return so != null && so.prefab != null && so.prefab.GetComponent<T>() != null;
	}

	private void SpawnInteractableObjects()
	{
		if (allPathTiles == null || allPathTiles.Count == 0) return;

		Vector2Int spawn = GetPlayerSpawnPosition();
		var depth = ComputeDepth(spawn);

		int maxDepth = 0;
		foreach (var kv in depth)
			if (kv.Value > maxDepth) maxDepth = kv.Value;

		// 1) Build candidate tiles (dead-end / junction / corridor), đã lọc safeRing
		var deadEnds = new List<Vector2Int>();
		var junctions = new List<Vector2Int>();
		var corridors = new List<Vector2Int>();

		foreach (var p in allPathTiles)
		{
			if (p == spawn) continue;

			int d = depth.TryGetValue(p, out var dd) ? dd : int.MaxValue;
			if (d <= safeRing) continue;

			int deg = Degree4(p);
			if (deg <= 1) deadEnds.Add(p);
			else if (deg >= 3) junctions.Add(p);
			else corridors.Add(p);
		}

		// 2) Group spawnables by group (data-driven)
		var byGroup = new Dictionary<InteracableGroup, List<SpawnableObject>>();
		foreach (var so in spawnableObjects)
		{
			if (so == null || so.prefab == null) continue;

			if (!byGroup.TryGetValue(so.group, out var list))
			{
				list = new List<SpawnableObject>(8);
				byGroup[so.group] = list;
			}
			list.Add(so);
		}

		if (byGroup.Count == 0) return;

		int budget = numberOfSpawnable;
		Vector2 mapCenter = new Vector2((Mathf.CeilToInt(mapRegionSize.x) - 1) * 0.5f, (Mathf.CeilToInt(mapRegionSize.y) - 1) * 0.5f);

		bool FarEnough(Vector2Int p, int minManhattan)
		{
			foreach (var kv in spawnedObjectInMap)
			{
				var q = kv.Key;
				int man = Mathf.Abs(p.x - q.x) + Mathf.Abs(p.y - q.y);
				if (man < minManhattan) return false;
			}
			return true;
		}

		bool TryPlace(SpawnableObject so, Vector2Int p, int localSpacing)
		{
			if (so == null || so.prefab == null) return false;
			if (spawnedObjectInMap.ContainsKey(p)) return false;

			int spacing = Mathf.Max(minSpacing, localSpacing);
			if (!FarEnough(p, spacing)) return false;

			Vector3 pos = GetWorldPosition(p);
			GameObject inst = Instantiate(so.prefab, pos, Quaternion.identity);

			var inter = inst.GetComponent<Interacable>();
			if (inter != null) inter.spawnableData = so;

			spawnedObjectInMap[p] = inst;
			return true;
		}

		// candidate picker by SpawnTileMask
		Vector2Int PickCandidateByMask(SpawnTileMask mask)
		{
			int a = (mask & SpawnTileMask.DeadEnd) != 0 ? deadEnds.Count : 0;
			int b = (mask & SpawnTileMask.Junction) != 0 ? junctions.Count : 0;
			int c = (mask & SpawnTileMask.Corridor) != 0 ? corridors.Count : 0;

			int total = a + b + c;
			if (total <= 0) return spawn;

			int r = Random.Range(0, total);
			if (a > 0)
			{
				if (r < a) return deadEnds[r];
				r -= a;
			}
			if (b > 0)
			{
				if (r < b) return junctions[r];
				r -= b;
			}
			return corridors[Mathf.Clamp(r, 0, Mathf.Max(0, corridors.Count - 1))];
		}

		bool PassDepthFilters(Vector2Int p, SpawnRule rule)
		{
			if (!depth.TryGetValue(p, out int d)) return false;
			if (d <= Mathf.Max(safeRing, rule.minDistanceFromSpawn)) return false;

			int minD = rule.minDepth;
			int maxD = rule.maxDepth;

			if (rule.useDepthPercent)
			{
				int pMin = Mathf.RoundToInt(maxDepth * Mathf.Clamp01(rule.minDepthPercent));
				int pMax = Mathf.RoundToInt(maxDepth * Mathf.Clamp01(rule.maxDepthPercent));
				minD = Mathf.Max(minD, pMin);
				maxD = Mathf.Min(maxD, pMax);
			}

			return d >= minD && d <= maxD;
		}

		float Score(Vector2Int p, SpawnRule rule)
		{
			float s = 0f;

			int d = depth.TryGetValue(p, out var dd) ? dd : 999999;
			if (rule.preferDeeper) s -= d * 10f;

			if (rule.preferCentered)
			{
				float dx = p.x - mapCenter.x;
				float dy = p.y - mapCenter.y;
				s += (dx * dx + dy * dy);
			}

			return s;
		}

		SpawnableObject PickFromRuleGroups(SpawnRule rule)
		{
			// rule.matchTypes là list InteracableGroup :contentReference[oaicite:1]{index=1}
			var avail = new List<InteracableGroup>(rule.matchTypes.Count);
			for (int i = 0; i < rule.matchTypes.Count; i++)
			{
				var g = rule.matchTypes[i];
				if (byGroup.TryGetValue(g, out var pool) && pool.Count > 0) avail.Add(g);
			}
			if (avail.Count == 0) return null;

			var chosenGroup = avail[Random.Range(0, avail.Count)];
			var list = byGroup[chosenGroup];
			return list[Random.Range(0, list.Count)];
		}

		bool TrySpawnByRule(SpawnRule rule)
		{
			if (rule == null || rule.matchTypes == null || rule.matchTypes.Count == 0) return false;

			var so = PickFromRuleGroups(rule);
			if (so == null) return false;

			int tries = Mathf.Max(1, rule.maxTriesPerObject);
			int bestOf = Mathf.Clamp(rule.bestOf, 1, 200);

			Vector2Int bestPos = default;
			float bestScore = float.PositiveInfinity;
			bool found = false;

			// best-of-k sampling
			int samples = Mathf.Min(bestOf, tries);
			for (int i = 0; i < samples; i++)
			{
				var p = PickCandidateByMask(rule.tileMask);
				if (p == spawn) continue;
				if (spawnedObjectInMap.ContainsKey(p)) continue;
				if (!PassDepthFilters(p, rule)) continue;
				if (!FarEnough(p, Mathf.Max(minSpacing, rule.minSpacing))) continue;

				float s = Score(p, rule);
				if (s < bestScore)
				{
					bestScore = s;
					bestPos = p;
					found = true;
				}
			}

			if (!found)
			{
				for (int t = 0; t < tries; t++)
				{
					var p = PickCandidateByMask(rule.tileMask);
					if (p == spawn) continue;
					if (spawnedObjectInMap.ContainsKey(p)) continue;
					if (!PassDepthFilters(p, rule)) continue;

					if (TryPlace(so, p, rule.minSpacing))
						return true;
				}
				return false;
			}

			return TryPlace(so, bestPos, rule.minSpacing);
		}

		// 3) Apply rules
		bool hasRules = spawnRules != null && spawnRules.Count > 0;

		if (hasRules)
		{
			foreach (var rule in spawnRules)
			{
				if (rule == null) continue;

				int target = rule.autoCountByPathTiles
					? Mathf.Clamp(Mathf.RoundToInt(allPathTiles.Count * rule.ratioOnPath), rule.minCount, rule.maxCount)
					: Mathf.Clamp(rule.maxCount, rule.minCount, rule.maxCount);

				if (rule.ensureAtLeastOne && target <= 0) target = 1;

				for (int i = 0; i < target; i++)
				{
					if (rule.useGlobalBudget && budget <= 0) break;

					bool ok = TrySpawnByRule(rule);
					if (!ok) break;

					if (rule.useGlobalBudget) budget--;
				}
			}
		}

		// 4) Fallback: nếu thiếu rule hoặc rule fail, vẫn spawn nhẹ để không bị "trống"
		if (budget > 0)
		{
			var allCandidates = new List<Vector2Int>(corridors.Count + junctions.Count + deadEnds.Count);
			allCandidates.AddRange(corridors);
			allCandidates.AddRange(junctions);
			allCandidates.AddRange(deadEnds);

			if (allCandidates.Count > 0)
			{
				int fallbackCount = Mathf.Clamp(Mathf.RoundToInt(allPathTiles.Count * 0.03f), 1, Mathf.Min(50, budget));
				var groups = byGroup.Keys.ToList();

				for (int i = 0; i < fallbackCount && budget > 0; i++)
				{
					var g = groups[Random.Range(0, groups.Count)];
					var pool = byGroup[g];
					if (pool == null || pool.Count == 0) continue;

					var so = pool[Random.Range(0, pool.Count)];

					for (int t = 0; t < 200; t++)
					{
						var p = allCandidates[Random.Range(0, allCandidates.Count)];
						int d = depth.TryGetValue(p, out var dd) ? dd : int.MaxValue;
						if (d <= safeRing) continue;

						if (TryPlace(so, p, localSpacing: 3))
						{
							budget--;
							break;
						}
					}
				}
			}
		}
	}

	public Vector2Int GetRandomPathTile()
	{
		if (allPathTiles == null || allPathTiles.Count == 0)
			return Vector2Int.zero;

		int idx = Random.Range(0, allPathTiles.Count);
		foreach (var p in allPathTiles)
		{
			if (idx-- == 0) return p;
		}
		foreach (var p in allPathTiles) return p;
		return Vector2Int.zero;
	}
	public Vector2Int WorldToGrid(Vector3 worldPos)
	{
		if (pathTilemap == null)
			return new Vector2Int(
				Mathf.RoundToInt(worldPos.x - 0.5f),
				Mathf.RoundToInt(worldPos.y - 0.5f)
			);

		Vector3Int cell = pathTilemap.WorldToCell(worldPos);
		return new Vector2Int(cell.x, cell.y);
	}
}
