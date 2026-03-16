using System.Collections.Generic;
using UnityEngine;

public enum MonsterKillFilterMode
{
	Any,
	MonsterType,
	SpecificMonster
}

public enum AchievementEventKind
{
	MonsterKill,
	BattleWin,
	ShopPurchase,
	ChestOpen,
	Recruit,
	Interaction
}

public enum InteractionAchievementFilterMode
{
	Any,
	InteractableType,
	SpecificSpawnableObject
}

public enum RecruitAchievementFilterMode
{
	Any,
	SpecificCharacterData,
	SpecificTemplate
}

public sealed class AchievementEventData
{
	public AchievementEventKind Kind;
	public int Count = 1;
	public MonsterCharacter Monster;
	public EquipableBaseData Equipable;
	public float PartyHealthRatio = -1f;
	public int AlivePartyMemberCount;
	public int TotalPartyMemberCount;
	public BattleType BattleType;
	public int BattleItemUseCount;
	public List<MonsterCharacter> BattleMonsters;
	public BaseEntityData RecruitedCharacterData;
	public RecruitableCharacterTemplate RecruitedTemplate;
	public bool HasInteractableType;
	public InteracableType InteractableType;
	public SpawnableObject SpawnableObject;
	public string InteractionKey;
}

public abstract class AchievementConditionData : ScriptableObject
{
	[SerializeField] private int requiredCount = 1;

	public int RequiredCount => Mathf.Max(1, requiredCount);

	public virtual int GetProgressIncrement(AchievementEventData achievementEvent)
	{
		return Mathf.Max(1, achievementEvent?.Count ?? 1);
	}

	public abstract bool Matches(AchievementEventData achievementEvent);
	public abstract string GetDescription();
}

[CreateAssetMenu(fileName = "MonsterKillCondition", menuName = "Achievements/Conditions/Monster Kill Count")]
public class MonsterKillAchievementConditionData : AchievementConditionData
{
	[SerializeField] private MonsterKillFilterMode filterMode = MonsterKillFilterMode.MonsterType;
	[SerializeField] private MonsterTypeName monsterType = MonsterTypeName.Orc;
	[SerializeField] private MonsterData specificMonster;

	public override bool Matches(AchievementEventData achievementEvent)
	{
		if (achievementEvent == null || achievementEvent.Kind != AchievementEventKind.MonsterKill || achievementEvent.Monster == null)
		{
			return false;
		}

		switch (filterMode)
		{
			case MonsterKillFilterMode.Any:
				return true;
			case MonsterKillFilterMode.MonsterType:
				return achievementEvent.Monster.RaceData != null &&
					achievementEvent.Monster.RaceData.monsterTypeDefinition == monsterType;
			case MonsterKillFilterMode.SpecificMonster:
				return specificMonster != null && achievementEvent.Monster.entityData == specificMonster;
			default:
				return false;
		}
	}

	public override string GetDescription()
	{
		return filterMode switch
		{
			MonsterKillFilterMode.Any => $"Defeat {RequiredCount} monsters",
			MonsterKillFilterMode.MonsterType => $"Defeat {RequiredCount} {monsterType}",
			MonsterKillFilterMode.SpecificMonster when specificMonster != null => $"Defeat {RequiredCount} {specificMonster.EntityName}",
			_ => $"Defeat {RequiredCount} monsters"
		};
	}
}

[CreateAssetMenu(fileName = "BattleWinCondition", menuName = "Achievements/Conditions/Battle Win Count")]
public class BattleWinAchievementConditionData : AchievementConditionData
{
	public override bool Matches(AchievementEventData achievementEvent)
	{
		return achievementEvent != null && achievementEvent.Kind == AchievementEventKind.BattleWin;
	}

	public override string GetDescription()
	{
		return $"Win {RequiredCount} battles";
	}
}

[CreateAssetMenu(fileName = "BattleWinLowPartyHpCondition", menuName = "Achievements/Conditions/Battle Win With Low Party HP")]
public class BattleWinWithLowPartyHpAchievementConditionData : AchievementConditionData
{
	[SerializeField] [Range(0.01f, 1f)] private float maxPartyHealthRatio = 0.3f;

	public override bool Matches(AchievementEventData achievementEvent)
	{
		return achievementEvent != null
			&& achievementEvent.Kind == AchievementEventKind.BattleWin
			&& achievementEvent.PartyHealthRatio >= 0f
			&& achievementEvent.PartyHealthRatio <= maxPartyHealthRatio;
	}

	public override string GetDescription()
	{
		int thresholdPercent = Mathf.RoundToInt(maxPartyHealthRatio * 100f);
		return $"Win {RequiredCount} battles while party HP is at or below {thresholdPercent}%";
	}
}

[CreateAssetMenu(fileName = "BattleWinByBattleTypeCondition", menuName = "Achievements/Conditions/Battle Win By Battle Type")]
public class BattleWinByBattleTypeAchievementConditionData : AchievementConditionData
{
	[SerializeField] private BattleType battleType = BattleType.Boss;

	public override bool Matches(AchievementEventData achievementEvent)
	{
		return achievementEvent != null
			&& achievementEvent.Kind == AchievementEventKind.BattleWin
			&& achievementEvent.BattleType == battleType;
	}

