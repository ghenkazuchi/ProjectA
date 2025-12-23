using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Poison Effect Data", menuName = "Effects/Poison Effect")]
public class PoisonEffectData : EffectData
{
	public float basePoisonDamage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new PoisonEffect(EffectType, Effect, Name, owner, target, duration,effectIcon, CanBeRemoved, Stackable, MaxStack)
		{
			BasePoisonDamage = basePoisonDamage,
		};
	}
}
