using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerCharacter : EntityBase
{
	[SerializeField] private int currentExp = 0;
	[SerializeField] private ClassData classData;
	[SerializeField] private CharacterRaceData raceData;
	private readonly SetBonusManager setTracker = new SetBonusManager();
	private object setSourceKey;
	public string RaceDataName => raceData.raceType.ToString();
	public ClassData GetClassData => classData;
	public int CurrentExp => currentExp;
	public PlayerCharacter(ClassData classData,CharacterRaceData raceData,int level,BaseEntityData entityData)
	{
		this.entityData = entityData;
		this.classData = classData;
		this.raceData = raceData;
		this.level = level;
		this.EquipmentEffectRunner = new EquipmentEffectRunner(this,null);
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
		int oldLevel = 1;
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
	public override void DistributeTraitPoints(int pointsToDistribute)
	{
		string gainedTraits = "";
		int n = _traitListCache.Count;
		while (n > 1) 
		{
			n--;
			int k = _random.Next(n + 1);
			Trait value = _traitListCache[k];
			_traitListCache[k] = _traitListCache[n];
			_traitListCache[n] = value;
		}

		for (int i = 0; i < Mathf.Min(pointsToDistribute, _traitListCache.Count); i++)
		{
			Trait traitToIncrease = _traitListCache[i];
			currentTraits[traitToIncrease]++;
			gainedTraits += $"{traitToIncrease}+1 ";
		}
		Debug.Log($"Player {entityData.name} (Level {level}) gained Trait points: {gainedTraits}");
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
		currentExp += amount;
		Debug.Log($"{entityData.name} gained {amount} exp !");
		CheckForLevelUp();
	}
	private void CheckForLevelUp()
	{
		int expNeeded = GetExpNeededForNextLevel();
		while (currentExp >= expNeeded && level < GetMaxLevel())
		{
			this.level++;
			currentExp -= expNeeded;
			Debug.Log($"{entityData.name} reached Level {level}!");
			DistributeTraitPoints(base.bonusTraitPointPerLevel);
			CalculateAllStats();
			CheckAndLearnSkill(this.level);
			expNeeded = GetExpNeededForNextLevel();
			if (expNeeded <= 0 || expNeeded == int.MaxValue) break;
		}
	}
	public int GetExpNeededForNextLevel()
	{
		int baseExpLevel1 = 24;
		float exponent = 1.5f;
		int currentLevelExp = Mathf.Max(1, level); 
		return Mathf.CeilToInt(baseExpLevel1 * Mathf.Pow(currentLevelExp, exponent) * growthModifier);
	}
	private int GetMaxLevel() { return 30; }

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

		float classMultiplier = classData.statMultipliers.ContainsKey(statToCalculate) ? classData.statMultipliers[statToCalculate] : 1.0f;
		float baseValue = 0;
		switch (statToCalculate)
		{
			case Stat.HP:
				baseValue = (20 + (vit * 2f)) * classMultiplier;
				break;
			case Stat.MP:
				baseValue = 5 + ((inte + pie) * classMultiplier * 0.5f);
				break;
			case Stat.SP:
				baseValue = 5 + ((str + dex) * classMultiplier * 0.5f);
				break;
			case Stat.AttackPower:
				baseValue = str * classMultiplier * 0.7f + dex * 0.3f * classMultiplier;
				break;
			case Stat.MagicPower:
				baseValue = inte * classMultiplier;
				break;
			case Stat.DivinePower:
				baseValue = pie * classMultiplier;
				break;
			case Stat.PhysicalDefense:
				baseValue = vit * classMultiplier;
				break;
			case Stat.MagicalDefense:
				baseValue = 0.5f * pie * classMultiplier + 0.5f * inte * classMultiplier;
				break;
			case Stat.ActionSpeed:
				baseValue = agi * classMultiplier;
				break;
			case Stat.Evasion:
				baseValue = agi * classMultiplier * 0.6f + luk * 0.4f * classMultiplier;
				break;
			case Stat.Accuracy:
				baseValue = classMultiplier * ((dex * 0.7f) + (luk * 0.3f));
				break;
			case Stat.Resistance:
				baseValue = classMultiplier * ((pie * 0.5f + vit * 0.5f));
				break;
			default:
				baseValue = 0;
				break;
		}
		float gearRaw = 0f;
		float gearPercentSum = 0f;
		if(weapon != null && weapon.WeaponBaseData != null)
		{
			foreach(var b in weapon.WeaponBaseData.EquipableStatBonus)
			{
				if(b.Stat != statToCalculate) continue;
				if (b.ModType == ModType.Flat) gearRaw += b.value;
				else gearPercentSum += b.value;
			}
		}

		foreach(var it in items)
		{
			if (it == null || it.itemBaseData == null) continue;
			var bonuses = it.itemBaseData.EquipableStatBonus;
			if(bonuses == null) continue;
			foreach(var b in bonuses)
			{
				if(b.Stat != statToCalculate) continue;
				float v = b.value;
				if(b.ModType == ModType.Flat) gearRaw += v;
				else gearPercentSum += v;
			}
		}
		float effectRawModifier = 0f;
		float effectPercentageModifier = 1f;
		foreach (var effect in currentActiveBuffs.Concat(currentActiveDebuffs).ToList())
		{
			if (effect is StatModifiEffect statModifiEffect && statModifiEffect.StatToModify == statToCalculate)
			{
				if (statModifiEffect.IsRawValue)
				{
					effectRawModifier += statModifiEffect.RawValue * statModifiEffect.CurrentStack;
				}
				else
				{
					effectPercentageModifier *= statModifiEffect.PercentageValue * statModifiEffect.CurrentStack;
				}
			}
		}
		float afterGear = (baseValue + gearRaw);
		float afterEffects = (afterGear + effectRawModifier);
		float final = afterEffects * (1f + gearPercentSum) * effectPercentageModifier;

		return Mathf.Round(final);
	}

	private void EnsureSetSource()
	{
		if (setSourceKey == null)
		{
			setSourceKey = new object();
		}
	}
	private void RefreshSetBonuses()
	{
		setTracker.Recalculate(items, weapon);
		EnsureSetSource();

		EquipmentEffectRunner.UnregisterEffectBinding(setSourceKey);
		var active = setTracker.GetAllActiveBindings();
		if (active.Count > 0)
		{
			EquipmentEffectRunner.RegisterEffectBinding(setSourceKey, active);
		}
	}
	public bool TryEquipWeapon(Weapon weaponToEquip)
	{
		if (weaponToEquip == null || !classData.usableWeaponTypes.Contains(weaponToEquip.WeaponBaseData.weaponType))
		{
			Debug.Log("Weapon type not usable by this class or weapon is null.");
			return false;
		}
		int requiredSlot = weaponToEquip.WeaponBaseData.requirement == WeaponRequirement.TwoHanded ? 2 : 1;
		int availableSlot = classData.itemSlotCount - items.Count;
		if(availableSlot < requiredSlot)
		{
			Debug.Log("Don't have enough slot");
			return false;
		}
		weapon = weaponToEquip;
		storedEquipmentBindings.AddRange(weapon.WeaponBaseData.effectData);

		for (int i = 0;i< requiredSlot; i++)
		{
			items.Add(null);
		}
		EquipmentEffectRunner.RegisterEffectBinding(weaponToEquip,weaponToEquip.WeaponBaseData.effectData);
		ApplyAllOnEquipEffect(weapon.WeaponBaseData.effectData);
		RefreshSetBonuses();
		CalculateAllStats();
		Debug.Log("Equipped weapon: " + weaponToEquip.WeaponBaseData.name);
		return true;
	}
	public void UnequipWeapon()
	{
		if (weapon == null) return;
		if (weapon.WeaponBaseData != null)
		{
			EquipmentEffectRunner.UnregisterEffectBinding(weapon);
			foreach (var b in weapon.WeaponBaseData.effectData)
				storedEquipmentBindings.Remove(b);
		}

		int slotToFree = weapon.WeaponBaseData != null && weapon.WeaponBaseData.requirement == WeaponRequirement.TwoHanded ? 2 : 1;
		weapon = null;

		int removed = 0;
		for (int i = items.Count - 1; i >= 0 && removed < slotToFree; i--)
		{
			if (items[i] == null)
			{
				items.RemoveAt(i);
				removed++;
			}
		}
		RefreshSetBonuses();
		Debug.Log("Unequipped weapon.");
	}

	public bool TryAddItem(Item item)
	{
		if (item == null)
			return false;
		if(items.Count >= classData.itemSlotCount)
		{
			Debug.Log("Not enough slots to add item: " + item.itemBaseData.itemName);
			return false;
		}
		bool hasSameBase = items.Any(it => it != null && it.itemBaseData == item.itemBaseData);
		items.Add(item);
		if(hasSameBase && item.itemBaseData.canDuplicateTrigger == false)
		{
			return true;
		}
		storedEquipmentBindings.AddRange(item.itemBaseData.effectData);
		EquipmentEffectRunner.RegisterEffectBinding(item, item.itemBaseData.effectData);
		ApplyAllOnEquipEffect(item.itemBaseData.effectData);
		RefreshSetBonuses();
		CalculateAllStats();
		Debug.Log("Added item: " + item.itemBaseData.itemName);
		return true;
	}

	public void ApplyAllOnEquipEffect(List<EquipEffectBinding> bindings)
	{
		if (bindings == null) return;

		foreach (var binding in bindings)
		{
			if (binding.trigger == EquipEffectTrigger.OnEquip)
			{
				var effect = binding.effect.CreateRuntimeEffect(this, this, binding.effect.MaxDuration);
				if (binding.effect.isInstantEffect)
				{
					effect.ApplyEffect().MoveNext();
				}
				else
				{
					effect.ApplyEffect().MoveNext();
				}
			}
		}
	}

	public Item RemoveItemAtSlot(int index)
	{
		if (index < 0 || index >= items.Count) return null;

		var oldItem = items[index];
		if (oldItem != null)
		{
			EquipmentEffectRunner.UnregisterEffectBinding(oldItem);
			foreach (var b in oldItem.itemBaseData.effectData)
				storedEquipmentBindings.Remove(b);
		}
		if(oldItem !=null && oldItem.itemBaseData.canDuplicateTrigger == true)
		{
			var replacement = items.FirstOrDefault(it => it != null && it != oldItem && it.itemBaseData == oldItem.itemBaseData);
			if(replacement != null)
			{
				EquipmentEffectRunner.RegisterEffectBinding(replacement, replacement.itemBaseData.effectData);
				ApplyAllOnEquipEffect(replacement.itemBaseData.effectData);
			}
		}
		items.RemoveAt(index);
		RefreshSetBonuses();
		Debug.Log("Removed item at slot: " + index);
		return oldItem;
	}
	public Item GetItemAtSlot(int index)
	{
		if (index < 0 || index >= items.Count) return null;
		return items[index];
	}
	public string GetItemSlotStatus()
	{
		int usedSlots = items.Count;
		int totalSlots = classData.itemSlotCount;
		return $"Slots: {usedSlots}/{totalSlots}";
	}

	public int GetTotalSlots()
	{
		return GetClassData.itemSlotCount;
	}

	public int GetUsedSlots()
	{
		return items.Count;
	}

	public int GetFreeSlots()
	{
		return GetTotalSlots() - GetUsedSlots();
	}

	public int GetSlotCostForEquipable(EquipableBaseData data)
	{
		if (data is WeaponBaseData w)
		{
			return w.requirement == WeaponRequirement.TwoHanded ? 2 : 1;
		}

		return 1;
	}

	public void ApplyReplaceSelection(bool removeWeapon, List<int> removeItemIndices)
	{
		if (removeWeapon && weapon != null)
		{
			UnequipWeapon();
		}

		if (removeItemIndices != null && removeItemIndices.Count > 0)
		{
			removeItemIndices.Sort();
			for (int i = removeItemIndices.Count - 1; i >= 0; i--)
			{
				int idx = removeItemIndices[i];
				RemoveItemAtSlot(idx);
			}
		}
	}


	public string GetWeaponStatus()
	{
		if (weapon != null && weapon.WeaponBaseData != null)
		{
			return weapon.WeaponBaseData.itemName;
		}
		else
		{
			return "No weapon";
		}
	}
	public void OnBattleStartSyncSet() => RefreshSetBonuses();
}