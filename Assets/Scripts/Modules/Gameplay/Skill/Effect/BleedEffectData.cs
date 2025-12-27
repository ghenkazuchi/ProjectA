using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bleed Effect Data", menuName = "Effects/Bleed")]
public class BleedEffectData : EffectData
{
	public float bleedMaxHealthPercentagePerTurn;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new BleedEffect(EffectType,
			Effect,
			Name,
			owner,
			target,
			duration,
			effectIcon,
			CanBeRemoved,
			Stackable,
			MaxStack)
		{
			bleedMaxHealthPercentagePerTurn = bleedMaxHealthPercentagePerTurn
		};
	}
}
