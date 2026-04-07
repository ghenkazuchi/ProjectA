using UnityEngine;

public enum UnlockableEquipableSourceKind
{
	Shop,
	Chest
}

public abstract class AchievementRewardData : ScriptableObject
{
	public abstract string GetSummary();
}
