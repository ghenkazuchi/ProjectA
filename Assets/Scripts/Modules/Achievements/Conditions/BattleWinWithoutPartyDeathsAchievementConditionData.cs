using UnityEngine;

[CreateAssetMenu(fileName = "BattleWinNoPartyDeathsCondition", menuName = "Achievements/Conditions/Battle Win Without Party Deaths")]
public class BattleWinWithoutPartyDeathsAchievementConditionData : AchievementConditionData<BattleWinEvent>
{
	protected override bool MatchesTyped(BattleWinEvent achievementEvent)
	{
		return achievementEvent.TotalPartyMemberCount > 0
			&& achievementEvent.AlivePartyMemberCount >= achievementEvent.TotalPartyMemberCount;
	}

	public override string GetDescription()
	{
		return $"Win {RequiredCount} battles without losing a party member";
	}
}
