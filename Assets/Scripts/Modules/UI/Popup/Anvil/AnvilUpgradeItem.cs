using HaKien;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AnvilUpgradeItem : MonoBehaviour
{
	[SerializeField] private Image item1Icon;
	[SerializeField] private Image item2Icon;
	[SerializeField] private Button closeButton;
	[SerializeField] private Button combineButton;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private Item item1;
	[SerializeField] private Item item2;
	private int index1 = -1;
	private int index2 = -1;

	private PlayerCharacter currentCharacter;
	//private System.Action onCombined;
	private System.Action<Item> onCombined;

	private void Awake()
	{
		Close();
		closeButton.onClick.AddListener(() => CloseAnvilPopup());
		combineButton.onClick.AddListener(() => Combine());
	}

	public void Init(PlayerCharacter pc, System.Action<Item> onCombined)
	{
		currentCharacter = pc;
		this.onCombined = onCombined;
		ClearSlots();
		UpdateUI();
	}

	public void Show()
	{
		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = true;
		UpdateUI();
	}
	public void Hide()
	{
		canvasGroup.alpha = 0;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
	}
	public void Close()
	{
		Hide();
		currentCharacter = null;
		ClearSlots();
		UpdateUI();
	}

	public void CloseAnvilPopup()
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnAnvilPopupClose));
	}
	public bool TrySelect(int inventoryIndex,Item item)
	{
		if (item == null || item.itemBaseData == null) return false;
		if(!item.itemBaseData.canBeUpgraded) return false;
		if (item.currentItemGrade == ItemGrade.Diamond) return false;

		if(item1 == null)
		{
			item1 = item;
			index1 = inventoryIndex;
			UpdateUI();
			return true;
		}
		if(item2 == null)
		{
			if (!IsSameTypeAndGrade(item1, item)) return false;

			item2 = item;
			index2 = inventoryIndex;
			UpdateUI();
			return true;
		}

		return false;
	}

	public void RemoveSelection(int inventoryIndex)
	{
		if (index1 == inventoryIndex)
		{
			item1 = null;
			index1 = -1;
			UpdateUI();
			return;
		}
		if (index2 == inventoryIndex)
		{
			item2 = null;
			index2 = -1;
			UpdateUI();
			return;
		}
	}

	private void ClearSlots()
	{
		item1 = null;
		item2 = null;
		index1 = -1;
		index2 = -1;
	}
	private void UpdateUI()
	{
		// icon + tint
		if (item1Icon != null)
		{
			if (item1 != null)
			{
				item1Icon.sprite = item1.itemBaseData.icon;
				item1Icon.color = item1.itemBaseData.GetTint(item1.currentItemGrade);
				item1Icon.enabled = true;
			}
			else
			{
				item1Icon.sprite = null;
				item1Icon.enabled = false;
			}
		}

		if (item2Icon != null)
		{
			if (item2 != null)
			{
				item2Icon.sprite = item2.itemBaseData.icon;
				item2Icon.color = item2.itemBaseData.GetTint(item2.currentItemGrade);
				item2Icon.enabled = true;
			}
			else
			{
				item2Icon.sprite = null;
				item2Icon.enabled = false;
			}
		}

		if (combineButton != null)
			combineButton.interactable = CanCombine();
	}

	private bool IsSameTypeAndGrade(Item a,Item b)
	{
		if(a == null || b == null) return false;
		return a.itemBaseData == b.itemBaseData && a.currentItemGrade == b.currentItemGrade;
	}
	private bool CanCombine()
	{
		if(item1 == null || item2 == null) return false;
		if(!IsSameTypeAndGrade(item1, item2)) return false;
		if (!item1.itemBaseData.canBeUpgraded) return false;
		if (item1.currentItemGrade == ItemGrade.Diamond) return false;
		return true;
	}

	private ItemGrade GetUpgradedGrade(ItemGrade g)
	{
		switch(g){
			case ItemGrade.Normal: return ItemGrade.Gold;
			case ItemGrade.Gold: return ItemGrade.Diamond;
			default: return g;
		}
	}
	private void Combine()
	{
		if(!CanCombine() || currentCharacter == null) return;	
		ItemGrade upgradedGrade = GetUpgradedGrade(item1.currentItemGrade);

		int first = Mathf.Max(index1, index2);
		int second = Mathf.Min(index1, index2);

		var removedA = currentCharacter.RemoveItemAtSlot(first);
		var removedB = currentCharacter.RemoveItemAtSlot(second);

		var newItem = new Item(item1.itemBaseData, upgradedGrade);
		bool added = currentCharacter.TryAddItem(newItem);
		if (added)
		{
			Debug.Log("SuccessFully combined items to " + upgradedGrade);
		}
		else
		{
			Debug.LogWarning("Failed to add combined item to inventory, returning old items");
		}
		ClearSlots();
		UpdateUI();
		onCombined?.Invoke(added ? newItem : null);
	}
}
