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
		if (allPathTiles.Count == 0) return;

		Vector2Int spawn = GetPlayerSpawnPosition();
		var depth = ComputeDepth(spawn);

		int budget = numberOfSpawnable;

		var chests = new List<SpawnableObject>();
		var monsters = new List<SpawnableObject>();
		var shops = new List<SpawnableObject>();
		var camps = new List<SpawnableObject>();
		var recruits = new List<SpawnableObject>();
		var others = new List<SpawnableObject>();

		foreach (var so in spawnableObjects)
		{
			if (so == null || so.prefab == null) continue;

			if (PrefabHas<ChestInteracableObject>(so)) chests.Add(so);
			else if (PrefabHas<MonsterInteracableObject>(so)) monsters.Add(so);
			else if (PrefabHas<ShopKeeperInteractableObject>(so)) shops.Add(so);
			else if (PrefabHas<CampInteracableObject>(so)) camps.Add(so);
			else if (PrefabHas<RecruitableCharacterInteracable>(so)) recruits.Add(so);
			else if (PrefabHas<Interacable>(so)) others.Add(so);
		}

		var deadEnds = new List<Vector2Int>();
		var junctions = new List<Vector2Int>();
		var corridors = new List<Vector2Int>();

		int maxDepth = 0;
		foreach (var kv in depth) if (kv.Value > maxDepth) maxDepth = kv.Value;

		foreach (var p in allPathTiles)
		{
			if (p == spawn) continue;
			int d = depth.TryGetValue(p, out var dd) ? dd : 999999;
			if (d <= safeRing) continue;

			int deg = Degree4(p);
			if (deg <= 1) deadEnds.Add(p);
			else if (deg >= 3) junctions.Add(p);
			else corridors.Add(p);
		}

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

			int spacing = Mathf.Max(this.minSpacing, localSpacing);
			if (!FarEnough(p, spacing)) return false;

			Vector3 pos = GetWorldPosition(p);
			GameObject inst = Instantiate(so.prefab, pos, Quaternion.identity);

			var inter = inst.GetComponent<Interacable>();
			if (inter != null) inter.spawnableData = so;

			spawnedObjectInMap[p] = inst;
			return true;
		}

		if (budget > 0 && shops.Count > 0 && junctions.Count > 0)
		{
			int targetMin = Mathf.RoundToInt(maxDepth * 0.35f);
			int targetMax = Mathf.RoundToInt(maxDepth * 0.55f);

			Vector2Int best = junctions[0];
			float bestScore = float.PositiveInfinity;
			Vector2 center = new Vector2((Mathf.CeilToInt(mapRegionSize.x) - 1) * 0.5f, (Mathf.CeilToInt(mapRegionSize.y) - 1) * 0.5f);

			foreach (var p in junctions)
			{
				int d = depth[p];
				if (d < targetMin || d > targetMax) continue;

				float dx = p.x - center.x;
				float dy = p.y - center.y;
				float dist2 = dx * dx + dy * dy;

				float score = dist2 + Mathf.Max(0, 6 - d) * 200f;
				if (score < bestScore) { bestScore = score; best = p; }
			}

			if (budget > 0 && TryPlace(shops[Random.Range(0, shops.Count)], best, localSpacing: 8)) budget--;
		}

		int campCount = Mathf.Min(2, camps.Count);
		campCount = Mathf.Min(campCount, budget);
		if (campCount > 0 && corridors.Count > 0)
		{
			corridors.Sort((a, b) => depth[a].CompareTo(depth[b]));
			int startIdx = Mathf.RoundToInt(corridors.Count * 0.45f);
			int endIdx = Mathf.RoundToInt(corridors.Count * 0.75f);

			for (int i = 0; i < campCount; i++)
			{
				for (int t = 0; t < 200; t++)
				{
					if (budget <= 0) break;

					int idx = Random.Range(startIdx, Mathf.Max(startIdx + 1, endIdx));
					var p = corridors[idx];

					if (budget > 0 && TryPlace(camps[Random.Range(0, camps.Count)], p, localSpacing: 10)) { budget--; break; }
				}
			}
		}

		int recruitCount = Mathf.Min(3, recruits.Count);
		recruitCount = Mathf.Min(recruitCount, budget);
		if (recruitCount > 0)
		{
			var recCandidates = new List<Vector2Int>();
			recCandidates.AddRange(deadEnds);
			recCandidates.AddRange(corridors);

			for (int i = 0; i < recruitCount; i++)
			{
				for (int t = 0; t < 300; t++)
				{
					if (budget <= 0) break;

					var p = recCandidates[Random.Range(0, recCandidates.Count)];
					int d = depth[p];
					if (d < 6 || d > Mathf.RoundToInt(maxDepth * 0.7f)) continue;

					if (budget > 0 && TryPlace(recruits[Random.Range(0, recruits.Count)], p, localSpacing: 8)) { budget--; break; }
				}
			}
		}

		if (chests.Count > 0 && deadEnds.Count > 0 && budget > 0)
		{
			deadEnds.Sort((a, b) => depth[b].CompareTo(depth[a]));
			int chestCount = Mathf.Min(deadEnds.Count, Mathf.Max(5, Mathf.RoundToInt(numberOfSpawnable * 0.12f)));
			chestCount = Mathf.Min(chestCount, budget);

			for (int i = 0; i < chestCount && budget > 0; i++)
			{
				var p = deadEnds[i];
				if (budget > 0 && TryPlace(chests[Random.Range(0, chests.Count)], p, localSpacing: 4)) budget--;
			}
		}
		if (monsters.Count > 0 && budget > 0)
		{
			int monsterCount = Mathf.Max(8, Mathf.RoundToInt(numberOfSpawnable * 0.18f));
			monsterCount = Mathf.Min(monsterCount, budget);

			var monsterCandidates = new List<Vector2Int>();
			junctions.Sort((a, b) => depth[b].CompareTo(depth[a]));
			corridors.Sort((a, b) => depth[b].CompareTo(depth[a]));
			monsterCandidates.AddRange(junctions);
			monsterCandidates.AddRange(corridors);

			int placed = 0;
			for (int i = 0; i < monsterCandidates.Count && placed < monsterCount && budget > 0; i++)
			{
				var p = monsterCandidates[i];
				int d = depth[p];
				if (d <= 5) continue;

				if (TryPlace(monsters[Random.Range(0, monsters.Count)], p, localSpacing: 6))
				{
					placed++;
					budget--;
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
