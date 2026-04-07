using UnityEngine;

[CreateAssetMenu(fileName = "BattleWinAgainstMonsterCondition", menuName = "Achievements/Conditions/Battle Win Against Monster")]
public class BattleWinAgainstMonsterAchievementConditionData : AchievementConditionData<BattleWinEvent>
{
	[SerializeField] private MonsterKillFilterMode filterMode = MonsterKillFilterMode.MonsterType;
	[SerializeField] private MonsterTypeName monsterType = MonsterTypeName.Orc;
	[SerializeField] private MonsterData specificMonster;

	protected override bool MatchesTyped(BattleWinEvent achievementEvent)
	{
		if (achievementEvent.BattleMonsters == null || achievementEvent.BattleMonsters.Count == 0)
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
