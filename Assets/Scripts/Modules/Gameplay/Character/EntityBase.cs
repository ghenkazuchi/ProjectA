using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UIElements;

public abstract class EntityBase
{
	//event
	public event System.Action<EntityBase,EffectBase> OnEffectAdded;
	public event System.Action<EntityBase, EffectBase> OnEffectRemoved;
	public event System.Action<EntityBase, EffectBase> OnEffectChanged;
	private readonly Dictionary<EffectBase, System.Action<EffectBase>> _effectChangedHandlers = new();

	public event System.Action<EntityBase> OnEntityDead;
	public System.Func<EntityBase, int, int> DamageModifier;
	[Header("Cons things")]
	[SerializeField] protected int bonusTraitPointPerLevel = 2;
	[SerializeField] protected int numberOfHoldableSkill = 6;
	protected List<Trait> _traitListCache = new List<Trait>();
	public int equipmentSlotCount;
	[Header("Character Info")]
	public BaseEntityData entityData;
	[SerializeField] protected int maxHP;
	[SerializeField] protected int maxMP;
	[SerializeField] protected int maxSP;
	[SerializeField]
	[Header("Current stuff")]
	public int level;
	public Element entityElement;
	[SerializeField] protected int currentHP;
	[SerializeField] protected int currentMP;
	[SerializeField] protected int currentSP;
	//equipment
	public List<Item> items = new List<Item>();
	public Weapon weapon;
	[Header("Current ActiveEffect")]
	[SerializeReference] public List<EffectBase> currentActiveBuffs = new List<EffectBase>();
	[SerializeReference] public List<EffectBase> currentActiveDebuffs = new List<EffectBase>();
	[Header("Traits and Stats")]
	[SerializeField]
	protected SerializableDictionaryBase<Trait, int> currentTraits = new SerializableDictionaryBase<Trait, int>();
	[SerializeField]
	protected SerializableDictionaryBase<Stat, float> finalStats = new SerializableDictionaryBase<Stat, float>();
	public List<ActiveSkill> usableSkills = new List<ActiveSkill>();
	public List<PassiveSkill> activePassiveSkills = new List<PassiveSkill>();

	protected System.Random _random = new System.Random();
	protected float growthModifier;
	public PassiveSkillRunner PassiveSkillRunner { get; private set; }
	public EquipmentEffectRunner EquipmentEffectRunner { get; set; }
	protected HashSet<ActiveSkillData> learnedActiveSkills = new HashSet<ActiveSkillData>();
	protected HashSet<PassiveSkillData> learnedPassiveSkills = new HashSet<PassiveSkillData>();
	//container
	List<EffectBase> targetList;

	public List<EquipEffectBinding> storedEquipmentBindings = new();

	//skill Stuff
	public bool AutoReplaceskill { get; set; } = false;
	protected HashSet<ActiveSkillData> lockedActiveSkill;

	protected bool isRecruitLocked (ActiveSkillData skillData) => entityAffiliation == Affiliation.Recruitable && lockedActiveSkill.Contains(skillData);

	//Turn Directive
	public TurnDirective TurnControl { get; } = new TurnDirective();

	public void ResetTurnDirective() => TurnControl.Clear();

	public void ProposeTurnDirective(TurnDirective proposal) => TurnControl.Propose(proposal);


	public void UnlockRecruitLocked()
	{
		if (lockedActiveSkill == null) lockedActiveSkill = new HashSet<ActiveSkillData>();
		lockedActiveSkill.Clear();
	}
	public virtual void Awake()
	{
		_traitListCache = System.Enum.GetValues(typeof(Trait)).Cast<Trait>().ToList();
		if (lockedActiveSkill == null) lockedActiveSkill = new HashSet<ActiveSkillData>();
	}
	public void InitializePassiveRunner(BattleSystem battleSystem)
	{
		PassiveSkillRunner = new PassiveSkillRunner(this, battleSystem);
	}
	public abstract void InitializeEntity(int initialLevel);
	public abstract void SetLevel(int targetLevel);
	public abstract void DistributeTraitPoints(int pointsToDistribute);
	public abstract void CheckAndLearnSkill(int currentLevel);
	public abstract bool AttemptToLearnSkill(ActiveSkillData skill);
	public virtual bool ForgetSkill(ActiveSkill skillToForget)
	{
		if (usableSkills.Contains(skillToForget))
		{
			usableSkills.Remove(skillToForget);
			return true;
		}
		return false;
	}

