using UnityEngine;

[CreateAssetMenu(fileName = "BattleWinByBattleTypeCondition", menuName = "Achievements/Conditions/Battle Win By Battle Type")]
public class BattleWinByBattleTypeAchievementConditionData : AchievementConditionData<BattleWinEvent>
{
	[SerializeField] private BattleType battleType = BattleType.Boss;

	protected override bool MatchesTyped(BattleWinEvent achievementEvent)
	{
		return achievementEvent.BattleType == battleType;
	}

	public override string GetDescription()
	{
		return $"Win {RequiredCount} {battleType} battles";
	}
}
