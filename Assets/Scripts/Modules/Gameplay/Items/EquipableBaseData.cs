using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipableBaseData : ScriptableObject
{
	[Header("Equipable Stats bonus")]
	public List<EquipableStatBonus> EquipableStatBonus = new List<EquipableStatBonus>();
	public string itemName;
	public string description;
	public Sprite icon;
	public int maxUsePerLifeCyle;
	public int maxUsePerBattle;	
	public ItemRarity itemRarity;
	public List<EquipEffectBinding> effectData;

	public bool canDuplicateTrigger;
	[Header("Optional Set Bonuses")]
	[UnityEngine.Serialization.FormerlySerializedAs("equipmentSet")]
	public List<EquipableSetData> equipmentSets = new List<EquipableSetData>();
	public int pieceWeight = 1;

	[Header("Shop Price")]
	public int basePrice;
}
