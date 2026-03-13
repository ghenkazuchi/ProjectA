using HaKien;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the lifecycle of a battle: setup, win/lose, cleanup, and end-condition checks.
/// Extracted from BattleSystem to keep the orchestrator focused on turn flow.
/// </summary>
public class BattleLifecycleManager
{
	private readonly BattleSystem sys;

	public BattleLifecycleManager(BattleSystem battleSystem)
	{
		sys = battleSystem;
	}

	/// <summary>
	/// Initializes all BattleUnits from party slots, wires up timeline and target selection.
	/// </summary>
	public void SetUpBattle(
		List<BattleUnit> playerBattleUnits,
		List<BattleUnit> monsterBattleUnits,
		Dictionary<GridPosition, int> positionToBattleUnitIndex)
	{
		foreach (var unit in playerBattleUnits)
		{
			if (unit != null)
			{
				unit.gameObject.SetActive(false);
				unit.character = null;
			}
		}
		foreach (var unit in monsterBattleUnits)
		{
			if (unit != null)
			{
				unit.gameObject.SetActive(false);
				unit.character = null;
			}
		}

		foreach (var partySlot in sys.playerParty.partySlots)
		{
			if (partySlot.entity == null) continue;

			GridPosition pos = partySlot.position;
			EntityBase character = partySlot.entity;

			if (positionToBattleUnitIndex.TryGetValue(pos, out int battleUnitIndex))
			{
				if (battleUnitIndex >= 0 && battleUnitIndex < playerBattleUnits.Count)
				{
					if (playerBattleUnits[battleUnitIndex] != null)
					{
						playerBattleUnits[battleUnitIndex].character = character;
						playerBattleUnits[battleUnitIndex].SetUp();
						playerBattleUnits[battleUnitIndex].gameObject.SetActive(true);
						playerBattleUnits[battleUnitIndex].SetUnitType(UnitType.PlayerUnit);
					}
				}
			}
		}

		foreach (var partySlot in sys.monsterParty.partySlots)
		{
			if (partySlot.entity == null) continue;

			GridPosition pos = partySlot.position;
			EntityBase monster = partySlot.entity;

			if (positionToBattleUnitIndex.TryGetValue(pos, out int battleUnitIndex))
			{
				if (battleUnitIndex >= 0 && battleUnitIndex < monsterBattleUnits.Count)
				{
					if (monsterBattleUnits[battleUnitIndex] != null)
					{
						monsterBattleUnits[battleUnitIndex].character = monster;
						monsterBattleUnits[battleUnitIndex].SetUp();
						monsterBattleUnits[battleUnitIndex].gameObject.SetActive(true);
						monsterBattleUnits[battleUnitIndex].SetUnitType(UnitType.MonsterUnit);
					}
				}
			}
		}

		sys.targetSelectionController.enemyUnits = monsterBattleUnits;
		sys.targetSelectionController.playerUnits = playerBattleUnits;
		sys.targetSelectionController.gameObject.SetActive(false);

		List<BattleUnit> allActiveBattleUnits = new List<BattleUnit>();
		allActiveBattleUnits.AddRange(playerBattleUnits);
		allActiveBattleUnits.AddRange(monsterBattleUnits);
		sys.timelineManager.SetAllActiveBattleUnits(allActiveBattleUnits);
		sys.timelineManager.Initialize(allActiveBattleUnits);
		sys.UpdateTimelineUI();
		sys.battleState = BattleState.Start;
	}

	/// <summary>
	/// Checks if all monsters or all players are dead. Triggers win/lose if so.
	/// </summary>
	public bool CheckBattleEndCondition(List<BattleUnit> playerBattleUnits, List<BattleUnit> monsterBattleUnits)
	{
		bool allMonsterDefeated = !monsterBattleUnits.Exists(u => u != null && u.character != null && u.IsAlive());
		bool allAllyDefeated = !playerBattleUnits.Exists(u => u != null && u.character != null && u.IsAlive());

		if (allAllyDefeated)
		{
			sys.battleOver = true;
			sys.uiController.battleDialogBox.EnableDialogText(false);
			HandlePlayerLose();
			return true;
		}
		if (allMonsterDefeated)
		{
			sys.battleOver = true;
			sys.uiController.battleDialogBox.EnableDialogText(false);
			HandlePlayerWin();
			return true;
		}
		return false;
	}

	/// <summary>
	/// Resets effects on all entities, cleans up, and sends the lose message.
	/// </summary>
	public void HandlePlayerLose(List<EntityBase> allEntities = null)
	{
		if (allEntities != null)
			BattleEventManager.ResetEffectOnEntities(allEntities);
		CleanupBattle();
		MessageManager.Instance.SendMessage(new Message(MessageType.OnGameLose, new object[] { sys.currentMonsterInteractable }));
	}

	/// <summary>
	/// Calculates and distributes EXP to surviving party members.
	/// </summary>
	public void HandlePlayerWin(List<EntityBase> allEntities = null)
	{
		if (allEntities != null)
			BattleEventManager.ResetEffectOnEntities(allEntities);

		Debug.Log("Exp calculation");
		float totalExp = 0f;
		foreach (var monsterSlot in sys.monsterParty.partySlots)
		{
			if (monsterSlot.entity is MonsterCharacter monster)
			{
				totalExp += monster.TotalExpToAward;
			}
		}

		List<PlayerCharacter> activeCharacters = sys.playerParty.partySlots
			.Where(s => s.entity is PlayerCharacter pc && pc.GetCurrentHP() > 0)
			.Select(s => s.entity as PlayerCharacter).Distinct()
			.ToList();

		Dictionary<PlayerCharacter, int> expGainedPerMember = new Dictionary<PlayerCharacter, int>();
		if (activeCharacters.Count > 0)
		{
			float expPerCharacter = totalExp / activeCharacters.Count;
			foreach (var character in activeCharacters)
			{
				expGainedPerMember[character] = Mathf.RoundToInt(expPerCharacter);
			}
		}

		if (sys.ExpDistribution != null)
			sys.ExpDistribution.ShowExpDistribution(expGainedPerMember);
	}

	/// <summary>
	/// Called after win/lose UI is done. Marks interactable as defeated and cleans up.
	/// </summary>
	public void HandleAfterMatch()
	{
		if (sys.currentBattleType == BattleType.RoamingMoster)
		{
			sys.currentMonsterInteractable.Defeated();
		}
		CleanupBattle();
	}

	/// <summary>
	/// Full teardown: stops coroutines, resets effects, clears state, hides UI.
	/// </summary>
	public void CleanupBattle(List<EntityBase> allEntities = null)
	{
		Debug.Log("[BattleSystem] CleanupBattle called");

		sys.StopAllCoroutines();

		if (sys.stateMachine != null)
		{
			sys.stateMachine.StopMachine();
		}

		sys.timelineManager?.Initialize(new List<BattleUnit>());

		sys.uiController.currentPlayerCharacterInfo?.EnableBattleHud(false);
		sys.uiController.battleDialogBox?.EnableDialogText(false);
		sys.uiController.battleDialogBox?.EnableActionSelector(false);
		sys.targetSelectionController?.gameObject.SetActive(false);

		if (allEntities != null)
		{
			foreach (var e in allEntities)
			{
				e?.ResetEffectAfterBattle();
				e?.PassiveSkillRunner?.ResetPassiveEffect();
			}
			allEntities.Clear();
		}

		sys.selectedTargets?.Clear();
		sys.currentTurnEntity = null;
		sys.selectedSkill = null;
		sys.currentMonsterInteractable = null;
		Resources.UnloadUnusedAssets();
		System.GC.Collect();
	}
}
