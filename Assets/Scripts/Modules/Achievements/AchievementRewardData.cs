using System.Collections.Generic;
using UnityEngine;

public enum UnlockableEquipableSourceKind
{
	Shop,
	Chest
}

[CreateAssetMenu(fileName = "UnlockableEquipablePool", menuName = "Achievements/Unlockable Equipable Pool")]
public class UnlockableEquipablePool : ScriptableObject
{
	[SerializeField] private string poolId;
	[SerializeField] private UnlockableEquipableSourceKind sourceKind;
	[SerializeField] private List<EquipableBaseData> baseContents = new List<EquipableBaseData>();

	public string PoolId => string.IsNullOrWhiteSpace(poolId) ? name : poolId;
	public UnlockableEquipableSourceKind SourceKind => sourceKind;
	public IReadOnlyList<EquipableBaseData> BaseContents => baseContents;
}

[CreateAssetMenu(fileName = "UnlockableSkillPool", menuName = "Achievements/Unlockable Skill Pool")]
public class UnlockableSkillPool : ScriptableObject
{
	[SerializeField] private string poolId;
	[SerializeField] private List<BaseSkillData> baseContents = new List<BaseSkillData>();

	public string PoolId => string.IsNullOrWhiteSpace(poolId) ? name : poolId;
	public IReadOnlyList<BaseSkillData> BaseContents => baseContents;
}

public abstract class AchievementRewardData : ScriptableObject
{
	public abstract string GetSummary();
}

[CreateAssetMenu(fileName = "UnlockShopEquipableReward", menuName = "Achievements/Rewards/Unlock Shop Equipable")]
public class UnlockShopEquipableAchievementRewardData : AchievementRewardData
{
	[SerializeField] private UnlockableEquipablePool targetPool;
	[SerializeField] private EquipableBaseData equipable;

	public UnlockableEquipablePool TargetPool => targetPool;
	public EquipableBaseData Equipable => equipable;

	public override string GetSummary()
	{
		string itemName = equipable != null ? equipable.itemName : "Unknown item";
		string poolName = targetPool != null ? targetPool.name : "shop pool";
		return $"Unlock {itemName} in {poolName}";
	}
}

[CreateAssetMenu(fileName = "UnlockChestEquipableReward", menuName = "Achievements/Rewards/Unlock Chest Equipable")]
public class UnlockChestEquipableAchievementRewardData : AchievementRewardData
{
	[SerializeField] private UnlockableEquipablePool targetPool;
	[SerializeField] private EquipableBaseData equipable;

	public UnlockableEquipablePool TargetPool => targetPool;
	public EquipableBaseData Equipable => equipable;

	public override string GetSummary()
	{
		string itemName = equipable != null ? equipable.itemName : "Unknown item";
		string poolName = targetPool != null ? targetPool.name : "chest pool";
		return $"Unlock {itemName} in {poolName}";
	}
}

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
