using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPlayerCharacterSelectionController : MonoBehaviour
{
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private Transform partyListTransform;
	[SerializeField] private Button partyMemberPrefabs;
	[SerializeField] private Button returnButton;
	[SerializeField] private ShopCharacterEquipmentReplaceUI replaceUI;
	private List<Button> memberButtons = new List<Button>();
	private EquipableBaseData currentSelectedEquipable;

	private void Awake()
	{
		Hide();
		returnButton.onClick.AddListener(OnReturnButtonClicked);
	}
	public void Hide()
	{
		canvasGroup.alpha = 0;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
	}

	public void Show(EquipableBaseData equipable)
	{
		currentSelectedEquipable = equipable;
		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.interactable = true;
		CreateMemberButtons();
	}

	private void CreateMemberButtons()
	{
		ClearMemberButton();
		List<PlayerCharacter> partyMembers = PlayerParty.Instance.GetAllPlayerCharacter();
		foreach(PlayerCharacter character in partyMembers)
		{
			Debug.Log("Create button for " + character.entityData.EntityName);
			Button button = Instantiate(partyMemberPrefabs, partyListTransform);
			button.GetComponent<ShopPlayerCharacterUIButton>().Setup(character);
			memberButtons.Add(button);

			var capturedCharacter = character;
			button.onClick.AddListener(() => OnCharacterChosen(capturedCharacter));
		}
	}

	private void ClearMemberButton()
	{
		foreach (Button button in memberButtons)
		{
			Destroy(button.gameObject);
		}
		memberButtons.Clear();
	}
	public void OnReturnButtonClicked()
	{
		Hide();
		//MessageManager.Instance.SendMessage(new Message(MessageType.OnShopReturnToItemList));
	}

	private void OnCharacterChosen(PlayerCharacter character)
	{
		if (currentSelectedEquipable == null) return;

		if (currentSelectedEquipable is WeaponBaseData wb)
		{
			if (!character.GetClassData.usableWeaponTypes.Contains(wb.weaponType))
			{
				Debug.Log("Character cannot use this weapon");
				return;
			}
		}
		int freeSlots = character.GetFreeSlots();
		int requiredSlots = character.GetSlotCostForEquipable(currentSelectedEquipable);
		int needtoFree = Mathf.Max(0,requiredSlots - freeSlots);
		bool isBuyingWeapon = currentSelectedEquipable is WeaponBaseData;
		bool hasEquipWeapon = character.weapon != null && character.weapon.WeaponBaseData != null;
		if(needtoFree > 0 || (isBuyingWeapon && hasEquipWeapon))
		{
			Hide();
			SendPurchaseRequest_ReplacingItems(character, needtoFree);
		}
		else
		{
			Hide();
			SendPurchaseRequest_NoReplace(character);
		}
	}
	
	private void SendPurchaseRequest_NoReplace(PlayerCharacter character)
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnShopCharacterSelected,new object[] {character,currentSelectedEquipable}));
	}
	private void SendPurchaseRequest_ReplacingItems(PlayerCharacter character,int requireSlot)
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnShopCharacterReplaceSelectionConfirmed,new object[] {character,currentSelectedEquipable,requireSlot }));
	}
}
