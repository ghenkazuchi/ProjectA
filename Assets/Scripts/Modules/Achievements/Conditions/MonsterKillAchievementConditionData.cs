using UnityEngine;

[CreateAssetMenu(fileName = "MonsterKillCondition", menuName = "Achievements/Conditions/Monster Kill Count")]
public class MonsterKillAchievementConditionData : AchievementConditionData<MonsterKillEvent>
{
	[SerializeField] private MonsterKillFilterMode filterMode = MonsterKillFilterMode.MonsterType;
	[SerializeField] private MonsterTypeName monsterType = MonsterTypeName.Orc;
	[SerializeField] private MonsterData specificMonster;

	protected override bool MatchesTyped(MonsterKillEvent achievementEvent)
	{
		if (achievementEvent.Monster == null)
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
