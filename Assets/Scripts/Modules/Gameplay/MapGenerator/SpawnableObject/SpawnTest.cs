using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTest : MonoBehaviour
{
	public VoronoiPathGenerator voronoiPathGenerator;
	public GameObject chestPrefabs;
	private void Start()
	{
		Spawn();
	}
	[ContextMenu("Spawn Chest")]
	private void Spawn()
	{
		Vector2Int spawnGridPosition = voronoiPathGenerator.GetRandomPathTile();
		Vector3 spawnWorldPosition = voronoiPathGenerator.GetWorldPosition(spawnGridPosition);
		Transform chest =  Instantiate(chestPrefabs, spawnWorldPosition, Quaternion.identity).transform;
		chest.localScale = new Vector3(3.6f, 3.6f, 0);
	}
}
