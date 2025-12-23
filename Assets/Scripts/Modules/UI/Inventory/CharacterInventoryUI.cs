using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CharacterInventoryUI : MonoBehaviour, IMessageHandle
{
	[SerializeField] InventoryItemSlot[] itemSlots;
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] PlayerCharacter currentPlayerCharacter;
	[SerializeField] InventoryWeaponSlot weaponSlot;
	[SerializeField] ChestLootEntry itemToReplaced;

	private void Awake()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		currentPlayerCharacter = null;
		itemToReplaced = default;
	}
	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnInventoryOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnInventoryItemSelected, this);
	}

	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnInventoryOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnInventoryItemSelected, this);
	}
	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnInventoryOpen:
				if (message.data != null && message.data.Length >= 2 && message.data[0] is PlayerCharacter playerCharacter && message.data[1] is ChestLootEntry selectedItem)
				{
					itemToReplaced = selectedItem;
					OpenCharacterInventory(playerCharacter);
				}
				break;
			case MessageType.OnInventoryItemSelected:
				if (message.data != null && message.data.Length >= 2 && message.data[0] is ItemBaseData itemToReplace && message.data[1] is int index)
				{
					Debug.Log("Clicked");
					currentPlayerCharacter.RemoveItemAtSlot(index);
					var itemToAdd = new Item(itemToReplaced.data as ItemBaseData, this.itemToReplaced.grade);
					currentPlayerCharacter.TryAddItem(itemToAdd);
					Debug.Log($"Replaced item at slot {index} with {itemToReplaced.data.itemName}.");
					CloseCharacterInventory();
					ChestOpenUIController.Instance.CloseChestUI();
				}
				break;
		}
	}



	public void SetUp(PlayerCharacter playerCharacter)
	{
		currentPlayerCharacter = playerCharacter;
		foreach (var slot in itemSlots)
		{
			slot.gameObject.SetActive(false);
		}

		for (int i = 0; i < currentPlayerCharacter.items.Count; i++)
		{
			if (currentPlayerCharacter.items[i] != null)
			{
				if (i < itemSlots.Length)
				{
					itemSlots[i].SetUp(currentPlayerCharacter.items[i], i);
				}
			}
		}
		var weapon = currentPlayerCharacter.weapon;
		if (weapon != null && weapon.WeaponBaseData != null)
		{
			Debug.Log(weapon.WeaponBaseData.name);
			weaponSlot.SetUp(currentPlayerCharacter.weapon);
		}
		else
		{
			weaponSlot.gameObject.SetActive(false);
		}

	}
	public void OpenCharacterInventory(PlayerCharacter playerCharacter)
	{
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		//gameObject.SetActive(true);
		SetUp(playerCharacter);
	}

	public void CloseCharacterInventory()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		//gameObject.SetActive(false);
		currentPlayerCharacter = null;
		itemToReplaced = default;
	}
}