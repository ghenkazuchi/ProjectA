using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Fleeting Step Effect Data", menuName = "Effects/Fleeting Step Effect")]
public class FleetingStepEffectData : EffectData
{
	public float evasionCurrentHpThreshold;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new FleetingStepEffect(EffectType, Effect, Name, owner, target, duration, effectIcon, CanBeRemoved, Stackable, MaxStack)
		{
			evasionPercentage = evasionCurrentHpThreshold,
		};
	}
}
