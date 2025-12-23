using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipableSet", menuName = "Equipable/Equipable Set")]
public class EquipableSetData : ScriptableObject
{
	public string setName;
	public List<ThresholdBonus> thresholds = new();

	public IEnumerable<EquipEffectBinding> GetEffects(int pieceCount)
	{
		foreach (var t in thresholds)
		{
			if (pieceCount >= t.requiredPieces && t.effects != null)
			{
				foreach (var e in t.effects)
				{
					yield return e;
				}

			}
		}
	}
}
