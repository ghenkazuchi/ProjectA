using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipmentEffectInfo : MonoBehaviour
{
	[SerializeReference] private EquipableBaseData equipmentData;
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] TextMeshProUGUI equipmentNameText;
	[SerializeField] TextMeshProUGUI equipmentEffectDescriptionText;

	private void Awake()
	{
		Close();
	}

	public void Close()
	{
		canvasGroup.alpha = 0;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
		equipmentData = null;
		equipmentNameText.text = "";
		equipmentEffectDescriptionText.text = "";

	}
	public void Open(EquipableBaseData equipment)
	{
		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = true;	
		canvasGroup.interactable = true;
		equipmentData = equipment;
		if (equipmentData != null)
		{
			equipmentNameText.text = equipmentData.itemName;
			equipmentEffectDescriptionText.text = equipmentData.description;
		}
		else
		{
			equipmentNameText.text = "";
			equipmentEffectDescriptionText.text = "";
		}
	}
	public void Open(EquipableBaseData equipment, string overrideDescription, Color titleTint)
	{
		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = true;
		equipmentData = equipment;

		if (equipmentData != null)
		{
			equipmentNameText.text = equipmentData.itemName;
			equipmentEffectDescriptionText.text = string.IsNullOrEmpty(overrideDescription)
				? equipmentData.description
				: overrideDescription;
		}
		else
		{
			equipmentNameText.text = "";
			equipmentEffectDescriptionText.text = "";
			equipmentNameText.color = Color.white;
		}
	}
}
