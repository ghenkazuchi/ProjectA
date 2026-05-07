using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipmentContextMenu : MonoBehaviour
{
	[SerializeReference] private EquipableBaseData equipmentData;
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] TextMeshProUGUI equipmentNameText;
	[SerializeField] TextMeshProUGUI equipmentTypeText; 
	[SerializeField] TextMeshProUGUI equipmentEffectDescriptionText;
	[SerializeField] private List<TextMeshProUGUI> equipmentStatBonusesText;

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
		if (equipmentNameText != null) equipmentNameText.text = "";
		if (equipmentTypeText != null) equipmentTypeText.text = "";
		if (equipmentEffectDescriptionText != null) equipmentEffectDescriptionText.text = "";
	}

	public void Open(EquipableBaseData equipment, string overrideDescription, Color titleTint)
	{
		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = true;
		equipmentData = equipment;

		if (equipmentData != null)
		{
			if (equipmentNameText != null)
			{
				equipmentNameText.text = equipmentData.itemName;
				equipmentNameText.color = titleTint;
			}

			if (equipmentTypeText != null)
			{
				if (equipmentData is WeaponBaseData weaponData)
				{
					string handReq = weaponData.requirement == WeaponRequirement.OneHanded ? "1 Handed" : "2 Handed";
					equipmentTypeText.text = $"{weaponData.weaponType} ({handReq})";
				}
				else if (equipmentData is ItemBaseData)
				{
					equipmentTypeText.text = "Item";
				}
				else
				{
					equipmentTypeText.text = "Equipment";
				}
			}

			if (equipmentStatBonusesText != null)
			{
				for (int i = 0; i < equipmentStatBonusesText.Count; i++)
				{
					if (equipment.EquipableStatBonus != null && i < equipment.EquipableStatBonus.Count)
					{
						var b = equipment.EquipableStatBonus[i];
						var sign = b.ModType == ModType.Percentage ? "%" : "";
						equipmentStatBonusesText[i].text = $"{b.Stat}: {b.value}{sign}";
						equipmentStatBonusesText[i].gameObject.SetActive(true);
					}
					else
					{
						equipmentStatBonusesText[i].gameObject.SetActive(false);
					}
				}
			}

			if (equipmentEffectDescriptionText != null)
			{
				equipmentEffectDescriptionText.text = string.IsNullOrEmpty(overrideDescription)
					? equipmentData.description
					: overrideDescription;
			}
		}
		else
		{
			if (equipmentNameText != null)
			{
				equipmentNameText.text = "";
				equipmentNameText.color = Color.white;
			}
			if (equipmentTypeText != null) equipmentTypeText.text = "";
			if (equipmentEffectDescriptionText != null) equipmentEffectDescriptionText.text = "";
		}
	}
}
