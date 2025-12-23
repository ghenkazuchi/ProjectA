using HaKien;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour, IMessageHandle
{
	[SerializeField] private DayNightCycleController dayNightController;
	[SerializeField] private BossPool pool;

	[Header("Clock")]
	public int dayIndex;
	public bool isNight;
	public float segment;

	public BossFormation currentBoss;
	public event Action<BossFormation> OnBossRolled;

	[SerializeField] private GameDay currentDay = GameDay.Day1;
	private bool wasNight = false;
	private bool awaitingBossOver = false;

	public void Start()
	{
		RollNewBoss();
	}

	public void RollNewBoss()
	{
		currentBoss = pool.formations[UnityEngine.Random.Range(0, pool.formations.Count)];
		OnBossRolled?.Invoke(currentBoss);
	}
	private void OnEnable()
	{
		if (dayNightController != null)
		{
			wasNight = dayNightController.isNight;
			isNight = wasNight;
		}
		dayIndex = 1;
		currentDay = GameDay.Day1;
		dayNightController.OnNightStateChanged += DayNightController_OnNightStateChanged;
		MessageManager.Instance.AddSubcriber(MessageType.OnBattleOver, this);
	}

	private void OnDisable()
	{
		if (dayNightController != null)
			dayNightController.OnNightStateChanged -= DayNightController_OnNightStateChanged;

		MessageManager.Instance.RemoveSubcriber(MessageType.OnBattleOver, this);
	}

	private void DayNightController_OnNightStateChanged(bool nowNight)
	{
		isNight = nowNight;
		if (wasNight && !nowNight)
		{
			dayIndex = Mathf.Min(dayIndex + 1, 3);
			AdvanceDayOrStartBoss();
		}
		wasNight = nowNight;

		segment = ComputeSegment01(dayNightController.time01,
								   dayNightController.nightWindowStart,
								   dayNightController.nightWindowEnd);
	}

	private float ComputeSegment01(float t, float nightStart, float nightEnd)
	{
		bool night = (t > nightStart) || (t <= nightEnd);
		if (night)
		{
			float nightLen = (1f - nightStart) + nightEnd;
			float elapsed = (t >= nightStart) ? (t - nightStart) : (t + 1f - nightStart);
			return Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, nightLen));
		}
		else
		{
			float dayLen = (nightStart - nightEnd);
			float elapsed = t - nightEnd;
			return Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, dayLen));
		}
	}


	private void AdvanceDayOrStartBoss()
	{
		if (currentDay == GameDay.Day3)
		{
			StartBossBattle();
		}
		else
		{
			currentDay = (currentDay == GameDay.Day1) ? GameDay.Day2 : GameDay.Day3;
			Debug.Log(currentDay);
		}
	}

	private void StartBossBattle()
	{
		if (awaitingBossOver) return;
		if (pool == null || pool.formations.Count == 0)
		{
			Debug.LogWarning("Error");
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
		awaitingBossOver = true;
	}

	public void Handle(Message message)
	{
		if (message.type == MessageType.OnBattleOver && awaitingBossOver)
		{
			dayNightController.AdvanceToNextMorning(true);
			currentDay = GameDay.Day1;
			dayIndex = 1;
			awaitingBossOver = false;

			RollNewBoss(); 
		}
	}
}
