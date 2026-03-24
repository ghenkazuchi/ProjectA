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

	[Header("Unique setting")]
	public bool uniquePerMap = false;
	public string uniqueKey;

	public string GetKey()
	{
		return string.IsNullOrWhiteSpace(uniqueKey) ? name : uniqueKey;
	}

}
