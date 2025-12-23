using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatModifiEffectData", menuName = "Effect/StatModifi")]
public class StatModifiEffectData : EffectData
{
	public Stat StatToModify;
	public int RawValue;
	public float PercentageValue;
	public bool IsRawValue;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new StatModifiEffect(
			EffectType,
			Effect,
			Name,
			owner,
			target,
			duration,
			effectIcon,
			CanBeRemoved,
			Stackable,
			MaxStack,
			StatToModify,
			IsRawValue,
			RawValue,
			PercentageValue
		);
	}

}
