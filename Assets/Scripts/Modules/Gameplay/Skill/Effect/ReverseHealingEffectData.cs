using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Reverse Healing Effect ", menuName = "Effects/Reverse Healing")]
public class ReverseHealingEffectData : EffectData
{
	public float healingToDamgeRation;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ReverseHealingEffect(this, owner, target, duration)
		{
			HealingToDamgeRatio = healingToDamgeRation
		};
	}
}
