using System.Collections.Generic;
using UnityEngine;

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
