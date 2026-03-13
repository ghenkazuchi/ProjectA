using HaKien;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using UnityEngine;

public class TimelineManager : Singleton<TimelineManager>
{
	private List<TurnUnit> units = new List<TurnUnit>();
	private List<BattleUnit> allActiveBattleUnits;
	public int Version { get; private set; } = 0;

	public event System.Action<EntityBase, List<EntityBase>, int> OnTimeLineChanged;

	const float MIN_SPEED = 0.01f;
	private static float AvFromSpeed(float speed) => 10000f / Mathf.Max(MIN_SPEED, speed);

	private void Bump(EntityBase current = null)
	{
		Version++;
		var preview = PeekNextEntitiesWithCurrent(current, 5);
		OnTimeLineChanged?.Invoke(current, preview, Version);
	}
	public void Initialize(List<BattleUnit> battleUnits)
	{
		units.Clear();
		foreach (var battleUnit in battleUnits)
		{
			if (battleUnit != null && battleUnit.character != null && battleUnit.IsAlive())
			{
				units.Add(new TurnUnit(battleUnit.character) { speed = battleUnit.character.GetFinalStat(Stat.ActionSpeed) });
			}
		}
		RecalculateAllActionValues();
		Bump(null);
	}
	public void SlowDownEntity(EntityBase entity, float slowAmount)
	{
		var turnUnit = units.FirstOrDefault(tu => tu.entity == entity);
		if (turnUnit != null)
		{
			turnUnit.actionValue += slowAmount;
			units = units.OrderBy(tu => tu.actionValue).ToList();
			Debug.Log($"{entity.entityData.EntityName} has been slowed down by {slowAmount}. New action value: {turnUnit.actionValue}");
		}
	}
	public void SpeedUpEntity(EntityBase entity, float increaseAmount)
	{
		var turnUnit = units.FirstOrDefault(tu => tu.entity == entity);
		if (turnUnit != null) 
		{
			turnUnit.actionValue -= increaseAmount;
			units = units.OrderBy(tu => tu.actionValue).ToList();
			Debug.Log($"{entity.entityData.EntityName} has been speed up by {increaseAmount}. New action value: {turnUnit.actionValue}");
		}
	}
	public EntityBase GetNextTurnEntity()
	{
		RemoveAllDeadUnits();

		if (units.Count == 0) return null;

		units.Sort((a, b) => a.actionValue.CompareTo(b.actionValue));
		var nextUnit = units[0];

		//BattleUnit correspondingBattleUnit = FindBattleUnitForEntity(nextUnit.entity);
		//if (correspondingBattleUnit == null || !correspondingBattleUnit.IsAlive())
		//{
		//	units.Remove(nextUnit);
		//	return GetNextTurnEntity(); 
		//}

		float minAV = nextUnit.actionValue;
		foreach (var unit in units)
		{
			unit.actionValue -= minAV;
		}

		nextUnit.actionValue = 10000f / nextUnit.speed;
		Bump(nextUnit.entity);
		return nextUnit.entity;
	}
	private void RemoveAllDeadUnits()
	{
		units.RemoveAll(u => {
			BattleUnit correspondingBattleUnit = FindBattleUnitForEntity(u.entity);
			return correspondingBattleUnit == null || !correspondingBattleUnit.IsAlive();
		});
	}
	public void RemoveDeadEntityFromTimeline(EntityBase entity)
	{
		units.RemoveAll(u => u.entity == entity);
	}
	public List<EntityBase> PeekNextEntities(int count)
	{
		var simulatedUnits = units.Where(u => {
			BattleUnit correspondingBattleUnit = FindBattleUnitForEntity(u.entity);
			return correspondingBattleUnit != null && correspondingBattleUnit.IsAlive();
		})
								  .Select(u => new TurnUnit(u.entity)
								  {
									  speed = u.speed,
									  actionValue = u.actionValue
								  }).ToList();

		var result = new List<EntityBase>();
		for (int i = 0; i < count && simulatedUnits.Count > 0; i++)
		{
			simulatedUnits.Sort((a, b) => a.actionValue.CompareTo(b.actionValue));
			var nextUnit = simulatedUnits[0];
			result.Add(nextUnit.entity);

			float minAV = nextUnit.actionValue;
			foreach (var unit in simulatedUnits)
			{
				unit.actionValue -= minAV;
			}
			nextUnit.actionValue = 10000f / nextUnit.speed;
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
		var upcomingEntities = PeekNextEntities(totalCount - result.Count);
		result.AddRange(upcomingEntities);

		return result;
	}

	public void UpdateEntityTimeline(EntityBase entityToUpdate)
	{
		var u = units.FirstOrDefault(x => x.entity == entityToUpdate);
		var bu = FindBattleUnitForEntity(entityToUpdate);
		bool isAlive = bu != null && bu.IsAlive();

		if (u != null)
		{
			if (!isAlive) { units.Remove(u); }
			else
			{
				float oldSpeed = Mathf.Max(MIN_SPEED, u.speed);
				float newSpeed = Mathf.Max(MIN_SPEED, entityToUpdate.GetFinalStat(Stat.ActionSpeed));
				u.actionValue = u.actionValue * (oldSpeed / newSpeed);
				u.speed = newSpeed;
			}
		}
		else if (isAlive)
		{
			units.Add(new TurnUnit(entityToUpdate) { speed = Mathf.Max(MIN_SPEED, entityToUpdate.GetFinalStat(Stat.ActionSpeed)) });
			RecalculateAllActionValues();
		}
		Bump(null);
	}
	public void RemoveDeadEntitiesFromTimeline(List<EntityBase> entities)
	{
		var entitySet = new HashSet<EntityBase>(entities);
		units.RemoveAll(u => {
			if (!entitySet.Contains(u.entity)) return false;
			var bu = FindBattleUnitForEntity(u.entity);
			return bu == null || !bu.IsAlive();
		});
		Bump(null);
	}

	private void RecalculateAllActionValues()
	{
		foreach (var unit in units)
			unit.actionValue = AvFromSpeed(unit.speed);
	}
	public void SetAllActiveBattleUnits(List<BattleUnit> units)
	{
		allActiveBattleUnits = units;
	}

	private BattleUnit FindBattleUnitForEntity(EntityBase entity)
	{
		if (allActiveBattleUnits == null) return null;
		return allActiveBattleUnits.FirstOrDefault(bu => bu.character == entity);
	}

	public TurnUnit GetTurnUnit(EntityBase entity)
	{
		return units.FirstOrDefault(u => u.entity == entity);
	}

	public void PushToFront(EntityBase entity)
	{
		var tu = units.FirstOrDefault(u => u.entity == entity);
		if (tu == null) return;
		SpeedUpEntity(entity, tu.actionValue + 0.001f);
	}

}
