using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Healing Received Modifier Effect ", menuName = "Effects/Healing Received Modifier")]
public class HealingReceivedModifierEffectData : EffectData
{
	public float modifyPercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new HealingReceiveModifierEffect(this, owner, target, duration)
		{
			HealingReceivedModifyPercentage = modifyPercentage
		};
	}
}
