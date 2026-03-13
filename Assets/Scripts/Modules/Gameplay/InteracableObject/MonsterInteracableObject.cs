using HaKien;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInteracableObject : Interacable, IMonsterInteracable
{
	public MonsterParty monsterParty;
	private MonsterAI monsterAI;

	void Start()
	{
		monsterAI = GetComponent<MonsterAI>();
	}

	public void StartBattle()
	{
		var context = new BattleContext(this.monsterParty, BattleType.RoamingMoster);

		MessageManager.Instance.SendMessage(new Message(MessageType.OnBattleStart, new object[] { context, this }));
	}

	public override void TriggerInteraction()
	{
		if (monsterAI != null)
		{
			monsterAI.enabled = false;
		}

		PrepareMonster();
		StartBattle();
	}

	public void PrepareMonster()
	{
		var spawnEntries = GamePoolManager.Instance.GetCurrentMonsterPool();
		var availablePositions = monsterParty.GetAllAvailablePositions();

		// Cap monster count based on player party size to prevent unfair early encounters
		int playerCount = GameController.Instance.GetPlayerParty().GetAllEntitiesInParty().Count;
		int maxMonsters;
		if (playerCount <= 2)
			maxMonsters = 2;     // Early game: 1-2 players → max 2 monsters
		else if (playerCount <= 4)
			maxMonsters = 4;     // Mid game: 3-4 players → max 4 monsters
		else
			maxMonsters = 6;     // Full party: 5-6 players → up to 6 monsters

		int upperBound = Mathf.Min(availablePositions.Count, maxMonsters);
		int count = Random.Range(1, Mathf.Max(2, upperBound + 1));
		for (int i = 0; i < count; i++)
		{
			var spawnData = spawnEntries[Random.Range(0, spawnEntries.Count)];
			var monster = new MonsterCharacter(spawnData.rankData, spawnData.raceData, spawnData.level, spawnData.monsterData);
			monster.InitializeEntity(spawnData.level);

			var pos = availablePositions[Random.Range(0, availablePositions.Count)];
			monsterParty.AddPartyMember(monster, pos);

			availablePositions.Remove(pos);
		}
	}

	public void Defeated()
	{
		Destroy(gameObject);
	}
}