	public override string GetDescription()
	{
		return $"Win {RequiredCount} {battleType} battles";
	}
}

[CreateAssetMenu(fileName = "BattleWinBySurvivorCountCondition", menuName = "Achievements/Conditions/Battle Win By Survivor Count")]
public class BattleWinBySurvivorCountAchievementConditionData : AchievementConditionData
{
	[SerializeField] [Min(0)] private int minAlivePartyMembers = 1;
	[SerializeField] [Min(0)] private int maxAlivePartyMembers = 1;

	public override bool Matches(AchievementEventData achievementEvent)
	{
		if (achievementEvent == null || achievementEvent.Kind != AchievementEventKind.BattleWin)
		{
			return false;
		}

		int min = Mathf.Min(minAlivePartyMembers, maxAlivePartyMembers);
		int max = Mathf.Max(minAlivePartyMembers, maxAlivePartyMembers);
		return achievementEvent.AlivePartyMemberCount >= min && achievementEvent.AlivePartyMemberCount <= max;
	}

	public override string GetDescription()
	{
		int min = Mathf.Min(minAlivePartyMembers, maxAlivePartyMembers);
		int max = Mathf.Max(minAlivePartyMembers, maxAlivePartyMembers);
		if (min == max)
		{
			if (min == 1)
			{
				return $"Win {RequiredCount} battles with only 1 survivor";
			}

			return $"Win {RequiredCount} battles with exactly {min} survivors";
		}

		return $"Win {RequiredCount} battles with {min}-{max} survivors";
	}
}

[CreateAssetMenu(fileName = "BattleWinNoPartyDeathsCondition", menuName = "Achievements/Conditions/Battle Win Without Party Deaths")]
public class BattleWinWithoutPartyDeathsAchievementConditionData : AchievementConditionData
{
	public override bool Matches(AchievementEventData achievementEvent)
	{
		return achievementEvent != null
			&& achievementEvent.Kind == AchievementEventKind.BattleWin
			&& achievementEvent.TotalPartyMemberCount > 0
			&& achievementEvent.AlivePartyMemberCount >= achievementEvent.TotalPartyMemberCount;
	}

	public override string GetDescription()
	{
		return $"Win {RequiredCount} battles without losing a party member";
	}
}

[CreateAssetMenu(fileName = "BattleWinWithoutItemUseCondition", menuName = "Achievements/Conditions/Battle Win Without Item Use")]
public class BattleWinWithoutItemUseAchievementConditionData : AchievementConditionData
{
	public override bool Matches(AchievementEventData achievementEvent)
	{
		return achievementEvent != null
			&& achievementEvent.Kind == AchievementEventKind.BattleWin
			&& achievementEvent.BattleItemUseCount <= 0;
	}

	public override string GetDescription()
	{
		return $"Win {RequiredCount} battles without using item effects";
	}
}

[CreateAssetMenu(fileName = "BattleWinAgainstMonsterCondition", menuName = "Achievements/Conditions/Battle Win Against Monster")]
public class BattleWinAgainstMonsterAchievementConditionData : AchievementConditionData
{
	[SerializeField] private MonsterKillFilterMode filterMode = MonsterKillFilterMode.MonsterType;
	[SerializeField] private MonsterTypeName monsterType = MonsterTypeName.Orc;
	[SerializeField] private MonsterData specificMonster;

	public override bool Matches(AchievementEventData achievementEvent)
	{
		if (achievementEvent == null
			|| achievementEvent.Kind != AchievementEventKind.BattleWin
			|| achievementEvent.BattleMonsters == null
			|| achievementEvent.BattleMonsters.Count == 0)
		{
			return false;
		}

		for (int i = 0; i < achievementEvent.BattleMonsters.Count; i++)
		{
			MonsterCharacter monster = achievementEvent.BattleMonsters[i];
			if (monster == null)
			{
				continue;
			}

			switch (filterMode)
			{
				case MonsterKillFilterMode.Any:
					return true;
				case MonsterKillFilterMode.MonsterType:
					if (monster.RaceData != null && monster.RaceData.monsterTypeDefinition == monsterType)
					{
						return true;
					}
					break;
				case MonsterKillFilterMode.SpecificMonster:
					if (specificMonster != null && monster.entityData == specificMonster)
					{
						return true;
					}
					break;
			}
		}

		return false;
	}

	public override string GetDescription()
	{
		return filterMode switch
		{
			MonsterKillFilterMode.Any => $"Win {RequiredCount} battles",
			MonsterKillFilterMode.MonsterType => $"Win {RequiredCount} battles against {monsterType}",
			MonsterKillFilterMode.SpecificMonster when specificMonster != null => $"Win {RequiredCount} battles against {specificMonster.EntityName}",
			_ => $"Win {RequiredCount} battles"
		};
	}
}

[CreateAssetMenu(fileName = "ShopPurchaseCondition", menuName = "Achievements/Conditions/Shop Purchase Count")]
public class ShopPurchaseAchievementConditionData : AchievementConditionData
{
	[SerializeField] private EquipableBaseData targetEquipable;

