using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Flags]
public enum SpawnTileMask
{
	DeadEnd = 1 << 0,
	Junction = 1 << 1,
	Corridor = 1 << 2,
	Any = DeadEnd | Junction | Corridor
}
public enum PlacementStrategy
{
	Random,
	DeepestFirst
}

[CreateAssetMenu(menuName = "Map/Spawn Rule (Simplified)", fileName = "SpawnRule")]
public class SpawnRule : ScriptableObject
{
	[Header("What to Spawn")]
	public List<InteracableGroup> matchTypes = new List<InteracableGroup>();
	
	[Header("Where to Spawn (Topology)")]
	public SpawnTileMask tileMask = SpawnTileMask.Any;
	public int minDepth = 0;
	public int maxDepth = 9999;

	[Header("How Many")]
	[Tooltip("Exact number of unique objects to spawn (e.g., 1 Boss, 2 Keys)")]
	public int count = 1;

	[Header("Placement Logic")]
	public PlacementStrategy strategy = PlacementStrategy.Random;
	public int minDistanceFromSpawn = 3;
	public int minSpacing = 3;
}
