using UnityEngine;

[CreateAssetMenu(fileName = "RecruitCountCondition", menuName = "Achievements/Conditions/Recruit Count")]
public class RecruitCountAchievementConditionData : AchievementConditionData<RecruitEvent>
{
	protected override bool MatchesTyped(RecruitEvent achievementEvent)
	{
		return true;
	}

	public override string GetDescription()
	{
		return $"Recruit {RequiredCount} companions";
	}
}
