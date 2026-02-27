using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IronWill", menuName = "Effects/IncreaseDamageReductionOnDefending")]
public class IncreaseDefendingDamageReductionEffectData : EffectData
{
	public float increasePercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new IncreaseDefendingDamageReductionEffect(this, owner, target, duration)
		{
			IncreasePercentage = increasePercentage
		};
	}
}
