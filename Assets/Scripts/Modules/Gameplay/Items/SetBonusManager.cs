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
			if (b == null || b.equipmentSet == null)
			{
				return;
			}
			var set = b.equipmentSet;
			if(!equipablePerSet.TryGetValue(set, out var seen))
			{
				equipablePerSet[set] = seen = new HashSet<EquipableBaseData>();
			}
			if (seen.Contains(b)) return;
			seen.Add(b);
			int w = Mathf.Max(1, b.pieceWeight);
			counts[b.equipmentSet] = counts.TryGetValue(b.equipmentSet, out var c) ? c + w : w;
		}
		if (weapon != null) 
		{
			if(weapon.WeaponBaseData != null)
			{
				Acc(weapon.WeaponBaseData);
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
