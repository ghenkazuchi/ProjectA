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
		equipmentNameText.text = "";
		equipmentEffectDescriptionText.text = "";

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
