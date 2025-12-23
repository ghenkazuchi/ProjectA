using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class RecruitableCharacterData
{
	[Header("Info Data")]
	public RecruitableCharacterTemplate characterTemplate;
	public int level;
	public int recruitmentCost;
	public string characterName;
	[Header("Generated Stats")]
	public SerializableDictionaryBase<Trait, int> generatedTraits = new SerializableDictionaryBase<Trait, int>();
	public SerializableDictionaryBase<Stat, float> previewStats = new SerializableDictionaryBase<Stat, float>();

	[Header("Available Skills")]
	public List<ActiveSkillData> availableActiveSkills = new List<ActiveSkillData>();
	public List<PassiveSkillData> availablePassiveSkills = new List<PassiveSkillData>();

	[Header("Final Charaacter Data")]
	public PlayerCharacter finalCharacterData;
	public RecruitableCharacterData(RecruitableCharacterTemplate template, int level)
	{
		this.characterTemplate = template;
		this.level = level;
		GenerateCharacterData();
		GenerateTraitsAndStats();
	}
	private void GenerateCharacterData()
	{
		characterName = characterTemplate.entityData.EntityName;
		recruitmentCost = CalculateRecruitmentCost();
	}
	private int CalculateRecruitmentCost()
	{
		int baseCost = characterTemplate.minRecruimentCost;
		float levelMultiplier = 1f + (level - characterTemplate.minLevel) * 0.1f;
		return Mathf.RoundToInt(baseCost * levelMultiplier);
	}
	private void GenerateTraitsAndStats()
	{
		availableActiveSkills.Clear();
		availablePassiveSkills.Clear();
		PlayerCharacter tempCharacter = new PlayerCharacter(
			characterTemplate.classData,
			characterTemplate.raceData,
			level,
			characterTemplate.entityData
		);

		tempCharacter.entityAffiliation = Affiliation.Recruitable;
		tempCharacter.AutoReplaceskill = true;
		tempCharacter.InitializeEntity(level);
		tempCharacter.AutoReplaceskill = false;

		generatedTraits.Clear();
		foreach (Trait trait in System.Enum.GetValues(typeof(Trait)))
		{
			int traitValue = tempCharacter.GetCurrentTrait(trait);
			if (traitValue > 0)
				generatedTraits[trait] = traitValue;
		}

		previewStats.Clear();
		foreach (Stat stat in System.Enum.GetValues(typeof(Stat)))
		{
			previewStats[stat] = tempCharacter.GetFinalStat(stat);
		}
		foreach(var activeSkill in tempCharacter.usableSkills)
		{
			availableActiveSkills.Add(activeSkill.SkillData);
		}
		foreach(var passiveSkill in tempCharacter.activePassiveSkills)
		{
			availablePassiveSkills.Add(passiveSkill.PassiveSkillData);
		}
		finalCharacterData = tempCharacter;
	}
	public PlayerCharacter CreatePlayerCharacter()
	{
		finalCharacterData.UnlockRecruitLocked();
		return finalCharacterData;
	}
}
