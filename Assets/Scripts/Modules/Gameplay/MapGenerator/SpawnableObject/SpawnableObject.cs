using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "newSpawnableObject", menuName = "SpawnableObject/Create new Spawnable Object")]
public class SpawnableObject : ScriptableObject
{
	public InteracableType interacableType;
	public Sprite ingameSprite;
	public GameObject prefab;

}
