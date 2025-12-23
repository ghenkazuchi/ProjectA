using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopEquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] private Image icon;
	[SerializeField] private Image indicator;
	private bool selected;
	private System.Action<bool> onSelectedChanged;

	private void Awake()
	{
		SetSelected(false, notify: false);
	}

	public bool IsSelected() => selected;


	public void SetUpWeapon(Weapon weapon, int slotCost, System.Action<bool> onChanged)
	{
		icon.sprite = weapon.WeaponBaseData.icon;
		Bind(onChanged);
	}
	public void SetupItem(Item item, int slotCost, System.Action<bool> onChanged)
	{
		icon.sprite = item.itemBaseData.icon;
		Bind(onChanged);
	}

	private void Bind(System.Action<bool> onChanged)
	{
		onSelectedChanged = onChanged;
		SetSelected(false, notify: false);
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		SetSelected(!selected, notify: true);
	}
	public void SetSelected(bool value, bool notify)
	{
		selected = value;
		indicator.enabled = selected;
		if(notify) onSelectedChanged?.Invoke(selected);
	}
}
