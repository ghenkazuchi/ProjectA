using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnUnit
{
	public EntityBase entity;
	public float speed;
	public float actionValue;
	public int stableOrder;
	public int nextRoundPriorityOffset;

	private const float MinSpeed = 0.01f;

	public TurnUnit(EntityBase entity, int stableOrder = 0)
	{
		this.entity = entity;
		this.stableOrder = stableOrder;
		RefreshSpeed(entity != null ? entity.GetFinalStat(Stat.ActionSpeed) : MinSpeed);
	}

	public TurnUnit(TurnUnit other)
	{
		entity = other.entity;
		speed = other.speed;
		actionValue = other.actionValue;
		stableOrder = other.stableOrder;
		nextRoundPriorityOffset = other.nextRoundPriorityOffset;
	}

	public void RefreshSpeed(float newSpeed)
	{
		speed = Mathf.Max(MinSpeed, newSpeed);
		actionValue = 10000f / speed;
	}
}
