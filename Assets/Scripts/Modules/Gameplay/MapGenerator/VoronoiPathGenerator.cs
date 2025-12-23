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
	[Header("SpawnableObject")]
	public List<SpawnableObject> spawnableObjects = new List<SpawnableObject>();
	public int numberOfSpawnable = 1000;
	private HashSet<Vector2Int> allPathTiles = new HashSet<Vector2Int>();
	private bool mapGenerated = false;
	private Dictionary<Vector2Int, GameObject> spawnedObjectInMap = new Dictionary<Vector2Int, GameObject>();
	public event System.Action OnMapGenerated; 
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

		//List<KeyValuePair<Vector2,Vector2>> edgePoints = new List<KeyValuePair<Vector2,Vector2>>();
		Vector2Int mapSizeInt = new Vector2Int(
			Mathf.CeilToInt(mapRegionSize.x),
			Mathf.CeilToInt(mapRegionSize.y)
		);
		int visibleEdges = 0;
		foreach (Edge e in edges)
		{
			if (e.Visible())
			{
				visibleEdges++;
				Vector2 point1 = e.ClippedVertices[LR.LEFT];
				Vector2 point2 = e.ClippedVertices[LR.RIGHT];
				//edgePoints.Add(new KeyValuePair<Vector2, Vector2>(point1, point2));
				Vector2Int startPoint = new Vector2Int(Mathf.RoundToInt(point1.x), Mathf.RoundToInt(point1.y));
				Vector2Int endPoint = new Vector2Int(Mathf.RoundToInt(point2.x), Mathf.RoundToInt(point2.y));
				startPoint.x = Mathf.Clamp(startPoint.x, 0, mapSizeInt.x - 1);
				startPoint.y = Mathf.Clamp(startPoint.y, 0, mapSizeInt.y - 1);
				endPoint.x = Mathf.Clamp(endPoint.x, 0, mapSizeInt.x - 1);
				endPoint.y = Mathf.Clamp(endPoint.y, 0, mapSizeInt.y - 1);
				List<Vector2Int> currentPath = Pathfinding.FindPathAStar(startPoint, endPoint, mapSizeInt);
				if (currentPath != null)
				{
					foreach (Vector2Int pathNode in currentPath)
					{
						if (pathNode.x >= 0 && pathNode.x < mapSizeInt.x && pathNode.y >= 0 && pathNode.y < mapSizeInt.y)
						{
							allPathTiles.Add(pathNode);
						}
					}
				}
			}
		}
		voronoiDiagram.Dispose();
		pathTilemap.ClearAllTiles();
		foreach (Vector2Int pathCoord in allPathTiles)
		{
			Vector3Int tilePosition = new Vector3Int(pathCoord.x, pathCoord.y, 0);
			pathTilemap.SetTile(tilePosition, pathTile);
		}
		regionTilemap.ClearAllTiles();
		int regionTileCount = 0;
		for (int x = 0; x < Mathf.CeilToInt(mapRegionSize.x); x++)
		{
			for (int y = 0; y < Mathf.CeilToInt(mapRegionSize.y); y++)
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
		SpawnInteractableObjects();
		mapGenerated = true;
		OnMapGenerated?.Invoke();
	}
	private void SpawnInteractableObjects()
	{
		List<Vector2Int> availablePathTiles = allPathTiles.ToList();
		if (playerSpawnPosition.HasValue)
		{
			availablePathTiles.Remove(playerSpawnPosition.Value);
		}
		int objectsToSpawnCount = Mathf.Min(numberOfSpawnable, availablePathTiles.Count);
		for (int i = 0; i < objectsToSpawnCount; i++)
		{
			if (availablePathTiles.Count == 0) break;
			int randomIndex = Random.Range(0, availablePathTiles.Count);
			Vector2Int spawnCoord = availablePathTiles[randomIndex];
			availablePathTiles.RemoveAt(randomIndex);
			SpawnableObject selectedType = spawnableObjects[Random.Range(0, spawnableObjects.Count)];
			Vector3 spawnPosition = GetWorldPosition(spawnCoord);
			GameObject spawnedInstance = Instantiate(selectedType.prefab, spawnPosition, Quaternion.identity);
			Interacable interactable = spawnedInstance.GetComponent<Interacable>();
			spawnedObjectInMap[spawnCoord] = spawnedInstance;
		}
	}
	[ContextMenu("ClearSpawnableObject")]
	public void ClearSpawnableObjects()
	{
		foreach (var entry in spawnedObjectInMap)
		{
			if (entry.Value != null)	
			{
				Destroy(entry.Value);
			}
		}
	}
	public Vector2Int GetRandomPathTile()
	{
		List<Vector2Int> pathList = allPathTiles.ToList();
		Debug.Log(pathList.Count);
		return pathList[Random.Range(0, pathList.Count)];
	}
	public bool IsPathTile(Vector2Int gridPosition)
	{
		return allPathTiles.Contains(gridPosition);
	}
	public Vector3 GetWorldPosition(Vector2Int gridPosition)
	{
		return new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0);
	}
	public bool IsMapGenerated()
	{
		return mapGenerated;
	}

	public void SetPlayerPosition(Vector2Int position)
	{
		playerSpawnPosition = position;
	}
}

