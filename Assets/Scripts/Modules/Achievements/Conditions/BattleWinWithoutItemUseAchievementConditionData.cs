using UnityEngine;

[CreateAssetMenu(fileName = "BattleWinWithoutItemUseCondition", menuName = "Achievements/Conditions/Battle Win Without Item Use")]
public class BattleWinWithoutItemUseAchievementConditionData : AchievementConditionData<BattleWinEvent>
{
	protected override bool MatchesTyped(BattleWinEvent achievementEvent)
	{
		return achievementEvent.BattleItemUseCount <= 0;
	}

	public override string GetDescription()
	{
		return $"Win {RequiredCount} battles without using item effects";
	}
}
