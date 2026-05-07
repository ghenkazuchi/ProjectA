using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChestItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	public ChestLootEntry entry;
	[SerializeField] Image itemIcon;
	[SerializeField] private Vector2 tooltipOffset = new Vector2(20f, -20f);

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

	private EquipmentContextMenu tooltip;
	private bool isHovering = false;

	public void BindTooltip(EquipmentContextMenu tooltipInfo)
	{
		tooltip = tooltipInfo;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (tooltip != null && entry.data != null && entry.data is EquipableBaseData equipable)
		{
			Color tint = Color.white;
			if (equipable is ItemBaseData itemData)
			{
				tint = itemData.GetTint(entry.grade);
			}
			tooltip.Open(equipable, "", tint);
			isHovering = true;
			UpdateTooltipPosition(eventData.position);
		}
	}

	private void Update()
	{
		if (isHovering && tooltip != null)
		{
			UpdateTooltipPosition(Input.mousePosition);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHovering = false;
		if (tooltip != null)
		{
			tooltip.Close();
		}
	}

	private void UpdateTooltipPosition(Vector2 mousePos)
	{
		RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
		if (tooltipRect != null)
		{
			Canvas canvas = tooltip.GetComponentInParent<Canvas>();
			if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				tooltipRect.position = mousePos + tooltipOffset; 
			}
			else if (canvas != null)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					canvas.transform as RectTransform,
					mousePos,
					canvas.worldCamera,
					out Vector2 localPoint);

				tooltipRect.localPosition = localPoint + tooltipOffset;
			}
		}
	}
}
