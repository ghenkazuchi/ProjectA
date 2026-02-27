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
	private HashSet<Vector2Int> allPathTiles = new HashSet<Vector2Int>();
	private bool mapGenerated = false;
	private Dictionary<Vector2Int, GameObject> spawnedObjectInMap = new Dictionary<Vector2Int, GameObject>();
	public event System.Action OnMapGenerated;
	public float pathNoiseScale = 0.5f;

	[Header("Spawn Settings")]
	[Range(0f, 0.5f)] public float globalSpawnDensity = 0.05f;
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

		var topology = new MapTopologyAnalyzer(allPathTiles, playerSpawnPosition.Value, MapSizeInt, safeRing);
		var director = new InteractableSpawnDirector(this, topology, spawnedObjectInMap);
		director.SpawnAll();

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

	public Vector2Int MapSizeInt => new Vector2Int(Mathf.CeilToInt(mapRegionSize.x), Mathf.CeilToInt(mapRegionSize.y));

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
