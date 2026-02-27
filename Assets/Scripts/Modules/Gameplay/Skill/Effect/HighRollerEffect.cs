using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighRollerEffect : EffectBase, IProcChanceModifier
{
	public float IncreasedProcChance;
	public HighRollerEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}

	public float ModifyProcChance(float baseProcChance)
	{
		return baseProcChance + IncreasedProcChance;
	}
}
