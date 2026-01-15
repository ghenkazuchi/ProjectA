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
[CreateAssetMenu(menuName = "Map/Spawn Rule", fileName = "SpawnRule")]
public class SpawnRule : ScriptableObject
{
	public List<InteracableGroup> matchTypes = new List<InteracableGroup>();
	public SpawnTileMask tileMask = SpawnTileMask.Any;
	public int minDepth = 0;
	public int maxDepth = 999999;
	[Header("Depth by Percent (optional)")]
	public bool useDepthPercent = false;
	[Range(0f, 1f)] public float minDepthPercent = 0f;
	[Range(0f, 1f)] public float maxDepthPercent = 1f;

	[Header("Counts")]
	public bool autoCountByPathTiles = false;
	[Range(0f, 0.5f)] public float ratioOnPath = 0.05f;
	public int minCount = 0;
	public int maxCount = 10;
	public bool useGlobalBudget = true;

	[Header("Spacing & Attempts")]
	public int minDistanceFromSpawn = 3;
	public int minSpacing = 3;
	public int maxTriesPerObject = 250;

	[Header("Quality (best-of-k)")]
	public int bestOf = 20;
	public bool preferDeeper = false;
	public bool preferCentered = false;

	[Header("Guarantee")]
	public bool ensureAtLeastOne = false;
}
