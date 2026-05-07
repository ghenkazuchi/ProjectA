using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Petrified Effect", menuName = "Effects/Petrified Effect")]
public class PetrifiedEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new PetrifiedEffect(this, owner, target, duration);
	}
}
