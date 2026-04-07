using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockableSkillPool", menuName = "Achievements/Unlockable Skill Pool")]
public class UnlockableSkillPool : ScriptableObject
{
	[SerializeField] private string poolId;
	[SerializeField] private List<BaseSkillData> baseContents = new List<BaseSkillData>();

	public string PoolId => string.IsNullOrWhiteSpace(poolId) ? name : poolId;
	public IReadOnlyList<BaseSkillData> BaseContents => baseContents;
}
