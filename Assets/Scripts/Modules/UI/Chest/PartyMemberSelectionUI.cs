using HaKien;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberSelectionUI : MonoBehaviour
{
	[SerializeField] private Transform memberButtonParent;
	[SerializeField] private Button partyMemberPrefabs;
	[SerializeField] private Button cancleButton;

	private List<Button> memberButtons = new List<Button>();
	private ChestLootEntry selectedEntry;

	private void Awake()
	{
		//cancleButton.onClick.AddListener(CancelSelection);
		gameObject.SetActive(false);
	}

	public void ShowPartySelection(ChestLootEntry entry)
	{
		selectedEntry = entry;
		CreateMemeberButton();
		gameObject.SetActive(true);
	}
	public void CreateMemeberButton()
	{
		ClearMemberButtons();
		List<PlayerCharacter> partyMembers = PlayerParty.Instance.GetAllPlayerCharacter();
		foreach(PlayerCharacter character in partyMembers)
		{
			Button button = Instantiate(partyMemberPrefabs, memberButtonParent);
			button.GetComponent<PartyMemberButtonUI>().SetupButton(character);
			PlayerCharacter memberRef = character;
			button.onClick.AddListener(() => OnMemberSelected(memberRef));
			memberButtons.Add(button);
		}
	}
	public void OnMemberSelected(PlayerCharacter selectedMember)
	{
		bool success = false; 
		if(selectedEntry.data is WeaponBaseData weaponData)
		{
			Weapon weapon = new Weapon(weaponData);
			success = PlayerParty.Instance.EquipWeaponToCharacter(selectedMember, weapon);
			if (success)
			{
				Debug.Log($"Successfully equipped {weaponData.itemName} to {selectedMember.entityData.EntityName}");
				CloseSelection();
				ChestOpenUIController.Instance.CloseChestUI();
			}
			else
			{
				Debug.Log($"Failed to equip {weaponData.itemName} to {selectedMember.entityData.EntityName}");
				ChestOpenUIController.Instance.ShowToast($"{selectedMember.entityData.EntityName} can't use {weaponData.itemName}");
			}
		}
		else
		{
			Item item = new Item(selectedEntry.data as ItemBaseData, selectedEntry.grade);
			success = PlayerParty.Instance.AddItemToCharacter(selectedMember, item);

			if (success)
			{
				Debug.Log($"Successfully added {selectedEntry.data.itemName} to {selectedMember.entityData.EntityName}");
				CloseSelection();
				ChestOpenUIController.Instance.CloseChestUI();
			}
			else
			{
				CloseSelection();
				MessageManager.Instance.SendMessage(new Message(MessageType.OnInventoryOpen, new object[] { selectedMember, selectedEntry}));
			}
		}
	}

	public void ClearMemberButtons()
	{
		foreach(Button button in memberButtons)
		{
			Destroy(button.gameObject);	
		}
		memberButtons.Clear();
	}
	private void CloseSelection()
	{
		ClearMemberButtons();
		gameObject.SetActive(false);
	}
}
