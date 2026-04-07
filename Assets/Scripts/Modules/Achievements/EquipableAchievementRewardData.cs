using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockEquipableReward", menuName = "Achievements/Rewards/Unlock Equipable")]
public class EquipableAchievementRewardData : AchievementRewardData
{
	[SerializeField] private EquipableBaseData equipable;
	[SerializeField] private List<UnlockableEquipablePool> targetPools = new List<UnlockableEquipablePool>();

	public EquipableBaseData Equipable => equipable;
	public IReadOnlyList<UnlockableEquipablePool> TargetPools => targetPools;

	public override string GetSummary()
	{
		string itemName = equipable != null ? equipable.itemName : "Unknown item";
		int count = targetPools != null ? targetPools.Count : 0;
		return $"Unlock {itemName} in {count} pools";
	}
}
