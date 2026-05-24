using RotaryHeart.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public enum Trait
{
	Strength,
	Intelligence,
	Piety,
	Vitality,
	Agility,
	Dexterity,
	Luck
}
public enum SkillDefinition
{
	Spell,
	BattleArt,
	Almighty
}
public enum Stat
{
	HP,
	MP,
	SP,
	AttackPower,
	MagicPower,
	DivinePower,
	Accuracy,
	PhysicalDefense,
	MagicalDefense,
	Evasion,
	Resistance,
	ActionSpeed,	
}
public enum MonsterType
{
	HumanNoid,
	Undead,
	MagicalBeast,
	Insect,
	Spirit,
}
public enum MonsterTypeName
{
	Orc,
	Goblin,
	Skeleton,
	Dragon,
	Chimera,
	HollowSpririt,
	Demon,
	ScorpionLike,
	KillerBee,
	Slime,
	Werewolf,
	Cyclop,
	DragonFly,
	HeadlessHorseman,
	ZombieWolf,
}
public enum MonsterRank
{
	RankD,
	RankC,
	RankB,
	RankA,
	RankS,

}
public enum ClassType
{
	Fighter,
	Rogue,
	Mage,
	Priest,
	Knight
}

public enum RaceType
{
	Human,
	Elf,
	Beastkin,
	Dwarf,
	Dragonoid
}
public enum ExpGrowth
{
	Slow,
	Medium,
	Fast
}
//skill stuff

public enum SkillType
{
	Active,
	Passive,
}
public enum TargetType
{
	Self,
	Enemy,
	Ally,
	SelfOrAllies
}
public enum SkillRange
{
	SingleTarget,
	AllTarget,
	SingleAlly,
	AllAllies,
	LineTarget,
	LineAllies,
}
public enum Element
{
	None,
	Fire,
	Water,
	Earth,
	Wind,
	Light,
	Dark
}
public enum UnitState
{
	Dead,
	Alive,
	Empty
}

public enum Affiliation
{
	PartyMember,
	Recruitable,
	Enemy,
}

[Serializable]
public struct LearnRequest
{
	public EntityBase pc;
	public ActiveSkillData skillData;
	public Action<bool> onResolved;
}
public enum AuraEffectTarget
{
	AllAllies,
	RowOfOwner
}

public enum DamageType
{
	PhysicalDamage,
	MagicalDamage,
	AllMightyDamage
}
public enum TargetHPType
{
	CurrentHP,
	MaxHP
}

public struct DamageInfo
{
	public EntityBase source;
	public EntityBase target;
	public SkillDefinition damageType;
	public int amount;
	public bool isEffectDamage;
}
public enum InteracableType
{
	Monster,
	Chest,
	WellOfFortune,
	BlackSmith,
	ShopKeeper,
	FireCamp,
	Companion,
	UniqueBoss,
	Portal
}
public enum InteracableGroup
{
	Combat,
	Consumeable,
	Service,
	Event,
	UniqueBoss,
	Portal,
}
public enum BattleType
{
	Boss,
	RoamingMoster,
	Tutorial
}

public enum PassiveTrigger
{
	OnBattleStart,
	OnTurnStart,
	OnTurnEnd,
	OnAttacked,
	OnAllyAttacked,
	OnAllyAttacking,
	OnEnemyDefeated,
	OnAllyDefeated,
	OnHealthLow,
	OnAllyHealthLow,
	Continuous,
	OnStatusApplied,
	OnStatusRemoved,
	OnSkillUse,
	OnDealingDamage,
	OnTakingDamage,
	OnBeforeTakingDamage,
	OnEvade
}
[System.Serializable]
public class GrantEffect
{
	public EffectData effectToGrant;
	public int duration;
}
public enum PassiveEffectType
{
	StatModification,
	DamageModification,
	ActionModification,
	StatusImmunity,
	StatusApplication,
	Healing,
	Shield,
	Redirect,
	Interrupt,
	Counter,
	ReduceMpCost,
	ApplyStatusOnHit,
	ProtectedAlly,
	SlowEnemyOnAttack
}
public enum ActiveSkillType
{
	Damage,
	Heal,
	Buff,
	Debuff
}
public enum ProtectRangeType
{
	All,
	Horizontal,
	Vertical,
	Adjacent
}
//Buff,Debuff


