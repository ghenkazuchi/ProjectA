using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementDefinition", menuName = "Achievements/Achievement Definition")]
public class AchievementDefinition : ScriptableObject
{
	[SerializeField] private string achievementId;
	[SerializeField] private string achievementTitle;
	[SerializeField] [TextArea] private string description;
	[SerializeField] private Sprite icon;
	[SerializeField] private bool hiddenUntilCompleted;
	[SerializeField] private List<AchievementConditionData> conditions = new List<AchievementConditionData>();
	[SerializeField] private List<AchievementRewardData> rewards = new List<AchievementRewardData>();

	public string AchievementId => string.IsNullOrWhiteSpace(achievementId) ? name : achievementId;
	public string AchievementTitle => string.IsNullOrWhiteSpace(achievementTitle) ? name : achievementTitle;
	public string Description => description;
	public Sprite Icon => icon;
	public bool HiddenUntilCompleted => hiddenUntilCompleted;
	public IReadOnlyList<AchievementConditionData> Conditions => conditions;
	public IReadOnlyList<AchievementRewardData> Rewards => rewards;
}
