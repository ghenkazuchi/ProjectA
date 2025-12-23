using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryWeaponSlot : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] Image weaponIcon;
	[SerializeField] Weapon weapon;

	public void SetUp(Weapon weapon)
	{
		weaponIcon.sprite = weapon.WeaponBaseData.icon;
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnInventoryItemSelected, new object[] {weapon} ));
	}
}
