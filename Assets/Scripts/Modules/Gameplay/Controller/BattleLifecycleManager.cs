using HaKien;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleLifecycleManager
{
	private readonly BattleSystem sys;

	public BattleLifecycleManager(BattleSystem battleSystem)
	{
		sys = battleSystem;
	}
	
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
				unit.SetUp();
			}
		}
		foreach (var unit in monsterBattleUnits)
		{
			if (unit != null)
			{
				unit.gameObject.SetActive(false);
				unit.character = null;
				unit.SetUp();
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

	public bool CheckBattleEndCondition(List<BattleUnit> playerBattleUnits, List<BattleUnit> monsterBattleUnits)
	{
		bool allMonsterDefeated = !monsterBattleUnits.Exists(u => u != null && u.character != null && u.IsAlive());
		bool allAllyDefeated = !playerBattleUnits.Exists(u => u != null && u.character != null && u.IsAlive());

		// Check if the custom created character is defeated
		bool createdCharacterDefeated = playerBattleUnits.Exists(u => u != null && u.character is PlayerCharacter pc && pc.IsCreatedCharacter && !u.IsAlive());

		// Tutorial: prevent game over, run tutorial completion on monster defeat
		if (sys.currentBattleType == BattleType.Tutorial)
		{
			if (allAllyDefeated)
			{
				return false;
			}
			if (allMonsterDefeated)
			{
				sys.battleOver = true;
				sys.uiController.battleDialogBox.EnableDialogText(false);
				HandleTutorialComplete();
				return true;
			}
			return false;
		}

		if (allAllyDefeated || createdCharacterDefeated)
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
	public void HandlePlayerLose(List<EntityBase> allEntities = null)
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnBattleLose));
		if (allEntities == null || allEntities.Count == 0)
		{
			allEntities = new List<EntityBase>();
			foreach (var s in sys.playerParty.partySlots) if (s.entity != null) allEntities.Add(s.entity);
			foreach (var s in sys.monsterParty.partySlots) if (s.entity != null) allEntities.Add(s.entity);
		}

		if (allEntities != null)
			BattleEventManager.ResetEffectOnEntities(allEntities);
		CleanupBattle();
		MessageManager.Instance.SendMessage(new Message(MessageType.OnGameLose, new object[] { sys.currentMonsterInteractable }));
	}
	public void HandlePlayerWin(List<EntityBase> allEntities = null)
	{
		MessageManager.Instance.SendMessage(new Message(MessageType.OnBattleWin));
		if (allEntities == null || allEntities.Count == 0)
		{
			allEntities = new List<EntityBase>();
			foreach (var s in sys.playerParty.partySlots) if (s.entity != null) allEntities.Add(s.entity);
			foreach (var s in sys.monsterParty.partySlots) if (s.entity != null) allEntities.Add(s.entity);
		}

		// Fire OnBattleEnd equipment triggers for surviving player characters BEFORE effect reset
		sys.StartCoroutine(FireBattleEndTriggers(allEntities));

		if (allEntities != null)
			BattleEventManager.ResetEffectOnEntities(allEntities);

		Debug.Log("Exp calculation");
		float totalExp = 0f;
		int totalSouldusk = 0;
		List<MonsterCharacter> defeatedMonsters = new List<MonsterCharacter>();
		foreach (var monsterSlot in sys.monsterParty.partySlots)
		{
			if (monsterSlot.entity is MonsterCharacter monster)
			{
				totalExp += monster.TotalExpToAward;
				if (monster.RankData != null)
				{
					totalSouldusk += monster.RankData.baseSoulduskReward;
				}
				defeatedMonsters.Add(monster);
			}
		}

		if (totalSouldusk > 0 && DataManager.Instance?.Currency != null)
		{
			DataManager.Instance.Currency.Add(CurrencyType.SoulDusk, totalSouldusk);
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
			sys.ExpDistribution.ShowExpDistribution(expGainedPerMember, totalSouldusk);

		int totalPartyHp = 0;
		int totalPartyMaxHp = 0;
		int alivePartyMembers = 0;
		int totalPartyMembers = 0;
		foreach (var partySlot in sys.playerParty.partySlots)
		{
			if (partySlot.entity is not PlayerCharacter playerCharacter)
			{
				continue;
			}

			totalPartyMembers++;
			totalPartyHp += Mathf.Max(0, playerCharacter.GetCurrentHP());
			totalPartyMaxHp += Mathf.Max(1, playerCharacter.MaxHp);
			if (playerCharacter.GetCurrentHP() > 0)
			{
				alivePartyMembers++;
			}
		}

		float partyHealthRatio = totalPartyMaxHp > 0 ? (float)totalPartyHp / totalPartyMaxHp : 0f;
		GameEventBus.Publish(new BattleWinEvent
		{
			PartyHealthRatio = partyHealthRatio,
			AlivePartyMemberCount = Mathf.Max(0, alivePartyMembers),
			TotalPartyMemberCount = Mathf.Max(0, totalPartyMembers),
			BattleType = sys.currentBattleType,
			BattleItemUseCount = Mathf.Max(0, sys.BattleItemEffectUseCount),
			BattleMonsters = defeatedMonsters != null ? new System.Collections.Generic.List<MonsterCharacter>(defeatedMonsters) : null
		});

		// Apply permanent death to dead recruited party members
		if (sys.currentBattleType != BattleType.Tutorial)
		{
			List<PlayerCharacter> deadCharacters = sys.playerParty.partySlots
				.Where(s => s.entity is PlayerCharacter pc && pc.GetCurrentHP() <= 0)
				.Select(s => s.entity as PlayerCharacter)
				.ToList();

			foreach (var deadChar in deadCharacters)
			{
				if (!deadChar.IsCreatedCharacter)
				{
					sys.playerParty.RemovePartyMember(deadChar);
					Debug.Log($"[Permadeath] {deadChar.entityData.EntityName} has died permanently and was removed from the party.");
				}
			}
		}
	}

	public void HandleAfterMatch()
	{
		if (sys.currentBattleType == BattleType.RoamingMoster)
		{
			sys.currentMonsterInteractable.Defeated();
		}
		CleanupBattle();
	}

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

		// If no specific entities were provided, grab all from both parties to ensure a full wipe
		if (allEntities == null || allEntities.Count == 0)
		{
			allEntities = new List<EntityBase>();
			foreach (var s in sys.playerParty.partySlots) if (s.entity != null) allEntities.Add(s.entity);
			foreach (var s in sys.monsterParty.partySlots) if (s.entity != null) allEntities.Add(s.entity);
		}

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

	private IEnumerator FireBattleEndTriggers(List<EntityBase> entities)
	{
		foreach (var entity in entities)
		{
			if (entity == null || entity.GetCurrentHP() <= 0) continue;
			if (entity.EquipmentEffectRunner != null)
				yield return entity.EquipmentEffectRunner.Trigger(EquipEffectTrigger.OnBattleEnd, entity);
		}
	}

	/// Tutorial-specific: skip EXP/loot, reset effects, run completion flow.
	private void HandleTutorialComplete()
	{
		// Reset effects on all entities
		var allEntities = new List<EntityBase>();
		foreach (var s in sys.playerParty.partySlots) if (s.entity != null) allEntities.Add(s.entity);
		foreach (var s in sys.monsterParty.partySlots) if (s.entity != null) allEntities.Add(s.entity);
		BattleEventManager.ResetEffectOnEntities(allEntities);

		// Run tutorial completion via the runner
		if (TutorialSequenceRunner.Instance != null)
		{
			sys.StartCoroutine(TutorialSequenceRunner.Instance.RunCompletionFlow());
		}
	}
}
