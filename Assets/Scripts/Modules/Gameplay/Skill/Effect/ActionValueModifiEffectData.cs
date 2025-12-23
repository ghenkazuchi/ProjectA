using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AdvanceForwardData", menuName = "Effect/AdvanceForward")]
public class ActionValueModifiEffectData : EffectData
{
	public float ActionAdvantageValue;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ActionValueModifiEffect(
			EffectType,
			Effect,
			Name,
			owner,
			target,
			duration,
			effectIcon,
			CanBeRemoved,
			Stackable,
			MaxStack
		)
		{
			ActionAdvantageValue = ActionAdvantageValue
		};
	}
}
