using UnityEngine;

[CreateAssetMenu(fileName = "BattleWinBySurvivorCountCondition", menuName = "Achievements/Conditions/Battle Win By Survivor Count")]
public class BattleWinBySurvivorCountAchievementConditionData : AchievementConditionData<BattleWinEvent>
{
	[SerializeField] [Min(0)] private int minAlivePartyMembers = 1;
	[SerializeField] [Min(0)] private int maxAlivePartyMembers = 1;

	protected override bool MatchesTyped(BattleWinEvent achievementEvent)
	{
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
