using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Parry Effect Data", menuName = "Effects/Parry Effect")]
public class ParryEffectData : EffectData
{
	public float successParryPercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ParryEffect(EffectType, Effect, Name, owner, target, duration,effectIcon, CanBeRemoved, Stackable, MaxStack)
		{
			successfulParryPercentage = successParryPercentage
		};
	}
}
