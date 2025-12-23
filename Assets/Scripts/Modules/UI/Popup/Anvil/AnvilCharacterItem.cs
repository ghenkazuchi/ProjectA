using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using HaKien;

public class AnvilCharacterItem : MonoBehaviour
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private PlayerCharacter currentCharacter;
	[SerializeField] private Transform characterEquipmentContainer;
	[SerializeField] private AnvilItemUIComponent anvilItemPrefab;
	[SerializeField] private TextMeshProUGUI characterNameText;
	[SerializeField] private Button returnButton;
	private Action<int, Item, bool> onToggleSelect;

	private Dictionary<int, AnvilItemUIComponent> uiByIndex = new Dictionary<int, AnvilItemUIComponent>();

	private void Awake()
	{
		Close();
		returnButton.onClick.AddListener(() => ReturnCharacterSelection());
	}

	public void Show(PlayerCharacter character, Action<int, Item, bool> onItemToggle)
	{
		currentCharacter = character;
		onToggleSelect = onItemToggle;
		characterNameText.text = character.entityData.EntityName;
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		SetUpCharacterItemSlot();
	}

	public void ReturnCharacterSelection()
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnSelectCharacterReturn));
	}

	public void Refresh()
	{
		if (currentCharacter == null) return;
		ClearContainer(characterEquipmentContainer);
		SetUpCharacterItemSlot();
	}
	private void SetUpCharacterItemSlot()
	{
		ClearContainer(characterEquipmentContainer);
		if (currentCharacter == null) return;
		for (int i = 0; i < currentCharacter.items.Count; i++)
		{
			var item = currentCharacter.items[i];
			if (item == null) continue;
			var itemUI = Instantiate(anvilItemPrefab, characterEquipmentContainer);
			itemUI.SetUp(item, i, onToggleSelect);
			uiByIndex[i] = itemUI;
		}
	}

	public void Close()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		ClearContainer(characterEquipmentContainer);
		currentCharacter = null;
	}

	private void ClearContainer(Transform container)
	{
		foreach (Transform child in container)
		{
			Destroy(child.gameObject);
		}
	}
	public void SetItemSelectedVisual(int index, bool value)
	{
		if (uiByIndex.TryGetValue(index, out var itemUI))
		{
			itemUI.ForceSelect(value);
		}
	}
}
