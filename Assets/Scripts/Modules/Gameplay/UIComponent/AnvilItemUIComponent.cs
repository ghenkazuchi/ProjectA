using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnvilItemUIComponent : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] private Image itemIcon;
	[SerializeField] private GameObject selectedFrame;
	[SerializeField] private bool isSelected;
	[SerializeField] private bool selectable = true;
	private Action<int, Item, bool> onToggleSelect;
	private int index = -1;
	private Item item;
	public Item Item => item;
	public int Index => index;
	

	public void OnPointerClick(PointerEventData eventData)
	{
		if(!selectable) return;
		SetSelected(!isSelected);
		onToggleSelect?.Invoke(index, item, isSelected);
	}

	public void SetUp(Item item,int index, Action<int,Item,bool> onToggle )
	{
		this.item = item;
		this.index = index;
		this.onToggleSelect = onToggle;

		itemIcon.sprite = item.itemBaseData.icon;
		itemIcon.color = item.itemBaseData.GetTint(item.currentItemGrade);
		selectable = item.itemBaseData.canBeUpgraded && item.currentItemGrade != ItemGrade.Diamond;

		SetSelected(false);
	}
	private void SetSelected(bool value)
	{
		isSelected = value;
		selectedFrame.SetActive(isSelected);
	}
	public void ForceSelect(bool value)
	{
		SetSelected(value);
	}
}
