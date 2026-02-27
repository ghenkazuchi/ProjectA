using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stun Effect ", menuName = "Effects/Stun Effect")]
public class StunEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new StunEffect(this, owner, target, duration);
	}
}
