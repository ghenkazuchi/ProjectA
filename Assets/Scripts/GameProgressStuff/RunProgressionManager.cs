using System;
using System.Collections.Generic;
using HaKien;
using UnityEngine;

public class RunProgressionManager : Singleton<RunProgressionManager>, IMessageHandle
{
	[Header("Loop Configs (one per loop, in order)")]
	[SerializeField] private List<LoopConfig> loopConfigs = new List<LoopConfig>();

	[Header("References")]
	[SerializeField] private DayNightCycleController dayNightController;
	[SerializeField] private BossManager bossManager;

	[Header("Runtime State (read-only)")]
	[SerializeField] private int currentLoop = 0;
	[SerializeField] private GameDay currentDay = GameDay.Day1;
	[SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
	[SerializeField] private int totalBossesDefeated = 0;

	public int CurrentLoop => currentLoop;
	public GameDay CurrentDay => currentDay;
	public TimeOfDay CurrentTimeOfDay => currentTimeOfDay;
	public int TotalBossesDefeated => totalBossesDefeated;
	public LoopConfig CurrentLoopConfig => (loopConfigs != null && currentLoop < loopConfigs.Count)
		? loopConfigs[currentLoop]
		: null;

	public event Action<int> OnLoopChanged;
	public event Action<GameDay> OnDayChanged;
	public event Action<TimeOfDay> OnTimeOfDayChanged;
	public event Action OnRunComplete;

	private bool wasNight = false;
	private bool awaitingBossResult = false;

	private void Start()
	{
		if (dayNightController == null)
			dayNightController = FindAnyObjectByType<DayNightCycleController>();
		if (bossManager == null)
			bossManager = FindAnyObjectByType<BossManager>();

		wasNight = dayNightController != null && dayNightController.isNight;

		// Initialize first loop
		ApplyLoopConfig();
		SyncGamePoolManager();
	}

	private void OnEnable()
	{
		if (dayNightController != null)
			dayNightController.OnNightStateChanged += HandleNightStateChanged;

		MessageManager.Instance.AddSubcriber(MessageType.OnBattleOver, this);
	}

	private void OnDisable()
	{
		if (dayNightController != null)
			dayNightController.OnNightStateChanged -= HandleNightStateChanged;

		MessageManager.Instance.RemoveSubcriber(MessageType.OnBattleOver, this);
	}

	// ──────────────────────────────────────────────
	// Day/Night Transition Logic
	// ──────────────────────────────────────────────

	private void HandleNightStateChanged(bool isNight)
	{
		if (awaitingBossResult) return;

		if (!wasNight && isNight)
		{
			// Day → Night
			currentTimeOfDay = TimeOfDay.Night;
			SyncGamePoolManager();
			OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
			Debug.Log($"[RunProgression] Now Night — Loop {currentLoop + 1}, {currentDay}");
		}
		else if (wasNight && !isNight)
		{
			// Night → Day transition = advance day
			currentTimeOfDay = TimeOfDay.Morning;

			if (currentDay == GameDay.Day3)
			{
				// End of loop — trigger boss
				TriggerBossBattle();
			}
			else
			{
				// Advance to next day
				currentDay = (currentDay == GameDay.Day1) ? GameDay.Day2 : GameDay.Day3;
				SyncGamePoolManager();
				OnDayChanged?.Invoke(currentDay);
				Debug.Log($"[RunProgression] Day advanced — Loop {currentLoop + 1}, {currentDay}");
			}
		}

		wasNight = isNight;
	}

	// ──────────────────────────────────────────────
	// Boss Logic
	// ──────────────────────────────────────────────

	private void TriggerBossBattle()
	{
		if (bossManager == null)
		{
			Debug.LogError("[RunProgression] BossManager not found!");
			return;
		}

		awaitingBossResult = true;
		bossManager.TriggerBoss();
		Debug.Log($"[RunProgression] Boss triggered — Loop {currentLoop + 1}");
	}

	private void HandleBossDefeated()
	{
		awaitingBossResult = false;
		totalBossesDefeated++;

		if (currentLoop >= loopConfigs.Count - 1)
		{
			// Final loop boss defeated — game won!
			Debug.Log("[RunProgression] All loops complete — VICTORY!");
			OnRunComplete?.Invoke();
			MessageManager.Instance.SendMessage(new Message(MessageType.OnGameWin));
			return;
		}

		// Advance to next loop
		currentLoop++;
		currentDay = GameDay.Day1;
		currentTimeOfDay = TimeOfDay.Morning;

		if (dayNightController != null)
			dayNightController.AdvanceToNextMorning(true);

		ApplyLoopConfig();
		SyncGamePoolManager();

		// Reset session claims so recruit pool is fresh for new loop
		if (GamePoolManager.Instance != null)
			GamePoolManager.Instance.ResetSessionClaims();

		OnLoopChanged?.Invoke(currentLoop);
		OnDayChanged?.Invoke(currentDay);

		Debug.Log($"[RunProgression] Advanced to Loop {currentLoop + 1}, Day1 Morning");
	}

	// ──────────────────────────────────────────────
	// Config Application
	// ──────────────────────────────────────────────

	private void ApplyLoopConfig()
	{
		var config = CurrentLoopConfig;
		if (config == null)
		{
			Debug.LogWarning($"[RunProgression] No LoopConfig found for loop index {currentLoop}");
			return;
		}

		// Set boss pool for this loop
		if (bossManager != null)
		{
			bossManager.SetPool(config.bossPool);
			bossManager.RollNewBoss();
		}

		// Set pools on GamePoolManager
		if (GamePoolManager.Instance != null)
		{
			GamePoolManager.Instance.SetPools(config.monsterPool, config.recruitPool);
		}

		Debug.Log($"[RunProgression] Applied config for Loop {currentLoop + 1}: '{config.loopName}'");
	}

	private void SyncGamePoolManager()
	{
		if (GamePoolManager.Instance != null)
		{
			GamePoolManager.Instance.SetGameTime(currentDay, currentTimeOfDay);
		}
	}

	// ──────────────────────────────────────────────
	// Message Handling
	// ──────────────────────────────────────────────

	public void Handle(Message message)
	{
		if (message.type == MessageType.OnBattleOver && awaitingBossResult)
		{
			HandleBossDefeated();
		}
	}
}