public enum EffectType
{
	Buff,
	Debuff,
	Other
}
public enum Effect
{
	TraitModifier,
	StatModifier,
	CrownControl,
	AdvanceFoward,
	DamageOverTime,
	Other
}
public enum EffectTriggerPhase
{
	StartOfTurn,
	EndOfTurn,
	Instant,
	Passive
}
public enum EffectActiveTiming
{
	OnCast,
	OnHit,
	OnDealingDamage
}
public enum ControlEffectType
{
	None,
	Stune,
	Charm,
	Confuse,
}
//item

public enum ModType
{
	Flat,
	Percentage
}

public enum ItemType
{
	Food,
	Equipment,
	Scroll,

}
public enum ItemRarity
{
	Common,
	Rare,
	Epic,
	Legendary,
}
public enum ItemGrade
{
	Normal,
	Gold,
	Diamond
}
public enum WeaponType
{
	Mace,
	Sword,
	GreatSword,
	Staff,
	Dagger,
	Bow,
	Spear,

}
public enum WeaponRequirement
{
	TwoHanded,
	OneHanded,
}
public enum EquipEffectTrigger
{
	OnEquip,
	OnBattleStart,
	OnBattleEnd,
	OnTurnStart,
	OnTurnEnd,
	OnDealingDamage,
	OnTakingDamage,
	OnCriticalHit,
	OnEnemyKill,
	OnDeath,
	OnBeforeDealingDamage,
	OnBeforeTakingDamage,
	OnBeforeStatusApplied,
	OnEvade
}

public enum EquipEffectTargetMode
{
	Self,
	Target
}

[System.Serializable] 
public struct EquipableStatBonus
{
	public Stat Stat;
	public ModType ModType;
	public float value;


}

[System.Serializable]
public class ThresholdBonus
{
	[Min(1)] public int requiredPieces = 2;
	public List<EquipEffectBinding> effects = new();
}

[System.Serializable]
public struct GradeTunning
{
	[Tooltip("Extra duration added to effects granted by this item at this grade.")]
	public int durationBonus;
	[Tooltip("An additional effect unlocked at this grade (e.g. extend buff duration).")]
	public EffectData additionalEffect;
	[Tooltip("Bonus proc chance added to equipment effects at this grade.")]
	public float procChanceMultiplier;
	[Tooltip("Extra uses per battle granted at this grade.")]
	public int bonusUsagePerBattle;
	[Tooltip("Extra uses per lifecycle granted at this grade.")]
	public int bonusUsagePerLifeCycle;
}

[System.Serializable]
public struct ChestLootEntry
{
	public EquipableBaseData data;
	public ItemGrade grade;

	public ChestLootEntry(EquipableBaseData data, ItemGrade grade)
	{
		this.data = data;
		this.grade = grade;
	}
}

[System.Serializable]
public class ChestReward
{
	public bool isGold;
	public int goldAmount;
	public List<ChestLootEntry> items;

	public static ChestReward Gold(int amount)
		=> new ChestReward { isGold = true, goldAmount = amount, items = null };

	public static ChestReward Items(List<ChestLootEntry> entries)
		=> new ChestReward { isGold = false, goldAmount = 0, items = entries };
}
	// gameprogress

	public enum GameDay { Day1, Day2, Day3 }
public enum TimeOfDay { Morning,Night}

public struct GameTime
{
	public GameDay day;
	public TimeOfDay time;
	public GameTime(GameDay day, TimeOfDay time)
	{
		this.day = day;
		this.time = time;
	}
}
//Game currency
public enum CurrencyType
{
	Gold,
	SoulDusk
}

public struct ShopReplaceSelection
{
	public PlayerCharacter target;
	public EquipableBaseData newEquip;
	public bool removeWeapon;
	public List<int> removeItemIndices;
}

