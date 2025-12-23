using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentInfoUIComponent : MonoBehaviour,	IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private Image equipmentIcon;
	private EquipableBaseData equipment;
	private EquipmentEffectInfo equipmentEffectInfo;

	private string effectDescription;
	private Color iconTint;
	public void OnPointerEnter(PointerEventData eventData)
	{
		Debug.Log($"Enter | hasData={equipment != null}, hasTooltip={equipmentEffectInfo != null}");
		if (equipment != null && equipmentEffectInfo != null)
		{
			Debug.Log("Show Info");
			equipmentEffectInfo.Open(equipment, effectDescription, iconTint);
		}
	}


	public void BindEquipmentEffectInfo(EquipmentEffectInfo e)
	{
		equipmentEffectInfo = e;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Debug.Log("Exit");
		if (equipmentEffectInfo!= null)
		{
			equipmentEffectInfo.Close();
		}
	}

	public void SetUpEquipmentInfo(EquipableBaseData equipment,Color iconColor,string description)
	{
		this.equipment = equipment;	
		this.effectDescription = description;
		if (equipment == null)
		{
			equipmentIcon.sprite = null;
			return;
		}
		equipmentIcon.sprite = equipment.icon;
		iconTint = iconColor;
		equipmentIcon.color = iconColor;
	}
}
