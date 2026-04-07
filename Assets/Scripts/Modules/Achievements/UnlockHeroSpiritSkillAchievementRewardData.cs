using UnityEngine;

[CreateAssetMenu(fileName = "UnlockHeroSpiritSkillReward", menuName = "Achievements/Rewards/Unlock Hero Spirit Skill")]
public class UnlockHeroSpiritSkillAchievementRewardData : AchievementRewardData
{
	[SerializeField] private UnlockableSkillPool targetPool;
	[SerializeField] private BaseSkillData skill;

	public UnlockableSkillPool TargetPool => targetPool;
	public BaseSkillData Skill => skill;

	public override string GetSummary()
	{
		string skillName = skill != null ? skill.skillName : "Unknown skill";
		string poolName = targetPool != null ? targetPool.name : "hero spirit pool";
		return $"Unlock {skillName} in {poolName}";
	}
}
