using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainTouchEffectData : EffectData
{
	public float healingPercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new DrainTouchEffect(EffectType, Effect, Name, owner, target, duration,effectIcon, CanBeRemoved, Stackable, MaxStack, healingPercentage);
	}
}
