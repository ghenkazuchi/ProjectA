using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseHealingEffect : EffectBase, IOnHealingReceived
{
	public float HealingToDamgeRatio;
	public ReverseHealingEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}

	public IEnumerator OnHealingReceived(HealingContext ctx)
	{
		if (ctx.Target == null) yield break;
		int damageAmount = Mathf.CeilToInt(ctx.FinalHealing * Mathf.Clamp01(HealingToDamgeRatio));
		ctx.FinalHealing = -damageAmount;
		yield break;
	}

}
