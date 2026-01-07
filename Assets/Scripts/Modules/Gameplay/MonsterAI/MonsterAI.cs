using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
	[Header("Vision")]
	public int visionRange = 4;

	[Header("Grid Speed (tiles / second)")]
	public float patrolTilesPerSecond = 3f;
	public float chaseTilesPerSecond = 6f;

	[Header("Pathfinding")]
	public float repathInterval = 0.4f;

	private VoronoiPathGenerator map;
	private Transform player;

	private Vector2Int currentGrid;
	private List<Vector2Int> path;
	private int pathIndex;

	private float nextStepTime;
	private float nextRepathTime;

	private enum State { Patrol, Chase }
	private State state = State.Patrol;

	void Start()
	{
		map = FindObjectOfType<VoronoiPathGenerator>();
		player = GameObject.FindGameObjectWithTag("Player")?.transform;

		StartCoroutine(WaitForMap());
	}

	IEnumerator WaitForMap()
	{
		while (map == null || !map.IsMapGenerated())
			yield return null;

		currentGrid = map.WorldToGrid(transform.position);
		if (!map.IsPathTile(currentGrid))
			currentGrid = map.GetRandomPathTile();

		transform.position = map.GetWorldPosition(currentGrid);
	}

	void Update()
	{
		if (map == null || player == null) return;
		if (!map.IsMapGenerated()) return;
		if (GameController.Instance.currentState != GameState.FreeRoam) return;

		Vector2Int playerGrid = map.WorldToGrid(player.position);
		int dist = Mathf.Abs(playerGrid.x - currentGrid.x) +
				   Mathf.Abs(playerGrid.y - currentGrid.y);

		if (dist <= visionRange)
			state = State.Chase;
		else
			state = State.Patrol;

		if (state == State.Chase && Time.time >= nextRepathTime)
		{
			Repath(playerGrid);
			nextRepathTime = Time.time + repathInterval;
		}

		StepMove();
	}

	void Repath(Vector2Int target)
	{
		Vector2Int mapSize = new Vector2Int(
			Mathf.CeilToInt(map.mapRegionSize.x),
			Mathf.CeilToInt(map.mapRegionSize.y)
		);

		path = Pathfinding.FindPathAStar(
			currentGrid,
			target,
			mapSize,
			map.IsPathTile 
		);

		if (path != null && path.Count > 1)
			pathIndex = 1;
		else
			path = null;
	}

	void StepMove()
	{
		if (path == null || pathIndex >= path.Count)
			return;

		float tilesPerSec = state == State.Chase
			? chaseTilesPerSecond
			: patrolTilesPerSecond;

		float stepInterval = 1f / Mathf.Max(tilesPerSec, 0.01f);
		if (Time.time < nextStepTime) return;

		Vector2Int next = path[pathIndex];

		transform.position = map.GetWorldPosition(next);
		currentGrid = next;
		pathIndex++;

		nextStepTime = Time.time + stepInterval;

		if (pathIndex >= path.Count)
			path = null;
	}
}
