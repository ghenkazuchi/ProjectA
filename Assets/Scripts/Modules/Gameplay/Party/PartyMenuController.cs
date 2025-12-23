using HaKien;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMenuController : Singleton<PartyMenuController>, IMessageHandle
{
	[SerializeField] PlayerParty playerParty;
	[SerializeField] List<PartyMemberInfo> partySlotUIs;
	[SerializeField] Button closeButton;
	[SerializeField] Transform mainContainer;
	[SerializeField] private CanvasGroup canvasGroup;

	private readonly Dictionary<GridPosition, int> positionToPartyMemberDisplay = new Dictionary<GridPosition, int>
	{
		{ new GridPosition(0, 0), 0 },
		{ new GridPosition(1, 0), 3 },
		{ new GridPosition(1, 2), 5 },
		{ new GridPosition(0, 1), 1 },
		{ new GridPosition(1, 1), 4 },
		{ new GridPosition(0, 2), 2 },
	};

	private void Start()
	{

		InitializeUI();
		SetUpListeners();
	}

	private void InitializeUI()
	{

		for (int i = 0; i < partySlotUIs.Count; i++)
		{
			if (partySlotUIs[i] != null)
			{
				GridPosition correspondingPosition = GetGridPositionForUIIndex(i);
				Debug.Log($"Initializing slot {i} at position ({correspondingPosition.x},{correspondingPosition.y})");
				partySlotUIs[i].Initialize(correspondingPosition, this);
			}
			else
			{
				Debug.LogWarning($"PartySlotUI at index {i} is null!");
			}
		}

		RefreshPartyDisplay();
	}

	private GridPosition GetGridPositionForUIIndex(int uiIndex)
	{
		foreach (var kvp in positionToPartyMemberDisplay)
		{
			if (kvp.Value == uiIndex)
			{
				return kvp.Key;
			}
		}
		Debug.LogWarning($"No grid position found for UI index {uiIndex}, using default (0,0)");
		return new GridPosition(0, 0);
	}

	private void SetUpListeners()
	{
		if (closeButton != null)
			closeButton.onClick.AddListener(ClosePartyMenu);
	}

	public void RefreshPartyDisplay()
	{
		for (int i = 0; i < partySlotUIs.Count; i++)
		{
			if (partySlotUIs[i] != null)
			{
				partySlotUIs[i].SetCharacterData(null);
			}
		}

		foreach (var partySlot in playerParty.partySlots)
		{
			GridPosition pos = partySlot.position;
			EntityBase character = partySlot.entity;

			if (positionToPartyMemberDisplay.TryGetValue(pos, out int partySlotIndex))
			{
				if (partySlotIndex >= 0 && partySlotIndex < partySlotUIs.Count)
				{
					if (partySlotUIs[partySlotIndex] != null)
					{
						Debug.Log($"Setting character {character?.entityData.EntityName} at UI slot {partySlotIndex}");
						partySlotUIs[partySlotIndex].SetCharacterData(character);
					}
				}
			}
		}

		foreach (var ui in partySlotUIs)
		{
			if (ui != null)
			{
				ui.RefreshDisplay();
			}
		}
	}

	public void SwapPartyMembers(GridPosition position1, GridPosition position2)
	{
		Debug.Log($"=== SwapPartyMembers Called ===");
		Debug.Log($"Swapping positions ({position1.x},{position1.y}) and ({position2.x},{position2.y})");

		EntityBase entity1 = playerParty.GetEntityAtPosition(position1);
		EntityBase entity2 = playerParty.GetEntityAtPosition(position2);

		Debug.Log($"Entity1: {entity1?.entityData.EntityName}");
		Debug.Log($"Entity2: {entity2?.entityData.EntityName}");

		if (entity1 == null && entity2 == null)
		{
			Debug.Log("Both positions are empty, no swap needed");
			return;
		}

		// Remove both entities
		if (entity1 != null)
		{
			Debug.Log($"Removing {entity1.entityData.EntityName} from position ({position1.x},{position1.y})");
			playerParty.RemovePartyMember(entity1);
		}
		if (entity2 != null)
		{
			Debug.Log($"Removing {entity2.entityData.EntityName} from position ({position2.x},{position2.y})");
			playerParty.RemovePartyMember(entity2);
		}

		// Add them back in swapped positions
		if (entity1 != null)
		{
			Debug.Log($"Adding {entity1.entityData.EntityName} to position ({position2.x},{position2.y})");
			playerParty.AddPartyMember(entity1, position2);
		}
		if (entity2 != null)
		{
			Debug.Log($"Adding {entity2.entityData.EntityName} to position ({position1.x},{position1.y})");
			playerParty.AddPartyMember(entity2, position1);
		}

		RefreshPartyDisplay();
		Debug.Log($"Swap completed!");
	}

	public void ClosePartyMenu()
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnPartyMenuClose));
	}
	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnPartyMemberInfoUpdate, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnPartyMemberInfoClose, this);
	}
	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPartyMemberInfoUpdate, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPartyMemberInfoClose, this);
	}

	private void Hide()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

	}
	private void Show()
	{
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;

	}
	public void Handle(Message message)
	{
		switch(message.type)
		{
			case MessageType.OnPartyMemberInfoUpdate:
				Hide();
			break;
			case MessageType.OnPartyMemberInfoClose:

				Show();

				break;
		}
	}
}