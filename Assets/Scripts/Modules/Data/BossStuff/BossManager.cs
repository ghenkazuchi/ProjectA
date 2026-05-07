using HaKien;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
	[SerializeField] private BossPool pool;

	public BossFormation currentBoss;
	public event Action<BossFormation> OnBossRolled;

	public void Start()
	{
		RollNewBoss();
	}

	/// <summary>
	/// Swaps the boss pool. Called by RunProgressionManager when a new loop starts.
	/// </summary>
	public void SetPool(BossPool newPool)
	{
		pool = newPool;
	}

	/// <summary>
	/// Rolls a random boss formation from the current pool.
	/// </summary>
	public void RollNewBoss()
	{
		if (pool == null || pool.formations.Count == 0)
		{
			Debug.LogWarning("[BossManager] No boss pool or empty formations list.");
			return;
		}

		currentBoss = pool.formations[UnityEngine.Random.Range(0, pool.formations.Count)];
		OnBossRolled?.Invoke(currentBoss);
		Debug.Log($"[BossManager] Boss rolled: {currentBoss.formationName}");
	}

	/// <summary>
	/// Starts the boss battle immediately. Called by RunProgressionManager.
	/// </summary>
	public void TriggerBoss()
	{
		if (pool == null || pool.formations.Count == 0)
		{
			Debug.LogWarning("[BossManager] Cannot trigger boss — no pool or formations.");
			return;
		}

		var party = new MonsterParty();
		var pick = currentBoss;

		foreach (var m in pick.members)
		{
			if (m.monster == null || m.rank == null || m.race == null) continue;
			var monster = new MonsterCharacter(m.rank, m.race, m.level, m.monster);
			monster.InitializeEntity(m.level);
			party.AddPartyMember(monster, m.position);
		}

		var ctx = new BattleContext(party, BattleType.Boss);
		MessageManager.Instance.SendMessage(new Message(MessageType.OnBattleStart, new object[] { ctx, null }));
		Debug.Log($"[BossManager] Boss battle started: {pick.formationName}");
	}
}
