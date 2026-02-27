using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterCharacter : EntityBase
{
	[SerializeField] private MonsterRankData rankData;
	[SerializeField] private MonsterRaceData raceData;
	public MonsterRankData RankData => rankData;
	public MonsterRaceData RaceData => raceData;
	[SerializeField] private float expDropWhenDie;
	public float TotalExpToAward
	{
		get
		{
			if (entityData is MonsterData monsterData)
			{
				return monsterData.baseExp * rankData.expMultipliers;
			}
			return 0f;
		}
	}
	// MonsterCharacter.cs
	public override bool AttemptToLearnSkill(ActiveSkillData skillToLearn)
	{
		if (skillToLearn == null) return false;
		if (usableSkills.Exists(s => s.SkillData == skillToLearn))
			return true;

		if (usableSkills.Count >= MaxActiveSkillSlots)
		{
			var removable = new List<int>();
			for (int i = 0; i < usableSkills.Count; i++)
			{
				var sd = usableSkills[i].SkillData;
				if (!lockedActiveSkill.Contains(sd))
					removable.Add(i);
			}

			if (removable.Count == 0) return false;

			int pick = UnityEngine.Random.Range(0, removable.Count);
			ForgetSkill(usableSkills[removable[pick]]);
		}

		usableSkills.Add(new ActiveSkill(skillToLearn));
		return true;
	}


	public RPGProgressionSystem progressionSystem;
	public MonsterCharacter(MonsterRankData rankData,MonsterRaceData monsterRaceData,int level, MonsterData monster)
	{
		this.statCalculator = new MonsterStatCalculator();
		this.progressionSystem = new MonsterProgressionSystem(this);
		this.rankData = rankData;
		this.raceData = monsterRaceData;
		this.level = level;
		this.entityData = monster;
		this.EquipmentEffectRunner = new EquipmentEffectRunner(this, null);
	}

	public override void CalculateAllStats()
	{
		finalStats.Clear();

		foreach (Stat stat in System.Enum.GetValues(typeof(Stat)))
		{
			float calculatedValue = CalculateSingleStat(stat, new Dictionary<Trait, int>(currentTraits));
			finalStats[stat] = calculatedValue;
		}
	}

	public override float CalculateSingleStat(Stat statToCalculate, Dictionary<Trait, int> effTraits)
	{
		return statCalculator.CalculateSingleStat(statToCalculate, effTraits, this);
	}

	public override void CheckAndLearnSkill(int currentLevel)
	{
		if (raceData == null || raceData.skillSet == null) return;

		foreach (var entry in raceData.skillSet)
		{
			if (currentLevel < entry.levelRequirements) continue;

			if (entry.skill is PassiveSkillData pd)
			{
				if (!learnedPassiveSkills.Contains(pd))
				{
					AddPassiveSkill(pd);
					learnedPassiveSkills.Add(pd);
				}
			}
			else if (entry.skill is ActiveSkillData ad)
			{
				if (!learnedActiveSkills.Contains(ad))
				{
					if (AttemptToLearnSkill(ad))
						learnedActiveSkills.Add(ad);
				}
			}
			else if (entry.skill is AuraSkillData)
			{
			}
		}
	}


	public override void DistributeTraitPoints(int pointsToDistribute)
	{
		progressionSystem.DistributeTraitPoints(pointsToDistribute);
	}

	public override void InitializeEntity(int initialLevel)
	{
		Awake();
		this.entityAffiliation = Affiliation.Enemy;
		MonsterData monsterData = entityData as MonsterData;
		currentTraits.Clear();
		usableSkills.Clear();
		learnedActiveSkills.Clear();
		learnedPassiveSkills.Clear();

		entityElement = monsterData.EntityElement;
		foreach (var kvp in entityData.BaseTraits)
		{
			currentTraits[kvp.Key] = kvp.Value;
		}
		foreach (Trait t in _traitListCache)
		{
			if (!currentTraits.ContainsKey(t)) currentTraits.Add(t, 0);
		}
		foreach (var kvp in rankData.traitBonuses)
		{
			if (currentTraits.ContainsKey(kvp.Key))
				currentTraits[kvp.Key] += kvp.Value;
			else
				currentTraits[kvp.Key] = kvp.Value;
		}
		foreach (var kvp in raceData.traitBonuses)
		{
			if (currentTraits.ContainsKey(kvp.Key))
				currentTraits[kvp.Key] += kvp.Value;
			else
				currentTraits[kvp.Key] = kvp.Value;
		}
		if (initialLevel > 0)
			SetLevel(initialLevel);
		maxHP = Mathf.CeilToInt(GetFinalStat(Stat.HP));
		maxSP = Mathf.CeilToInt(GetFinalStat(Stat.SP));
		maxMP = Mathf.CeilToInt(GetFinalStat(Stat.MP));
		currentHP = maxHP;
		currentMP = maxMP;
		currentSP = maxSP;
	}
	public override void SetLevel(int targetLevel)
	{
		progressionSystem.SetLevel(targetLevel);
	}
}
