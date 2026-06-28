using System.Collections;
using System.Collections.Generic;
using HaKien;
using UnityEngine;

public class TutorialBattleBootstrapper : MonoBehaviour
{
	public static TutorialScenarioData PendingScenario;

	[SerializeField] private BattleSystem battleSystem;

	private void Start()
	{
		if (PendingScenario == null) return;
		if (battleSystem == null) battleSystem = FindFirstObjectByType<BattleSystem>();

		StartCoroutine(SetupTutorialBattle());
	}

	private IEnumerator SetupTutorialBattle()
	{
		yield return null;
		yield return null;

		var scenario = PendingScenario;
		PendingScenario = null;

		Debug.Log($"[TutorialBootstrapper] Setting up tutorial: {scenario.scenarioTitle}");

		var playerParty = battleSystem.playerParty;
		var monsterParty = battleSystem.monsterParty;

		playerParty.partySlots.Clear();
		monsterParty.partySlots.Clear();

		// Build player party from scenario data
		if (scenario.fixedPlayerCharacters != null)
		{
			foreach (var entry in scenario.fixedPlayerCharacters)
			{
				if (entry.template == null) continue;

				var recruitData = new RecruitableCharacterData(entry.template, entry.level);
				var playerChar = recruitData.CreatePlayerCharacter();

				playerParty.AddPartyMember(playerChar, entry.position);

				if (entry.startingHPPercent > 0)
				{
					int targetHP = Mathf.RoundToInt(playerChar.MaxHp * (entry.startingHPPercent / 100f));
					playerChar.SetCurrentHP(targetHP);
				}

				if (entry.startingEffects != null)
				{
					foreach (var startingEffect in entry.startingEffects)
					{
						if (startingEffect.effectData == null) continue;
						int duration = startingEffect.durationOverride > 0 ? startingEffect.durationOverride : startingEffect.effectData.MaxDuration;
						for (int i = 0; i < startingEffect.stacks; i++)
						{
							var runtimeEffect = startingEffect.effectData.CreateRuntimeEffect(playerChar, playerChar, duration);
							yield return playerChar.AddEffect(runtimeEffect);
						}
					}
				}
			}
		}

		// Build monster party from scenario data
		if (scenario.fixedMonsters != null)
		{
			foreach (var entry in scenario.fixedMonsters)
			{
				if (entry.monsterData == null) continue;

				var monster = new MonsterCharacter(
					entry.rankData, entry.raceData, entry.level, entry.monsterData);
				monster.InitializeEntity(entry.level);
				monster.AddExclusiveSkill();

				monsterParty.AddPartyMember(monster, entry.position);

				if (entry.startingHPPercent > 0)
				{
					int targetHP = Mathf.RoundToInt(monster.MaxHp * (entry.startingHPPercent / 100f));
					monster.SetCurrentHP(targetHP);
				}

				if (entry.startingEffects != null)
				{
					foreach (var startingEffect in entry.startingEffects)
					{
						if (startingEffect.effectData == null) continue;
						int duration = startingEffect.durationOverride > 0 ? startingEffect.durationOverride : startingEffect.effectData.MaxDuration;
						for (int i = 0; i < startingEffect.stacks; i++)
						{
							var runtimeEffect = startingEffect.effectData.CreateRuntimeEffect(monster, monster, duration);
							yield return monster.AddEffect(runtimeEffect);
						}
					}
				}
			}
		}

		// Start the tutorial runner
		if (TutorialSequenceRunner.Instance != null)
		{
			TutorialSequenceRunner.Instance.Begin(scenario);
		}
		else
		{
			Debug.LogError("TutorialSequenceRunner not found in scene!");
		}

		//Start the battle
		battleSystem.playerParty = playerParty;
		var context = new BattleContext(monsterParty, BattleType.Tutorial);
		MessageManager.Instance.SendMessage(new Message(MessageType.OnBattleStart, new object[] { context, null }));
	}
}
