using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] Image itemIcon;
	[SerializeField] Item item;
	private int index;
	public void SetUp(Item item,int index)
	{
		this.index = index;
		gameObject.SetActive(true);
		this.item = item;
		itemIcon.sprite = item.itemBaseData.icon;
		itemIcon.color = item.itemBaseData.GetTint(item.currentItemGrade);
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnInventoryItemSelected, new object[] { item.itemBaseData,index }));
	}
}
