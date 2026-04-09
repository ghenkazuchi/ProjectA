using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SetBonusManager
{
	private readonly Dictionary<EquipableSetData, int> counts = new();
	private readonly Dictionary<EquipableSetData, List<EquipEffectBinding>> activeSets = new();
	private readonly Dictionary<EquipableSetData, HashSet<EquipableBaseData>> equipablePerSet = new();
	public void Recalculate(List<Item> items, Weapon weapon)
	{
		counts.Clear();
		activeSets.Clear();
		equipablePerSet.Clear();
		void Acc(EquipableBaseData b)
		{
			if (b == null || b.equipmentSets == null)
			{
				return;
			}
			foreach (var set in b.equipmentSets)
			{
				if (set == null) continue;
				if(!equipablePerSet.TryGetValue(set, out var seen))
				{
					equipablePerSet[set] = seen = new HashSet<EquipableBaseData>();
				}
				if (seen.Contains(b)) continue;
				seen.Add(b);
				int w = Mathf.Max(1, b.pieceWeight);
				counts[set] = counts.TryGetValue(set, out var c) ? c + w : w;
			}
		}
		if (weapon != null) 
		{
			if(weapon.WeaponBaseData != null)
			{
				var wb = weapon.WeaponBaseData;
				if (wb.equipmentSets != null)
				{
					foreach (var set in wb.equipmentSets)
					{
						if (set == null) continue;
						if(!equipablePerSet.TryGetValue(set, out var seen))
						{
							equipablePerSet[set] = seen = new HashSet<EquipableBaseData>();
						}
						if (!seen.Contains(wb))
						{
							seen.Add(wb);
							int w = Mathf.Max(1, wb.pieceWeight) * weapon.CurrentStack;
							counts[set] = counts.TryGetValue(set, out var c) ? c + w : w;
						}
					}
				}
			}
		}
		if (items != null)
		{
			foreach (var item in items)
			{
				if(item == null)
				{
					continue;
				}
				if(item.itemBaseData == null)
				{
					continue;
				}
				Acc(item.itemBaseData);
			}
		}

		foreach (var kv in counts)
		{
			var set = kv.Key;
			var n = kv.Value;
			var list = new List<EquipEffectBinding>();
			foreach (var e in set.GetEffects(n)) list.Add(e);
			if (list.Count > 0)
			{
				activeSets[set] = list;
			}
		}
	}
	public List<EquipEffectBinding> GetAllActiveBindings()
	{
		var outList = new List<EquipEffectBinding>();
		foreach (var kv in activeSets)
		{
			Debug.Log("Adding set bonuses from " + kv.Key.setName);	
			outList.AddRange(kv.Value);
		}
		return outList;
	}
	public IReadOnlyDictionary<EquipableSetData, List<EquipEffectBinding>> GetActiveSets()
	{
		return activeSets;
	}
}