	public override bool Matches(AchievementEventData achievementEvent)
	{
		if (achievementEvent == null || achievementEvent.Kind != AchievementEventKind.ShopPurchase)
		{
			return false;
		}

		return targetEquipable == null || achievementEvent.Equipable == targetEquipable;
	}

	public override string GetDescription()
	{
		if (targetEquipable != null)
		{
			return $"Buy {RequiredCount} {targetEquipable.itemName}";
		}

		return $"Buy {RequiredCount} shop items";
	}
}

[CreateAssetMenu(fileName = "ChestOpenCondition", menuName = "Achievements/Conditions/Chest Open Count")]
public class ChestOpenAchievementConditionData : AchievementConditionData
{
	public override bool Matches(AchievementEventData achievementEvent)
	{
		return achievementEvent != null && achievementEvent.Kind == AchievementEventKind.ChestOpen;
	}

	public override string GetDescription()
	{
		return $"Open {RequiredCount} chests";
	}
}

[CreateAssetMenu(fileName = "RecruitCountCondition", menuName = "Achievements/Conditions/Recruit Count")]
public class RecruitCountAchievementConditionData : AchievementConditionData
{
	public override bool Matches(AchievementEventData achievementEvent)
	{
		return achievementEvent != null && achievementEvent.Kind == AchievementEventKind.Recruit;
	}

	public override string GetDescription()
	{
		return $"Recruit {RequiredCount} companions";
	}
}

[CreateAssetMenu(fileName = "RecruitSpecificCharacterCondition", menuName = "Achievements/Conditions/Recruit Specific Character")]
public class RecruitSpecificCharacterAchievementConditionData : AchievementConditionData
{
	[SerializeField] private RecruitAchievementFilterMode filterMode = RecruitAchievementFilterMode.SpecificCharacterData;
	[SerializeField] private BaseEntityData targetCharacterData;
	[SerializeField] private RecruitableCharacterTemplate targetTemplate;

	public override bool Matches(AchievementEventData achievementEvent)
	{
		if (achievementEvent == null || achievementEvent.Kind != AchievementEventKind.Recruit)
		{
			return false;
		}

		return filterMode switch
		{
			RecruitAchievementFilterMode.Any => true,
			RecruitAchievementFilterMode.SpecificCharacterData => targetCharacterData != null && achievementEvent.RecruitedCharacterData == targetCharacterData,
			RecruitAchievementFilterMode.SpecificTemplate => targetTemplate != null && achievementEvent.RecruitedTemplate == targetTemplate,
			_ => false
		};
	}

	public override string GetDescription()
	{
		string targetName = filterMode switch
		{
			RecruitAchievementFilterMode.SpecificCharacterData when targetCharacterData != null => targetCharacterData.EntityName,
			RecruitAchievementFilterMode.SpecificTemplate when targetTemplate != null && targetTemplate.entityData != null => targetTemplate.entityData.EntityName,
			_ => "companions"
		};

		if (RequiredCount <= 1)
		{
			return filterMode == RecruitAchievementFilterMode.Any
				? "Recruit a companion"
				: $"Recruit {targetName}";
		}

		return filterMode == RecruitAchievementFilterMode.Any
			? $"Recruit {RequiredCount} companions"
			: $"Recruit {targetName} {RequiredCount} times";
	}
}

[CreateAssetMenu(fileName = "InteractionCondition", menuName = "Achievements/Conditions/Interaction Count")]
public class InteractionAchievementConditionData : AchievementConditionData
{
	[SerializeField] private InteractionAchievementFilterMode filterMode = InteractionAchievementFilterMode.InteractableType;
	[SerializeField] private InteracableType interactableType = InteracableType.FireCamp;
	[SerializeField] private SpawnableObject specificSpawnableObject;

	public override bool Matches(AchievementEventData achievementEvent)
	{
		if (achievementEvent == null || achievementEvent.Kind != AchievementEventKind.Interaction)
		{
			return false;
		}

		return filterMode switch
		{
			InteractionAchievementFilterMode.Any => true,
			InteractionAchievementFilterMode.InteractableType => achievementEvent.HasInteractableType && achievementEvent.InteractableType == interactableType,
			InteractionAchievementFilterMode.SpecificSpawnableObject => specificSpawnableObject != null && achievementEvent.SpawnableObject == specificSpawnableObject,
			_ => false
		};
	}

	public override string GetDescription()
	{
		string targetName = filterMode switch
		{
			InteractionAchievementFilterMode.Any => "objects",
			InteractionAchievementFilterMode.InteractableType => interactableType.ToString(),
			InteractionAchievementFilterMode.SpecificSpawnableObject when specificSpawnableObject != null => specificSpawnableObject.name,
			_ => "objects"
		};

		if (RequiredCount <= 1)
		{
			return filterMode == InteractionAchievementFilterMode.Any
				? "Interact with an object for the first time"
				: $"Interact with {targetName} for the first time";
		}

		return filterMode == InteractionAchievementFilterMode.Any
			? $"Interact with objects {RequiredCount} times"
			: $"Interact with {targetName} {RequiredCount} times";
	}
}
