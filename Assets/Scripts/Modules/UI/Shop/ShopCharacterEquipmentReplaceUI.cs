using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCharacterEquipmentReplaceUI : MonoBehaviour
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private Transform itemsTransform;
	[SerializeField] private ShopEquipmentSlotUI slotPrefab;
	[SerializeField] private TextMeshProUGUI requireSlotText;
	[SerializeField] private Button confirmButton;
	[SerializeField] private Button cancelButton;

	private PlayerCharacter currentCharacter;
	private EquipableBaseData pendingEquipable;
	private int requiredSlots;
	private bool mustRemoveWeapon;
	private bool removeWeapon = false;
	private int weaponSlotCost = 0;
	private List<int> selectedItemIndices = new List<int>();
	private Dictionary<int, int> itemIndexToSlotCost = new Dictionary<int, int>();

	private void Awake()
	{
		Hide();
		confirmButton.onClick.AddListener(OnConfirmClicked);
		cancelButton.onClick.AddListener(Hide);
	}
	public void Hide()
	{
		canvasGroup.alpha = 0;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		currentCharacter = null;
		pendingEquipable = null;
		foreach (Transform child in itemsTransform)
		{
			Destroy(child.gameObject);
		}
	}

	public void Show(PlayerCharacter character, EquipableBaseData equipable, int requiredSlots)
	{
		this.currentCharacter = character;
		this.pendingEquipable = equipable;
		this.requiredSlots = requiredSlots;

		mustRemoveWeapon = (pendingEquipable is WeaponBaseData) && currentCharacter.weapon != null && currentCharacter.weapon.WeaponBaseData != null; 


		removeWeapon = false;
		weaponSlotCost = 0;
		selectedItemIndices.Clear();
		itemIndexToSlotCost.Clear();

		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;

		confirmButton.interactable = false;
		RebuildSlots();
		RefreshRequirementUI();
	}
	private void RebuildSlots()
	{
		foreach (Transform child in itemsTransform)
		{
			Destroy(child.gameObject);
		}

		if (currentCharacter == null) return;

		int currentFree = currentCharacter.GetFreeSlots();

		if (currentCharacter.weapon != null && currentCharacter.weapon.WeaponBaseData != null)
		{
			int cost = currentCharacter.GetSlotCostForEquipable(currentCharacter.weapon.WeaponBaseData);
			var slot = Instantiate(slotPrefab, itemsTransform);
			slot.SetUpWeapon(currentCharacter.weapon, cost, isOn =>
			{
				removeWeapon = isOn;
				weaponSlotCost = isOn ? cost : 0;
				RefreshRequirementUI();
			});
		}

		// Items
		for (int i = 0; i < currentCharacter.items.Count; i++)
		{
			var item = currentCharacter.items[i];
			if (item == null || item.itemBaseData == null) continue;

			int cost = 1;
			itemIndexToSlotCost[i] = cost;

			var slot = Instantiate(slotPrefab, itemsTransform);
			int capturedIndex = i;
			slot.SetupItem(item, cost, isOn =>
			{
				if (isOn)
				{
					if (!selectedItemIndices.Contains(capturedIndex))
						selectedItemIndices.Add(capturedIndex);
				}
				else
				{
					selectedItemIndices.Remove(capturedIndex);
				}
				RefreshRequirementUI();
			});
		}
	}
	private void OnConfirmClicked()
	{
		if (currentCharacter == null || pendingEquipable == null)
		{
			Hide();
			return;
		}
		int freed = GetFreedslots();
		if (freed < requiredSlots)
		{
			return;
		}
		if(mustRemoveWeapon  && !removeWeapon)
		{
			return;
		}

		var selection = new ShopReplaceSelection
		{
			target = currentCharacter,
			newEquip = pendingEquipable,
			removeWeapon = removeWeapon,
			removeItemIndices = new List<int>(selectedItemIndices)
		};
		MessageManager.Instance.SendMessage(new Message(MessageType.OnSelectedEquipableConclude, new object[] { selection }));

		Hide();
	}

	private int GetFreedslots()
	{
		int freed = weaponSlotCost;
		foreach (var idx in selectedItemIndices)
		{
			if (itemIndexToSlotCost.TryGetValue(idx, out int c)) freed += c;
			else freed += 1;
		}
		return freed;
	}
	private void RefreshRequirementUI()
	{
		int freed = GetFreedslots();
		int stillNeeded = requiredSlots - freed;
		if (requireSlotText != null)
		{
			requireSlotText.text = $"seleted: {freed} | remaining: {stillNeeded}";
			if (confirmButton != null)
			{
				confirmButton.interactable = freed >= requiredSlots;
			}
		}
	}
}
