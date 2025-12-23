using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EquipmentHelper
{
	public static int FindFreeSlot(PlayerCharacter character, EquipableBaseData equipableData)
	{
		if(equipableData is WeaponBaseData weapon)
		{
			if(character.weapon == null)
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}
		if(equipableData is ItemBaseData)
		{
			var usedSlots = character.items.Count;
			var totalSlots = character.GetClassData.itemSlotCount;
			if (usedSlots < totalSlots)
			{
				return totalSlots - usedSlots;
			}
			else
			{
				return 0;
			}
		}
		return 0;
	}

	public static void Equip(PlayerCharacter character, EquipableBaseData equipableData)
	{
	}
}
