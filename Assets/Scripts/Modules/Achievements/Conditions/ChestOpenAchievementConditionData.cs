using UnityEngine;

[CreateAssetMenu(fileName = "ChestOpenCondition", menuName = "Achievements/Conditions/Chest Open Count")]
public class ChestOpenAchievementConditionData : AchievementConditionData<ChestOpenEvent>
{
	protected override bool MatchesTyped(ChestOpenEvent achievementEvent)
	{
		return true;
	}

	public override string GetDescription()
	{
		return $"Open {RequiredCount} chests";
	}
}
