using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InstantKill", menuName = "Effect/InstantKillData")]
public class InstantKillEffectData : EffectData
{
	public float thresholdPercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new InstantKillEffect(EffectType, Effect, Name, owner, target, duration,effectIcon, CanBeRemoved, Stackable, MaxStack,thresholdPercentage);
	}

}
