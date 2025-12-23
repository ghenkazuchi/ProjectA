using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyDamageDealEffect : EffectBase, IModifiOutcomingDamage
{
	public readonly float percent;

	public ModifyDamageDealEffect(EntityBase owner, EntityBase target, int duration,Sprite icon, float percent)
		: base(EffectType.Buff, Effect.Other, "Increased Damage Deal", owner, target, duration,icon,
			   /*isInstant*/ false, /*Stackable?*/ false, /*maxStack*/ 1)
	{
		this.percent = Mathf.Max(0f, percent);
	}
	public float GetOutcomingDamageModifier(EntityBase source)
	{
		return 1 + percent;
	}
}
