using UnityEngine;

[CreateAssetMenu(fileName = "BattleWinLowPartyHpCondition", menuName = "Achievements/Conditions/Battle Win With Low Party HP")]
public class BattleWinWithLowPartyHpAchievementConditionData : AchievementConditionData<BattleWinEvent>
{
	[SerializeField] [Range(0.01f, 1f)] private float maxPartyHealthRatio = 0.3f;

	protected override bool MatchesTyped(BattleWinEvent achievementEvent)
	{
		return achievementEvent.PartyHealthRatio >= 0f
			&& achievementEvent.PartyHealthRatio <= maxPartyHealthRatio;
	}

	public override string GetDescription()
	{
		int thresholdPercent = Mathf.RoundToInt(maxPartyHealthRatio * 100f);
		return $"Win {RequiredCount} battles while party HP is at or below {thresholdPercent}%";
	}
}
