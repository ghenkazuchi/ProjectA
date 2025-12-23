using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChestItemSlot : MonoBehaviour,IPointerClickHandler
{
	public ChestLootEntry entry;
	[SerializeField] Image itemIcon;

	public System.Action<EquipableBaseData> OnItemSelected;
	public void InitializeItemSlot()
	{
		if (entry.data != null)
		{
			itemIcon.sprite = entry.data.icon;
			if(entry.data is ItemBaseData itemData)
			{
				itemIcon.color = itemData.GetTint(entry.grade);
			}
			else
			{
				itemIcon.color = Color.white;
			}
		}
		else
		{
			ClearItemSlot();
		}
	}

	public void SetItemData(ChestLootEntry data)
	{
		entry = data;
		InitializeItemSlot();
	}
	public void ClearItemSlot()
	{
		entry = default;
		itemIcon.sprite = null;
		itemIcon.color = Color.white;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnItemSelected, new object[] { entry }));
	}
}
