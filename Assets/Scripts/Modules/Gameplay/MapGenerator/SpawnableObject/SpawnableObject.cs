using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "newSpawnableObject", menuName = "SpawnableObject/Create new Spawnable Object")]
public class SpawnableObject : ScriptableObject
{
	[Header("Core setting")]
	public InteracableGroup group;
	public InteracableType interacableType;
	public Sprite ingameSprite;
	public GameObject prefab;
	[Min(1)] public int spawnWeight = 1;

	[Header("Spawn Limits")]
	[Tooltip("Max copies of this object per map. 0 = unlimited")]
	[Min(0)] public int maxPerMap = 0;

	[Tooltip("Min grid distance (Manhattan) between copies of the SAME object type. 0 = no restriction")]
	[Min(0)] public int minSelfSpacing = 0;

	[Header("Unique setting")]
	public bool uniquePerMap = false;
	public string uniqueKey;

	public string GetKey()
	{
		return string.IsNullOrWhiteSpace(uniqueKey) ? name : uniqueKey;
	}

}
