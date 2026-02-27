using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sleep Effect ", menuName = "Effects/Sleep Effect")]
public class SleepEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new SleepEffect(this, owner, target, duration);
	}
}
