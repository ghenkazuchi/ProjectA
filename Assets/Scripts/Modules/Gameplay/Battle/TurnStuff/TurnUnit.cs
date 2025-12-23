using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnUnit
{
	public EntityBase entity;
	public float speed;
	public float actionValue;
	public TurnUnit(EntityBase entity)
	{
		this.entity = entity;
		this.speed = entity.GetFinalStat(Stat.ActionSpeed);
		this.actionValue = 10000f / speed;
	}
}
