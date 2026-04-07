using UnityEngine;

[CreateAssetMenu(fileName = "RecruitSpecificCharacterCondition", menuName = "Achievements/Conditions/Recruit Specific Character")]
public class RecruitSpecificCharacterAchievementConditionData : AchievementConditionData<RecruitEvent>
{
	[SerializeField] private RecruitAchievementFilterMode filterMode = RecruitAchievementFilterMode.SpecificCharacterData;
	[SerializeField] private BaseEntityData targetCharacterData;
	[SerializeField] private RecruitableCharacterTemplate targetTemplate;

	protected override bool MatchesTyped(RecruitEvent achievementEvent)
	{
		return filterMode switch
		{
			RecruitAchievementFilterMode.Any => true,
			RecruitAchievementFilterMode.SpecificCharacterData => targetCharacterData != null && achievementEvent.RecruitedCharacterData == targetCharacterData,
			RecruitAchievementFilterMode.SpecificTemplate => targetTemplate != null && achievementEvent.RecruitedTemplate == targetTemplate,
			_ => false
		};
	}

	public override string GetDescription()
	{
		string targetName = filterMode switch
		{
			RecruitAchievementFilterMode.SpecificCharacterData when targetCharacterData != null => targetCharacterData.EntityName,
			RecruitAchievementFilterMode.SpecificTemplate when targetTemplate != null && targetTemplate.entityData != null => targetTemplate.entityData.EntityName,
			_ => "companions"
		};

		if (RequiredCount <= 1)
		{
			return filterMode == RecruitAchievementFilterMode.Any
				? "Recruit a companion"
				: $"Recruit {targetName}";
		}

		return filterMode == RecruitAchievementFilterMode.Any
			? $"Recruit {RequiredCount} companions"
			: $"Recruit {targetName} {RequiredCount} times";
	}
}
