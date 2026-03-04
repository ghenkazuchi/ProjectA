using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Manages the registration, lookup, and state updates for BattleUnits in a battle.
/// Extracted from BattleSystem to separate unit handling from battle flow logic.
/// </summary>
public class BattleUnitRegistry
{
	private readonly BattleSystem sys;
	public List<BattleUnit> PlayerBattleUnits { get; private set; }
	public List<BattleUnit> MonsterBattleUnits { get; private set; }
	
	private readonly Dictionary<GridPosition, int> positionToBattleUnitIndex = new Dictionary<GridPosition, int>
	{
		{ new GridPosition(0, 0), 0 },
		{ new GridPosition(1, 0), 3 },
		{ new GridPosition(1, 2), 5 },
		{ new GridPosition(0, 1), 1 },
		{ new GridPosition(1, 1), 4 },
		{ new GridPosition(0, 2), 2 },
	};

	public BattleUnitRegistry(BattleSystem battleSystem, List<BattleUnit> playerUnits, List<BattleUnit> monsterUnits)
	{
		sys = battleSystem;
		PlayerBattleUnits = playerUnits;
		MonsterBattleUnits = monsterUnits;
	}

	public Dictionary<GridPosition, int> GetPositionMap() => positionToBattleUnitIndex;

	public BattleUnit GetBattleUnitAt(int index) => PlayerBattleUnits[index];

	public GridPosition GetPositionByUnitIndex(int index)
	{
		foreach (var kv in positionToBattleUnitIndex)
			if (kv.Value == index) return kv.Key;

		return new GridPosition(-999, -999);
	}

	public BattleUnit FindUnit(EntityBase entity)
	{
		foreach (var unit in PlayerBattleUnits)
		{
			if (unit != null && unit.character == entity)
			{
				return unit;
			}
		}

		foreach (var unit in MonsterBattleUnits)
		{
			if (unit != null && unit.character == entity)
			{
				return unit;
			}
		}

		return null;
	}

	public bool IsEntityAlive(EntityBase entity)
	{
		var unit = FindUnit(entity);
		return unit != null && unit.IsAlive();
	}

	public void UpdateUnitHealth(EntityBase entity)
	{
		foreach (var unit in PlayerBattleUnits)
		{
			if (unit.character == entity)
			{
				unit.UpdateHP();
				return;
			}
		}

		foreach (var unit in MonsterBattleUnits)
		{
			if (unit.character == entity)
			{
				unit.UpdateHP();
				return;
			}
		}
	}

	public bool UpdateUnitState(EntityBase entity)
	{
		var unit = FindUnit(entity);
		if (unit != null && !unit.IsAlive())
		{
			sys.timelineManager.RemoveDeadEntityFromTimeline(entity);
			sys.UpdateTimelineUI();
			return sys.CheckBattleEndConditionPublic();
		}
		return false;
	}

	public void RemoveDeadUnitsFromTimeline()
	{
		var deadEntities = ListPool<EntityBase>.Get();

		foreach (var unit in PlayerBattleUnits)
			if (unit?.character != null && !unit.IsAlive())
				deadEntities.Add(unit.character);

		foreach (var unit in MonsterBattleUnits)
			if (unit?.character != null && !unit.IsAlive())
				deadEntities.Add(unit.character);

		if (deadEntities.Count > 0)
		{
			sys.timelineManager.RemoveDeadEntitiesFromTimeline(deadEntities);
			sys.UpdateTimelineUI();
		}

		ListPool<EntityBase>.Release(deadEntities);
	}

	public void SyncPlayerBattleUnitsFromPartySlots()
	{
		for (int i = 0; i < PlayerBattleUnits.Count; i++)
		{
			var pos = GetPositionByUnitIndex(i);
			var entity = sys.playerParty.GetEntityAtPosition(pos);

			var unit = PlayerBattleUnits[i];
			unit.character = entity;

			unit.gameObject.SetActive(entity != null);

			if (entity != null)
				unit.SetUp();
		}
	}
}
