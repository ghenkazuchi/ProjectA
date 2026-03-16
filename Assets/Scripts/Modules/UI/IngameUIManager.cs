using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameUIManager : Singleton<IngameUIManager>, IMessageHandle
{
	[SerializeField] CanvasGroup characterCreationCanvasGroup;
	[SerializeField] CanvasGroup battleCanvasGroup;
	[SerializeField] CanvasGroup recruitCharacterCanvasGroup;
	[SerializeField] CanvasGroup gameLoseUICanvasGroup;
	[SerializeField] CanvasGroup partyMenuCanvasGroup;
	[SerializeField] private OverWorldUI overworldUIController;
	[SerializeField] private RecruitUIController recruitUIController;
	[SerializeField] private PartyMenuController partyMenuController;

	private void Awake()
	{
		characterCreationCanvasGroup.alpha = 1f;
		characterCreationCanvasGroup.interactable = true;
		characterCreationCanvasGroup.blocksRaycasts = true;
		
		battleCanvasGroup.alpha = 0f;
		
		recruitCharacterCanvasGroup.alpha = 0f;
		recruitCharacterCanvasGroup.interactable = false;
		recruitCharacterCanvasGroup.blocksRaycasts = false;
		
		partyMenuCanvasGroup.alpha = 0f;
		partyMenuCanvasGroup.blocksRaycasts = false;
		partyMenuCanvasGroup.interactable = false;
	}

	private void OnEnable()
	{
		MessageManager.Instance.AddSubcriber(MessageType.OnGameStart, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnBattleStart, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnRecruitEnter, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnRecruitCharacter, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnRecruitCharacterUIClose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnPartyMenuOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnPartyMenuClose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnGameLose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnBattleOver, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnTargetSelection, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnInteract, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnInteractEnd, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnChestClose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnShopClose, this);
	}
	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnGameStart, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnBattleStart, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnRecruitEnter, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnRecruitCharacter, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnRecruitCharacterUIClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPartyMenuOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPartyMenuClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnGameLose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnBattleOver, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnTargetSelection, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnInteract, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnInteractEnd, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnChestClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnShopClose, this);
	}
	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnTargetSelection:
				break;
			case MessageType.OnGameStart:
				overworldUIController.Show();
				break;
			case MessageType.OnInteract:
				Interacable interactable = message.data != null && message.data.Length > 0 ? message.data[0] as Interacable : null;
				if (ShouldKeepOverworldVisible(interactable))
				{
					overworldUIController.Show();
				}
				else
				{
					overworldUIController.Hide();
				}
				break;
			case MessageType.OnInteractEnd:
			case MessageType.OnChestClose:
			case MessageType.OnShopClose:
				overworldUIController.Show();
				break;
			case MessageType.OnBattleStart:
				overworldUIController.Hide();
				battleCanvasGroup.alpha = 1f;
				break;
			case MessageType.OnRecruitEnter:
				recruitCharacterCanvasGroup.alpha = 1f;
				recruitCharacterCanvasGroup.interactable = true;
				recruitCharacterCanvasGroup.blocksRaycasts = true;
				
				RecruitableCharacterData data = message.data?[0] as RecruitableCharacterData;
				IRecruitableCharacter interactive = message.data?[1] as IRecruitableCharacter;
				if (data != null && interactive != null && recruitUIController != null)
				{
					recruitUIController.ShowRecruitUI(data, interactive);
				}
				break;
			case MessageType.OnRecruitCharacter:
			case MessageType.OnRecruitCharacterUIClose:
				overworldUIController.Show();
				recruitCharacterCanvasGroup.alpha = 0f;
				recruitCharacterCanvasGroup.interactable = false;
				recruitCharacterCanvasGroup.blocksRaycasts = false;
				break;
			case MessageType.OnPartyMenuOpen:
				overworldUIController.Hide();
				partyMenuCanvasGroup.alpha = 1f;
				partyMenuCanvasGroup.blocksRaycasts = true;
				partyMenuCanvasGroup.interactable = true;
				if (partyMenuController != null) partyMenuController.RefreshPartyDisplay();
				break;
			case MessageType.OnPartyMenuClose:
				overworldUIController.Show();
				partyMenuCanvasGroup.alpha = 0f;
				partyMenuCanvasGroup.blocksRaycasts = false;
				partyMenuCanvasGroup.interactable = false;
				break;
			case MessageType.OnGameLose:
				gameLoseUICanvasGroup.alpha = 1f;
				gameLoseUICanvasGroup.interactable = true;
				gameLoseUICanvasGroup.blocksRaycasts = true;
				break;
			case MessageType.OnBattleOver:
				battleCanvasGroup.alpha = 0f;
				battleCanvasGroup.interactable = false;
				battleCanvasGroup.blocksRaycasts = false;
				break;
		}
	}

	private static bool ShouldKeepOverworldVisible(Interacable interactable)
	{
		if (interactable is CampInteracableObject)
		{
			return true;
		}

		return interactable != null
			&& interactable.spawnableData != null
			&& interactable.spawnableData.interacableType == InteracableType.FireCamp;
	}
}
