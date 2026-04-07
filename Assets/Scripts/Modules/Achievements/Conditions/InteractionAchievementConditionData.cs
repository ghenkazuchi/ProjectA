using UnityEngine;

[CreateAssetMenu(fileName = "InteractionCondition", menuName = "Achievements/Conditions/Interaction Count")]
public class InteractionAchievementConditionData : AchievementConditionData<InteractionEvent>
{
	[SerializeField] private InteractionAchievementFilterMode filterMode = InteractionAchievementFilterMode.InteractableType;
	[SerializeField] private InteracableType interactableType = InteracableType.FireCamp;
	[SerializeField] private SpawnableObject specificSpawnableObject;

	protected override bool MatchesTyped(InteractionEvent achievementEvent)
	{
		return filterMode switch
		{
			InteractionAchievementFilterMode.Any => true,
			InteractionAchievementFilterMode.InteractableType => achievementEvent.HasInteractableType && achievementEvent.InteractableType == interactableType,
			InteractionAchievementFilterMode.SpecificSpawnableObject => specificSpawnableObject != null && achievementEvent.SpawnableObject == specificSpawnableObject,
			_ => false
		};
	}

	public override string GetDescription()
	{
		string targetName = filterMode switch
		{
			InteractionAchievementFilterMode.Any => "objects",
			InteractionAchievementFilterMode.InteractableType => interactableType.ToString(),
			InteractionAchievementFilterMode.SpecificSpawnableObject when specificSpawnableObject != null => specificSpawnableObject.name,
			_ => "objects"
		};

		if (RequiredCount <= 1)
		{
			return filterMode == InteractionAchievementFilterMode.Any
				? "Interact with an object for the first time"
				: $"Interact with {targetName} for the first time";
		}

		return filterMode == InteractionAchievementFilterMode.Any
			? $"Interact with objects {RequiredCount} times"
			: $"Interact with {targetName} {RequiredCount} times";
	}
}
