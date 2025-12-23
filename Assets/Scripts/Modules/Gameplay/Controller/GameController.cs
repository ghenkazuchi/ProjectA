using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GameState
{
	FreeRoam,
	Battle,
	Dialog,
	CharacterCreation,
	RecruitMenu,
	PartyMenu,
	ChestOpen,
	OnInteract
}

public class GameController : Singleton<GameController>,IMessageHandle
{
	[SerializeField] private BattleSystem battleSystem;
	[SerializeField] private RecruitUIController recruitUIController;
	[SerializeField] private PartyMenuController partyMenuController;
	[SerializeField] CanvasGroup characterCreationCanvasGroup;
	[SerializeField] CanvasGroup battleCanvasGroup;
	[SerializeField] CanvasGroup recruitCharacterCanvasGroup;
	[SerializeField] PlayerParty playerParty;
	[SerializeField] CanvasGroup gameLoseUICanvasGroup;
	public GameState currentState;
	[SerializeField] CanvasGroup partyMenuCanvasGroup;
	public GameDay currentGameDay;
	public Time currentTime;
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
		MessageManager.Instance.AddSubcriber(MessageType.OnGameWin, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnGameLose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnBattleOver, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnChestOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnChestClose,this);
		MessageManager.Instance.AddSubcriber(MessageType.OnInteract, this);	
		MessageManager.Instance.AddSubcriber(MessageType.OnInteractEnd, this);
	}
	private void OnDisable()
	{
		MessageManager.Instance.RemoveSubcriber(MessageType.OnBattleStart, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnGameStart, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnRecruitEnter, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnRecruitCharacter, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnRecruitCharacterUIClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPartyMenuOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPartyMenuClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnGameWin, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnGameLose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnBattleOver, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnChestClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnChestOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnInteract, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnInteractEnd, this);
	}
	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnBattleStart:
				currentState = GameState.Battle;
				battleCanvasGroup.alpha = 1f;
				var context = message.data?[0] as BattleContext;
				var monsterInteracable = message.data?[1] as IMonsterInteracable;
				battleSystem.currentMonsterInteractable = monsterInteracable;
				battleSystem.monsterParty = context.monsterParty;
				battleSystem.StartBattle(context.battleType);
			break;
			case MessageType.OnRecruitEnter:
				currentState = GameState.RecruitMenu;
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
				RecruitableCharacterData recruitedCharData = message.data?[0] as RecruitableCharacterData;
				IRecruitableCharacter recruitedInteractive = message.data?[1] as IRecruitableCharacter;
				PlayerCharacter newCharacter = recruitedCharData.CreatePlayerCharacter();
				playerParty.AddCharacter(newCharacter);
				recruitedInteractive?.Recruit();
				currentState = GameState.FreeRoam;
				recruitCharacterCanvasGroup.alpha = 0f;
				recruitCharacterCanvasGroup.interactable = false;
				recruitCharacterCanvasGroup.blocksRaycasts = false;
			break;
			case MessageType.OnRecruitCharacterUIClose:
				recruitCharacterCanvasGroup.alpha = 0f;	
				recruitCharacterCanvasGroup.blocksRaycasts = false;
				recruitCharacterCanvasGroup.interactable = false;
				currentState = GameState.FreeRoam;
			break;

			case MessageType.OnPartyMenuOpen:
				partyMenuCanvasGroup.alpha = 1f;
				partyMenuCanvasGroup.blocksRaycasts = true;
				partyMenuCanvasGroup.interactable = true;
				currentState = GameState.PartyMenu;
				partyMenuController.RefreshPartyDisplay();
			break;
			case MessageType.OnPartyMenuClose:
				partyMenuCanvasGroup.alpha = 0f;
				partyMenuCanvasGroup.blocksRaycasts = false;
				partyMenuCanvasGroup.interactable = false;
				currentState = GameState.FreeRoam;
			break;
			case MessageType.OnGameLose:
				gameLoseUICanvasGroup.alpha = 1f;
				gameLoseUICanvasGroup.interactable = true;
				gameLoseUICanvasGroup.blocksRaycasts = true;
				Debug.Log("Game Over");
			break;
			case MessageType.OnBattleOver:
				battleCanvasGroup.alpha = 0f;
				battleCanvasGroup.interactable = false;
				battleCanvasGroup.blocksRaycasts = false;
				battleSystem.currentMonsterInteractable = null;
				currentState = GameState.FreeRoam;
			break;
			case MessageType.OnChestOpen:
				currentState = GameState.ChestOpen;
			break;
			case MessageType.OnChestClose:
				currentState = GameState.FreeRoam;
				break;
			case MessageType.OnInteract:
				currentState = GameState.OnInteract;
				break;
			case MessageType.OnInteractEnd:
				currentState = GameState.FreeRoam;
				break;
		}
	}
	public PlayerParty GetPlayerParty()
	{
		return playerParty;
	}
}
