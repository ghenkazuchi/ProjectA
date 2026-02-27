using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyDamageDealEffect : EffectBase, IModifiOutcomingDamage
{
	public readonly float percent;

	public ModifyDamageDealEffect(EffectData data, EntityBase owner, EntityBase target, int duration, float percent)
		: base(data, owner, target, duration)
	{
		this.percent = Mathf.Max(0f, percent);
	}
	public float GetOutcomingDamageModifier(EntityBase source)
	{
		return 1 + percent;
	}
}