public static class ElementalChart
{
	private static readonly Dictionary<(Element attackerElement, Element defenderElement), float> elementalMultiplier = new()
	{
		{ (Element.Fire, Element.Wind), 1.5f },
		{ (Element.Wind, Element.Earth), 1.5f },
		{ (Element.Water, Element.Fire), 1.5f },
		{ (Element.Earth, Element.Water), 1.5f },
		{ (Element.Fire, Element.Water), 0.5f },
		{ (Element.Wind, Element.Fire), 0.5f },
		{ (Element.Water, Element.Earth), 0.5f },
		{ (Element.Earth, Element.Wind), 0.5f },
		{ (Element.None, Element.None), 1f },
		{ (Element.Dark, Element.Wind), 1.25f },
		{ (Element.Dark, Element.Fire), 1.25f },
		{ (Element.Dark, Element.Water), 1.25f },
		{ (Element.Dark, Element.Earth), 1.25f },
		{ (Element.Water, Element.Dark), 1f },
		{ (Element.Earth, Element.Dark), 1f },
		{ (Element.Fire, Element.Dark), 1f },
		{ (Element.Wind, Element.Dark), 1f },
		{ (Element.Wind, Element.Light), 1f },
		{ (Element.Fire, Element.Light), 1f },
		{ (Element.Earth, Element.Light), 1f },
		{ (Element.Water, Element.Light), 1f },
		{ (Element.Light, Element.Dark), 1.75f },
		{ (Element.Dark, Element.Light), 0.75f },
	};
	public static float GetMultiplier(Element attackerElement, Element defenderElement)
	{
		if (elementalMultiplier.TryGetValue((attackerElement, defenderElement), out float multiplier))
			return multiplier;

		return 1f; 
	}
}

public enum CritFocedType
{
	None,
	Sleep,
}
public sealed class DamageContext
{
	public EntityBase Source;
	public EntityBase Target;

	public bool CritDecided;
	public bool IsCritical;
	public bool HasElementalAdvantage;
	public float CritMultiplier = 1f;
	public CritFocedType CritForce = CritFocedType.None;
	public int BaseDamage;
	public int EffectiveDamage;

	public SkillDefinition Origin;
	public bool isEffectDamage;
	public bool CancleApply;

	public int ReflectAmount;
	public EntityBase RedirectTo;

	public EntityBase SplitRedirectTo;
	public float SplitPercent;

	public bool BlockFurtherSharing;
	public string SkillName = "";
	//Damage Element

	public float defenseIgnorePercentage = 0f;
	public float attackIncreasePercentage = 1f;
	public float propertyDamagePercentage = 0f;

	// Critical hit bonuses (from skill modifiers, equipment, buffs)
	public float BonusCritChance = 0f;
	public float BonusCritMultiplier = 0f;


	public void Reset(EntityBase src, EntityBase tgt, int damage, SkillDefinition origin, string skillName = "", bool isEffect = false)
	{
		Source = src;
		Target = tgt;
		BaseDamage = damage;
		EffectiveDamage = damage;
		this.Origin = origin;
		this.SkillName = skillName;
		isEffectDamage = isEffect;
		CancleApply = false;
		ReflectAmount = 0;
		RedirectTo = null;
		SplitRedirectTo = null;
		SplitPercent = 0f;
		BlockFurtherSharing = false;
		CritDecided = false;
		IsCritical = false;
		CritMultiplier = 1f;
		CritForce = CritFocedType.None;
		defenseIgnorePercentage = 0;
		attackIncreasePercentage = 1;
		BonusCritChance = 0f;
		BonusCritMultiplier = 0f;
		HasElementalAdvantage = false;
	}
}
public sealed class HealingContext
{
	public EntityBase Healer;
	public EntityBase Target;
	public int BaseHealing;
	public int FinalHealing;

	public void Reset(EntityBase healer, EntityBase target, int baseHealing)
	{
		Healer = healer;
		Target = target;
		BaseHealing = baseHealing;
		FinalHealing = baseHealing;
	}
}
[System.Serializable]
public class ActiveSkillEffect
{
	public EffectData effectData;
	public float procChance;
	public int turnDuration;
	public EffectActiveTiming activeTiming;

	public ActiveSkillEffect(EffectData effectData, float procChance,int turnDuration, EffectActiveTiming activeTiming, TargetType target)
	{
		this.effectData = effectData;
		this.procChance = procChance;
		this.turnDuration = turnDuration;
		this.activeTiming = activeTiming;
	}
}
