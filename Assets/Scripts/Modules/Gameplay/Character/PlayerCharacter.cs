using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerCharacter : EntityBase
{
	public int currentExp = 0;
	[SerializeField] private ClassData classData;
	[SerializeField] private CharacterRaceData raceData;
	public string RaceDataName => raceData.raceType.ToString();
	public ClassData GetClassData => classData;
	public RPGProgressionSystem progressionSystem;
	public EquipmentManager equipmentManager;
	public int CurrentExp => currentExp;
	public int GetExpNeededForNextLevel() => progressionSystem.GetExpNeededForNextLevel();
	public PlayerCharacter(ClassData classData,CharacterRaceData raceData,int level,BaseEntityData entityData)
	{
		this.statCalculator = new PlayerStatCalculator();
		this.progressionSystem = new PlayerProgressionSystem(this);
		this.equipmentManager = new EquipmentManager(this);
		this.entityData = entityData;
		this.classData = classData;
		this.raceData = raceData;
		this.level = level;
		EquipmentEffectRunner = new EquipmentEffectRunner(this, null);
	}
	public override void InitializeEntity(int initialLevel)
	{
		Awake();
		//setTracker.SetOwner(this);

		currentTraits.Clear();
		usableSkills.Clear();
		entityElement = entityData.EntityElement;
		foreach (var kvp in entityData.BaseTraits)
		{
			currentTraits[kvp.Key] = kvp.Value;
		}
		foreach (Trait t in _traitListCache)
		{
			if (!currentTraits.ContainsKey(t)) currentTraits.Add(t, 0);
		}
		foreach (var kvp in classData.traitBonuses)
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
		switch (classData.expGrowthRate)
		{
			case ExpGrowth.Slow: growthModifier = 2f; break;
			case ExpGrowth.Medium: growthModifier = 1.5f; break;
			case ExpGrowth.Fast: growthModifier = 1f; break;
		}
		if (initialLevel > 0)
		{
			SetLevel(initialLevel);
		}
		equipmentSlotCount = classData.itemSlotCount;
		maxHP = Mathf.CeilToInt(GetFinalStat(Stat.HP));
		maxSP = Mathf.CeilToInt(GetFinalStat(Stat.SP));
		maxMP = Mathf.CeilToInt(GetFinalStat(Stat.MP));
		currentHP = maxHP;
		currentMP = maxMP;
		currentSP = maxSP;
	}

	public void UpdateBattleSystem(BattleSystem battleSystem)
	{
		EquipmentEffectRunner?.UpdateBattleSystem(battleSystem);
	}

	public override void SetLevel(int targetLevel)
	{
		progressionSystem.SetLevel(targetLevel);
	}
	public override void DistributeTraitPoints(int pointsToDistribute)
	{
		progressionSystem.DistributeTraitPoints(pointsToDistribute);
	}

	public override void CheckAndLearnSkill(int currentLevel)
	{
		var pool = new List<SkillEntry>(classData.skillSet);

		if (classData.classType == ClassType.Mage && classData is ElementalClassData ecls)
		{
			var extra = ecls.GetSkillSetFor(entityElement);
			if (extra != null && extra.Count > 0)
			{
				pool.AddRange(extra);
			}
		}

			foreach (var entry in pool)
		{
			if (entry.skill is PassiveSkillData passiveSkillData)
			{
				if (currentLevel >= entry.levelRequirements && !learnedPassiveSkills.Contains(passiveSkillData))
				{
					AddPassiveSkill(passiveSkillData);
					learnedPassiveSkills.Add(passiveSkillData);
				}
			}
			else if (entry.skill is ActiveSkillData activeSkillData)
			{
				if (currentLevel >= entry.levelRequirements && !learnedActiveSkills.Contains(activeSkillData))
				{
					if (AttemptToLearnSkill(activeSkillData))
					{
						learnedActiveSkills.Add(activeSkillData);
					}
				}
			}
			//Aura Skill later
		}
	}

	public override bool AttemptToLearnSkill(ActiveSkillData data)
	{
		var policy = LearnSkillPolicyProvider.GetPolicy(this);
		if (!AutoReplaceskill)
		{
			policy.Resolve(new LearnRequest
			{
				pc = this,
				skillData = data,
				onResolved = null
			});
			return true;
		}
		else
		{
			if(usableSkills.Count < MaxActiveSkillSlots)
			{
				var newSkillInstance = new ActiveSkill(data);
				usableSkills.Add(newSkillInstance);
				return true;
			}
			else
			{
				var removable = new List<int>();
				for (int i = 0; i < usableSkills.Count; i++)
				{
					if (!isRecruitLocked(usableSkills[i].SkillData))
						removable.Add(i);
				}
				if (removable.Count == 0) return false;

				int pick = UnityEngine.Random.Range(0, removable.Count);
				var skillToRemove = usableSkills[removable[pick]];
				ForgetSkill(skillToRemove);
				usableSkills.Add(new ActiveSkill(data));
				return true;
			}
		}
	}
	
	public void AddExp(int amount)
	{
		progressionSystem.AddExp(amount);
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

	public bool TryEquipWeapon(Weapon weaponToEquip) => equipmentManager.TryEquipWeapon(weaponToEquip);
	public void UnequipWeapon() => equipmentManager.UnequipWeapon();
	public bool TryAddItem(Item item) => equipmentManager.TryAddItem(item);
	public Item RemoveItemAtSlot(int index) => equipmentManager.RemoveItemAtSlot(index);
	public Item GetItemAtSlot(int index) => equipmentManager.GetItemAtSlot(index);
	public string GetItemSlotStatus() => equipmentManager.GetItemSlotStatus();
	public int GetTotalSlots() => equipmentManager.GetTotalSlots();
	public int GetUsedSlots() => equipmentManager.GetUsedSlots();
	public int GetFreeSlots() => equipmentManager.GetFreeSlots();
	public int GetSlotCostForEquipable(EquipableBaseData data) => equipmentManager.GetSlotCostForEquipable(data);
	public void ApplyReplaceSelection(bool removeWeapon, List<int> removeItemIndices) => equipmentManager.ApplyReplaceSelection(removeWeapon, removeItemIndices);
	public string GetWeaponStatus() => equipmentManager.GetWeaponStatus();
	public void OnBattleStartSyncSet() => equipmentManager.OnBattleStartSyncSet();
	public void ApplyAllOnEquipEffect(List<EquipEffectBinding> bindings) => equipmentManager.ApplyAllOnEquipEffect(bindings);
}