	public void AddExclusiveSkill()
	{
		var exA = entityData?.ExclusiveActiveSkill;
		var exP = entityData?.ExclusivePassiveSkill;
		if (exA == null && exP == null)
		{
			Debug.LogWarning("No exclusive skill data found for " + entityData.EntityName);
			return;
		}
		if (exA != null)
		{
			if (!usableSkills.Exists(s => s.SkillData == exA))
			{
				if (usableSkills.Count < MaxActiveSkillSlots)
					usableSkills.Add(new ActiveSkill(exA));
				else
				{
					var idx = usableSkills.FindIndex(s => !isRecruitLocked(s.SkillData));
					if (idx >= 0) { ForgetSkill(usableSkills[idx]); usableSkills.Add(new ActiveSkill(exA)); }
					else Debug.LogWarning("All slots are locked; cannot insert exclusive active skill.");
				}
			}
			lockedActiveSkill.Add(exA);
			MarkActiveSkillLearned(exA);
		}
		if (exP != null)
		{
			if (!activePassiveSkills.Exists(p => p.PassiveSkillData == exP))
				activePassiveSkills.Add(new PassiveSkill(exP, exP.initCoolDown));
			learnedPassiveSkills.Add(exP);
		}
	}

	public void AddPassiveSkill(PassiveSkillData passiveSkillData)
	{
		activePassiveSkills.Add(new PassiveSkill(passiveSkillData, passiveSkillData.initCoolDown));
	}
	public void RemovePassiveSkill(PassiveSkillData passiveSkillData)
	{
		activePassiveSkills.RemoveAll(s => s.PassiveSkillData == passiveSkillData);
	}
	public abstract void CalculateAllStats();
	public abstract float CalculateSingleStat(Stat statToCalculate, Dictionary<Trait, int> effTraits);
	public int GetCurrentHP()
	{
		return currentHP;
	}
	public int GetCurrentSP()
	{
		return currentSP;
	}
	public int GetCurrentMP()
	{
		return currentMP;
	}
	public int MaxHp => maxHP;
	public int MaxMP => maxMP;
	public int MaxSP => maxSP;
	public int MaxActiveSkillSlots => numberOfHoldableSkill;

	public IReadOnlyList<ActiveSkill> GetUsableSkills() => usableSkills;

	public Affiliation entityAffiliation { get; set; }

	public void ReduceMP(int amount)
	{
		currentMP -= amount;
	}
	public void ReduceSP(int amount)
	{
		currentSP -= amount;
	}
	public float GetFinalStat(Stat statType)
	{
		finalStats.TryGetValue(statType, out float value);
		var effects = GetAllEffect();
		if (effects != null)
		{
			for (int i = 0; i < effects.Count; i++)
			{
				if (effects[i] is IStatModify statModifier)
				{
					value = statModifier.ModifyStat(statType,value,this);
				}
			}
		}
		return value;
	}
	public int GetCurrentTrait(Trait traitType)
	{
		currentTraits.TryGetValue(traitType, out int value);
		return value;
	}
	public int Level => level;
	public virtual void TakeDamage(int amount, EntityBase source = null)
	{
		int originalAmount = amount;

		int finalDamage = amount;

		if (DamageModifier != null)
		{
			finalDamage = DamageModifier(this, finalDamage);
		}

		currentHP -= finalDamage;
		currentHP = Mathf.Max(currentHP, 0);
		if(currentHP == 0)
		{
			OnEntityDead?.Invoke(this);
		}
	}

	public virtual void Heal(int amount, EntityBase source = null)
	{
		currentHP += amount;
		currentHP = Mathf.Min(currentHP, maxHP);
	}
	public virtual void RestoreMP(int amount)
	{
		currentMP += amount;
		currentMP = Mathf.Min(currentMP, maxMP);
	}

	public virtual void RestoreSP(int amount)
	{
		currentSP += amount;
		currentSP = Mathf.Min(currentSP, maxSP);
	}

	public void MarkActiveSkillLearned(ActiveSkillData data)
	{
		if (!learnedActiveSkills.Contains(data))
			learnedActiveSkills.Add(data);
	}
	public IEnumerator AddEffect(EffectBase effect)
	{
		if(effect != null && effect.Effect == Effect.CrownControl && EquipmentEffectRunner != null)
		{
			var sctx = new StatusApplyContext
			{
				Source = effect.Owner,
				Target = this,
				IncomingEffect = effect,
				Cancle = false,
			};
			yield return EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnBeforeStatusApplied, this, sctx);

			if (sctx.Cancle)
			{
				if(!string.IsNullOrEmpty(sctx.CancleReason))
					yield return BattleSystem.Instance.ShowDialog(sctx.CancleReason);
				yield break;
			}
		}
		if (effect.EffectType == EffectType.Buff)
		{
			targetList = currentActiveBuffs;
		}
		else if (effect.EffectType == EffectType.Debuff)
		{
			targetList = currentActiveDebuffs;
		}
		EffectBase existingEffect = targetList.FirstOrDefault(e => e.Effect == effect.Effect && e.Name == effect.Name);

