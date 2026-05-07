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
	OnInteract,
	AchievementMenu,
	PortalMenu,
	MapView
}

public class GameController : Singleton<GameController>,IMessageHandle
{
	[SerializeField] private BattleSystem battleSystem;
	[SerializeField] private DayNightCycleController dayNightCycleController;
	[SerializeField] PlayerParty playerParty;
	
	public GameState currentState;

	private void Awake()
	{

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
		MessageManager.Instance.AddSubcriber(MessageType.OnAchievementScreenOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnAchievementScreenClose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnPortalOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnPortalClose, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnMapViewOpen, this);
		MessageManager.Instance.AddSubcriber(MessageType.OnMapViewClose, this);
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
		MessageManager.Instance.RemoveSubcriber(MessageType.OnAchievementScreenOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnAchievementScreenClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPortalOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnPortalClose, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnMapViewOpen, this);
		MessageManager.Instance.RemoveSubcriber(MessageType.OnMapViewClose, this);
	}
	public void Handle(Message message)
	{
		switch (message.type)
		{
			case MessageType.OnGameStart:
				break;
			case MessageType.OnBattleStart:
				currentState = GameState.Battle;
				var context = message.data?[0] as BattleContext;
				var monsterInteracable = message.data?[1] as IMonsterInteracable;
				battleSystem.currentMonsterInteractable = monsterInteracable;
				battleSystem.monsterParty = context.monsterParty;
				battleSystem.StartBattle(context.battleType);
			break;
			case MessageType.OnRecruitEnter:
				currentState = GameState.RecruitMenu;
			break;
			case MessageType.OnRecruitCharacter:
				RecruitableCharacterData recruitedCharData = message.data?[0] as RecruitableCharacterData;
				IRecruitableCharacter recruitedInteractive = message.data?[1] as IRecruitableCharacter;
				PlayerCharacter newCharacter = recruitedCharData.CreatePlayerCharacter();
				playerParty.AddCharacter(newCharacter);
				recruitedInteractive?.Recruit();
				GameEventBus.Publish(new RecruitEvent
				{
					RecruitedCharacterData = recruitedCharData?.characterTemplate?.entityData,
					RecruitedTemplate = recruitedCharData?.characterTemplate
				});
				currentState = GameState.FreeRoam;
			break;
			case MessageType.OnRecruitCharacterUIClose:
				currentState = GameState.FreeRoam;
			break;

			case MessageType.OnPartyMenuOpen:
				currentState = GameState.PartyMenu;
			break;
			case MessageType.OnPartyMenuClose:
				currentState = GameState.FreeRoam;
			break;
			case MessageType.OnGameLose:
				Debug.Log("Game Over");
			break;
			case MessageType.OnBattleOver:
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
			case MessageType.OnAchievementScreenOpen:
				currentState = GameState.AchievementMenu;
				break;
			case MessageType.OnAchievementScreenClose:
				currentState = GameState.FreeRoam;
				break;
			case MessageType.OnPortalOpen:
				currentState = GameState.PortalMenu;
				break;
			case MessageType.OnPortalClose:
				currentState = GameState.FreeRoam;
				break;
			case MessageType.OnMapViewOpen:
				currentState = GameState.MapView;
				break;
			case MessageType.OnMapViewClose:
				currentState = GameState.FreeRoam;
				break;
		}
	}
	public PlayerParty GetPlayerParty()
	{
		return playerParty;
	}
}
