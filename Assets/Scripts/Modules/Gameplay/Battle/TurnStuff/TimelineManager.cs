using HaKien;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimelineManager : Singleton<TimelineManager>
{
	private readonly List<TurnUnit> units = new();
	private readonly List<EntityBase> bonusTurnQueue = new();
	private List<TurnUnit> currentRoundOrder = new();
	private List<BattleUnit> allActiveBattleUnits;
	private int currentRoundIndex;
	private int nextStableOrder;

	public int Version { get; private set; } = 0;
	public int CurrentRoundNumber { get; private set; } = 0;

	public event System.Action<EntityBase, List<EntityBase>, int> OnTimeLineChanged;

	private void Bump(EntityBase current = null)
	{
		Version++;
		var preview = PeekNextEntitiesWithCurrent(current, 5);
		OnTimeLineChanged?.Invoke(current, preview, Version);
	}

	public void Initialize(List<BattleUnit> battleUnits)
	{
		units.Clear();
		bonusTurnQueue.Clear();
		currentRoundOrder.Clear();
		currentRoundIndex = 0;
		nextStableOrder = 0;
		CurrentRoundNumber = 0;

		foreach (var battleUnit in battleUnits)
		{
			if (battleUnit == null || battleUnit.character == null || !battleUnit.IsAlive())
			{
				continue;
			}

			units.Add(new TurnUnit(battleUnit.character, nextStableOrder++));
		}

		BuildNextRound();
		Bump(null);
	}

	public void SlowDownEntity(EntityBase entity, float slowAmount)
	{
		var turnUnit = units.FirstOrDefault(tu => tu.entity == entity);
		if (turnUnit == null)
		{
			return;
		}

		int delaySlots = CalculateShiftSlots(turnUnit, slowAmount);
		if (delaySlots <= 0)
		{
			return;
		}

		if (!ShiftWithinCurrentRound(turnUnit, delaySlots))
		{
			turnUnit.nextRoundPriorityOffset += delaySlots;
		}

		Debug.Log($"{entity.entityData.EntityName} was delayed by {delaySlots} slot(s).");
		Bump(null);
	}

	public void SpeedUpEntity(EntityBase entity, float increaseAmount)
	{
		var turnUnit = units.FirstOrDefault(tu => tu.entity == entity);
		if (turnUnit == null)
		{
			return;
		}

		int advanceSlots = CalculateShiftSlots(turnUnit, increaseAmount);
		if (advanceSlots <= 0)
		{
			return;
		}

		if (!ShiftWithinCurrentRound(turnUnit, -advanceSlots))
		{
			turnUnit.nextRoundPriorityOffset -= advanceSlots;
		}

		Debug.Log($"{entity.entityData.EntityName} was advanced by {advanceSlots} slot(s).");
		Bump(null);
	}

	public EntityBase GetNextTurnEntity()
	{
		RemoveAllDeadUnits();

		while (bonusTurnQueue.Count > 0)
		{
			EntityBase bonusEntity = bonusTurnQueue[0];
			bonusTurnQueue.RemoveAt(0);
			if (bonusEntity == null || !IsEntityAlive(bonusEntity))
			{
				continue;
			}

			Bump(bonusEntity);
			return bonusEntity;
		}

		EnsureRoundReady();

		if (currentRoundOrder.Count == 0)
		{
			return null;
		}

		var nextUnit = currentRoundOrder[currentRoundIndex];
		currentRoundIndex++;
		Bump(nextUnit.entity);
		return nextUnit.entity;
	}

	public void RemoveDeadEntityFromTimeline(EntityBase entity)
	{
		var turnUnit = units.FirstOrDefault(u => u.entity == entity);
		if (turnUnit == null)
		{
			return;
		}

		RemoveTurnUnit(turnUnit);
		Bump(null);
	}

	public List<EntityBase> PeekNextEntities(int count)
	{
		return PeekNextEntitiesInternal(count, null);
	}

	private List<EntityBase> PeekNextEntitiesInternal(int count, EntityBase assumedCurrentActor)
	{
		var result = new List<EntityBase>();
		if (count <= 0)
		{
			return result;
		}

		var simulatedUnits = units
			.Where(IsTurnUnitAlive)
			.Select(u => new TurnUnit(u))
			.ToList();

		if (simulatedUnits.Count == 0)
		{
			return result;
		}

		var simulatedMap = simulatedUnits.ToDictionary(u => u.entity, u => u);
		var simulatedBonusQueue = bonusTurnQueue
			.Where(e => e != null && simulatedMap.ContainsKey(e))
			.ToList();
		var remainingBonusTurns = BuildRemainingBonusTurnMap();
		var simulatedRound = new List<TurnUnit>();
		if (currentRoundOrder != null)
		{
			for (int i = 0; i < currentRoundOrder.Count; i++)
			{
				var original = currentRoundOrder[i];
				if (original == null || !simulatedMap.TryGetValue(original.entity, out var copy))
				{
					continue;
				}

				simulatedRound.Add(copy);
			}
		}

		int simulatedIndex = 0;
		if (currentRoundOrder != null)
		{
			int actedCount = Mathf.Min(currentRoundIndex, currentRoundOrder.Count);
			for (int i = 0; i < actedCount; i++)
			{
				var original = currentRoundOrder[i];
				if (original != null && simulatedMap.ContainsKey(original.entity))
				{
					simulatedIndex++;
				}
			}
		}

		simulatedIndex = Mathf.Clamp(simulatedIndex, 0, simulatedRound.Count);

		if (assumedCurrentActor != null && simulatedMap.ContainsKey(assumedCurrentActor))
		{
			TryQueueSimulatedBonusTurn(assumedCurrentActor, remainingBonusTurns, simulatedBonusQueue);
		}

		while (result.Count < count && simulatedUnits.Count > 0)
		{
			if (simulatedBonusQueue.Count > 0)
			{
				EntityBase bonusEntity = simulatedBonusQueue[0];
				simulatedBonusQueue.RemoveAt(0);
				result.Add(bonusEntity);
				TryQueueSimulatedBonusTurn(bonusEntity, remainingBonusTurns, simulatedBonusQueue);
				continue;
			}

			if (simulatedRound.Count == 0 || simulatedIndex >= simulatedRound.Count)
			{
				simulatedRound = OrderForNextRound(simulatedUnits);
				simulatedIndex = 0;
				for (int i = 0; i < simulatedUnits.Count; i++)
				{
					simulatedUnits[i].nextRoundPriorityOffset = 0;
				}
			}

			if (simulatedRound.Count == 0)
			{
				break;
			}

			EntityBase nextEntity = simulatedRound[simulatedIndex].entity;
			result.Add(nextEntity);
			simulatedIndex++;
			TryQueueSimulatedBonusTurn(nextEntity, remainingBonusTurns, simulatedBonusQueue);
		}

		return result;
	}

	public List<EntityBase> PeekNextEntitiesWithCurrent(EntityBase currentEntity, int totalCount)
	{
		var result = new List<EntityBase>();
		if (currentEntity != null)
		{
			BattleUnit currentBattleUnit = FindBattleUnitForEntity(currentEntity);
			if (currentBattleUnit != null && currentBattleUnit.IsAlive())
			{
				result.Add(currentEntity);
			}
		}

		var upcomingEntities = PeekNextEntitiesInternal(totalCount - result.Count, currentEntity);
		result.AddRange(upcomingEntities);

		return result;
	}

	public void UpdateEntityTimeline(EntityBase entityToUpdate)
	{
		var turnUnit = units.FirstOrDefault(x => x.entity == entityToUpdate);
		var battleUnit = FindBattleUnitForEntity(entityToUpdate);
		bool isAlive = battleUnit != null && battleUnit.IsAlive();

		if (turnUnit != null)
		{
			if (!isAlive)
			{
				RemoveTurnUnit(turnUnit);
			}
			else
			{
				turnUnit.RefreshSpeed(entityToUpdate.GetFinalStat(Stat.ActionSpeed));
			}
		}
		else if (isAlive)
		{
			var newUnit = new TurnUnit(entityToUpdate, nextStableOrder++);
			units.Add(newUnit);

			if (currentRoundOrder.Count > 0 && currentRoundIndex < currentRoundOrder.Count)
			{
				currentRoundOrder.Add(newUnit);
			}
		}

		Bump(null);
	}

	public void RemoveDeadEntitiesFromTimeline(List<EntityBase> entities)
	{
		var entitySet = new HashSet<EntityBase>(entities);
		for (int i = units.Count - 1; i >= 0; i--)
		{
			var turnUnit = units[i];
			if (!entitySet.Contains(turnUnit.entity))
			{
				continue;
			}

			var battleUnit = FindBattleUnitForEntity(turnUnit.entity);
			if (battleUnit == null || !battleUnit.IsAlive())
			{
				RemoveTurnUnit(turnUnit);
			}
		}

		Bump(null);
	}

	public void SetAllActiveBattleUnits(List<BattleUnit> units)
	{
		allActiveBattleUnits = units;
	}

	private BattleUnit FindBattleUnitForEntity(EntityBase entity)
	{
		if (allActiveBattleUnits == null)
		{
			return null;
		}

		return allActiveBattleUnits.FirstOrDefault(bu => bu.character == entity);
	}

	public TurnUnit GetTurnUnit(EntityBase entity)
	{
		return units.FirstOrDefault(u => u.entity == entity);
	}

	public void PushToFront(EntityBase entity)
	{
		GrantBonusTurn(entity);
	}

	public bool GrantBonusTurn(EntityBase entity)
	{
		if (entity == null || !units.Any(u => u.entity == entity) || !IsEntityAlive(entity))
		{
			return false;
		}

		bonusTurnQueue.Insert(0, entity);
		Bump(null);
		return true;
	}

	private void EnsureRoundReady()
	{
		if (units.Count == 0)
		{
			currentRoundOrder.Clear();
			currentRoundIndex = 0;
			return;
		}

		if (currentRoundOrder.Count == 0 || currentRoundIndex >= currentRoundOrder.Count)
		{
			BuildNextRound();
		}
	}

	private void BuildNextRound()
	{
		currentRoundOrder = OrderForNextRound(units.Where(IsTurnUnitAlive).ToList());
		currentRoundIndex = 0;
		if (currentRoundOrder.Count > 0)
		{
			CurrentRoundNumber++;
		}

		for (int i = 0; i < units.Count; i++)
		{
			units[i].nextRoundPriorityOffset = 0;
		}
	}

	private List<TurnUnit> OrderForNextRound(List<TurnUnit> source)
	{
		return source
			.Where(u => u != null)
			.OrderBy(u => u.nextRoundPriorityOffset)
			.ThenByDescending(u => u.speed)
			.ThenBy(u => u.stableOrder)
			.ToList();
	}

	private void RemoveAllDeadUnits()
	{
		for (int i = units.Count - 1; i >= 0; i--)
		{
			if (!IsTurnUnitAlive(units[i]))
			{
				RemoveTurnUnit(units[i]);
			}
		}
	}

	private void RemoveTurnUnit(TurnUnit turnUnit)
	{
		if (turnUnit == null)
		{
			return;
		}

		int roundIndex = currentRoundOrder.IndexOf(turnUnit);
		if (roundIndex >= 0)
		{
			currentRoundOrder.RemoveAt(roundIndex);
			if (roundIndex < currentRoundIndex)
			{
				currentRoundIndex = Mathf.Max(0, currentRoundIndex - 1);
			}
		}

		units.Remove(turnUnit);
		bonusTurnQueue.RemoveAll(entity => entity == turnUnit.entity);
		if (currentRoundOrder.Count == 0)
		{
			currentRoundIndex = 0;
		}
	}

	private bool IsTurnUnitAlive(TurnUnit turnUnit)
	{
		if (turnUnit == null || turnUnit.entity == null)
		{
			return false;
		}

		return IsEntityAlive(turnUnit.entity);
	}

	private bool IsEntityAlive(EntityBase entity)
	{
		if (entity == null)
		{
			return false;
		}

		BattleUnit battleUnit = FindBattleUnitForEntity(entity);
		return battleUnit != null && battleUnit.IsAlive();
	}

	private Dictionary<EntityBase, int> BuildRemainingBonusTurnMap()
	{
		var result = new Dictionary<EntityBase, int>();
		int roundNumber = CurrentRoundNumber;

		for (int i = 0; i < units.Count; i++)
		{
			EntityBase entity = units[i].entity;
			if (!IsEntityAlive(entity) || entity.activePassiveSkills == null)
			{
				continue;
			}

			int remaining = 0;
			for (int skillIndex = 0; skillIndex < entity.activePassiveSkills.Count; skillIndex++)
			{
				PassiveSkill skill = entity.activePassiveSkills[skillIndex];
				if (skill == null || skill.PassiveSkillData == null || skill.RuntimeEffects == null)
				{
					continue;
				}

				if (skill.PassiveSkillData.trigger != PassiveTrigger.OnTurnEnd)
				{
					continue;
				}

				if (skill.CurrentSkillCoolDown > 0)
				{
					continue;
				}

				float activationChance = skill.PassiveSkillData.activationChance;
				if (activationChance > 0f && activationChance < 1f)
				{
					continue;
				}

				for (int effectIndex = 0; effectIndex < skill.RuntimeEffects.Count; effectIndex++)
				{
					if (skill.RuntimeEffects[effectIndex] is IBonusTurnPreviewEffect previewEffect)
					{
						remaining += Mathf.Max(0, previewEffect.GetRemainingBonusTurnsForRound(roundNumber));
					}
				}
			}

			if (remaining > 0)
			{
				result[entity] = remaining;
			}
		}

		return result;
	}

	private void TryQueueSimulatedBonusTurn(EntityBase entity, Dictionary<EntityBase, int> remainingBonusTurns, List<EntityBase> simulatedBonusQueue)
	{
		if (entity == null || remainingBonusTurns == null || simulatedBonusQueue == null)
		{
			return;
		}

		if (!remainingBonusTurns.TryGetValue(entity, out int remaining) || remaining <= 0)
		{
			return;
		}

		simulatedBonusQueue.Insert(0, entity);
		remainingBonusTurns[entity] = remaining - 1;
	}

	private int CalculateShiftSlots(TurnUnit turnUnit, float magnitude)
	{
		if (turnUnit == null || magnitude <= 0f)
		{
			return 0;
		}

		float baseTurnInterval = Mathf.Max(0.0001f, turnUnit.actionValue);
		float ratio = magnitude / baseTurnInterval;
		int range = Mathf.Max(1, units.Count - 1);
		return Mathf.Clamp(Mathf.CeilToInt(range * ratio), 1, range);
	}

	private bool ShiftWithinCurrentRound(TurnUnit turnUnit, int delta)
	{
		if (turnUnit == null || delta == 0)
		{
			return false;
		}

		int currentIndex = currentRoundOrder.IndexOf(turnUnit);
		if (currentIndex < currentRoundIndex)
		{
			return false;
		}

		if (currentIndex < 0)
		{
			return false;
		}

		int targetIndex = Mathf.Clamp(currentIndex + delta, currentRoundIndex, currentRoundOrder.Count - 1);
		if (targetIndex == currentIndex)
		{
			return false;
		}

		currentRoundOrder.RemoveAt(currentIndex);
		currentRoundOrder.Insert(targetIndex, turnUnit);
		return true;
	}
}
