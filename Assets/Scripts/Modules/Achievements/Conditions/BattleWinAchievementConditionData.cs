using UnityEngine;

[CreateAssetMenu(fileName = "BattleWinCondition", menuName = "Achievements/Conditions/Battle Win Count")]
public class BattleWinAchievementConditionData : AchievementConditionData<BattleWinEvent>
{
	protected override bool MatchesTyped(BattleWinEvent achievementEvent)
	{
		return true; // We already know it's a battle win event because of generic filtering!
	}

	public override string GetDescription()
	{
		return $"Win {RequiredCount} battles";
	}
}
