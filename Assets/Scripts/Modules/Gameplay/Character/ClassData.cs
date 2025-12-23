using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewClassData", menuName = "Character/Create Class Data")]
public class ClassData : ScriptableObject
{
	[Header("Class Definition")]
	public ClassType classType;
	public string description;

	[Header("ExpGrowth")]
	public ExpGrowth expGrowthRate;
	[Header("Stats Multipliers")]
	public SerializableDictionaryBase<Stat,float> statMultipliers = new SerializableDictionaryBase<Stat,float>();
	[Header("Trait Bonus")]
	public SerializableDictionaryBase<Trait,int> traitBonuses = new SerializableDictionaryBase<Trait, int> { };
	[Header("Learnable SkillSet by level")]
	public List<SkillEntry> skillSet = new List<SkillEntry>();
	[Header("Item slot")]
	public int itemSlotCount;
	[Header("Weapon Type can use")]
	public List<WeaponType> usableWeaponTypes = new List<WeaponType>();
}
[System.Serializable]
public class SkillEntry
{
	public BaseSkillData skill;
	public int levelRequirements;
}