		if (existingEffect != null)
		{
			if (effect.Stackable && existingEffect.CurrentStack < existingEffect.MaxStack)
			{
				existingEffect.AddStack(1);
			}
			else if (!effect.Stackable)
			{
				existingEffect.RefreshEffect();
			}
		}
		else
		{
			targetList.Add(effect);
			System.Action<EffectBase> handler = (e) => OnEffectChanged?.Invoke(this, e);
			_effectChangedHandlers[effect] = handler;
			effect.OnChanged += handler;
			Debug.Log("Effect added: " + effect.Name + " to " + entityData.EntityName);
			OnEffectAdded?.Invoke(this, effect);
			yield return effect.ApplyEffect();
		}
		if (effect.Effect == Effect.StatModifier)
		{
			CalculateAllStats();
		}
	}
	public void RemoveEffect(EffectBase effect)
	{
		Debug.Log(effect.Name);

		bool removed = false;
		if (effect.EffectType == EffectType.Buff)
			removed = currentActiveBuffs.Remove(effect);
		else if (effect.EffectType == EffectType.Debuff)
			removed = currentActiveDebuffs.Remove(effect);
		if (!removed) return;

		if (_effectChangedHandlers.TryGetValue(effect, out var handler))
		{
			effect.OnChanged -= handler;
			_effectChangedHandlers.Remove(effect);
		}
		OnEffectRemoved?.Invoke(this, effect);	
		CalculateAllStats();
	}

	public IEnumerator RemoveEffectCoroutine(EffectBase effect)
	{
		bool removed = false;
		if (effect.EffectType == EffectType.Buff)
			removed = currentActiveBuffs.Remove(effect);
		else if (effect.EffectType == EffectType.Debuff)
			removed = currentActiveDebuffs.Remove(effect);
		if (!removed) yield break;

		if (_effectChangedHandlers.TryGetValue(effect, out var handler))
		{
			effect.OnChanged -= handler;
			_effectChangedHandlers.Remove(effect);
		}

		OnEffectRemoved?.Invoke(this, effect);

		yield return effect.RemoveEffect();

		CalculateAllStats();
	}



	public IEnumerator ProcessEffectOnTurnStart()
	{
		yield return RunEffectPhase(currentActiveBuffs,true);
		yield return RunEffectPhase(currentActiveDebuffs, true);
		yield return EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnTurnStart, this);
		yield return PassiveSkillRunner.Trigger(PassiveTrigger.OnTurnStart, this);
	}
	public IEnumerator ProcessEffectOnTurnEnd()
	{
		yield return RunEffectPhase(currentActiveBuffs, false);
		yield return RunEffectPhase(currentActiveDebuffs, false);
		yield return EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnTurnEnd, this);
		yield return PassiveSkillRunner.Trigger(PassiveTrigger.OnTurnEnd, this);
	}
	public IEnumerator TriggerEffectDirectly(EffectBase effect)
	{
		yield return effect.ApplyEffect();
	}
	private IEnumerator RunEffectPhase(List<EffectBase> source,bool isStartPhase)
	{
		if (source == null || source.Count == 0) yield break;	
		var snapshot = new List<EffectBase>(source);
		foreach(var e in snapshot)
		{
			if (e == null) continue;
			if (isStartPhase && e is ITurnStart ts)
				yield return ts.OnTurnStart();

			if (!isStartPhase && e is ITurnEnd te)
				yield return te.OnTurnEnd();
		}
		if(!isStartPhase)
			yield return TickAllEffectDown();	
	}

	public IEnumerator TickAllEffectDown()
	{
		var allEffect = GetAllEffect();
		var snapShot = new List<EffectBase>(allEffect);
		foreach(var e in snapShot)
		{
			if (e == null) continue;
			yield return e.TickDownAndRemove();
		}
	}

	public void ResetEffectAfterBattle()
	{
		Debug.Log(currentActiveBuffs.Count);
		Debug.Log(currentActiveDebuffs.Count);
		for (int i = currentActiveBuffs.Count - 1; i >= 0; i--)
		{
			RemoveEffect(currentActiveBuffs[i]);
		}
		for(int i = currentActiveDebuffs.Count-1; i >= 0; i--)
		{
			RemoveEffect(currentActiveDebuffs[i]);
		}
		ResetEquipmentBattleUsage();
		CalculateAllStats();
	}
	public void ResetEquipmentBattleUsage()
	{
		if (weapon != null && weapon.WeaponBaseData != null)
		{
			weapon.ResetBattleUsage();
		}
		if (items != null)
		{
			foreach (var item in items)
			{
				if (item == null) continue;                 
				if (item.itemBaseData == null) continue;   
				item.ResetBattleUsage();
			}
		}
	}
	public List<EffectBase> GetAllEffect()
	{
		List<EffectBase> allEffects = new List<EffectBase>();
		allEffects.AddRange(currentActiveBuffs);
		allEffects.AddRange(currentActiveDebuffs);
		return allEffects;
	}
}
