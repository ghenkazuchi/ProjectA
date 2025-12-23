using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterCharacter : EntityBase
{
	[SerializeField] private MonsterRankData rankData;
	[SerializeField] private MonsterRaceData raceData;
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


	public MonsterCharacter(MonsterRankData rankData,MonsterRaceData monsterRaceData,int level, MonsterData monster)
	{
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
		int str = effTraits.GetValueOrDefault(Trait.Strength, 0);
		int inte = effTraits.GetValueOrDefault(Trait.Intelligence, 0);
		int pie = effTraits.GetValueOrDefault(Trait.Piety, 0);
		int vit = effTraits.GetValueOrDefault(Trait.Vitality, 0);
		int agi = effTraits.GetValueOrDefault(Trait.Agility, 0);
		int luk = effTraits.GetValueOrDefault(Trait.Luck, 0);
		int dex = effTraits.GetValueOrDefault(Trait.Dexterity, 0);

		float rankMultiplier = rankData.statMultipliers.ContainsKey(statToCalculate) ? rankData.statMultipliers[statToCalculate] : 1.0f;
		float baseValue = 0;
		switch (statToCalculate)
		{
			case Stat.HP:
				baseValue = (20 + (vit * 2f)) * rankMultiplier;
				break;
			case Stat.MP:
				baseValue = 5 + ((inte + pie) * rankMultiplier * 0.5f);
				break;
			case Stat.SP:
				baseValue = 5 + ((str + dex) * rankMultiplier * 0.5f);
				break;
			case Stat.AttackPower:
				baseValue = str * rankMultiplier;
				break;
			case Stat.MagicPower:
				baseValue = inte * rankMultiplier;
				break;
			case Stat.DivinePower:
				baseValue = pie * rankMultiplier;
				break;
			case Stat.PhysicalDefense:
				baseValue = vit * rankMultiplier;
				break;
			case Stat.MagicalDefense:
				baseValue = 0.5f * pie * rankMultiplier + 0.5f * inte * rankMultiplier;
				break;
			case Stat.ActionSpeed:
				baseValue = agi * rankMultiplier;
				break;
			case Stat.Evasion:
				baseValue = agi * rankMultiplier * 0.6f + luk * 0.4f * rankMultiplier;
				break;
			case Stat.Accuracy:
				baseValue = rankMultiplier * ((dex * 0.7f) + (luk * 0.3f));
				break;
			case Stat.Resistance:
				baseValue = rankMultiplier * ((pie * 0.5f + vit * 0.5f));
				break;
			default:
				baseValue = 0;
				break;
		}
		return Mathf.Round(baseValue);
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
		MonsterData monsterData = entityData as MonsterData;
		string gainedTraits = "";
		Trait dominant = monsterData.DominantTrait;
		float preferenceWeight = monsterData.DominantTraitPreferenceWeight;

		for (int p = 0; p < pointsToDistribute; p++)
		{
			if (_traitListCache.Count == 0) break;

			List<float> weights = new List<float>();
			foreach (Trait t in _traitListCache)
			{
				weights.Add(t == dominant ? preferenceWeight : 1.0f);
			}

			Trait chosenTrait = GetWeightedRandomTrait(_traitListCache, weights);

			if (currentTraits.ContainsKey(chosenTrait))
			{
				currentTraits[chosenTrait]++;
			}
			else
			{
				currentTraits.Add(chosenTrait, 1);
			}
			gainedTraits += $"{chosenTrait}+1 ";
		}

		if (!string.IsNullOrEmpty(gainedTraits))
		{
			Debug.Log($"Monster {monsterData.EntityName} (Level {level}) gained Trait points: {gainedTraits}");
		}
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
		int oldLevel = (this.level > 0 ? this.level : 1);
		this.level = Mathf.Max(1, targetLevel);

		if (this.level > oldLevel)
		{
			int pointsForNewLevels = (this.level - oldLevel) * base.bonusTraitPointPerLevel;
			DistributeTraitPoints(pointsForNewLevels);
		}
		CalculateAllStats();
		AddExclusiveSkill();
		CheckAndLearnSkill(this.level);
	}


	private Trait GetWeightedRandomTrait(List<Trait> traits, List<float> weights)
	{
		float totalWeight = weights.Sum();
		float randomNumber = (float)_random.NextDouble() * totalWeight;

		for (int i = 0; i < traits.Count; i++)
		{
			if (randomNumber < weights[i])
			{
				return traits[i];
			}
			randomNumber -= weights[i];
		}
		return traits[traits.Count - 1];
	}
}
