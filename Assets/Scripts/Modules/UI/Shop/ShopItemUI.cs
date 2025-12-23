using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour,IPointerClickHandler
{
	[SerializeField] private Image equipableIcon;
	private EquipableBaseData equipableBaseData;

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("Clicked");
		MessageManager.Instance.SendMessage(new Message(MessageType.OnShopItemSelected, new object[] { equipableBaseData }));
	}

	public void SetUp(EquipableBaseData equipableBaseData)
	{
		this.equipableBaseData = equipableBaseData;
		equipableIcon.sprite = equipableBaseData.icon;
	}
}
