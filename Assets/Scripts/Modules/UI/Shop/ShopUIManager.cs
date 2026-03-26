using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour, IMessageHandle
{
	[SerializeField] private ShopEquipableDetailUI equipableDetailUI;
	[SerializeField] private ShopItemListUIController itemListUIController;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private ShopPlayerCharacterSelectionController playerCharacterSelectionController;
	[SerializeField] private ShopCharacterEquipmentReplaceUI characterInventoryUI;
	[SerializeField] private Button closeButton;
	private List<EquipableBaseData> currentEquipableStock;
	private ShopKeeperInteractableObject currentInteractShopKeeper;
	[SerializeField] private ToastUI shopToast;
	public void Awake()
	{
		Hide();
		equipableDetailUI.OnBuyButtonClicked += OnBuyClicked;
		equipableDetailUI.OnRerollClicked += OnRerollClicked;
		closeButton.onClick.AddListener(Hide);
	}
	public void Hide()
	{
		canvasGroup.alpha = 0;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
		MessageManager.Instance.SendMessage(new Message(MessageType.OnInteractEnd));
	}
	public void Show()
	{
		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = true;
	}
	private void OnEnable()
	{

		MessageManager.Instance.AddSubcriber(MessageType.OnShopItemSelected, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopClose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopCharacterSelectionOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopCharacterInventoryClose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopUpdated, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopItemSelected, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopCharacterSelected, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopCharacterReplaceSelectionConfirmed, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnSelectedEquipableConclude,this);
	}
	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopItemSelected, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopCharacterSelectionOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopCharacterInventoryClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopUpdated, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopItemSelected, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopCharacterSelected, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopCharacterReplaceSelectionConfirmed, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnSelectedEquipableConclude, this);

	}
	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnShopOpen:
				currentEquipableStock = (List<EquipableBaseData>)message.data[0];
				currentInteractShopKeeper = (ShopKeeperInteractableObject)message.data[1];
				Show();
				itemListUIController.Show();
				itemListUIController.InitShopItemList(currentEquipableStock);
				if (currentEquipableStock.Count > 0)
				{
					equipableDetailUI.SetUp(currentEquipableStock[0]);
				}
				break;
			case MessageType.OnShopClose:
				Hide();
				itemListUIController.Hide();
				currentEquipableStock = null;
				break;
			case MessageType.OnShopItemSelected:
				EquipableBaseData equipableBaseData = (EquipableBaseData)message.data[0];
				equipableDetailUI.SetUp(equipableBaseData);
				break;
			case MessageType.OnShopUpdated:
				currentEquipableStock = (List<EquipableBaseData>)message.data[0];
				itemListUIController.InitShopItemList(currentEquipableStock);
				if (currentEquipableStock.Count > 0)
				{
					equipableDetailUI.SetUp(currentEquipableStock[0]);
				}
				break;
			case MessageType.OnShopCharacterSelectionOpen:
				EquipableBaseData selectedEquipable = (EquipableBaseData)message.data[0];
				playerCharacterSelectionController.Show(selectedEquipable);
				break;
			case MessageType.OnShopCharacterSelected:
				var equipable = (EquipableBaseData)message.data[1];
				var price = equipable.basePrice;
				var selection = new ShopReplaceSelection
				{
					target = (PlayerCharacter)message.data[0],
					newEquip = equipable,
					removeWeapon = false,
					removeItemIndices = null
				};
				bool successPure = currentInteractShopKeeper.TryPurchase(selection,price);
				if (!successPure && selection.target != null && selection.newEquip != null)
				{
					if (!DataManager.Instance.Currency.HasEnough(CurrencyType.Gold, price))
					{
						ShowToastMessage("Not enough gold!");
					}
					else
					{
						ShowToastMessage($"{selection.target.entityData.EntityName} can't use {selection.newEquip.itemName}");
					}
				}
				break;
			case MessageType.OnShopCharacterReplaceSelectionConfirmed:
				var selectedCharacter = (PlayerCharacter)message.data[0];
				var newEquip = (EquipableBaseData)message.data[1];
				var requireSlot = (int)message.data[2];
				characterInventoryUI.Show(selectedCharacter,newEquip,requireSlot);
				break;
			case MessageType.OnSelectedEquipableConclude:
				var concludedEquipable = (ShopReplaceSelection)message.data[0];
				var equipablePrice = concludedEquipable.newEquip.basePrice;
				bool successReplace = currentInteractShopKeeper.TryPurchase(concludedEquipable, equipablePrice);
				if (!successReplace && concludedEquipable.target != null && concludedEquipable.newEquip != null)
				{
					if (!DataManager.Instance.Currency.HasEnough(CurrencyType.Gold, equipablePrice))
					{
						ShowToastMessage("Not enough gold!");
					}
					else
					{
						ShowToastMessage($"{concludedEquipable.target.entityData.EntityName} can't use {concludedEquipable.newEquip.itemName}");
					}
				}
				break;
		}
	}
	private void OnBuyClicked(EquipableBaseData data)
	{

		int price = data.basePrice;

		var currency = DataManager.Instance.Currency;

		if (!currency.HasEnough(CurrencyType.Gold, price))
		{
			ShowToastMessage("Not enough gold!");
			return;
		}

		playerCharacterSelectionController.Show(data);
	}
	private void OnRerollClicked()
	{
		currentInteractShopKeeper.RestockEquipables();
	}
	public void ShowToastMessage(string message)
	{
		if (shopToast != null)
		{
			shopToast.ShowToast(message);
		}
	}
}
