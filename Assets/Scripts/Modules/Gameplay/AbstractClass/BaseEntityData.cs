using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEntityData : ScriptableObject
{
	[Header("General Info")]
	[SerializeField] private string entityName;
	[SerializeField] private Sprite entitySprite;
	[SerializeField] private Sprite entityPortrait;
	[SerializeField] private Element entityElement;
	[Header("Base Traits at Level 1")]
	[SerializeField] private SerializableDictionaryBase<Trait, int> baseTraits = new SerializableDictionaryBase<Trait, int>();
	[Header("Exculive Active/Passsive Skills(optional)")]
	public ActiveSkillData exclusiveActiveSkill;
	public PassiveSkillData exclusivePassiveSkill;

	public ActiveSkillData ExclusiveActiveSkill => exclusiveActiveSkill;
	public PassiveSkillData ExclusivePassiveSkill => exclusivePassiveSkill;		
	public string EntityName
	{
		get => entityName;
		set => entityName = value;
	}

	public Sprite EntitySprite
	{
		get => entitySprite;
		set => entitySprite = value;
	}
	public Element EntityElement
	{
		get => entityElement;
		set => entityElement = value;
	}
	public Sprite EntityPortrait
	{
		get => entityPortrait;
		set => entityPortrait = value;
	}
	public SerializableDictionaryBase<Trait, int> BaseTraits => baseTraits;
}
