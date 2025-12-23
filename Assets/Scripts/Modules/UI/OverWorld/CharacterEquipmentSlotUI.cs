using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterEquipmentSlotUI : MonoBehaviour
{
	[SerializeField] EquipableBaseData itemData;
	[SerializeField] Image itemIcon;
	public void SetUp(EquipableBaseData item)
	{
		itemData = item;
		itemIcon.sprite = itemData.icon;
	}
}
