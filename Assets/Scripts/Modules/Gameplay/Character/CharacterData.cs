using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/Create Character Data")]
public class CharacterData : BaseEntityData
{
	[Header("Info")]
	[SerializeField] private string description;
	public string Description => description;
}
