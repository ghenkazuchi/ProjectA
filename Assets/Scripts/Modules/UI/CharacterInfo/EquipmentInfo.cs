using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

public class EquipmentInfo : MonoBehaviour
{
	[SerializeField] Transform equipmentContainer;
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] PlayerCharacter currentCharacter;
	[SerializeField] Button closeButton;
	[SerializeField] EquipmentInfoUIComponent equipmentInfoPrefab;
	[SerializeField] EquipmentEffectInfo equipmentEffectInfo;
	

	private void Awake()
	{
		Close();
		closeButton.onClick.AddListener(() => Close());
	}

	public void Show(PlayerCharacter character)
	{
		currentCharacter = character;
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		SetUpCharacterEquipmentSlot();

	}

	public void SetUpCharacterEquipmentSlot()
	{
		ClearContainer(equipmentContainer);
		List<EquipableBaseData> equipments = new List<EquipableBaseData>();
		var tints = new List<Color>();
		var descs = new List<string>();
		int equipmentCount = currentCharacter.equipmentSlotCount;
		foreach(var item in currentCharacter.items)
		{
			if(item ==null ) continue;
			var data = item.itemBaseData;
			equipments.Add(data);
			var ib = data as ItemBaseData;
			if(ib != null)
			{
				tints.Add(ib.GetTint(item.currentItemGrade));
				descs.Add(ib.GetEffectDescriptionByGrade(item.currentItemGrade));
			}
		}
		if(currentCharacter.weapon != null)
		{
			var wbd = currentCharacter.weapon.WeaponBaseData;
			equipments.Add(wbd);
			tints.Add(Color.white);
			descs.Add(wbd.description);
		}
		for (int i = 0; i < equipmentCount; i++)
		{
			GameObject equipmentObj = Instantiate(equipmentInfoPrefab.gameObject, equipmentContainer);
			EquipmentInfoUIComponent equipmentInfo = equipmentObj.GetComponent<EquipmentInfoUIComponent>();
			equipmentInfo.BindEquipmentEffectInfo(equipmentEffectInfo);
			if (i < equipments.Count)
			{
				equipmentInfo.SetUpEquipmentInfo(equipments[i], tints[i], descs[i]);
			}
			else
			{
				equipmentInfo.SetUpEquipmentInfo(null,Color.white,null);
			}
		}
	}
	public void Close()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		currentCharacter = null;
		ClearContainer(equipmentContainer);
		if (equipmentEffectInfo)
			equipmentEffectInfo.Close();
	}

	private void ClearContainer(Transform container)
	{
		foreach (Transform child in container)
		{
			Destroy(child.gameObject);
		}
	}
}
