using UnityEngine;

[CreateAssetMenu(fileName = "ShopPurchaseCondition", menuName = "Achievements/Conditions/Shop Purchase Count")]
public class ShopPurchaseAchievementConditionData : AchievementConditionData<ShopPurchaseEvent>
{
	[SerializeField] private EquipableBaseData targetEquipable;

	protected override bool MatchesTyped(ShopPurchaseEvent achievementEvent)
	{
		return targetEquipable == null || achievementEvent.Equipable == targetEquipable;
	}

	public override string GetDescription()
	{
		if (targetEquipable != null)
		{
			return $"Buy {RequiredCount} {targetEquipable.itemName}";
		}

		return $"Buy {RequiredCount} shop items";
	}
}
